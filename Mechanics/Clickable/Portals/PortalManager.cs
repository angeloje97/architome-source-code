using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class PortalManager : MonoBehaviour
    {
        // Start is called before the first frame update
        public static PortalManager active;
        public List<PortalInfo> portalsOnMap;
        [SerializeField] PortalInfo startingPortal;

        public PortalInfo StartPortal { get { return startingPortal; } private set { startingPortal = value; } }

        void Start()
        {

        }

        private void Awake()
        {
            active = this;
            startingPortal = null;
            portalsOnMap = new();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void HandleNewPortal(PortalInfo portal)
        {
            portalsOnMap.Add(portal);

            if (StartPortal == null)
            {
                StartPortal = portal;
            }
        }
    }

}