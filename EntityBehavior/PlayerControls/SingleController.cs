using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace Architome
{
    public class SingleController : MonoBehaviour
    {
        public static SingleController active;

        Movement movement;
        PlayerController controller;

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
            var gameManager = GameManager.active;

            if (controllableEntity)
            {
                gameManager.AddPlayableCharacter(controllableEntity);

                movement = controllableEntity.Movement();
                controller = controllableEntity.PlayerController();
            }
        }

        public void OnAction()
        {
            controller.HandleActionButton(true);
        }
    }
}
