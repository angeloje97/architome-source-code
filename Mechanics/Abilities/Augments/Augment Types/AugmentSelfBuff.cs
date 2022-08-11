using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class AugmentSelfBuff : AugmentType
    {
        [Header("Self Buff Properties")]
        BuffsManager buffManager;
        public List<BuffInfo> abilityBuffs;
        public List<BuffInfo> successfulCastBuffs;

        [SerializeField] bool update;
        void Start()
        {
            GetDependencies();
        }

        private void OnValidate()
        {
            if (!update) return; update = false;


            UpdateAbilityBuffs();
            UpdateSuccessfulCastBuffs();

            void UpdateAbilityBuffs()
            {
                if (abilityBuffs == null) return;

                for (int i = 0; i < abilityBuffs.Count; i++)
                {
                    var buff = abilityBuffs[i];
                    if (buff == null)
                    {
                        abilityBuffs.RemoveAt(i);
                        i--;
                        continue;
                    }
                }
            }
            void UpdateSuccessfulCastBuffs()
            {
                if (successfulCastBuffs == null) return;

                for (int i = 0; i < successfulCastBuffs.Count; i++)
                {
                    var buff = successfulCastBuffs[i];
                    if (buff == null)
                    {
                        successfulCastBuffs.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        new async void GetDependencies()
        {
            await base.GetDependencies();

            buffManager = augment.entity.Buffs();

            if (buffManager == null) return;

            EnableSuccesfulCast();
            EnableAbilityStartEnd();
        }



        public override string Description()
        {
            var result = "";

            var buffsDescription = new List<string>();


            if (abilityBuffs != null && abilityBuffs.Count > 0)
            {
                result += $"While casting, {BuffNameList(abilityBuffs)} will be applied to caster for the duration of the cast";
            }

            if (successfulCastBuffs != null && successfulCastBuffs.Count > 0)
            {
                if (result.Length > 0) result += "\n";

                result += $"Upon successful cast {BuffNameList(successfulCastBuffs)} will be applied to the caster.";
            }

            return result;
        }

        string BuffNameList(List<BuffInfo> buffs)
        {
            var stringList = new List<string>();

            foreach (var buff in buffs)
            {
                stringList.Add(buff.name);
            }

            return ArchString.StringList(stringList);
        }

        public override async void HandleAbility(AbilityInfo ability, bool start)
        {
            if (abilityBuffs == null) return;

            if (!start) return;

            foreach (var buff in abilityBuffs)
            {
                buffManager.ApplyBuff(new(buff.gameObject, ability));
            }

            await ability.EndActivation();

            foreach (var buff in abilityBuffs)
            {
                buffManager.CleanseBuff(buff);
            }

        }

        public override void HandleSuccessfulCast(AbilityInfo ability)
        {
            if (successfulCastBuffs == null) return;

            foreach (var buff in successfulCastBuffs)
            {
                buffManager.ApplyBuff(new(buff.gameObject, ability));
            }
        }
    }
}
