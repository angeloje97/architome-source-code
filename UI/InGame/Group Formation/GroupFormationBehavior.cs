using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Architome
{
    public class GroupFormationBehavior : MonoBehaviour
    {
        // Start is called before the first frame update
        [Header("Party Info")]
        public PartyInfo partyInfo;
        public PartyFormation partyFormation;
        public ModuleInfo moduleInfo;

        [Header("Group Formation Behavior Properties")]
        public GameObject radiusCircle;
        public List<GameObject> memberSpots;
        public List<Vector2> memberCoordinates;

        public float radius;

        public bool update;
        public bool isInteracting;


        public PartyInfo currentParty;
        void GetDependencies()
        {
            radius = radiusCircle.GetComponent<RectTransform>().sizeDelta.x / 2;

            if (GetComponent<ModuleInfo>())
            {
                moduleInfo = GetComponent<ModuleInfo>();
            }
        }

        void Awake()
        {
            GetDependencies();
            HandleMemberSpotRestrictions();
        }

        public void OnNewPlayableParty(PartyInfo party, int index)
        {

        }


        public void OnValidate()
        {
            if (radiusCircle == null) { return; }
            radius = radiusCircle.GetComponent<RectTransform>().sizeDelta.x / 2;

            HandleMemberSpotRestrictions();

            update = false;
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (!moduleInfo.isActive) { return; }

            HandleNewPlayableCharacters();
            HandleUserInput();
            if (!IsInteracting()) { return; }
            HandleMemberSpotRestrictions();
        }

        void HandleNewPlayableCharacters()
        {
            if (GMHelper.GameManager().playableParties.Count == 1
                && partyInfo != GMHelper.GameManager().playableParties[0])
            {
                partyInfo = GMHelper.GameManager().playableParties[0];
            }

            if (currentParty != partyInfo)
            {
                partyFormation = partyInfo.partyFormation;

                partyFormation.spotPositions = memberCoordinates;

                currentParty = partyInfo;
            }
        }

        void HandleMemberSpotRestrictions()
        {
            for (int i = 0; i < memberSpots.Count; i++)
            {


                if (V3Helper.Abs(memberSpots[i].transform.localPosition) > radius)
                {
                    memberSpots[i].transform.localPosition *= radius / V3Helper.Abs(memberSpots[i].transform.localPosition);
                }
                var coordX = memberSpots[i].GetComponent<RectTransform>().localPosition.x;
                var coordY = memberSpots[i].GetComponent<RectTransform>().localPosition.y;

                coordX /= radius;
                coordY /= radius;

                memberCoordinates[i] = new Vector2(coordX, coordY);
            }





            if (partyFormation)
            {
                partyFormation.HandleSpotPosition();
            }
        }

        void HandleUserInput()
        {
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                HandleMemberSpotRestrictions();
            }
        }

        private void OnEnable()
        {
            //ShowFormationGraphics(true);
        }

        private void OnDisable()
        {
            //ShowFormationGraphics(false);
        }

        void ShowFormationGraphics(bool val)
        {
            if (partyFormation)
            {
                foreach (Transform spot in partyFormation.spots)
                {
                    if (spot.GetComponent<Renderer>())
                    {
                        spot.GetComponent<Renderer>().enabled = val;
                    }
                }

                if (partyFormation.radiusCircle && partyFormation.radiusCircle.GetComponent<Renderer>())
                {
                    partyFormation.radiusCircle.GetComponent<Renderer>().enabled = val;
                }
            }
        }


        bool IsInteracting()
        {
            foreach (GameObject spot in memberSpots)
            {
                if (spot.GetComponent<DragAndDrop>() && spot.GetComponent<DragAndDrop>().isDragging)
                {
                    isInteracting = true;
                    return true;

                }
            }
            isInteracting = false;
            return false;
        }
    }

}
