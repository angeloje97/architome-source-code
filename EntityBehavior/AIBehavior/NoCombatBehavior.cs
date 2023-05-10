using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System;
using Pathfinding;
using UnityEngine.Events;
using Architome.Enums;

namespace Architome
{
    public class NoCombatBehavior : MonoBehaviour
    {
        // Start is called before the first frame update


        public GameObject entityObject;
        public EntityInfo entityInfo;

        public AIBehavior behavior;
        public Movement movement;

        public CharacterInfo character;

        public bool sheathsWeapon;
        public bool walkSpeed;
        public bool returnToStartPosition;
        public bool isSocial;

        public float originalMovementSpeed;

        public Vector3 startingPosition;
        public Quaternion startingDirection;
        public Transform patrolSpot;

        public void GetDependencies()
        {
            entityInfo = GetComponentInParent<EntityInfo>();

            if (entityInfo)
            {
                entityInfo = GetComponentInParent<EntityInfo>();
                entityObject = entityInfo.gameObject;

                behavior = entityInfo.AIBehavior();
                movement = entityInfo.Movement();
                character = entityInfo.CharacterInfo();

                entityInfo.OnCombatChange += OnCombatChange;
                

                originalMovementSpeed = 1;
            }

            if (movement)
            {
                movement.OnArrival += OnArrival;
                movement.OnAway += OnAway;
            }

            if (isSocial)
            {
                gameObject.AddComponent<SocialBehavior>();
            }
        }


        void Start()
        {
            GetDependencies();
            if (entityInfo)
            {
                StartCoroutine(DelayCombatChange());
                IEnumerator DelayCombatChange()
                {

                    yield return new WaitForSeconds(1f);
                    OnCombatChange(entityInfo.isInCombat);
                }
            }

            if (entityObject)
            {
                startingPosition = entityObject.transform.position;
                startingDirection = entityInfo.CharacterInfo().transform.localRotation;
            }
        }


        // Update is called once per frame

        public void OnCombatChange(bool val)
        {
            if (entityInfo.states.Contains(EntityState.MindControlled))
            {
                HandleWalking(true);
                HandleSheathing(true);
                return;
            }

            HandleWalking(val);
            HandleSheathing(val);
            StartCoroutine(DelayedCombatRoutine());


            IEnumerator DelayedCombatRoutine()
            {
                yield return new WaitForSeconds(.25f);
                HandleReturn(val);
                HandlePatrol(val);
            }
        }

        public void HandleSheathing(bool val)
        {
            if (!sheathsWeapon) { return; }
            if (character == null) { return; }
            character.SheathWeapons(!val);
        }
        public void HandleReturn(bool val)
        {
            if (!returnToStartPosition) { return; }
            if (val) { return; }

            if (movement)
            {
                _= movement.MoveToAsync(startingPosition);
            }
        }

        public async void HandlePatrol(bool isInCombat)
        {
            if (isInCombat) { return; }
            if (patrolSpot == null) { return; }
            await movement.MoveToAsync(patrolSpot);

            ManagePatrolDistance();

            async void ManagePatrolDistance()
            {
                int tries = 4;
                int current = 0;

                await Task.Delay(1000);
                while (movement.Target() == patrolSpot)
                {
                    if (movement.DistanceFromTarget() != 0f)
                    {
                        await movement.MoveToAsync(patrolSpot);
                        break;
                    }
                    current++;
                    if (current >= tries) break;
                    await Task.Delay(1000);
                    
                }
            }
            //movement.MoveTo(patrolSpot);
        }
        public void OnArrival(Movement movement, Transform transform)
        {
            if (!returnToStartPosition) { return; }
            if (movement.TargetPosition() == startingPosition)
            {
                Debugger.InConsole(8321, "HasArrived");
                ArchAction.Delay(() => LookAtOriginalPosition(), .5f);
            }
        }

        public void OnAway(Movement movement, Transform transform)
        {

        }

        public void LookAtOriginalPosition()
        {
            character.transform.localRotation = startingDirection;
        }
        public void HandleWalking(bool val)
        {
            if (movement == null) { return; }

            if (!walkSpeed)
            {
                movement.SetWalk(false);
            }
            else
            {
                movement.SetWalk(!val);
            }

        }
    }

}
