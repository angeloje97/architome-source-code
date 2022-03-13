using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class MapHelper : MonoBehaviour
    {
        // Start is called before the first frame update


        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public static MapInfo MapInfo()
        {
            if (GameObject.Find("MapInfo"))
            {
                if (GameObject.Find("MapInfo").GetComponent<MapInfo>())
                {
                    return GameObject.Find("MapInfo").GetComponent<MapInfo>();
                }
            }
            return null;
        }


    }

}
