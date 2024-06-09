using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Architome
{
    public class BossNoCombat : EntityProp
    {
        // Start is called before the first frame update
        [SerializeField] Movement movement;
        [SerializeField] BossRoom roomInfo;
        [SerializeField] EntityInfo entity;
        [SerializeField] BossBehavior behavior;



        public List<Transform> patrolSpots;
        public Transform currentPatrolSpot;
        bool patroling;

        public override void GetDependencies()
        {
            entity = GetComponentInParent<EntityInfo>();

            if (entity)
            {
                movement = entity.Movement();
            }

            behavior = GetComponentInParent<BossBehavior>();

            Subscribe();
            OnCombatChange(false);
            HandlePatrol();
        }

        void Subscribe()
        {
            if (behavior)
            {
                roomInfo = behavior.BossRoom();
            }

            if (entity)
            {
                entity.OnCombatChange += OnCombatChange;
                patroling = entity.isInCombat;
            }

            if (roomInfo)
            {
                patrolSpots = roomInfo.bossPatrolSpots.GetComponentsInChildren<Transform>(true).ToList();

                if (patrolSpots.Contains(roomInfo.bossPatrolSpots))
                {
                    patrolSpots.Remove(roomInfo.bossPatrolSpots);
                }

                patrolSpots.Add(behavior.originalPosition);
            }
        }

        private void OnValidate()
        {

        }

        void OnCombatChange(bool val)
        {
            HandlePatrol();
        }

        async void HandlePatrol()
        {
            if (movement == null) return;
            if (patrolSpots == null) return;
            if (patrolSpots.Count <= 1) return;
            
            if (patroling) return;
            patroling = true;
            while (!entity.isInCombat)
            {
                var newPatrolSpot = ArchGeneric.RandomItem(patrolSpots);

                while (newPatrolSpot == currentPatrolSpot)
                {
                    newPatrolSpot = ArchGeneric.RandomItem(patrolSpots);
                    await Task.Yield();
                }

                currentPatrolSpot = newPatrolSpot;

                var arrived = await movement.MoveToAsync(currentPatrolSpot);

                await Task.Delay(4000);
            }
            patroling = false;
        }
    }

}