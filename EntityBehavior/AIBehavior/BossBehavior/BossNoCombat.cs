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

            if (movement)
            {
                movement.OnEndMove += OnEndMove;
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
            OnEndMove(movement);
        }

        private void OnValidate()
        {
        }
        // Update is called once per frame
        void Update()
        {

        }

        void OnEndMove(Movement movement)
        {
            if (entity.isInCombat) return;
            if (!patroling) return;
            if(patrolSpots == null) return;
            if (patrolSpots.Count == 0) return;
            ArchAction.Delay(() => {

                if (entity.isInCombat) return;
                if (movement.isMoving) return;

                var currentSpot = currentPatrolSpot;
                

                currentPatrolSpot = patrolSpots[Random.Range(0, patrolSpots.Count)];

                movement.MoveTo(currentPatrolSpot);


            }, 4f);
        }

        void OnCombatChange(bool val)
        {
            patroling = !val;
        }
    }

}