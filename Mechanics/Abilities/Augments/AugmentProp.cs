using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System;
using static Architome.EntityInfo;

namespace Architome
{
    [Serializable]
    public class AugmentProp
    {
        public int augmentId;

        [Serializable]
        public class DestroyConditions
        {
            public bool destroyOnCollisions;
            public bool destroyOnStructure;
            public bool destroyOnNoTickDamage;
            public bool destroyOnReturn;
            public bool destroyOnOutOfRange;
            public bool destroyOnLiveTime;
            public bool destroyOnDeadTarget;
            public bool destroyOnCantFindTarget;
            public bool destroyOnNoGround;


            public void Add(DestroyConditions other)
            {
                foreach (var field in this.GetType().GetFields())
                {
                    var value = field.GetValue(this);

                    if (value.GetType() != typeof(bool)) continue;

                    if ((bool)field.GetValue(this) || (bool)field.GetValue(other))
                    {
                        field.SetValue(this, true);
                    }

                }
            }

            public DestroyConditions Copy()
            {
                var conditions = new DestroyConditions();

                foreach (var field in GetType().GetFields())
                {
                    var value = field.GetValue(this);

                    field.SetValue(conditions, value);
                }

                return conditions;
            }

            public void Subtract(DestroyConditions other)
            {
                foreach (var field in this.GetType().GetFields())
                {
                    var value = field.GetValue(other);

                    if (value.GetType() != typeof(bool)) continue;

                    if ((bool)value != true) continue;

                    field.SetValue(this, false);
                }
            }
        }

        [Serializable]
        public class Restrictions
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
            public bool onlyTargetsDead;
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

            public bool hideUI;

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

            public void Add(Restrictions other)
            {
                foreach (var field in this.GetType().GetFields())
                {
                    var value = field.GetValue(other);

                    if (value.GetType() != typeof(bool)) continue;

                    if ((bool)field.GetValue(this) || (bool)field.GetValue(other))
                    {
                        field.SetValue(this, true);
                    }
                    else
                    {
                        field.SetValue(this, false);
                    }


                }
            }

            public void Subtract(Restrictions other)
            {
                foreach (var field in this.GetType().GetFields())
                {
                    var value = field.GetValue(other);

                    if (value.GetType() != typeof(bool)) continue;

                    if ((bool)value != true) continue;

                    field.SetValue(this, false);
                }
            }

            public void UpdateSelf(AbilityInfo ability)
            {
                foreach (var field in this.GetType().GetFields())
                {
                    foreach (var abilityField in ability.GetType().GetFields())
                    {
                        if (field.Name != abilityField.Name) continue;

                        field.SetValue(this, abilityField.GetValue(ability));
                    }
                }
            }

            public Restrictions Copy()
            {
                var restrictions = new Restrictions();

                foreach (var field in this.GetType().GetFields())
                {
                    field.SetValue(restrictions, field.GetValue(this));
                }

                return restrictions;
            }

            public string Description()
            {

                var nextLineList = new List<string>();

                if (destroysSummons)
                {
                    nextLineList.Add($"Destroys any unit that is summoned by this target with this ability.");
                }

                if (onlyCastOutOfCombat)
                {
                    nextLineList.Add("Only cast out of combat.");
                }

                if (onlyCastSelf)
                {
                    nextLineList.Add($"Only target self.");
                }

                return ArchString.NextLineList(nextLineList);
            }
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
        public struct Threat
        {
            public bool enabled;
            public float additiveThreatMultiplier;

            public bool setsThreat { get; set; }
            public float threatSet { get; set; }
            public bool clearThreat { get; set; }

        }


        public DestroyConditions additiveConditions;
        public Restrictions additiveRestrictions;

        public RecastProperties recast;
        public Threat threat;
    }
}
