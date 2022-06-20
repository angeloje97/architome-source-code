using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Architome
{
    public class BuffSizeChanger : BuffType
    {
        // Start is called before the first frame update
        public float sizeIncrease;
        public Vector3 originalSize;
        new void GetDependencies()
        {
            base.GetDependencies();

            buffInfo.OnBuffStart += OnBuffStart;
            buffInfo.OnBuffEnd += OnBuffEnd;
        }
        void Start()
        {
            GetDependencies();
        }

        public override string Description()
        {
            return $"Unit's size is {ArchString.FloatToSimple(sizeIncrease*100)}% of its original size.\n";
        }

        public override string GeneralDescription()
        {
            return $"Change the unit's size to {ArchString.FloatToSimple(sizeIncrease * 100)}% of its original size.\n";
        }

        async void OnBuffStart(BuffInfo buff)
        {
            originalSize = OriginalSize();
            ArchAction.Delay(() => { buffInfo.expireDelay = 3f; }, .125f);

            while (buffInfo.hostInfo.transform.localScale != originalSize * sizeIncrease)
            {
                await Task.Yield();
                if (buffInfo.IsComplete) return;
                buffInfo.hostInfo.transform.localScale = Vector3.Lerp(buffInfo.hostInfo.transform.localScale, originalSize * sizeIncrease, .125f);
            }
        }

        public Vector3 OriginalSize()
        {
            foreach (var buff in buffInfo.buffsManager.GetComponentsInChildren<BuffInfo>())
            {
                if (buff == buffInfo) continue;
                var otherSizeChanger = buff.GetComponent<BuffSizeChanger>();
                if (otherSizeChanger == null) continue;

                return otherSizeChanger.originalSize;
            }

            return buffInfo.hostInfo.transform.localScale;
        }

        async void OnBuffEnd(BuffInfo buff)
        {
            while (buffInfo.hostInfo.transform.localScale != originalSize)
            {
                await Task.Yield();

                buffInfo.hostInfo.transform.localScale = Vector3.Lerp(buffInfo.hostInfo.transform.localScale, originalSize, .125f);
            }
        }

        private void OnDestroy()
        {
            buffInfo.hostInfo.transform.localScale = originalSize;
        }

    }

}