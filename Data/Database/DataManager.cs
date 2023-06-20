using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class DataManager : MonoBehaviour
    {
        static DataManager active;

        public DataMap map;
        private void Awake()
        {
            if (active)
            {
                Destroy(gameObject);
                return;
            }

            active = this;
            ArchGeneric.DontDestroyOnLoad(gameObject);
            map.SetData();
        }
    }
}
