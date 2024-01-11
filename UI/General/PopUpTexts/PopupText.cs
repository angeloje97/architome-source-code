using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Threading.Tasks;
using Pathfinding;

namespace Architome
{
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

        public class PopUpParameters
        {
            public eAnimatorBools boolean;
            public eAnimatorTriggers trigger;
            //Animation Parameters

            public bool screenPosition;
        }

        public enum eAnimatorBools
        {
            None,
            HealthChange,
            StateChange,
            StateImmunity,
            CurrencyTop,
            Currency,
        }

        public enum eAnimatorTriggers
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

        public void SetAnimation(PopUpParameters parameters)
        {
            this.parameters = parameters;

            if (parameters.boolean != eAnimatorBools.None)
            {
                info.animator.SetBool(parameters.boolean.ToString(), true);
            }

            if(parameters.trigger != eAnimatorTriggers.None)
            {
                info.animator.SetTrigger(parameters.trigger.ToString());
            }
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

        public void SetPopUp(Transform target, string text, Color color)
        {
            this.target = target;
            StickToTarget();
            info.text.text = text;
            info.text.color = color;
            info.text.enabled = true;
            HandleRandomDirection();
        }

        public void UpdatePopUp(string text, PopUpParameters parameters)
        {
            info.text.text = text;
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

        async void HandleRandomDirection()
        {
            if (!randomDirection) return;
            var angle = UnityEngine.Random.Range(0f, 360f);

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
        }
    }
}