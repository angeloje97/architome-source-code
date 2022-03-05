using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Architome
{
    public class ERoomHandler : EntityProp
    {
        // Start is called before the first frame update

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
            if(previousRoom) { previousRoom.OnShowRoom -= OnShowRoom; }
            if(nextRoom) { nextRoom.OnShowRoom += OnShowRoom; }
        }

        public void OnShowRoom(RoomInfo room, bool isShown)
        {
            entityInfo.OnCurrentShowRoom?.Invoke(room, isShown);
        }

        public void OnDestroy()
        {
            if(entityInfo.currentRoom)
            {
                entityInfo.currentRoom.OnShowRoom -= OnShowRoom;
            }
        }
        void Start()
        {
            GetDependenices();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
