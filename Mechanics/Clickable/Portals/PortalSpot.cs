using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class PortalSpot : MonoBehaviour
    {
        // Start is called before the first frame update
        public PortalInfo portalInfo;
        void GetDependencies()
        {
            portalInfo = GetComponentInParent<PortalInfo>();
        }

        private void OnValidate()
        {
            GetDependencies();
        }
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnTriggerEnter(Collider other)
        {
            if (!Entity.IsPlayer(other.gameObject)) return;
            if (portalInfo.entitiesInPortal.Contains(other.gameObject)) return;

            portalInfo.entitiesInPortal.Add(other.gameObject);
            portalInfo.events.OnPortalEnter?.Invoke(portalInfo, other.gameObject);


        }

        private void OnTriggerExit(Collider other)
        {
            if (!Entity.IsPlayer(other.gameObject)) return;
            if (!portalInfo.entitiesInPortal.Contains(other.gameObject)) return;

            portalInfo.entitiesInPortal.Remove(other.gameObject);
            portalInfo.events.OnPortalExit?.Invoke(portalInfo, other.gameObject);

        }
    }

}