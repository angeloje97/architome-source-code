using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

namespace Architome
{
    public class ERoomHandler : EntityProp
    {
        // Start is called before the first frame update

        public RoomInfo previousRoom;

        public override async Task GetDependencies(Func<Task> extension)
        {
            await base.GetDependencies(async () => {

                entityInfo.OnPhysicsEvent += OnPhysicsEvent;
                entityInfo.OnRoomChange += OnRoomChange;
                entityInfo.sceneEvents.OnTransferScene += OnTransferScene;

                entityInfo.infoEvents.OnSignificantMovementChange += OnSignificantMovementChange;

                AcquireRoom();

                await extension();
            });
        }

        private void OnSignificantMovementChange(Vector3 newPosition)
        {
            entityInfo.currentRoom = Entity.Room(newPosition);
        }

        public override void EUpdate()
        {
            HandleEvents();
        }

        public void OnPhysicsEvent(EntityInfo entity, GameObject other, bool isEnter)
        {
            if(!other.GetComponent<WalkThroughActivate>()) { return; }
            entityInfo.currentRoom = entityInfo.CurrentRoom();
        }

        public void OnRoomChange(RoomInfo previousRoom, RoomInfo nextRoom)
        {
            if(previousRoom) 
            { 
                previousRoom.events.OnShowRoom -= OnShowRoom;
                previousRoom.entities.HandleEntityExit(entityInfo);
            }

            if(nextRoom) 
            {
                nextRoom.events.OnShowRoom += OnShowRoom;
                nextRoom.entities.HandleEntityEnter(entityInfo);
            }

        }

        public void OnTransferScene(string sceneName)
        {
            ArchAction.Delay(() => {
                entityInfo.currentRoom = Entity.Room(entityInfo.transform.position);
            }, .125f);
        }

        public void OnShowRoom(RoomInfo room, bool isShown)
        {
            entityInfo.OnCurrentShowRoom?.Invoke(room, isShown);
        }

        public void OnEntityDestroy()
        {
            if(entityInfo.currentRoom)
            {
                entityInfo.currentRoom.events.OnShowRoom -= OnShowRoom;
            }
        }
        

        async void AcquireRoom()
        {
            if (MapRoomGenerator.active == null) return;
            var tries = 5;
            var current = 0;
            while (this != null && entityInfo.currentRoom == null && current < tries)
            {
                
                entityInfo.currentRoom = entityInfo.CurrentRoom();
                current++;
                await Task.Delay(1000);
            }

        }

        void HandleEvents()
        {
            if (entityInfo.currentRoom != previousRoom)
            {
                entityInfo.OnRoomChange?.Invoke(previousRoom, entityInfo.currentRoom);
                previousRoom = entityInfo.currentRoom;
            }
        }
    }

}
