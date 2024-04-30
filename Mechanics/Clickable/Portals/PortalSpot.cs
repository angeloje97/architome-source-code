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

            var eventData = new PortalEventData(portalInfo, ePortalEvent.OnEnter, info);

            portalInfo.entitiesInPortal.Add(info);

            portalInfo.InvokePortal(ePortalEvent.OnEnter, eventData);
            info.infoEvents.InvokePortal(ePortalEvent.OnEnter, eventData);

            


            HandlePlayer();

            void HandlePlayer()
            {
                if (!Entity.IsPlayer(info.gameObject)) return;

                portalInfo.InvokePortal(ePortalEvent.OnPlayerEnter, eventData);
                info.infoEvents.InvokePortal(ePortalEvent.OnPlayerEnter, eventData);

                var playableEntities = new List<EntityInfo>();
                foreach (var entity in portalInfo.entitiesInPortal)
                {
                    if (!entity.IsPlayer()) continue;
                    playableEntities.Add(entity);
                }

                if (playableEntities.Count == Entity.PlayableEntities().Count)
                {
                    portalInfo.InvokePortal(ePortalEvent.OnAllPartyMembersInside, eventData);
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

            var eventData = new PortalEventData(portalInfo, ePortalEvent.OnExit, info);


            portalInfo.InvokePortal(ePortalEvent.OnExit, eventData);
            info.infoEvents.InvokePortal(ePortalEvent.OnExit, eventData);

            HandlePlayer();

            void HandlePlayer()
            {
                if(info.rarity != EntityRarity.Player)return;

                info.infoEvents.InvokePortal(ePortalEvent.OnExit, eventData);
                portalInfo.InvokePortal(ePortalEvent.OnPlayerExit, eventData);
            }
        }
    }
}