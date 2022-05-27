using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class BuffRadiateThreat : BuffType
    {
        [SerializeField] bool requiresLOS = true;
        LayerMask entityLayerMask;
        LayerMask structureLayerMask;

        new void GetDependencies()
        {
            base.GetDependencies();

            buffInfo.OnBuffInterval += OnBuffInterval;

            entityLayerMask = GMHelper.LayerMasks().entityLayerMask;
            structureLayerMask = GMHelper.LayerMasks().structureLayerMask;
        }
        private void Start()
        {
            GetDependencies();
        }

        public void OnBuffInterval(BuffInfo buff)
        {
            var position = buff.hostInfo.transform.position;
            var range = buff.properties.radius;

            var entities = Physics.OverlapSphere(position, range, entityLayerMask);


            foreach (var entity in entities)
            {
                var info = entity.GetComponent<EntityInfo>();

                if (!info) continue;

                if (requiresLOS)
                {
                    var distance = Vector3.Distance(info.transform.position, position);
                    var direction = V3Helper.Direction(info.transform.position, position);

                    if (Physics.Raycast(position, direction, distance, structureLayerMask))
                    {
                        continue;
                    }
                }

                info.combatEvents.OnPingThreat?.Invoke(buff.hostInfo, this.value);
            }

        }
    }
}
