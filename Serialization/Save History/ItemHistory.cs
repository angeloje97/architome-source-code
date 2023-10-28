using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Architome
{
    [Serializable]
    public class ItemHistory
    {
        //Singleton;
        public static ItemHistory active;

        Dictionary<int, ItemHistoryData> itemHashes;


        public void SetActiveSingleTon()
        {
            itemHashes ??= new();

            active = this;
        }
    }

    [Serializable]
    public class ItemHistoryData 
    {
        public int itemId;

        public bool obtained;
    }
}
