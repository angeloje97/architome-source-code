using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class AugmentSelfBuff : AugmentType
    {
        [Header("Self Buff Properties")]
        BuffsManager buffManager;
        public List<BuffInfo> abilityBuffs;
        public List<BuffInfo> castStartBuffs;
        public List<BuffInfo> successfulCastBuffs;

        [SerializeField] bool update;
        [SerializeField] bool casting;
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
            EnableAbilityStartEnd();
        }
        protected override string Description()
        {
            var result = "";

            var buffsDescription = new List<string>();

            if (castStartBuffs != null && castStartBuffs.Count > 0)
            {
               buffsDescription.Add($"At the start of the cast, {BuffNameList(castStartBuffs)} will be applied to the caster");
            }

            if (abilityBuffs != null && abilityBuffs.Count > 0)
            {
                
                buffsDescription.Add($"While casting, {BuffNameList(abilityBuffs)} will be applied to caster for the duration of the cast");
            }


            if (successfulCastBuffs != null && successfulCastBuffs.Count > 0)
            {
                buffsDescription.Add($"Upon successful cast {BuffNameList(successfulCastBuffs)} will be applied to the caster.");
            }

            result += ArchString.NextLineList(buffsDescription);
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

        public override void HandleAbility(AbilityInfo ability, bool start)
        {
            casting = start;

            HandleStartCasts();
            HandleAbilityBuffs();

            void HandleStartCasts()
            {
                if (!start) return;
                if (castStartBuffs == null) return;
                if (castStartBuffs.Count == 0) return;

                foreach (var buff in castStartBuffs)
                {
                    buffManager.ApplyBuff(new(buff.gameObject, ability));
                }
            }

            async void HandleAbilityBuffs()
            {
                if (abilityBuffs == null) return;
                if (abilityBuffs.Count == 0) return;

                foreach (var buff in abilityBuffs)
                {
                    buffManager.ApplyBuff(new(buff.gameObject, ability));
                }

                while (casting)
                {
                    await Task.Yield();
                }

                foreach (var buff in abilityBuffs)
                {
                    buffManager.CleanseBuff(buff);
                }

            }

        }
        public override void HandleSuccessfulCast(AbilityInfo ability)
        {
            if (successfulCastBuffs == null) return;

            foreach (var buff in successfulCastBuffs)
            {
                var newBuff = buffManager.ApplyBuff(new(buff.gameObject, ability));
                if (ability.targetLocked && newBuff)
                {
                    newBuff.SetTarget(ability.targetLocked);
                }
            }
        }


    }
}
