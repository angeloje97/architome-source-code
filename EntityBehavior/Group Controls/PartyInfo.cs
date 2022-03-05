using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
namespace Architome
{
    public class PartyInfo : MonoBehaviour
    {
        // Start is called before the first frame update
        public List<GameObject> members;
        public List<GameObject> enemiesInLineOfSight;
        public List<GameObject> liveMembers;
        public GameObject raidObject;
        public GameObject center;

        public ActionBarsInfo actionBarsInfo;

        public KeyBindings keyBindings;
        public EntityControlType partyControl;
        public PartyFormation partyFormation;
        public ContainerTargetables targetManager;

        public List<int> partyAbilityIndexes;
        public List<Color> membersColor;
        public LayerMask walkableLayer;

        [Header("Party Controls")]
        public string actionButton;
        public string selectMultipleButton;

        [Header("Individual Controls Action")]
        public string alt1;
        public string alt2;
        public string alt3;
        public string alt4;
        public string alt5;

        [Header("Individual Controls Abiilties")]
        public string ability1;
        public string ability2;
        public string ability3;
        public string ability4;
        public string ability5;


        public Vector3 midPoint;
        public bool partyIsInCombat;

        //Party Events


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
            AddMembersToGameManager();
            ProcessMembers();
            //Invoke("SetActionBars", .125f);
        }

        // Update is called once per frame
        void Update()
        {
            HandlePartyInputs();
            UpdateMidPoint();
        }
        public bool IsPartyMember(GameObject checkEntity) { return members.Contains(checkEntity); }
        public void HandlePartyInputs()
        {
            if (partyControl != EntityControlType.PartyControl)
            {
                return;
            }

            HandlePartyActions();
            HandlePartyLocation();

            void HandlePartyLocation()
            {
                if (Input.GetKey(keyBindings.keyBinds[selectMultipleButton]))
                {
                    if (targetManager)
                    {
                        if (Input.GetKeyDown(keyBindings.keyBinds[actionButton]))
                        {
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
                                    Attack(targetManager.currentHover);
                                }
                            }

                        }
                    }

                }

            }
        }
        public void MoveParty()
        {
            for (int i = 0; i < members.Count; i++)
            {
                EntityInfo memberInfo = members[i].GetComponent<EntityInfo>();

                if (memberInfo.Movement())
                {
                    memberInfo.Movement().MoveTo(partyFormation.spots[i].transform.position);
                    memberInfo.Movement().MoveTo(partyFormation.spots[i].transform);
                }
            }
        }
        public bool HandleClickable(GameObject clickable)
        {
            if (!clickable.GetComponent<Clickable>()) { return false; }
            if (!clickable.GetComponent<Clickable>().partyCanClick) { return false; }
            if (Mouse.IsMouseOverUI()) { return false; }

            foreach (GameObject member in members)
            {
                if (member.GetComponent<EntityInfo>())
                {
                    clickable.GetComponent<Clickable>().Click(member.GetComponent<EntityInfo>());
                }
            }
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
                    if (memberInfo.PlayerController().abilityManager)
                    {
                        memberInfo.PlayerController().HandlePlayerTargetting();

                    }

                    if (memberInfo.CombatBehavior())
                    {
                        memberInfo.CombatBehavior().SetFocus(target);
                    }
                }

            }
        }
        public void HandlePartyActions()
        {
            if (Input.GetKeyDown(keyBindings.keyBinds[alt1]))
            {
                if (members.Count > 0)
                {
                    members[0].GetComponent<EntityInfo>().PlayerController().HandleActionButton();
                }
            }
            if (Input.GetKeyDown(keyBindings.keyBinds[alt2]))
            {
                if (members.Count > 1)
                {
                    members[1].GetComponent<EntityInfo>().PlayerController().HandleActionButton();
                }
            }
            if (Input.GetKeyDown(keyBindings.keyBinds[alt3]))
            {
                if (members.Count > 2)
                {
                    members[2].GetComponent<EntityInfo>().PlayerController().HandleActionButton();
                }
            }
            if (Input.GetKeyDown(keyBindings.keyBinds[alt4]))
            {
                if (members.Count > 3)
                {
                    members[3].GetComponent<EntityInfo>().PlayerController().HandleActionButton();
                }
            }
            if (Input.GetKeyDown(keyBindings.keyBinds[alt5]))
            {
                if (members.Count > 4)
                {
                    members[4].GetComponent<EntityInfo>().PlayerController().HandleActionButton();
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
