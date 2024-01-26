using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Architome
{
    public class EntityProp : MonoBehaviour
    {
        // Start is called before the first frame update
        public EntityInfo entityInfo;

        public bool initiated { get; private set; }

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


        public async Task UntilInitiationComplete()
        {
            await ArchAction.WaitUntil(() => initiated, true);
        }

        protected virtual async void Start()
        {
            await Initiate();
        }

        protected virtual void Update()
        {
            if (!initiated) return;
            EUpdate();
        }

        public virtual void EUpdate()
        {

        }
    }

}
