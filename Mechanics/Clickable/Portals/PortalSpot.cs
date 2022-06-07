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

        async void CheckEntities()
        {
            if (!portalInfo.entryPortal) return;
            var position = transform.position;

            while (transform.position != position + new Vector3(0, 10, 0))
            {
                transform.position = Vector3.Lerp(transform.position, position + new Vector3(0, 10, 0), .25f);
                await Task.Yield();
            }

            while (transform.position != position )
            {
                transform.position = Vector3.Lerp(transform.position, position, .25f);
                await Task.Yield();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null) return;
            if (portalInfo == null) return;
            if (portalInfo.entitiesInPortal == null) return;

            if (!Entity.IsPlayer(other.gameObject)) return;
            if (portalInfo.entitiesInPortal.Contains(other.gameObject)) return;
            var info = other.GetComponent<EntityInfo>();

            portalInfo.entitiesInPortal.Add(other.gameObject);
            portalInfo.events.OnPortalEnter?.Invoke(portalInfo, other.gameObject);

            info.portalEvents.OnPortalEnter?.Invoke(portalInfo, info.gameObject);

            var playableEntitiesInPortal = new List<GameObject>();

            foreach (var entity in portalInfo.entitiesInPortal)
            {
                if (!Entity.IsPlayer(entity)) continue;

                playableEntitiesInPortal.Add(entity);
            }

            Debugger.InConsole(91065, $"{playableEntitiesInPortal.Count == Entity.PlayableEntities().Count}");

            if (playableEntitiesInPortal.Count == Entity.PlayableEntities().Count)
            {
                portalInfo.events.OnAllPartyMembersInPortal?.Invoke(portalInfo, playableEntitiesInPortal);
                portalInfo.events.OnAllMembersInPortal?.Invoke();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!Entity.IsPlayer(other.gameObject)) return;
            if (!portalInfo.entitiesInPortal.Contains(other.gameObject)) return;
            var info = other.GetComponent<EntityInfo>();
            portalInfo.entitiesInPortal.Remove(other.gameObject);

            portalInfo.events.OnPortalExit?.Invoke(portalInfo, other.gameObject);


            info.portalEvents.OnPortalExit?.Invoke(portalInfo, info.gameObject);
        }
    }
}