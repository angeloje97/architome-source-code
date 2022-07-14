using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System;

namespace Architome
{
    [Serializable]
    public class Augment 
    {
        public int augmentId;

        [Serializable]
        public struct DestroyConditions
        {
            public bool destroyOnCollisions;
            public bool destroyOnStructure;
            public bool destroyOnNoTickDamage;
            public bool destroyOnReturn;
            public bool destroyOnOutOfRange;
            public bool destroyOnLiveTime;
            public bool destroyOnDeadTarget;
            public bool destroyOnCantFindTarget;

            public static DestroyConditions operator +(DestroyConditions condition1, DestroyConditions condition2)
            {
                DestroyConditions newCondition = new();

                foreach (var field in newCondition.GetType().GetFields())
                {
                    var value = field.GetValue(newCondition);

                    if (value.GetType() != typeof(bool)) continue;

                    if ((bool)field.GetValue(condition1) || (bool)field.GetValue(condition2))
                    {
                        field.SetValue(newCondition, true);
                    }
                    else
                    {
                        field.SetValue(newCondition, false);
                    }


                }

                return newCondition;
            }
        }

        [Serializable]
        public struct Restrictions
        {
            public bool activated;
            public bool playerAiming;
            public bool canCastSelf;
            public bool onlyCastSelf;
            public bool onlyCastOutOfCombat;
            public bool isHealing;
            public bool isAssisting;
            public bool isHarming;
            public bool destroysSummons;
            public bool targetsDead;
            public bool requiresLockOnTarget;
            public bool requiresLineOfSight;
            public bool canHitSameTarget;
            public bool canAssistSameTarget;
            public bool explosive;
            public bool canBeIntercepted;
            public bool nullifyDamage;
            public bool interruptable;
            public bool isAttack;
            public bool usesWeaponCatalyst;
            public bool usesWeaponAttackDamage;
            public bool active;

            public static Restrictions operator +(Restrictions restriction1, Restrictions restrictions2)
            {
                Restrictions newRestriction = new();

                foreach (var field in newRestriction.GetType().GetFields())
                {
                    var value = field.GetValue(newRestriction);

                    if (value.GetType() != typeof(bool)) continue;

                    if ((bool)field.GetValue(restriction1) || (bool)field.GetValue(restrictions2))
                    {
                        field.SetValue(newRestriction, true);
                    }
                    else
                    {
                        field.SetValue(newRestriction, false);
                    }


                }

                return newRestriction;
            }
        }
        [Serializable]
        public struct Cataling
        {
            public bool enable;
            public GameObject catalyst;
            public AbilityType catalingType;
            public CatalystEvent releaseCondition;
            public int releasePerInterval;
            public float interval, targetFinderRadius, valueContribution, rotationPerInterval, startDelay;
        }
        [Serializable]
        public struct Bounce
        {
            public bool enable, requireLOS;
            public float radius;
        }

        [Serializable]
        public struct RecastProperties
        {
            public bool enabled;
            public bool isActive;
            public int maxRecast;
            public int currentRecast;
            public float recastTimeFrame;
            public Action<AbilityInfo> OnRecast;

            public bool CanRecast()
            {
                if (currentRecast > 0 && isActive)
                {
                    return true;
                }

                return false;
            }
        }


        [Serializable]
        public struct Tracking
        {
            public bool tracksTarget;
            public bool predictsTarget;
            public bool predicting;

            [Range(0, 1)]
            public float trackingInterpolation;
        }

        [Serializable]
        public struct ChannelProperties
        {
            public bool enabled;
            public bool active;
            public float time;
            public int invokeAmount;
            public bool cancel;

            [Header("Restrictions")]
            public bool canMove;
            public bool cancelChannelOnMove;
            public float deltaMovementSpeed;
        }

        [Serializable]
        public struct SplashProperties
        {
            public bool enable;
            public CatalystEvent trigger;
            public bool requiresLOS;
            public bool appliesBuffs;
            public int maxSplashTargets;
            public float valueContribution;
            public float radius;
            public float delay;

        }

        [Serializable]
        public struct Threat
        {
            public bool enabled;
            public float additiveThreatMultiplier;

            public bool setsThreat;
            public float threatSet;

            public bool clearThreat;
        }

        [Serializable]
        public struct SummoningProperty
        {
            public bool enabled;
            public List<GameObject> summonableEntities;

            [Header("Summoning Settings")]
            public float radius;
            public float liveTime;
            public float valueContributionToStats;
            public Stats additiveStats;

            [Header("Death Settings")]
            public bool masterDeath;
            public bool masterCombatFalse;
        }


        public DestroyConditions additiveConditions;
        public Restrictions additiveRestrictions;

        public Bounce bounce;
        public Cataling cataling;
        public Tracking tracking;
        public RecastProperties recast;
        public ChannelProperties channel;
        public SplashProperties splash;
        public Threat threat;
        public SummoningProperty summoning;


        public void ApplyAugment(AbilityInfo ability)
        {
            ability.destroyConditions += additiveConditions;
            ability.restrictions += additiveRestrictions;

            if (cataling.enable)
            {
                ability.cataling = cataling;
            }

            if (bounce.enable)
            {
                ability.bounce = bounce;
            }

            if (tracking.tracksTarget)
            {
                ability.tracking = tracking;
            }
        }



    }
}
