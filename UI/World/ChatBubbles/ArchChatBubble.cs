using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using System.Threading.Tasks;

namespace Architome
{
    public class ArchChatBubble : MonoBehaviour
    {
        // Start is called before the first frame update
        public Transform target;
        public CameraAnchor cameraAnchor;

        [Serializable]
        public struct Info
        {
            public TextMeshProUGUI text;
            public float rangePlayerCanSee;
            public Vector3 offset;
        }

        public Info info;


        public ArchChatBubble SetBubble(Transform target, string text, float time = 1f)
        {
            this.target = target;
            GenerateText(text, time);
            cameraAnchor = CameraManager.active.cameraAnchor;

            

            return this;
        }

        async void GenerateText(string text, float time)
        {
            int index = 0;
            info.text.text = "";
            while (info.text.text != text)
            {
                info.text.text += $"{text[index]}";
                index++;
                await Task.Yield();
            }

            ArchAction.Delay(() => { Destroy(gameObject); }, time);
        }

          

        private void Update()
        {
            if (target == null) return;
            FollowTarget();
        }

        void FollowTarget()
        {
            transform.position = CameraManager.active.Current.WorldToScreenPoint(target.position) + info.offset;
        }
    }

}