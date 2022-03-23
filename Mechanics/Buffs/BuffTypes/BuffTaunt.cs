using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
namespace Architome
{
    public class BuffTaunt : MonoBehaviour
    {
        // Start is called before the first frame update
        public BuffInfo buff;
        public BuffStateChanger stateChanger;
        public GameObject originalFocus;


        public void GetDependencies()
        {
            stateChanger = GetComponent<BuffStateChanger>();
            buff = GetComponent<BuffInfo>();

            if (stateChanger == null || buff == null)
            {
                return;
            }

            stateChanger.OnSuccessfulStateChange += OnSuccessfulStateChange;
            stateChanger.OnStateChangerEnd += OnStateChangerEnd;
            
        }
        void Awake()
        {
            GetDependencies();
        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnSuccessfulStateChange(BuffStateChanger stateChanger, EntityState state)
        {
            if (state != EntityState.Taunted) return;
            if (buff == null) { return; };

            originalFocus = buff.hostInfo.CombatBehavior().GetFocus();
            buff.hostInfo.AIBehavior().CombatBehavior().SetFocus(buff.sourceObject);
            
        }

        void OnStateChangerEnd(BuffStateChanger stateChanger, EntityState newState)
        {
            if (buff == null) { return; }
            if (newState == EntityState.Taunted)
            {
                return;
            }

            
            buff.hostInfo.CombatBehavior().SetFocus(originalFocus);

        }
    }

}
