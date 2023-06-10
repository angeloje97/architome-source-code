using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        #region Run Time

        private void Start()
        {
            firstFrame.SetTransform(transform);
        }

        public void Lerp(float lerpValue)
        {
            var transformFrame = new TransformFrame();
            transformFrame.LerpTransformFrame(firstFrame, secondFrame, lerpValue);
            transformFrame.SetTransform(transform);
            
        }
        #endregion
    }
}
