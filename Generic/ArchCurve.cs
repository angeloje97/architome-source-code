using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    public enum CurveType
    {
        EaseInOut,
        EaseIn,
        EaseOut,
        Linear,
    }
    public class ArchCurve : MonoBehaviour
    {
        public static ArchCurve active;

        [Serializable]
        public struct CurveData
        {
            public CurveType type;
            public AnimationCurve animationCurve;
        }

        [SerializeField] List<CurveData> curves;
        [SerializeField] Dictionary<CurveType, AnimationCurve> curveDict;

        private void Awake()
        {
            if(active && active != this)
            {
                Destroy(gameObject);
                return;
            }

            active = this;

            ArchGeneric.DontDestroyOnLoad(gameObject);
            CreateDictionary();
        }

        Action OnLateUpdate;
        private void LateUpdate()
        {
            OnLateUpdate?.Invoke();
        }

        void CreateDictionary()
        {
            curveDict = new();
            if (curves == null) return;

            foreach(var curve in curves)
            {
                if (curveDict.ContainsKey(curve.type)) continue;
                curveDict.Add(curve.type, curve.animationCurve);
            }
        }

        public AnimationCurve Curve(CurveType type)
        {
            if (!curveDict.ContainsKey(type))
            {
                return curveDict[CurveType.Linear];
            }

            return curveDict[type];
        }

        #region Static Functions


        public static async Task Smooth(Action<float> action, CurveType type, float totalTime)
        {
            var instance = active;

            if(instance == null)
            {
                action(1f);
                return;
            }

            var curve = instance.Curve(type);
            if(curve == null)
            {
                action(1f);
                return;
            }
            
            if(totalTime <= 0)
            {
                action(1f);
            }


            var time = 0f;

            while(time < totalTime)
            {
                time += Time.deltaTime;
                var lerpValue = curve.Evaluate(time / totalTime);
                action(lerpValue);
                await Task.Yield();
            }
        }

        public static async Task SmoothLate(Action<float> action, CurveType type, float totalTime)
        {
            var instance = active;

            if (instance == null)
            {
                action(1f);
                return;
            }

            var curve = instance.Curve(type);
            if (curve == null)
            {
                action(1f);
                return;
            }

            if (totalTime <= 0)
            {
                action(1f);
            }


            var time = 0f;

            active.OnLateUpdate += HandleLateUpdate;

            while (time < totalTime)
            {
                await Task.Yield();
            }

            active.OnLateUpdate -= HandleLateUpdate;

            void HandleLateUpdate()
            {
                if(time < totalTime)
                {
                    time += Time.deltaTime;
                    var lerpValue = curve.Evaluate(time / totalTime);
                    action(lerpValue);
                }
            }
        }

        #endregion
    }
}
