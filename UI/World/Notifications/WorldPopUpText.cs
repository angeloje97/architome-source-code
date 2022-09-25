using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Threading.Tasks;

namespace Architome
{
    public class WorldPopUpText : MonoBehaviour
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
            public bool healthChange;
            public bool stateChange;
            public bool stateImmunity;
        }


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
            foreach (var field in bools.GetType().GetFields())
            {
                info.animator.SetBool(field.Name, (bool)field.GetValue(bools));
            }
        }

        public Vector3 WorldToScreenPoint(Vector3 position)
        {
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

            var position = WorldToScreenPoint(target ? target.position : lastLocation);

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
            
            

            Play();
        }

        public void SetOffset(Vector3 offset)
        {
            this.offset = offset;
        }

        async void Play()
        {
            return;
            var startXPos = UnityEngine.Random.Range(-info.startXPositionRange, info.startXPositionRange);

            info.text.transform.localPosition = new Vector3(startXPos, 0, 0);


            var xSpeed = UnityEngine.Random.Range(-info.xSpeedRange, info.xSpeedRange);
            var timer = info.fadeDelay;
            var xPosition = 0f;
            while ( info.canvasGroup.alpha > 0)
            {
                await Task.Yield();
                info.height += info.speed * Time.deltaTime;
                xPosition += xSpeed * Time.deltaTime;
                info.textTransform.localPosition = new Vector3(startXPos + xPosition, info.height, 0);

                if (timer <= 0)
                {
                    info.canvasGroup.alpha -= info.fadeSpeed * Time.deltaTime;
                }
                else
                {
                    timer -= Time.deltaTime;
                }

            }
            Destroy(gameObject);
        }

        public void EndAnimation()
        {
            Destroy(gameObject);
        }

        


    }
}