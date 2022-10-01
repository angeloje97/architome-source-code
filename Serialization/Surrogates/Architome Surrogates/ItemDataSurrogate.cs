using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;

namespace Architome
{
    public class ItemDataSurrogate : ArchitomeSurrogate
    {

        public override void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            ItemData itemData = (ItemData)obj;

            if (itemData.item != null)
            {
                info.AddValue("id", itemData.item._id);
                info.AddValue("amount", itemData.amount);
                info.AddValue("empty", false);

                HandleEquipment();
            }
            else
            {
                info.AddValue("empty", true);
            }
            

            void HandleEquipment()
            {
                if (!Item.Equipable(itemData.item)) return;
                var equipment = (Equipment)itemData.item;
                info.AddValue("stats", equipment.stats);

                info.AddValue("buffs", equipment.equipmentEffects);
                info.AddValue("itemLevel", equipment.itemLevel);
                info.AddValue("levelRequired", equipment.LevelRequired);

            }
        }


        public override object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            ItemData itemData = (ItemData)obj;

            var isEmpty =(bool) info.GetValue("empty", typeof(bool));

            if (isEmpty)
            {
                obj = new ItemData();
                return obj;
            }

            HandleEquipment();

            var id = (int)info.GetValue("id", typeof(int));
            var amount = (int)info.GetValue("amount", typeof(int));

            itemData.item = DataMaps.items[id];
            itemData.amount = amount;

            obj = itemData;
            return obj;

            void HandleEquipment()
            {
                if (!Item.Equipable(itemData.item)) return;
                var equipment = (Equipment)itemData.item;
                equipment.stats = (Stats)info.GetValue("stats", typeof(Stats));
                equipment.itemLevel = (int)info.GetValue("itemLevel", typeof(int));
                equipment.LevelRequired = (int)info.GetValue("levelRequired", typeof(int));

            }
        }
    }
}
