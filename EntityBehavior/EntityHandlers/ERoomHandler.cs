using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;


namespace Architome
{
    public class ERoomHandler : EntityProp
    {
        // Start is called before the first frame update

        public RoomInfo previousRoom;

        new public void GetDependencies()
        {
            base.GetDependencies();
            entityInfo.OnPhysicsEvent += OnPhysicsEvent;
            entityInfo.OnRoomChange += OnRoomChange;

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
        private void Awake()
        {
            GetDependencies();
            AcquireRoom();
        }
        // Update is called once per frame
        void Update()
        {
            HandleEvents();
        }

        async void AcquireRoom()
        {
            if (MapRoomGenerator.active == null) return;
            while (entityInfo.currentRoom == null)
            {
                await Task.Delay(1000);
                entityInfo.currentRoom = entityInfo.CurrentRoom();
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
