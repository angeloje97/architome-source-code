using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace Architome
{
    public class ArchChatBubble : MonoBehaviour
    {
        // Start is called before the first frame update
        public Transform target;
        public CameraAnchor cameraAnchor;
        Camera currentCamera;
        public bool bubbleActive;
        public bool showing;

        [Serializable]
        public struct Info
        {
            public TextMeshProUGUI text;
            public CanvasGroup canvasGroup;
            public float rangePlayerCanSee;
            public Vector3 offset;
        }

        public Info info;


        public ArchChatBubble SetBubble(Transform target, string text, float time = 1f)
        {
            this.target = target;
            GenerateText(text, time);
            currentCamera = CameraManager.Main;
            cameraAnchor = CameraManager.active.cameraAnchor;

            

            return this;
        }

        float generationDuration = 1.5f;
        float fadeDuration = .25f;

        async void GenerateText(string text, float time)
        {
            bubbleActive = true;
            showing = true;
            info.text.text = "";
            var length = text.Length;

            await ArchCurve.Smooth((float lerpValue) => {
                var targetIndex = (int) Mathf.Lerp(0f, length, lerpValue);
                info.text.text = text.Substring(0, targetIndex);
                
            }, CurveType.Linear, generationDuration);

            await Task.Delay((int)(time * 1000));

            bubbleActive = false;

            await info.canvasGroup.SetCanvasAsync(false, fadeDuration);
            showing = false;
            Destroy(gameObject);
        }

        public async Task UntilActiveDone() => await ArchAction.WaitUntil(() => bubbleActive, false);
        public async Task UntilDoneShowing() => await ArchAction.WaitUntil(() => showing, false);

        private void Update()
        {
            if (target == null) return;
            FollowTarget();
        }

        void FollowTarget()
        {
            if (currentCamera == null) return;
            transform.position = currentCamera.WorldToScreenPoint(target.position) + info.offset;
        }
    }

}