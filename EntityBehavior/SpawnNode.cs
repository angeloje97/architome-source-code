using Architome.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class SpawnNode : MonoBehaviour
    {
        #region Common Data
        [SerializeField] EntityInfo entity;
        [SerializeField] PhysicsEventHandler physicsEvents;
        public float radius;


        #endregion

        #region Initialization

        private void Awake()
        {
        }

        public void Start()
        {
            AdjustHitBox();
            GetDependencies();
        }

        void AdjustHitBox()
        {
        }

        void GetDependencies()
        {
            physicsEvents = PhysicsEventHandler.HandleObject(gameObject, (PhysicsEventHandler handler) => { }, true);
        }

        #endregion
        public void Update()
        {
            
        }
    }
}
