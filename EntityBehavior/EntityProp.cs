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
            Action onInitiated = null;

            try
            {
                var parent = transform.parent;

                entityInfo = parent.GetComponent<EntityInfo>();

                if (entityInfo == null)
                {
                    var entityPropParent = parent.GetComponent<EntityProp>();

                    if (entityPropParent == null) throw new Exception($"Could not find parent entity prop for {gameObject.name} gameObject");

                    var result = await entityPropParent.UntilInitiationComplete();

                    if (!result) return;

                    entityInfo = entityPropParent.entityInfo;

                    onInitiated += infoEvents.AddListenerCheck(InfoEvents.EventType.OnHasGatheredDependenciesCheck, (entityData, checks) => {
                        checks.Add(false);
                    }, this);


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
                if (this == null) return;
                Defect.CreateIndicator(transform, "EntityProp", e);
                throw e;
            }
            finally
            {
                onInitiated?.Invoke();
                if (entityInfo)
                {
                    entityInfo.UpdateGatheredDependencies();
                }
            }

        }
        public virtual async Task GetDependenciesTask() => await Task.Yield();

        public virtual void GetDependencies() { }
        public async Task<bool> UntilInitiationComplete()
        {
            bool success = false;
            await ArchAction.WaitUntil(
                () => {

                    if (this == null) return true;

                    if (initiated)
                    {
                        success = true;
                        return true;
                    }

                    return false;
                } , true);

            return success;
        }

        #endregion

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
