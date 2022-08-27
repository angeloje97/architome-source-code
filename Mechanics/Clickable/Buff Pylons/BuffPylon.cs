using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class BuffPylon : MonoBehaviour
    {

        public float baseValue, radius;
        public List<BuffInfo> buffs;
        public List<BuffInfo> aura;

        [SerializeField] bool requiresLos;
        [SerializeField] bool auraActive;
        
        List<EntityInfo> entitiesWithinAura;
        LayerMask entityLayerMask;
        LayerMask structureLayerMask;
        CapsuleCollider capsuleCollider;
        

        private void Start()
        {
            GetDependencies();
            HandleAura();
        }

        void GetDependencies()
        {
            var layerMasksData = LayerMasksData.active;
            if (layerMasksData == null) return;

            entityLayerMask = layerMasksData.entityLayerMask;
            structureLayerMask = layerMasksData.structureLayerMask;

            capsuleCollider = GetComponent<CapsuleCollider>();

            if (capsuleCollider)
            {
                capsuleCollider.radius = radius;
            }
        }

        private void OnValidate()
        {
            capsuleCollider = GetComponent<CapsuleCollider>();

            if (capsuleCollider)
            {
                capsuleCollider.radius = radius;
            }
        }

        public void ApplyBuffs(WorkInfo workStation)
        {
            if (workStation == null) return;
            if (buffs == null || buffs.Count == 0) return;
            foreach (var worker in workStation.Workers())
            {
                var buffManager = worker.Buffs();


                foreach (var buff in buffs)
                {
                    var newBuff = buffManager.ApplyBuff(new(buff) { sourceInfo = worker });
                    ArchAction.Yield(() => { 
                        if (newBuff)
                        {
                            newBuff.ApplyBaseValue(baseValue);
                        }
                    });
                }
            }
        }

        public void ToggleAura()
        {
            auraActive = !auraActive;
            HandleAura();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!auraActive) return;
            AddEntity(other.GetComponent<EntityInfo>());

        }

        private void OnTriggerExit(Collider other)
        {
            if (!auraActive) return;
            RemoveEntity(other.GetComponent<EntityInfo>());
        }

        public void AddEntity(EntityInfo entity)
        {
            if (entity == null) return;
            if (entitiesWithinAura == null) entitiesWithinAura = new();
            if (entitiesWithinAura.Contains(entity)) return;
            ApplyBuffs(entity);
            entitiesWithinAura.Add(entity);
        }

        public void RemoveEntity(EntityInfo entity)
        {
            if (entity == null) return;
            if (entitiesWithinAura == null) entitiesWithinAura = new();
            if (!entitiesWithinAura.Contains(entity)) return;
            RemoveBuffs(entity);
            entitiesWithinAura.Remove(entity);
        }

        public void ClearEntities()
        {
            if (entitiesWithinAura == null) return;
            for (int i = 0; i < entitiesWithinAura.Count; i++)
            {
                RemoveBuffs(entitiesWithinAura[i]);
                entitiesWithinAura.RemoveAt(i);
                
                i--;
            }
        }

        public void ApplyBuffs(EntityInfo entity)
        {
            var buffManager = entity.Buffs();
            if (buffManager == null) return;

            foreach (var buff in buffs)
            {
                var newBuff = buffManager.ApplyBuff(new(buff) { sourceInfo = entity });

                ArchAction.Yield(() => {
                    if (newBuff)
                    {
                        newBuff.ApplyBaseValue(baseValue);
                    }
                });
            }
        }

        public void RemoveBuffs(EntityInfo entity)
        {
            var buffManager = entity.Buffs();
            if (buffManager == null) return;

            foreach (var buff in buffs)
            {
                buffManager.CleanseBuff(buff);
            }
        }

        async void HandleAura()
        {
            while (Application.isPlaying)
            {
                await Task.Delay(2000);
                if (entitiesWithinAura == null) entitiesWithinAura = new();

                if (!auraActive) break;

                var entities = Physics.OverlapSphere(transform.position, radius, entityLayerMask);

                var validEntities = new List<EntityInfo>();

                foreach (var entity in entities)
                {
                    var info = entity.GetComponent<EntityInfo>();
                    if (info == null) continue;

                    if (requiresLos)
                    {
                        var distance = V3Helper.Distance(entity.transform.position, transform.position);
                        var direction = V3Helper.Direction(entity.transform.position, transform.position);

                        var ray = new Ray(transform.position, direction);

                        if (Physics.Raycast(ray, out RaycastHit hit, distance, structureLayerMask))
                        {
                            continue;
                        }
                    }

                    if (!entitiesWithinAura.Contains(info))
                    {
                        AddEntity(info);
                    }

                    validEntities.Add(info);
                }

                for (int i = 0; i < entitiesWithinAura.Count; i++)
                {
                    if (!validEntities.Contains(entitiesWithinAura[i]))
                    {
                        RemoveEntity(entitiesWithinAura[i]);
                        i--;
                    }
                }

            }
        }

    }
}
