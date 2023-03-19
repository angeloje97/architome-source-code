using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System;
using System.Threading.Tasks;

namespace Architome
{

    public class Consumable : Item
    {
        public List<GameObject> consumableBuffs = new();
        public float power;
        public float coolDown = 1f;
        public float consumptionTime = 0f;

        float currentConsumptionTime;

        public struct ValidUserStates
        {
            public bool noCombat;
            public bool noMovement;

            public bool ValidEntity(EntityInfo entity)
            {
                if(noCombat && entity.isInCombat)
                {
                    return false;
                }

                return true;
            }

            public string Requirements()
            {
                var result = new List<string>();

                if (noCombat)
                {
                    result.Add("Cannot be used during combat.");
                }

                if (noMovement)
                {
                    result.Add("Must be standing still.");
                }

                return ArchString.NextLineList(result);
            }
        }

        [SerializeField] ValidUserStates validStates;


        private void OnValidate()
        {
            itemType = ItemType.Consumable;

            if (consumableBuffs == null) consumableBuffs = new();


            for (int i = 0; i < consumableBuffs.Count; i++)
            {
                var buff = consumableBuffs[i];
                var buffInfo = buff.GetComponent<BuffInfo>();

                if (buffInfo == null)
                {
                    consumableBuffs.RemoveAt(i);
                    i--;
                }
            }
        }

        public override string SubHeadline()
        {
            return "Consumable";
        }
        public override string Attributes()
        {
            var result = "";

            foreach (var buff in consumableBuffs)
            {
                var info = buff.GetComponent<BuffInfo>();

                result += info.TypeDescriptionFace(value);
            }

            return result;
        }


        public override async void Use(UseData data)
        {
            var info = data.itemInfo;
            var entity = data.entityUsed;

            if (!validStates.ValidEntity(entity))
            {
                return;
            }

            var success = info.ReduceStacks();
            if (!success) return;

            currentConsumptionTime = 0;

            var successTime = true;

            entity.infoEvents.OnConsumeStart?.Invoke(entity, this);

            while(currentConsumptionTime < consumptionTime)
            {
                await Task.Yield();
                currentConsumptionTime += Time.deltaTime;
            }

            entity.infoEvents.OnConsumeEnd?.Invoke(entity, this);

            if (successTime)
            {
                entity.infoEvents.OnConsumeComplete?.Invoke(entity, this);
            }

            var buffsManager = entity.Buffs();

            foreach (var buff in consumableBuffs)
            {
                var buffInfo = buff.GetComponent<BuffInfo>();

                buffsManager.ApplyBuff(new(buffInfo, this, data.entityUsed));
            }
        }

        public override string Requirements()
        {

            return ArchString.NextLineList(new() {
                validStates.Requirements()
            });


        }
    }
}
