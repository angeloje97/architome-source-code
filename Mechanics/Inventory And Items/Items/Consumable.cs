using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{

    public class Consumable : Item
    {
        public List<GameObject> consumableBuffs = new();
        public float value;
        public float coolDown = 1f;
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


        public override void Use(UseData data)
        {
            var info = data.itemInfo;
            var success = info.ReduceStacks();

            if (!success) return;

            var buffsManager = data.entityUsed.Buffs();

            foreach (var buff in consumableBuffs)
            {
                var buffInfo = buff.GetComponent<BuffInfo>();

                buffsManager.ApplyBuff(new(buffInfo, this, data.entityUsed));
            }

        }
    }
}
