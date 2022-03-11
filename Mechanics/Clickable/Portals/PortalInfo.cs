using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class PortalInfo : MonoBehaviour
    {
        // Start is called before the first frame update
        public static List<PortalInfo> portals;

        public List<PortalInfo> portalList;
        public Transform portalSpot;
        public Clickable clickable;
        public int portalNum;

        

        void GetDependencies()
        {
            if (GetComponent<Clickable>())
            {
                clickable = GetComponent<Clickable>();
                clickable.OnSelectOption += OnSelectOption;
            }
        }

        public void SetOptions()
        {
            clickable.options = new List<string>();
            clickable.options.Add("Enter Portal");
        }
        void Start()
        {
            GetDependencies();
            SetOptions();
            if (portals == null) { portals = new List<PortalInfo>(); }
            portals.Add(this);
            portalList = portals;
            portalNum = portals.IndexOf(this);
        }

        private void Update()
        {
        }


        // Update is called once per frame

        public void OnSelectOption(Clickable clickable)
        {
            HandleEnterPortal(clickable.selectedString);
            
        }

        void HandleEnterPortal(string enterPortal)
        {
            if (!enterPortal.Equals("Enter Portal")) return;


            if (clickable.clickedEntities.Count > 0)
            {
                HandleMoveTargets();
                return;
            }
        }

        void HandleMoveTargets()
        {
            if (clickable == null) { return; }
            if (clickable.clickedEntities.Count > 0)
            {
                var entities = clickable.clickedEntities;

                foreach(var entity in entities)
                {
                    entity.Movement().MoveTo(portalSpot);
                }
            }

        }
    }

}
