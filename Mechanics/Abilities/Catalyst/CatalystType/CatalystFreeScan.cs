using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class CatalystFreeScan : MonoBehaviour
    {
        // Start is called before the first frame update

        public AbilityInfo abilityInfo;
        public CatalystInfo catalystInfo;
        public Vector3 location;

        float zGrowth = 1;
        float xGrowth = 1;
        public void GetDependencies()
        {
            if (catalystInfo == null)
            {
                catalystInfo = gameObject.GetComponent<CatalystInfo>();


            }


        }
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            transform.LookAt(catalystInfo.location);
            zGrowth += Time.deltaTime * abilityInfo.speed;
            xGrowth = transform.localScale.x;
            if (abilityInfo.scannerRadialAoe)
            {
                xGrowth += Time.deltaTime * abilityInfo.speed;
            }

            gameObject.transform.localScale = new Vector3(xGrowth, transform.localScale.y, zGrowth);
        }
    }

}