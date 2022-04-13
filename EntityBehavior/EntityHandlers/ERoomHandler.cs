using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Architome
{
    public class ERoomHandler : EntityProp
    {
        // Start is called before the first frame update

        public RoomInfo previousRoom;

        public void GetDependenices()
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

        public void OnDestroy()
        {
            if(entityInfo.currentRoom)
            {
                entityInfo.currentRoom.events.OnShowRoom -= OnShowRoom;
            }
        }
        void Start()
        {
            GetDependenices();
        }

        // Update is called once per frame
        void Update()
        {
            HandleEvents();
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
