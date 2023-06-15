using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using static CombatInfo.CombatLogs;

namespace Architome
{
    public class ItemContainer : MonoBehaviour
    {
        WorldModuleCore worldModule;

        public Action<ItemContainer> OnCloseContainer;
        public Action<ItemContainer> OnOpenContainer;
        public Action<ItemContainer> OnForceClose;

        [Serializable]
        public struct ContainerEvents
        {
            public UnityEvent<ItemContainer> OnCloseContainer;
            public UnityEvent<ItemContainer> OnOpenContainer;
        }

        public ContainerEvents events;

        public int maxCapacity;
        public List<ItemData> items;
        public InteractionType interactionType;

        private void OnValidate()
        {
            items ??= new();

            if (items.Count == maxCapacity) return;

            while(items.Count < maxCapacity)
            {
                items.Add(new ItemData());
            }
        }

        private void Start()
        {
            GetDependencies();
        }

        void GetDependencies()
        {
            worldModule = WorldModuleCore.active;
        }

        public async void OpenContainer(TaskEventData task)
        {
            if (interactionType == InteractionType.ShowModule)
            {
                worldModule.HandleItemContainer(this);
                return;
            }

            var world = WorldActions.active;
            if (world == null) return;
            OnOpenContainer?.Invoke(this);
            events.OnOpenContainer?.Invoke(this);
            foreach (var item in items)
            {
                world.DropItem(item, transform.position + new Vector3(0, 1.5F, 0), true, true);

                await Task.Delay(333);
            }
        }


        public void ForceClose()
        {
            OnForceClose?.Invoke(this);
        }
    }
}
