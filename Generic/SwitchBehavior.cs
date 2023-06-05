using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Architome
{
    [RequireComponent(typeof(WorkInfo))]
    public class SwitchBehavior : MonoBehaviour
    {

        [SerializeField] WorkInfo workInfo;
        
        [SerializeField] bool on;

        [SerializeField] UnityEvent<bool> OnChangeState;
        [SerializeField] UnityEvent OnTurnOff;
        [SerializeField] UnityEvent OnTurnOn;




        void Start()
        {
            GetDependencies();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        void GetDependencies()
        {
            workInfo = GetComponent<WorkInfo>();
        }

        public void SetState(bool state)
        {
            on = state;

            OnChangeState?.Invoke(on);

            if (on)
            {
                OnTurnOn?.Invoke();
            }
            else
            {
                OnTurnOff?.Invoke();
            }
        }
    }
}
