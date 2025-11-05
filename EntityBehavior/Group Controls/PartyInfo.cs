using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace Architome
{
    public class PartyInfo : MonoActor
    {
        // Start is called before the first frame update
        public bool loadEntitiesFromSave;
        public List<EntityInfo> members;
        public List<EntityInfo> liveMembers;
        public GameObject raidObject;
        public GameObject center;
        public KeyBindings keyBindings { get; set; }
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
            public Action<EntityInfo> OnAddMember { get; set; }
            public Action<EntityInfo> OnRemoveMember;
            public Action<PartyFormation> OnMoveFormation { get; set; }
            public Action<string> OnTransferScene;
            public Action<EntityInfo> OnPartyFocus { get; set; }
        }

        public struct EventHandlers
        {
            public bool previousCombat;
        }

        public PartyEvents events;
        public EventHandlers eventHandlers;


        GameManager manager;
        void Start()
        {
            ArchAction.Delay(() =>
            {
                GetDependencies();
                HandleStartingMembers();
                AddSelfToGameManager();
            }, .50f);

        }


        void Update()
        {
            UpdateMidPoint();
            HandleEvents();
        }

        void GetDependencies()
        {
            partyControl = EntityControlType.PartyControl;

            partyFormation = GetComponentInChildren<PartyFormation>();
            foreach (Transform child in transform)
            {

                if (child.name.Equals("Center"))
                {
                    center = child.gameObject;
                }
            }

            var raidInfo = GetComponentInParent<RaidInfo>();

            if (raidInfo)
            {
                raidObject = raidInfo.gameObject;
                partyControl = raidObject.GetComponent<RaidInfo>().raidControl;

                foreach (var member in members)
                {
                    member.entityControlType = EntityControlType.RaidControl;
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
            var archInput = ArchInput.active;
            archInput.OnActionMultiple += OnActionMultiple;
            archInput.OnAction += OnAction;
        }
        void AddSelfToGameManager()
        {
            var raidInfo = GetComponentInParent<RaidInfo>();
            var manager = GameManager.active;


            if (manager == null) return;

            if (manager.playableParties != null && manager.playableParties.Count == 1)
            {
                if (!raidInfo)
                {
                    return;
                }
            }

            manager.AddPlayableParty(this);

        }

        async void HandleStartingMembers()
        {
            manager = GameManager.active;

            var tasks = new List<Task>();
            var entities = GetComponentsInChildren<EntityInfo>();

            foreach (var entity in entities)
            {
                tasks.Add(entity.UntilGatheredDependencies());
            }

            await Task.WhenAll(tasks);
            await Task.Delay(500);

            foreach(var entity in entities)
            {
                AddMember(entity);
            }
        }

        void ProcessMember(EntityInfo info)
        {
            info.entityControlType = partyControl;


            Action unsubscribe = () => { };


            unsubscribe += info.combatEvents.AddListenerHealth(eHealthEvent.OnDeath, OnEntityDeath, this);
            unsubscribe += info.combatEvents.AddListenerHealth(eHealthEvent.OnGetRevive, OnEntityRevive, this);

            info.OnLifeChange += OnEntityLifeChange;
            info.OnCombatChange += OnEntityCombatChange;

            events.OnRemoveMember += (EntityInfo member) => {
                info.OnCombatChange -= OnEntityCombatChange;
                info.OnLifeChange -= OnEntityLifeChange;

                unsubscribe?.Invoke();
            };
        }

        public void AddMember(EntityInfo entity)
        {
            members ??= new();

            if (members.Contains(entity)) return;

            if (entity.transform.parent != transform)
            {
                entity.transform.SetParent(transform);
            }

            members.Add(entity);
            ProcessMember(entity);


            liveMembers = Entity.LiveEntities(members);
            entity.partyEvents.currentParty = this;
            events.OnAddMember?.Invoke(entity);
            entity.partyEvents.OnAddedToParty?.Invoke(this);
            //entity.rarity = EntityRarity.Player;
            entity.SetRarity(EntityRarity.Player);

            if (manager == null) manager = GameManager.active;

            if (manager == null) return;

            Debugger.Environment(1594, $"Adding {entity} to playable character in game manager");
            manager.AddPlayableCharacter(entity);
        }

        public void RemoveMember(EntityInfo entity, Transform newParent = null)
        {
            if (!members.Contains(entity)) return;

            events.OnRemoveMember?.Invoke(entity);
            entity.partyEvents.OnRemovedFromParty?.Invoke(this);
            if (entity.partyEvents.currentParty == this) entity.partyEvents.currentParty = null;
            members.Remove(entity);
            entity.transform.SetParent(newParent);
        }

        //Player Input
        public void OnAlternateAction(int index)
        {
            if (index >= members.Count) return;
            if (partyControl != EntityControlType.PartyControl) return;

            members[index].PlayerController().HandleActionButton(true);
        }

        public void OnActionMultiple()
        {
            if (partyControl != EntityControlType.PartyControl) return;

            var currentObject = Mouse.CurrentHoverObject();
            if (currentObject)
            {

                if (HandleClickable(currentObject)) { return; }
                else if (partyFormation && targetManager.currentHover == null)
                {
                    var location = Mouse.CurrentPositionLayer(walkableLayer);
                    if (location == new Vector3(0, 0, 0)) { return; }

                    MovePartyTo(location);
                }
                else if (targetManager.currentHover != null)
                {
                    if (members[0].CanAttack(targetManager.currentHover))
                    {
                        Focus(targetManager.currentHover);

                    }
                }
            }
        }

        public void OnAction()
        {
            foreach (var entity in members)
            {
                if (!targetManager.selectedTargets.Contains(entity)) continue;
                entity.partyEvents.OnSelectedAction?.Invoke(this);
                var controller = entity.PlayerController();
                if (controller == null) continue;
                controller.HandleActionButton(true);

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
        public bool IsPartyMember(EntityInfo checkEntity) { return members.Contains(checkEntity); }
        public void MoveParty()
        {
            for (int i = 0; i < members.Count; i++)
            {
                var memberInfo = members[i];
                var movement = memberInfo.Movement();



                if (movement)
                {
                    movement.OnTryMove?.Invoke(movement);
                    _ = movement.MoveToAsync(partyFormation.spots[i].transform);
                }
            }
        }
        public bool HandleClickable(GameObject clickable)
        {
            if (!clickable.GetComponent<Clickable>()) { return false; }
            return true;
        }
        public void MovePartyTo(Vector3 position)
        {
            events.OnMoveFormation?.Invoke(partyFormation);
            partyFormation.MoveFormation(position);
            MoveParty();
        }
        void Focus(EntityInfo target)
        {
            events.OnPartyFocus?.Invoke(target);
        }
        void UpdateMidPoint()
        {
            if (liveMembers.Count <= 0) { return; }
            midPoint = V3Helper.Average(liveMembers);

            center.transform.position = midPoint;
        }
        void OnEntityDeath(HealthEvent eventData)
        {
        }
        void OnEntityRevive(HealthEvent eventData)
        {
        }
        void OnEntityLifeChange(bool isAlive)
        {
            liveMembers = Entity.LiveEntities(members);
        }
        void OnEntityCombatChange(bool isInCombat)
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


        #region Properties

        public int AveragePartyLevel
        {
            get
            {
                float total = 0;
                float count = 0;
                foreach(var member in members)
                {
                    count++;
                    total += member.stats.Level;
                }

                return (int) (total / count);
            }
        }

        #endregion
    }

}
