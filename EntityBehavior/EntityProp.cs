using Architome.Events;
using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Architome
{
    public class EntityProp : MonoActor
    {

        //Last Checked: Character Equipment Handler
        #region Common Data

        public EntityInfo entityInfo;

        public CombatEvents combatEvents => entityInfo.combatEvents;
        public InfoEvents infoEvents => entityInfo.infoEvents;


        #endregion

        #region Initiation
        public bool initiated { get; private set; }


        protected virtual async void Start()
        {
            await Initiate();
        }

        protected async Task Initiate()
        {
            try
            {
                var parent = transform.parent;

                entityInfo = parent.GetComponent<EntityInfo>();

                if (entityInfo == null)
                {
                    var entityPropParent = parent.GetComponent<EntityProp>();

                    if (entityPropParent == null) throw new Exception($"Could not find parent entity prop for {gameObject.name} gameObject");

                    await entityPropParent.UntilInitiationComplete();

                    entityInfo = entityPropParent.entityInfo;
                }
                else
                {
                    await ArchAction.WaitUntil(() => entityInfo.initiated, true);
                }

                if (entityInfo == null) throw new Exception($"Could not find source entity for {gameObject.name} gameObject");
                if (this == null) return;
                GetDependencies();
                await GetDependenciesTask();
                initiated = true;
            }
            catch (Exception e)
            {
                Defect.CreateIndicator(transform, "EntityProp", e);
                throw e;
            }

        }
        public virtual async Task GetDependenciesTask() => await Task.Yield();

        public virtual void GetDependencies() { }

        #endregion

        public async Task UntilInitiationComplete()
        {
            await ArchAction.WaitUntil(() => initiated, true);
        }

        #region Loop

        protected virtual void Update()
        {
            if (!initiated) return;
            EUpdate();
        }

        public virtual void EUpdate()
        {

        }
        #endregion

        #region Events

        ArchEventHandler<ePropEvent, EntityProp> propertyEventHandler;

        public Action AddPropListener(ePropEvent trigger, Action<EntityProp> action, MonoActor actor) => propertyEventHandler.AddListener(trigger, action, actor);

        public void InvokeProp(ePropEvent trigger, EntityProp data) => propertyEventHandler.Invoke(trigger, data);

        public Action AddPropListenerCheck(ePropEvent trigger, Action<EntityProp, List<bool>> action, MonoActor actor) => propertyEventHandler.AddListenerCheck(trigger, action, actor);

        public bool InvokeCheck(ePropEvent trigger, EntityProp data) => propertyEventHandler.InvokeCheck(trigger, data);

        #endregion
    }

    public enum ePropEvent
    {
        OnCanUpdateCheck,
    }
}
