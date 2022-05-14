using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Architome
{
    public class Search
    {

        public static BuffInfo Buff(List<BuffInfo> buffs, int buffId)
        {
            int min = 0;
            int max = buffs.Count - 1;

            while (min <= max)
            {
                int mid = (min + max) / 2;

                if (buffs[mid]._id == buffId)
                {
                    return buffs[mid];
                }
                else if (buffId < buffs[mid]._id)
                {
                    max = mid - 1;
                }
                else
                {
                    min = mid + 1;
                }
            }

            return null;
        }

        public static EntityInfo Entity(List<EntityInfo> entities, int id)
        {
            int min = 0;
            int max = entities.Count;

            while (min <= max)
            {
                int mid = (min + max) / 2;

                if (entities[mid]._id == id)
                {
                    return entities[mid];
                }

                else if(entities[mid]._id < id)
                {
                    max = mid - 1;
                }

                else
                {
                    min = mid + 1;
                }
                
            }

            return null;
        }

        public static ArchClass Class(List<ArchClass> classes, int id)
        {
            int min = 0;
            int max = classes.Count - 1;

            while (min <= max)
            {
                int mid = (min + max) / 2;

                if (classes[mid]._id == id)
                {
                    return classes[mid];
                }

                else if (classes[mid]._id < id)
                {
                    max = mid - 1;
                }

                else
                {
                    min = mid + 1;
                }
            }

            return null;
        }

        public static Item Item(List<Item> items, int id)
        {
            int min = 0;
            int max = items.Count - 1;

            while (min <= max)
            {
                int mid = (min + max) / 2;

                if (items[mid]._id == id)
                {
                    return items[mid];
                }

                else if (id < items[mid]._id)
                {
                    max = mid - 1;
                }

                else
                {
                    min = mid + 1;
                }
            }

            return null;
        }
    }
}
