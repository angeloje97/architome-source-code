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
        public Info info;

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

        void StickToTarget()
        {
            if (target == null) return;

            var position = MainCamera3.active.WorldToScreenPoint(target.position);

            position = new Vector3(position.x, position.y, 0);

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