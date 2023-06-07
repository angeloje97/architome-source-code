using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class SingleController : MonoBehaviour
    {
        public static SingleController active;

        Movement movement;

        private void Awake()
        {
            if (!active)
            {
                active = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        EntityInfo controllableEntity;
        public EntityInfo CurrentEntity => controllableEntity;


        private void Start()
        {
            ArchAction.Delay(GetDependencies, .25f);
        }

        void GetDependencies()
        {
            controllableEntity = GetComponentInChildren<EntityInfo>();
        }
    }
}
