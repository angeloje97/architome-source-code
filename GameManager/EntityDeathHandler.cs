using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Architome
{

    public class EntityDeathHandler : MonoBehaviour
    {
        public static EntityDeathHandler active;
        


        // Start is called before the first frame update
        [SerializeField]
        private List<EntityInfo> deadPlayableEntities;
        public bool allPlayableEntitiesDead;

        public Action<CombatEventData> OnEntityDeath;
        public Action<CombatEventData> OnNPCDeath;
        public Action<CombatEventData> OnPlayableEntityDeath;
        public Action<List<EntityInfo>> OnAllPlayableEntityDeath;

        //Private

        GameManager gameManager;


        private void Awake()
        {
            active = this;

            if(GMHelper.GameManager())
            {
                gameManager = GMHelper.GameManager();
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void HandleDeadEntity(CombatEventData eventData)
        {
            CheckDeadPlayableEntites();
            var entity = eventData.target;
            var source = eventData.source;

            OnEntityDeath?.Invoke(eventData);

            if(Entity.IsPlayer(entity.gameObject))
            {
                OnPlayableEntityDeath?.Invoke(eventData);
            }
            else
            {
                OnNPCDeath?.Invoke(eventData);
            }
        }

        public void HandleLifeChange(GameObject entity)
        {
            if (!Entity.IsEntity(entity)) { return; }
            var entityInfo = entity.GetComponent<EntityInfo>();
            var isAlive = entityInfo.isAlive;
            var isPlayer = Entity.IsPlayer(entity);

            HandleDead();
            HandleAlive();

            void HandleDead()
            {
                if (!isPlayer) { return; }
                if (isAlive) { return; }

                if(!deadPlayableEntities.Contains(entityInfo))
                {
                    deadPlayableEntities.Add(entityInfo);


                    if(deadPlayableEntities.Count == GMHelper.GameManager().playableEntities.Count)
                    {
                        OnAllPlayableEntityDeath?.Invoke(deadPlayableEntities);
                    }
                }
            }

            void HandleAlive()
            {
                if (!isPlayer) { return; }
                if (!isAlive) { return; }

                if(deadPlayableEntities.Contains(entityInfo))
                {
                    deadPlayableEntities.Remove(entityInfo);
                }
            }

        }


        public void CheckDeadPlayableEntites()
        {
            for(int i = 0; i < deadPlayableEntities.Count; i++)
            {
                var info = deadPlayableEntities[i].GetComponent<EntityInfo>();

                if(info.isAlive)
                {
                    deadPlayableEntities.RemoveAt(i);
                    i--;
                }
            }
        }


        public List<EntityInfo> DeadPlayableEntities()
        {
            CheckDeadPlayableEntites();

            return deadPlayableEntities;
        }
    }
}
