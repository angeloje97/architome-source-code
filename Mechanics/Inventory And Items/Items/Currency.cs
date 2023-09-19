using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    [CreateAssetMenu(fileName = "New Currency", menuName = "Architome/Item/Currency")]
    public class Currency : Item
    {
        public string fieldStringValue;
        public void OnValidate()
        {
            itemType = ItemType.Currency;
            infiniteStacks = true;
            maxStacks = -1;
        }

        public override int NewStacks(int currentStacks, int stacksToAdd, out int leftover)
        {
            leftover = 0;
            return currentStacks + stacksToAdd;
        }

        public override bool ValidStacks(int countCheck)
        {
            return true;
        }

        public override bool IsCurrency()
        {
            return true;
        }

        public override bool Useable(UseData data)
        {
            return true;
        }

        public override string UseString()
        {
            return "Claim";
        }

        public override void Use(UseData data)
        {
            data.guildManager.GainCurrency(this, data.itemInfo.currentStacks);

            data.itemInfo.DestroySelf(true);
        }
    }
}
