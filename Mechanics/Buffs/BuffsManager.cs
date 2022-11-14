using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System.Linq;
using System;

namespace Architome
{
    public class BuffsManager : MonoBehaviour
    {
        // Start is called before the first frame update
        public List<GameObject> buffObjects;

        public GameObject entityObject;
        public EntityInfo entityInfo;
        public CharacterInfo character;
        public Movement movement;
        public Stats stats;

        public Action<BuffData> OnBuffStack;
        public Action<BuffData> OnBuffTimerReset;
        public Action<BuffData> OnResetBuff;
        public void GetDependencies()
        {
            entityInfo = GetComponentInParent<EntityInfo>();
            if (entityInfo)
            {
                entityObject = entityInfo.gameObject;
                movement = entityInfo.Movement();
                character = entityInfo.CharacterInfo();

                entityInfo.OnLifeChange += OnLifeChange;
                entityInfo.OnChangeNPCType += OnChangeNPCType;
                entityInfo.OnCombatChange += OnCombatChange;
                entityInfo.OnDamageTaken += OnDamageTaken;


            }

            if (movement)
            {
                movement.OnStartMove += (Movement movement) => { OnMoveChange(true); };
                movement.OnEndMove += (Movement movement) => { OnMoveChange(false); };
            }

            if (character)
            {
                character.OnChangeEquipment += OnChangeEquipment;
            }

        }
        void Start()
        {
            GetDependencies();
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void OnCombatChange(bool isInCombat)
        {

            foreach (Transform child in transform)
            {
                var buff = child.GetComponent<BuffInfo>();
                if (buff == null) continue;

                HandleExitCombat(buff);
                HandleEnterCombat(buff);
            }


            void HandleExitCombat(BuffInfo buff)
            {
                if (isInCombat) return;
                if (!buff.cleanseConditions.exitCombat) return;

                buff.Cleanse();

            }

            void HandleEnterCombat(BuffInfo buff)
            {
                if (!isInCombat) return;
                if (!buff.cleanseConditions.enterCombat) return;
                buff.Cleanse();
            }
        }
        public void OnDamageTaken(CombatEventData eventData)
        {
            foreach (Transform child in transform)
            {
                var buff = child.GetComponent<BuffInfo>();
                if (buff == null) continue;

                if (buff.cleanseConditions.damageTaken)
                {
                    buff.Cleanse();
                }
            }
        }
        public void OnChangeEquipment(EquipmentSlot slot, Equipment before, Equipment after)
        {
            if (before)
            {
                foreach (var buff in before.equipmentEffects)
                {
                    CleanseBuff(buff);
                }
            }

            if (after)
            {
                if (after.equipmentEffects == null) return;
                foreach (var buff in after.equipmentEffects)
                {

                    ApplyBuff(new(buff, after, entityInfo));
                }
            }
        }
        public void UpdateStats()
        {
            stats.ZeroOut();

            foreach (var buffStat in GetComponentsInChildren<BuffStatChange>())
            {
                stats += buffStat.stats;
            }

            entityInfo.UpdateCurrentStats();
        }
        public void OnLifeChange(bool isAlive)
        {
            var buffs = GetComponentsInChildren<BuffInfo>();
            if (!isAlive)
            {
                foreach (var buff in buffs)
                {
                    buff.CompleteEarly();
                }
            }
        }
        public void OnMoveChange(bool isMoving)
        {
            if (isMoving)
            {
                foreach (var buff in GetComponentsInChildren<BuffInfo>())
                {
                    if (!buff.cleanseConditions.isMoving) continue;
                    buff.Cleanse();
                }
            }
        }
        public void CleanseBuff(BuffInfo buff)
        {
            foreach (var buffInfo in Buffs())
            {
                if (buffInfo._id != buff._id) continue;
                buffInfo.Cleanse();
            }
        }
        public void CleanseImmunity()
        {
            for (int i = 0; i < buffObjects.Count; i++)
            {
                var buffObject = buffObjects[i];

                if (buffObject == null)
                {
                    buffObjects.RemoveAt(i);
                    i--;
                    continue;
                }
                var buff = buffObject.GetComponent<BuffInfo>();
                var stateChanger = buff.GetComponent<BuffStateChanger>();

                if (stateChanger == null) continue;
                if (!entityInfo.stateImmunities.Contains(stateChanger.stateToChange)) continue;

                buff.Cleanse();
            }
        }

        public void CleanseBuffs(BuffCategory category, string reason = "")
        {

            foreach (var buff in Buffs())
            {
                if (IsCategory(buff.buffTargetType))
                {
                    buff.Cleanse(reason);
                }
            }

            bool IsCategory(BuffTargetType targetType)
            {
                if (category == BuffCategory.All) return true;
                if (targetType.ToString() == category.ToString()) return true;
                return false;
            }
        }

        public void CleanseBuffs(Predicate<BuffInfo> predicate)
        {
            foreach (var buff in Buffs())
            {
                if (predicate(buff))
                {
                    buff.Cleanse();
                }
            }
        }

        public void OnChangeNPCType(NPCType before, NPCType after)
        {
            foreach (var buff in Buffs())
            {
                if (buff.GetComponent<BuffMindControl>()) continue;
                switch (buff.buffTargetType)
                {
                    case BuffTargetType.Harm:
                        if (buff.sourceInfo.CanAttack(after))
                        {
                            break;
                        }

                        buff.Cleanse();
                        break;
                    case BuffTargetType.Assist:
                        if (buff.sourceInfo.CanHelp(after))
                        {
                            break;
                        }

                        buff.Cleanse();
                        break;
                }
            }
        }
        public BuffInfo ApplyBuff(BuffData data)
        {
            var buffObject = data.buffObject;
            if (!buffObject.GetComponent<BuffInfo>()) return null;

            var sourceAbility = data.sourceAbility;
            var sourceInfo = data.sourceInfo;
            var sourceCatalyst = data.sourceCatalyst;
            var sourceItem = data.sourceItem;

            var buffInfo = data.buffInfo;
            var buffProperties = buffInfo.properties;
            bool hasBuff = HasBuff(buffObject);
            bool canStack = buffProperties.canStack;
            bool resetsTimer = buffProperties.reapplyResetsTimer;
            bool resetBuff = buffProperties.reapplyResetsBuff;


            HandleExistingBuff();
            return HandleNewBuff();

            void HandleExistingBuff()
            {
                Debugger.InConsole(9152, $"{hasBuff}");
                if (hasBuff)
                {
                    BuffInfo currentBuff = Buff(buffInfo._id);

                    if (canStack)
                    {
                        //currentBuff.stacks += currentBuff.stacksPerApplication;
                        OnBuffStack?.Invoke(data);
                    }
                    if (resetsTimer)
                    {
                        // currentBuff.buffTimer = currentBuff.buffTime;
                        OnBuffTimerReset?.Invoke(data);
                    }

                    if (resetBuff)
                    {
                        currentBuff.buffTimer = 0;
                        hasBuff = false;
                        OnResetBuff?.Invoke(data);
                        return;
                    }

                    if (canStack || resetsTimer)
                    {
                        entityInfo.OnBuffApply?.Invoke(currentBuff, currentBuff.sourceInfo);
                    }
                }


            }

            BuffInfo HandleNewBuff()
            {
                if (hasBuff) { return null; }
                return ApplyBuffProperties();

            }

            BuffInfo ApplyBuffProperties()
            {
                var newBuff = buffObject.GetComponent<BuffInfo>();

                newBuff.stacks = 1;
                newBuff.sourceInfo = sourceInfo;
                newBuff.sourceObject = sourceInfo.gameObject;
                newBuff.sourceAbility = sourceAbility;
                newBuff.sourceCatalyst = sourceCatalyst;
                newBuff.sourceItem = sourceItem;

                if (sourceCatalyst && sourceCatalyst.target)
                {
                    
                    newBuff.targetObject = sourceCatalyst.target;
                    newBuff.targetInfo = sourceCatalyst.target.GetComponent<EntityInfo>();
                }

                var buffInfo = Instantiate(buffObject, transform).GetComponent<BuffInfo>();


                buffObjects.Add(buffInfo.gameObject);
                buffInfo.OnBuffEnd += OnBuffEnd;
                entityInfo.OnBuffApply?.Invoke(buffInfo, sourceInfo);
                return buffInfo;

            }
        }
                
        public bool HasBuff(GameObject buffObject)
        {
            if (!buffObject.GetComponent<BuffInfo>()) { return false; }
            BuffInfo buffInfo = buffObject.GetComponent<BuffInfo>();
            int buffId = buffInfo._id;

            foreach (Transform child in transform)
            {
                if (child.GetComponent<BuffInfo>()._id == buffId)
                {
                    return true;
                }
            }
            return false;
        }

        public BuffInfo Buff(int id)
        {
            foreach (Transform child in transform)
            {
                if (child.GetComponent<BuffInfo>())
                {
                    if (child.GetComponent<BuffInfo>()._id == id)
                    {
                        return child.GetComponent<BuffInfo>();
                    }
                }
            }
            return null;
        }
        public List<BuffInfo> Buffs()
        {
            return GetComponentsInChildren<BuffInfo>().ToList();
        }

        public void OnBuffEnd(BuffInfo buff)
        {
            if (!buffObjects.Contains(buff.gameObject)) return;
            buffObjects.Remove(buff.gameObject);
            buff.OnBuffEnd -= OnBuffEnd;

        }

        public class BuffData
        {
            public GameObject buffObject { get; private set; }
            public BuffInfo buffInfo { get; private set; }
            public EntityInfo sourceInfo;
            public AbilityInfo sourceAbility;
            public CatalystInfo sourceCatalyst;
            public Item sourceItem;

            public BuffData(GameObject buffData, CatalystInfo sourceCatalyst)
            {
                buffObject = buffData;

                buffInfo = buffData.GetComponent<BuffInfo>();

                this.sourceCatalyst = sourceCatalyst;
                sourceAbility = sourceCatalyst.abilityInfo;
                sourceInfo = sourceCatalyst.entityInfo;
            }

            public BuffData(GameObject buffObject, BuffInfo sourceBuff)
            {
                this.buffObject = buffObject;
                buffInfo = buffObject.GetComponent<BuffInfo>();

                sourceItem = sourceBuff.sourceItem;
                sourceInfo = sourceBuff.sourceInfo;
                sourceCatalyst = sourceBuff.sourceCatalyst;
                sourceAbility = sourceBuff.sourceAbility;
            }

            public BuffData(GameObject buff, AbilityInfo sourceAbility)
            {
                buffObject = buff;
                buffInfo = buffObject.GetComponent<BuffInfo>();

                this.sourceAbility = sourceAbility;
                sourceCatalyst = sourceAbility.catalystInfo;
                sourceInfo = sourceAbility.entityInfo;
            }

            public BuffData(BuffInfo buff, Item sourceItem, EntityInfo sourceInfo)
            {
                buffObject = buff.gameObject;
                buffInfo = buff;
                this.sourceItem = sourceItem;
                this.sourceInfo = sourceInfo;
            }

            public BuffData(BuffInfo buff)
            {
                buffObject = buff.gameObject;
                buffInfo = buff;
            }
            
        }
    }

    
}
