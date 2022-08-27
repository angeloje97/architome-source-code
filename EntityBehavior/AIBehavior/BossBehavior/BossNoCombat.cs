using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Architome
{
    public class BossNoCombat : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField] Movement movement;
        [SerializeField] BossRoom roomInfo;
        [SerializeField] EntityInfo entity;
        [SerializeField] BossBehavior behavior;



        public List<Transform> patrolSpots;
        public Transform currentPatrolSpot;
        bool patroling;

        void AcquireDependencies()
        {
            entity = GetComponentInParent<EntityInfo>();

            if (entity)
            {
                movement = entity.Movement();
            }

            behavior = GetComponentInParent<BossBehavior>();
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

        void Start()
        {
            AcquireDependencies();
            Subscribe();
            OnCombatChange(false);
            HandlePatrol();
        }

        private void OnValidate()
        {
        }
        // Update is called once per frame
        void Update()
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