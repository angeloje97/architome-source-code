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
        public Stats stats;

        public Action<BuffData> OnBuffStack;
        public Action<BuffData> OnBuffTimerReset;
        public Action<BuffData> OnResetBuff;
        public void GetDependencies()
        {
            if (GetComponentInParent<EntityInfo>())
            {
                entityInfo = GetComponentInParent<EntityInfo>();
                entityObject = entityInfo.gameObject;

                entityInfo.OnLifeChange += OnLifeChange;
                entityInfo.OnChangeNPCType += OnChangeNPCType;
                entityInfo.OnCombatChange += OnCombatChange;
                entityInfo.OnDamageTaken += OnDamageTaken;
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
        public void ApplyBuff(BuffData data)
        {
            var buffObject = data.buffObject;
            if (!buffObject.GetComponent<BuffInfo>()) return;

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
            HandleNewBuff();

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

            void HandleNewBuff()
            {
                if (hasBuff) { return; }
                ApplyBuffProperties();

            }

            void ApplyBuffProperties()
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

                //newBuff.properties = buffProperties;

                //newBuff.buffRadius = sourceAbility.buffRadius;
                //newBuff.buffRadius = sourceAbility.buffRadius;
                //newBuff.value = sourceAbility.buffValue;
                //newBuff.aoeValue = sourceAbility.buffAOEValue;
                //newBuff.intervals = sourceAbility.buffIntervals;
                //newBuff.buffTime = sourceAbility.buffTime;
                //newBuff.buffTimer = sourceAbility.buffTime;
                //newBuff.stacksPerApplication = sourceAbility.stacksPerApplication;
                //newBuff.canStack = sourceAbility.canStack;
                //newBuff.reapplyResetsTimer = sourceAbility.reapplyResetsTimer;

                var buffInfo = Instantiate(buffObject, transform).GetComponent<BuffInfo>();




                buffObjects.Add(buffInfo.gameObject);
                buffInfo.OnBuffEnd += OnBuffEnd;
                entityInfo.OnBuffApply?.Invoke(buffInfo, sourceInfo);
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
            
        }
    }

    
}
