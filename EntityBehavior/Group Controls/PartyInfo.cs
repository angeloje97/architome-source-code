using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System.Linq;
using System;

namespace Architome
{
    public class PartyInfo : MonoBehaviour
    {
        // Start is called before the first frame update
        public List<GameObject> members;
        public List<GameObject> liveMembers;
        public GameObject raidObject;
        public GameObject center;

        public KeyBindings keyBindings;
        public EntityControlType partyControl;
        public PartyFormation partyFormation;
        public ContainerTargetables targetManager;

        public List<int> partyAbilityIndexes;
        public LayerMask walkableLayer;

        public Vector3 midPoint;
        public bool partyIsInCombat;

        //Party Events
        public struct PartyEvents
        {
            public Action<bool> OnCombatChange;
        }

        public struct EventHandlers
        {
            public bool previousCombat;
        }

        public PartyEvents events;
        public EventHandlers eventHandlers;

        public void GetDependencies()
        {
            partyControl = EntityControlType.PartyControl;

            foreach (Transform child in transform)
            {
                if (child.tag == "Entity")
                {
                    members.Add(child.gameObject);
                }

                if (child.GetComponent<PartyFormation>())
                {
                    partyFormation = child.GetComponent<PartyFormation>();
                }

                if (child.name.Equals("Center"))
                {
                    center = child.gameObject;
                }
            }

            if (transform.parent)
            {
                if (transform.parent.tag == "Raid")
                {
                    raidObject = transform.parent.gameObject;
                    partyControl = raidObject.GetComponent<RaidInfo>().raidControl;

                    foreach (GameObject member in members)
                    {
                        member.GetComponent<EntityInfo>().entityControlType = EntityControlType.RaidControl;
                    }
                }
            }

            if (keyBindings == null && GMHelper.KeyBindings())
            {
                keyBindings = GMHelper.KeyBindings();
            }

            if (targetManager == null && GMHelper.TargetManager())
            {
                targetManager = GMHelper.TargetManager();
            }

            //Input Manager
            ArchInput.active.OnAlternateAction += OnAlternateAction;
            ArchInput.active.OnActionMultiple += OnActionMultiple;
        }
        public void AddMembersToGameManager()
        {
            if (GMHelper.GameManager())
            {
                foreach (GameObject member in members)
                {
                    GMHelper.GameManager().AddPlayableCharacter(member.GetComponent<EntityInfo>());
                }
                GMHelper.GameManager().AddPlayableParty(this);
            }
        }
        public void ProcessMembers()
        {
            foreach (GameObject member in members)
            {
                var info = member.GetComponent<EntityInfo>();
                if (info == null) { return; }

                info.OnDeath += OnEntityDeath;
                info.OnReviveThis += OnEntityRevive;
                info.OnLifeChange += OnEntityLifeChange;
                info.OnCombatChange += OnEntityCombatChange;
            }

            liveMembers = Entity.LiveEntityObjects(members);
        }
        void Start()
        {
            GetDependencies();
            ProcessMembers();
            ArchAction.Delay(() => { AddMembersToGameManager(); }, .50f);
        }

        void Update()
        {
            UpdateMidPoint();
            HandleEvents();
        }

        //Player Input
        public void OnAlternateAction(int index)
        {
            if (index >= members.Count) return;
            if (partyControl != EntityControlType.PartyControl) return;

            members[index].GetComponent<EntityInfo>().PlayerController().HandleActionButton(true);
        }
        public void OnActionMultiple()
        {
            if (partyControl != EntityControlType.PartyControl) return;

            if (Mouse.CurrentHoverObject())
            {
                var currentObject = Mouse.CurrentHoverObject();

                if (HandleClickable(currentObject)) { return; }
                else if (partyFormation && targetManager.currentHover == null)
                {
                    var location = Mouse.CurrentPositionLayer(walkableLayer);
                    if (location == new Vector3(0, 0, 0)) { return; }
                    partyFormation.MoveFormation(location);
                    MoveParty();
                }
                else if (targetManager.currentHover != null)
                {
                    if (members[0].GetComponent<EntityInfo>().CanAttack(targetManager.currentHover))
                    {
                        Attack(targetManager.currentHover);

                    }
                }
            }
        }
        void HandleEvents()
        {
            if (eventHandlers.previousCombat != partyIsInCombat)
            {
                eventHandlers.previousCombat = partyIsInCombat;
                events.OnCombatChange?.Invoke(partyIsInCombat);
            }
        }
        public bool IsPartyMember(GameObject checkEntity) { return members.Contains(checkEntity); }
        public void MoveParty()
        {
            for (int i = 0; i < members.Count; i++)
            {
                EntityInfo memberInfo = members[i].GetComponent<EntityInfo>();

                if (memberInfo.Movement())
                {
                    memberInfo.Movement().OnTryMove?.Invoke(memberInfo.Movement());
                    memberInfo.Movement().MoveTo(partyFormation.spots[i].transform);
                }
            }
        }
        public bool HandleClickable(GameObject clickable)
        {
            if (!clickable.GetComponent<Clickable>()) { return false; }
            return true;
        }
        public void MovePartyTo(Transform position)
        {
            foreach (GameObject member in members)
            {
                if (member.GetComponent<EntityInfo>())
                {
                    var memberInfo = member.GetComponent<EntityInfo>();
                    var memberMovement = memberInfo.Movement();
                    
                    memberMovement.MoveTo(position.position);
                }
            }
        }
        public void Attack(GameObject target)
        {
            for (int i = 0; i < members.Count; i++)
            {
                EntityInfo memberInfo = members[i].GetComponent<EntityInfo>();

                if (memberInfo.PlayerController() && memberInfo.role != Role.Healer)
                {
                    if (memberInfo.AbilityManager())
                    {
                        memberInfo.AbilityManager().target = target;
                        memberInfo.AbilityManager().Attack();
                        memberInfo.AbilityManager().target = null;

                    }

                    if (memberInfo.CombatBehavior())
                    {
                        memberInfo.CombatBehavior().SetFocus(target);
                    }
                }

            }
        }

        public void UpdateMidPoint()
        {
            if (liveMembers.Count <= 0) { return; }
            midPoint = V3Helper.Sum(liveMembers) / liveMembers.Count;
            center.transform.position = midPoint;
        }

        public void OnEntityDeath(CombatEventData eventData)
        {
        }

        public void OnEntityRevive(CombatEventData eventData)
        {
        }

        public void OnEntityLifeChange(bool isAlive)
        {
            liveMembers = Entity.LiveEntityObjects(members);
        }

        public void OnEntityCombatChange(bool isInCombat)
        {
            if (isInCombat)
            {
                partyIsInCombat = true;
                return;
            }



            foreach (var member in members)
            {
                if (member.GetComponent<EntityInfo>().isInCombat)
                {
                    return;
                }
            }

            partyIsInCombat = false;
        }

    }

}
