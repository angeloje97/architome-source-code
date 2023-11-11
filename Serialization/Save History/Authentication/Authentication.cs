using PixelCrushers.DialogueSystem.UnityGUI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Architome
{
    public class AuthenticationDetails
    {
        public List<string> validValues;
        public List<string> invalidValues;

        public AuthenticationDetails()
        {
            validValues = new();
            invalidValues = new();
        }
    }

    public class Authentication : MonoBehaviour
    {
        public UnityEvent<bool> OnStartAuthentication;
        public UnityEvent<bool> OnDestroyAuthentication;
        public UnityEvent<bool> OnAuthenticationChange;

        [Header("Authentication Fields")]
        public LogicType authenticationLogic;
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

        public virtual bool Validated(bool updateValues = false) => true;

        protected bool ValidDictionary<T>(Dictionary<T, bool> dict)
        {
            var valueList = dict.Select((KeyValuePair<T, bool> pairs) => {
                return pairs.Value;
            }).ToList();

            authenticated = new ArchLogic(valueList).Valid(authenticationLogic);
            return authenticated;
        }

        public virtual AuthenticationDetails Details() => new();


        
    }
}
