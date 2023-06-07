using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Architome
{
    public enum LogicGateType
    {
        And,
        Or,
    }
    public class LogicGate : MonoBehaviour
    {
        [SerializeField] bool active;
        [SerializeField] bool[] bools;
        [SerializeField] LogicGateType logicType;

        [SerializeField] UnityEvent<bool> OnStateChange;

        bool activeCheck;

        private void Start()
        {
            Invoke(nameof(CheckBools), 1f);
        }

        private void Update()
        {
            HandleEvents();
        }

        void HandleEvents()
        {
            if(activeCheck != active)
            {
                activeCheck = active;
                OnStateChange?.Invoke(active);
            }
        }

        public void CheckBools()
        {
            if (bools == null) return;
            active = false;

            var andActive = HandleAnd();
            var orActive = HandleOr();

            if(andActive || orActive)
            {
                active = true;
            }


            bool HandleAnd()
            {
                if (logicType != LogicGateType.And) return false;

                foreach(var value in bools)
                {
                    if (!value) return false;
                }

                return true;
            }

            bool HandleOr()
            {
                if (logicType != LogicGateType.Or) return false;

                foreach(var value in bools)
                {
                    if (value) return true;
                }

                return false;
            }
        }

        public void SetTrue(int boolIndex)
        {
            if (bools == null) return;
            if (boolIndex >= bools.Length) return;
            if (boolIndex < 0) return;

            bools[boolIndex] = true;
            CheckBools();
        }

        public void SetFalse(int boolIndex)
        {
            if (bools == null) return;
            if (boolIndex >= bools.Length) return;
            if (boolIndex < 0) return;

            bools[boolIndex] = false ;
            CheckBools();
        }

    }
}
