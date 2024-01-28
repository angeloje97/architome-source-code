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
        public Collider hitBox;

        public HashSet<EntityInfo> entitiesWithinRadius;


        #endregion

        #region Initialization

        public void Start()
        {
        }

        #endregion
    }
}
