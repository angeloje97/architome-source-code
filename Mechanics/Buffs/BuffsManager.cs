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

        public Action<BuffInfo> OnBuffStack;
        public Action<BuffInfo> OnBuffTimerReset;
        public Action<BuffInfo> OnResetBuff;
        public void GetDependencies()
        {
            if (GetComponentInParent<EntityInfo>())
            {
                entityInfo = GetComponentInParent<EntityInfo>();
                entityObject = entityInfo.gameObject;

                entityInfo.OnLifeChange += OnLifeChange;
                entityInfo.OnChangeNPCType += OnChangeNPCType;
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

        public void UpdateStats()
        {
            stats.ZeroOut();

            foreach (var buffStat in GetComponentsInChildren<BuffStatChange>())
            {
                stats = stats.Sum(stats, buffStat.stats);
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
        public void ApplyBuff(GameObject buffObject, EntityInfo sourceInfo, AbilityInfo sourceAbility, CatalystInfo sourceCatalyst)
        {
            if (!buffObject.GetComponent<BuffInfo>()) return;
            BuffInfo buffInfo = buffObject.GetComponent<BuffInfo>();
            var buffProperties = sourceAbility.buffProperties;
            bool hasBuff = HasBuff(buffObject);
            bool canStack = buffProperties.canStack;
            bool resetsTimer = buffProperties.reapplyResetsTimer;
            bool resetBuff = buffProperties.reapplyResetsBuff;


            HandleExistingBuff();
            HandleNewBuff();
            UpdateBuffs();

            void HandleExistingBuff()
            {
                Debugger.InConsole(9152, $"{hasBuff}");
                if (hasBuff)
                {
                    BuffInfo currentBuff = Buff(buffInfo.buffId);

                    if (canStack)
                    {
                        //currentBuff.stacks += currentBuff.stacksPerApplication;
                        OnBuffStack?.Invoke(currentBuff);
                    }
                    if (resetsTimer)
                    {
                        // currentBuff.buffTimer = currentBuff.buffTime;
                        OnBuffTimerReset?.Invoke(currentBuff);
                    }

                    if (resetBuff)
                    {
                        currentBuff.buffTimer = 0;
                        hasBuff = false;
                        OnResetBuff?.Invoke(currentBuff);
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
                if (!hasBuff)
                {
                    ApplyBuffProperties();
                }
            }

            void ApplyBuffProperties()
            {
                var newBuff = buffObject.GetComponent<BuffInfo>();

                newBuff.stacks = 1;
                newBuff.sourceInfo = sourceInfo;
                newBuff.sourceObject = sourceInfo.gameObject;
                newBuff.sourceAbility = sourceAbility;
                newBuff.sourceCatalyst = sourceCatalyst;

                if (sourceCatalyst && sourceCatalyst.target)
                {
                    
                    newBuff.targetObject = sourceCatalyst.target;
                    newBuff.targetInfo = sourceCatalyst.target.GetComponent<EntityInfo>();
                }

                newBuff.properties = buffProperties;

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
                entityInfo.OnBuffApply?.Invoke(buffInfo, sourceInfo);
            }
        }
        public bool HasBuff(GameObject buffObject)
        {
            if (!buffObject.GetComponent<BuffInfo>()) { return false; }
            BuffInfo buffInfo = buffObject.GetComponent<BuffInfo>();
            int buffId = buffInfo.buffId;

            foreach (Transform child in transform)
            {
                if (child.GetComponent<BuffInfo>().buffId == buffId)
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
                    if (child.GetComponent<BuffInfo>().buffId == id)
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


        public void UpdateBuffs()
        {
            foreach (Transform child in transform)
            {
                if (!buffObjects.Contains(child.gameObject) && child.GetComponent<BuffInfo>())
                {
                    buffObjects.Add(child.gameObject);
                }
            }
        }


    }

}
