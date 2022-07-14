using Architome.Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Architome
{
    public class PortalSpot : MonoBehaviour
    {
        // Start is called before the first frame update
        public PortalInfo portalInfo;

        public Vector3 localPosition;
        void GetDependencies()
        {
            portalInfo = GetComponentInParent<PortalInfo>();
        }

        private void OnValidate()
        {
            GetDependencies();

            localPosition = transform.localPosition;
        }
        void Start()
        {
            GetDependencies();
            CheckEntities();

            transform.localPosition = localPosition;
        }

        // Update is called once per frame
        void Update()
        {

        }

        void CheckEntities()
        {
            if (!portalInfo.entryPortal) return;
            var entityLayer = LayerMasksData.active.entityLayerMask;
            var entities = Physics.OverlapSphere(transform.position, 3f, entityLayer);

            portalInfo.entitiesInPortal = new();

            foreach (var entity in entities)
            {
                var info = entity.GetComponent<EntityInfo>();
                if (info == null) continue;
                portalInfo.entitiesInPortal.Add(info);
            }

        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null) return;
            if (portalInfo == null) return;
            if (portalInfo.entitiesInPortal == null) portalInfo.entitiesInPortal = new();
            var info = other.GetComponent<EntityInfo>();
            if (info == null) return;
            if (portalInfo.entitiesInPortal.Contains(info)) return;


            portalInfo.entitiesInPortal.Add(info);
            portalInfo.events.OnPortalEnter?.Invoke(portalInfo, other.gameObject);
            info.portalEvents.OnPortalEnter?.Invoke(portalInfo, info.gameObject);


            


            HandlePlayer();

            void HandlePlayer()
            {
                if (!Entity.IsPlayer(info.gameObject)) return;
                portalInfo.events.OnPlayerEnter?.Invoke(portalInfo, info);
                info.portalEvents.OnPlayerEnter?.Invoke(portalInfo, info);

                var playableEntities = new List<EntityInfo>();
                foreach (var entity in portalInfo.entitiesInPortal)
                {
                    if (entity.rarity != Enums.EntityRarity.Player) continue;
                    playableEntities.Add(entity);
                }

                if (playableEntities.Count == Entity.PlayableEntities().Count)
                {

                    portalInfo.events.OnAllPartyMembersInPortal?.Invoke(portalInfo, playableEntities);
                    portalInfo.events.OnAllMembersInPortal?.Invoke();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var info = other.GetComponent<EntityInfo>();
            if (info == null) return;
            if (!portalInfo.entitiesInPortal.Contains(info)) return;
            
            portalInfo.entitiesInPortal.Remove(info);
            portalInfo.events.OnPortalExit?.Invoke(portalInfo, other.gameObject);


            info.portalEvents.OnPortalExit?.Invoke(portalInfo, info.gameObject);

            HandlePlayer();

            void HandlePlayer()
            {
                if(info.rarity != EntityRarity.Player)return;

                info.portalEvents.OnPlayerExit?.Invoke(portalInfo, info);
                portalInfo.events.OnPlayerExit?.Invoke(portalInfo, info);
            }
        }
    }
}