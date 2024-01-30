using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class SpawnNode : MonoBehaviour
    {
        #region Common Data
        [SerializeField] EntityInfo entity;
        public float radius;
        public SphereCollider detectionRadius;

        public HashSet<EntityInfo> entitiesWithinRadius;


        #endregion

        #region Initialization

        private void Awake()
        {
            entitiesWithinRadius = new();
        }

        public void Start()
        {
            AdjustHitBox();
            GetDependencies();
        }

        void AdjustHitBox()
        {
            detectionRadius.radius = radius;
        }

        void GetDependencies()
        {

        }

        #endregion
        public void Update()
        {
            
        }
    }
}
