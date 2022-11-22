using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class AugmentCatalystModifier : AugmentType
    {
        [Header("Catalyst Modifier Properties")]
        public int additiveTicks;
        public float additiveRange, additiveLiveTime, additiveSpeed;

        void Start()
        {
            GetDependencies();
        }

        new async void GetDependencies()
        {
            await base.GetDependencies();

            EnableCatalyst();
        }

        public override async void HandleNewCatlyst(CatalystInfo catalyst)
        {
            await Task.Yield();
            if (catalyst.isCataling) return;

            catalyst.IncreaseTicks(false, additiveTicks);

            catalyst.range += additiveRange;
            catalyst.liveTime += additiveLiveTime;
            catalyst.speed += additiveSpeed;
        }

        public override string Description()
        {
            
            var resultList = new List<string>();

            if (additiveTicks > 0)
            {
                resultList.Add($"Increases catalyst ticks by {additiveTicks}");
            }
            else if (additiveTicks < 0)
            {
                resultList.Add($"Decreases catalyst ticks by {additiveTicks}");

            }

            if (additiveRange > 0)
            {
                resultList.Add($"Increase catalyst range by {additiveRange} meters.");
            }
            else if (additiveRange < 0)
            {
                resultList.Add($"Decrease catalyst range by {additiveRange} meters.");
            }

            if (additiveLiveTime > 0)
            {
                resultList.Add($"Increases catalyst live time by {additiveLiveTime} seconds");
            }
            else if(additiveLiveTime < 0)
            {
                resultList.Add($"Decreases catalyst live time by {additiveLiveTime} seconds");
            }

            if (additiveSpeed > 0)
            {
                resultList.Add($"Increases catalyst max speed by ${additiveSpeed} meters per second");
            }
            else if (additiveSpeed < 0)
            {
                resultList.Add($"Decreases catalyst max speed by {additiveSpeed} meters per second");

            }

            return ArchString.NextLineList(resultList);

        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
