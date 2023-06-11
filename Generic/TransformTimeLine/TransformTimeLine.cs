using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Architome
{
    [Serializable]
    public struct TransformFrame
    {
        public float time;
        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotation;

        public void SetTransformFrame(Transform trans)
        {
            position = trans.position;
            scale = trans.localScale;
            rotation = trans.rotation;
        }

        public void LerpTransformFrame(TransformFrame a, TransformFrame b, float lerpValue)
        {
            position = Vector3.Lerp(a.position, b.position, lerpValue);
            rotation = Quaternion.Lerp(a.rotation, b.rotation, lerpValue);
            scale = Vector3.Lerp(a.scale, b.scale, lerpValue);
        }

        public void SetTransform(Transform transform)
        {
            transform.position = position;
            transform.rotation = rotation;
            transform.localScale = scale;
        }
    }
    public class TransformTimeLine : MonoBehaviour
    {

        

        [Header("Time Line Actions")]
        [SerializeField] bool saveFirstFrame;
        [SerializeField] bool saveSecondFrame;

        [Header("Properties")]
        [SerializeField] TransformFrame firstFrame;
        [SerializeField] TransformFrame secondFrame;

        [Header("Actions")]
        public UnityEvent OnMaxLerp;
        public UnityEvent OnNoLerp;

        #region Validation Region
        private void OnValidate()
        {
            HandleSaveFirstFrame();
            HandleSaveSecondFrame();
        }

        void HandleSaveFirstFrame()
        {
            if (!saveFirstFrame) return;
            saveFirstFrame = false;

            firstFrame.SetTransformFrame(transform);
        }

        void HandleSaveSecondFrame()
        {
            if (!saveSecondFrame) return;
            saveSecondFrame = false;
            secondFrame.SetTransformFrame(transform);
        }
        #endregion

        float lerpValue;
        float lerpCheck;

        #region Run Time

        protected virtual void Start()
        {
            firstFrame.SetTransform(transform);
        }

        private void Update()
        {
            HandleEvents();
        }

        void HandleEvents()
        {
            if (lerpCheck != lerpValue)
            {
                if (lerpCheck == 1f)
                {
                    OnMaxLerp?.Invoke();
                }

                if (lerpCheck == 0f)
                {
                    OnNoLerp?.Invoke();
                }
                lerpCheck = lerpValue;
            }
        }

        public virtual void  Lerp(float lerpValue)
        {
            this.lerpValue = Mathf.Clamp(lerpValue, 0f, 1f);
            var transformFrame = new TransformFrame();
            transformFrame.LerpTransformFrame(firstFrame, secondFrame, lerpValue);
            transformFrame.SetTransform(transform);
            
        }
        #endregion
    }
}
