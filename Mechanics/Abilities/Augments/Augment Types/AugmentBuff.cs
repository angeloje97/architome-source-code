using Architome.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class AugmentBuff : AugmentType
    {
        [Serializable]
        public class AbilityBuff 
        {
            public BuffInfo buff;
            public bool useProperties;
            public BuffProperties properties;
            enum AbilityBuffType
            {
                Start,
                End,
                Successful,
                ActivatedDuration
            }

            [SerializeField] AbilityBuffType buffType;

            Augment augment;
            AugmentType augmentType;

            BuffsManager buffsManager;
            public void SetAugment(AugmentType augmentType)
            {
                this.augmentType = augmentType;
                augment = augmentType.augment;
                buffsManager = augment.entity.Buffs();
            }

            public async void HandleAbilityStart(AbilityInfo ability)
            {
                if (!buffsManager) return;

                if(buffType == AbilityBuffType.Start)
                {
                    ApplyBuff(ability);
                    return;
                }

                if(buffType == AbilityBuffType.ActivatedDuration)
                {

                    var newBuff = ApplyBuff(ability);
                    await ability.EndActivation();

                    if(newBuff && !newBuff.buffTimeComplete)
                    {
                        newBuff.Cleanse();

                    }
                    return;
                }

                if (buffType != AbilityBuffType.End) return;
                await ability.EndActivation();

                ApplyBuff(ability);
                
            }

            public void HandleSuccessfulCast(AbilityInfo ability)
            {
                if (buffType != AbilityBuffType.Successful) return;
                ApplyBuff(ability);
            }

            public string Prefix()
            {
                return buffType switch
                {
                    AbilityBuffType.ActivatedDuration => "While the ability is being used",
                    AbilityBuffType.Start => "At the start of the ability",
                    AbilityBuffType.End => "At the end of the ability",
                    AbilityBuffType.Successful => "Upon successful cast",
                    _=> "",
                };
            }

            BuffInfo ApplyBuff(AbilityInfo ability)
            {
                var newBuff = buffsManager.ApplyBuff(new(buff) { sourceInfo = augment.entity});

                if (useProperties)
                {
                    newBuff.properties = properties;
                }
                Debugger.Combat(7541, $"{newBuff}");

                newBuff.properties.value = augmentType.value;

                newBuff.SetTarget(ability.targetLocked);


                return newBuff;
            }
        }

        [Serializable]
        public struct CatalystBuff 
        {
            public BuffInfo buff;
            public bool harm, assist, self;
            public BuffInfo triggeredBuff;
            public CatalystEvent trigger;

            Augment augment;

            public void SetAugment(Augment augment)
            {
                this.augment = augment;


            }

            public void HandleNewCatalyst(CatalystInfo catalyst)
            {

            }
        }


        public List<AbilityBuff> abilityBuffs;
        public List<CatalystBuff> catalystBuffs;
        void Start()
        {
            GetDependencies();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        new async void GetDependencies()
        {
            await base.GetDependencies();

            EnableCatalyst();
            EnableSuccesfulCast();
            
            EnableAbilityStartEnd();


            foreach(var buff in catalystBuffs)
            {
                buff.SetAugment(augment);
            }

            foreach(var buff in abilityBuffs)
            {
                buff.SetAugment(this);
            }
        }

        public override void HandleNewCatlyst(CatalystInfo catalyst)
        {
            if (catalystBuffs == null) return;

            foreach(var buff in catalystBuffs)
            {
                buff.HandleNewCatalyst(catalyst);
            }
        }

        public override void HandleAbility(AbilityInfo ability, bool start)
        {
            if (!start) return;
            if (abilityBuffs != null)
            {
                foreach(var buff in abilityBuffs)
                {
                    buff.HandleAbilityStart(ability);
                }
            }
        }

        protected override string Description()
        {
            var abilityDescription = new Dictionary<string, List<string>>();
            var finalDescription = new List<string>();

            foreach(var buff in abilityBuffs)
            {
                if (abilityDescription.ContainsKey(buff.Prefix()))
                {
                    abilityDescription[buff.Prefix()].Add($"{buff.buff}");
                }
                else
                {
                    abilityDescription.Add(buff.Prefix(), new List<string>()
                    {
                        buff.Prefix(),
                        $"apply {buff.buff}",
                    });
                }
            }

            foreach(var description in abilityDescription)
            {
                finalDescription.Add(ArchString.StringList(abilityDescription[description.Key]));
            }


            return ArchString.NextLineList(finalDescription);
        }
        public override void HandleSuccessfulCast(AbilityInfo ability)
        {
            if (abilityBuffs == null) return;

            foreach(var buff in abilityBuffs)
            {
                buff.HandleSuccessfulCast(ability);
            }
        }
    }
}
