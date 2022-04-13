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
        }

        public Info info;


        public ArchChatBubble SetBubble(Transform target, string text, float time = 1f)
        {
            this.target = target;
            info.text.text = text;
            cameraAnchor = CameraManager.active.cameraAnchor;
            return this;
        }

          

        private void Update()
        {
            if (target == null) return;
            FollowTarget();
        }

        void FollowTarget()
        {
            transform.position = CameraManager.active.Current.WorldToScreenPoint(target.position);
        }
    }

}