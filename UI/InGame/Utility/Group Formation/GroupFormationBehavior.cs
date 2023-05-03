
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Architome
{
    [RequireComponent(typeof(GroupFormationSaveHandler))]
    public class GroupFormationBehavior : MonoBehaviour
    {
        [Header("Party Info")]
        public PartyInfo partyInfo;
        public PartyFormation partyFormation;
        public ModuleInfo moduleInfo;

        [Header("Group Formation Behavior Properties")]
        public GameObject radiusCircle;
        public List<GameObject> memberSpots;
        public List<Image> positionImages;
        public List<Vector2> memberCoordinates;

        public List<EntityInfo> members;
        public float radius;

        public bool update;
        public bool isInteracting;

        bool interactingCheck;


        public PartyInfo currentParty;

        public Action<GroupFormationBehavior, List<EntityInfo>> OnSetGroup;
        public Action<GroupFormationBehavior, bool> OnInteractingChange;
        void GetDependencies()
        {
            radius = radiusCircle.GetComponent<RectTransform>().sizeDelta.x / 2;

            if (GetComponent<ModuleInfo>())
            {
                moduleInfo = GetComponent<ModuleInfo>();
            }

            GameManager.active.OnNewPlayableParty += OnNewPlayableParty;

            for(int i = 0; i < memberSpots.Count; i++)
            {
                var spot = memberSpots[i];
                var toolTipElement = spot.GetComponent<ToolTipElement>();
                if (!toolTipElement) continue;
                toolTipElement.OnCanShowCheck += (ToolTipElement element) => { HandleToolTipElement(element, memberSpots.IndexOf(spot)); };
            }
        }

        void Start()
        {
            GetDependencies();
            HandleMemberSpotRestrictions();

            OnInteractingChange += HandleInteractingChange;
        }

        void HandleInteractingChange(GroupFormationBehavior behavior, bool isInteracting)
        {
            partyFormation.OnHoldingChange?.Invoke(isInteracting);

            partyFormation.effects.SetParticles(isInteracting);
        }

        public void OnNewPlayableParty(PartyInfo party, int index)
        {
            UpdateCurrent();
            SetMemberCoordinates();
            UpdateFormation();

            void UpdateCurrent()
            {
                partyInfo = party;

                if (party)
                {
                    partyFormation = party.partyFormation;

                    currentParty = partyInfo;
                }

                UpdateMembers(party.members);

                party.events.OnAddMember += (EntityInfo newMember) => {
                    UpdateMembers(party.members);
                };
            }


        }

        void UpdateMembers(List<EntityInfo> members)
        {
            this.members = members.ToList();

            if (positionImages == null) return;
            
            
            for(int i = 0; i < members.Count; i++)
            {
                if (i >= positionImages.Count) return;
                var member = members[i];
                var archClass = member.archClass;
                if (archClass == null) continue;
                var classSprite = archClass.classIcon;

                positionImages[i].sprite = classSprite;
            }
            OnSetGroup?.Invoke(this, this.members);
        }


        public void OnValidate()
        {
            if (radiusCircle == null) { return; }
            radius = radiusCircle.GetComponent<RectTransform>().sizeDelta.x / 2;

            HandleMemberSpotRestrictions();

            update = false;
        }

        void HandleToolTipElement(ToolTipElement element, int index)
        {
            Debugger.UI(6759, $"Trying to show tool tip for {this}");
            if (index >= members.Count)
            {
                element.checks.Add(false);
                Debugger.UI(6760, $"Won't show tooltip because index out of range {index} >= {members.Count}");
                return;
            }

            Debugger.UI(6761, $"Successfuly showing tooltip");

            element.data = new() {
                name = $"{members[index]}",
            };
        }

        void SetMemberCoordinates()
        {
            if (partyFormation == null) return;
            if (partyFormation.spotPositions == null || partyFormation.spotPositions.Count != 5)
            {
                partyFormation.spotPositions = memberCoordinates;
                return;
            }

            memberCoordinates = partyFormation.spotPositions;

            for (int i = 0; i < memberCoordinates.Count; i++)
            {
                if (i < 0 || i >= memberSpots.Count) continue;
                
                memberSpots[i].GetComponent<RectTransform>().localPosition = memberCoordinates[i];
            }
        }

        // Update is called once per frame
        void LateUpdate()
        {
            HandleEvents();
            if (!moduleInfo.isActive) { return; }


            UpdateFormation();
            
        }

        void HandleEvents()
        {
            if (!moduleInfo.isActive)
            {
                if (isInteracting)
                {
                    isInteracting = false;
                }
                else
                {
                    return;
                }
            }

            if(interactingCheck != isInteracting)
            {
                interactingCheck = isInteracting;
                OnInteractingChange?.Invoke(this, isInteracting);
            }
        }

        public void UpdateFormation(bool forceInteract = false)
        {
            if (!forceInteract)
            {
                if (!IsInteracting()) { return; }

            }
            HandleUserInput();
            HandleMemberSpotRestrictions();
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
            if (!moduleInfo.isActive) return false;
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
