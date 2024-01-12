using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Threading.Tasks;
using Pathfinding;
using Language.Lua;
using static Architome.PopupText;

namespace Architome
{
    public class PopUpParameters
    {
        public PopUpParameters(Transform target, string text)
        {
            this.target = target;
            this.text = text;

            states = new();
        }

        public string text;
        public Color color;
        public Transform target;

        public eAnimatorBool boolean;
        public eAnimatorTrigger trigger;
        public HashSet<eAnimatorState> states;
        //Animation Parameters

        public bool screenPosition;
    }

    public class PopupText : MonoBehaviour
    {
        [Serializable]
        public struct Info
        {
            [Header("Components")]
            public TextMeshProUGUI text;
            public Transform textTransform;
            public CanvasGroup canvasGroup;
            public Animator animator;

        }

        public Transform target;
        public Vector3 lastLocation;
        public Vector3 offset;
        public Info info;
        public Camera mainCamera;

        

        public enum eAnimatorBool
        {
            None,
            HealthChange,
            StateChange,
            StateImmunity,
            CurrencyTop,
            Currency,
        }

        public enum eAnimatorState
        {
            EnableRepeat,
        }

        public enum eAnimatorTrigger
        {
            None,
            HealthChangeRepeat,
        }

        [SerializeField] PopUpParameters parameters;


        // Start is called before the first frame update

        private void Awake()
        {

        }

        private void Start()
        {
            transform.SetAsLastSibling();
        }
        // Update is called once per frame
        void Update()
        {
            StickToTarget();
        }

        void SetAnimation(PopUpParameters parameters)
        {
            this.parameters = parameters;

            if (parameters.boolean != eAnimatorBool.None)
            {
                info.animator.SetBool(parameters.boolean.ToString(), true);
            }

            if(parameters.trigger != eAnimatorTrigger.None)
            {
                info.animator.SetTrigger(parameters.trigger.ToString());
            }

            if (parameters.states != null)
            {
                foreach(var state in parameters.states)
                {
                    info.animator.SetBool(state.ToString(), true);
                }
            }
        }

        public bool ContainsState(eAnimatorState state)
        {
            if (parameters.states == null) return false;
            return parameters.states.Contains(state);
        }

        public Vector3 TargetPosition(Vector3 position)
        {
            if (parameters != null && parameters.screenPosition)
            {
                return position;
            }

            if (mainCamera == null)
            {
                mainCamera = MainCamera3.active;
            }

            return mainCamera.WorldToScreenPoint(position);
        }

        void StickToTarget()
        {
            if (target == null && lastLocation == new Vector3()) 
            {
                Debugger.UI(2422, $"Target is null and last location is not set ");
                return;
            }

            if (target)
            {
                lastLocation = target.position;
            }

            var position = TargetPosition(target ? target.position : lastLocation);

            Debugger.UI(2423, $"Target position is {position}");

            position = new Vector3(position.x, position.y, 0);
            position += offset;

            transform.position = position;
        }

        public void SetPopUp(PopUpParameters parameters)
        {
            this.target = parameters.target;
            StickToTarget();
            SetAnimation(parameters);
            info.text.text = parameters.text;
            info.text.color = parameters.color;
            info.text.enabled = true;
            HandleRandomDirection();

        }

        public void UpdatePopUp(PopUpParameters parameters)
        {
            info.text.text = parameters.text;
            SetAnimation(parameters);
            transform.SetAsLastSibling();
        }

        public void SetOffset(Vector3 offset)
        {
            this.offset = offset;
        }



        bool ignoreEndAnimation;
        public void EndAnimation()
        {
            if (!this) return;
            if (ignoreEndAnimation) return;
            Destroy(gameObject);
        }

        [Header("Direction Settings")]
        [SerializeField] bool randomDirection;
        [SerializeField] float randomDirectionRange;

        float xDirection;
        float yDirection;
        float previousAngle = 0f;


        async void HandleRandomDirection()
        {
            if (!randomDirection) return;
            var angle = 0f;

            do
            {
                angle = UnityEngine.Random.Range(0f, 360f);

            }
            while (RequiresNewAngle(angle, previousAngle));

            previousAngle = angle;

            xDirection = (float) Math.Sin(Math.PI * 2 * angle / 360f);
            yDirection = (float) Math.Cos(Math.PI * 2 * angle / 360f);



            Debugger.UI(2425, $"x: {xDirection}, y: {yDirection}");
            var currentX = 0f;
            var currentY = 0f;


            await ArchCurve.SmoothLate((float lerp) =>
            {
                currentX = randomDirectionRange * xDirection * lerp;
                currentY = randomDirectionRange * yDirection * lerp;
                info.text.transform.localPosition = new Vector3(currentX, currentY, 0f);

                Debugger.UI(2427, $"Local position: {info.text.transform.localPosition}");
                Debugger.UI(2426, $"xCurrent {currentX} yCurrent: {currentY}");
                Debugger.UI(2424, $"Lerp Value is {lerp}");
            }, CurveType.EaseOut, 1f);

            await World.UpdateAction((float deltaTime) => {
                if(this == null) return false;

                info.text.transform.localPosition = new Vector3(currentX, currentY, 0f);

                return true;
            }, true);

            bool RequiresNewAngle(float a, float b)
            {
                if (Mathf.Abs(a - b) < 35f) return true;

                if(a < b)
                {
                    var aOffset = a + 360f;
                    if (Mathf.Abs(aOffset - b) < 35f) return true;
                }
                else
                {
                    var bOffset = b + 360f;
                    if (Mathf.Abs(a - bOffset) < 35f) return true;
                }

                return false;
            }
        }
    }
}