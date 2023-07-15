using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Architome
{
    public class Authentication : MonoBehaviour
    {
        public UnityEvent<bool> OnStartAuthentication;
        public UnityEvent<bool> OnDestroyAuthentication;
        public UnityEvent<bool> OnAuthenticationChange;

        protected bool authenticated;

        public virtual void Start()
        {
            OnAuthenticationStart();
        }

        public virtual void OnDestroy()
        {
            OnAuthenticationDestroy();
        }

        public virtual void OnAuthenticationStart() { }
        public virtual void OnAuthenticationDestroy() { }


    }
}
