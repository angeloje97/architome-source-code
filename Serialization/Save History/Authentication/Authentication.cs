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

        protected virtual void UpdateValues() { }

        /// <summary>
        /// updateValues: Will Invoke the UpdateValues which will also overrwrite the authenticated variable.
        /// </summary>
        /// <param name="updateValues"></param>
        /// <returns></returns>
        public bool Validated(bool updateValues = true, bool fromStart=false)
        {
            var original = authenticated;
            if (updateValues) UpdateValues();

            if(!fromStart && original != authenticated)
            {
                OnAuthenticationChange?.Invoke(authenticated);
            }
            else
            {
                OnStartAuthentication?.Invoke(authenticated);
            }
            
            return authenticated;
        }

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
