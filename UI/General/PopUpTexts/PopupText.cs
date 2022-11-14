using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Threading.Tasks;

namespace Architome
{
    public class PopupText : MonoBehaviour
    {
        [Serializable]
        public struct Info
        {
            public float height;
            public float speed;
            public float time;
            public float fadeDelay;
            public float startXPositionRange;
            public float xSpeedRange;
            [Range(0, 1)]
            public float fadeSpeed;

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
            //Animation Parameters
            public bool healthChange;
            public bool stateChange;
            public bool stateImmunity;
            public bool currencyTop;
            public bool currency;


            public bool screenPosition;
        }

        readonly HashSet<string> animatorFields = new HashSet<string>() {
            "healthChange",
            "stateChange",
            "stateImmunity",
            "currencyTop",
            "currecy"
            
        };

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

        public void SetAnimation(PopUpParameters bools)
        {
            parameters = bools;
            foreach (var field in bools.GetType().GetFields())
            {
                if (!animatorFields.Contains(field.Name)) continue;
                info.animator.SetBool(field.Name, (bool)field.GetValue(bools));
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
            if (target == null && lastLocation == new Vector3()) return;

            if (target)
            {
                lastLocation = target.position;
            }

            var position = TargetPosition(target ? target.position : lastLocation);

            position = new Vector3(position.x, position.y, 0);
            position += offset;

            transform.position = position;
        }

        public void SetPopUp(Transform target, string text, Color color, float time = 3f)
        {
            this.target = target;
            StickToTarget();
            info.text.text = text;
            info.time = time;
            info.text.color = color;
            info.text.enabled = true;
            
            

        }

        public void SetOffset(Vector3 offset)
        {
            this.offset = offset;
        }


        public void EndAnimation()
        {
            if (!this) return;
            Destroy(gameObject);
        }

        


    }
}