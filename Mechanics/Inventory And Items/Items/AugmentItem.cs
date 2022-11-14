using System.Collections;
using System.Collections.Generic;
using Architome.Enums;
using UnityEngine;

namespace Architome
{
    [CreateAssetMenu(fileName = "New Augment", menuName = "Architome/Item/Augment")]
    public class AugmentItem : Item
    {
        public Augment augment;

        private void OnValidate()
        {
            itemType = ItemType.Augment;

            augment.name = this.itemName;
        }

        public override string Attributes()
        {
            var stringList = new List<string>()
            {
                augment.Description(),
                augment.TypeDescription(),
            };

            return ArchString.NextLineList(stringList);
        }

        public bool CanAttachTo(AbilityInfo ability)
        {
            return true;
        }
    }
}
