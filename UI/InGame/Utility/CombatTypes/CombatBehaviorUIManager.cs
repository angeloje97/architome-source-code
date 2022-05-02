using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class CombatBehaviorUIManager : MonoBehaviour
    {
        [System.Serializable]
        public struct Prefabs
        {
            public GameObject combatBehaviorTemplate;
        }

        public Prefabs prefabs;

        void GetDependencies()
        {
            GMHelper.GameManager().OnNewPlayableEntity += OnNewPlayableEntity;
        }
        void Start()
        {
            GetDependencies();
        }

        void OnNewPlayableEntity(EntityInfo entity, int index)
        {
            if (prefabs.combatBehaviorTemplate == null) return;

            Instantiate(prefabs.combatBehaviorTemplate, transform).GetComponent<CombatBehaviorUI>().SetEntity(entity);
        }
    }
}
