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

        async void OnBuffStart(BuffInfo buff)
        {
            originalSize = buffInfo.hostInfo.transform.localScale;
            ArchAction.Delay(() => { buffInfo.expireDelay = 3f; }, .125f);

            while (buffInfo.hostInfo.transform.localScale != originalSize * sizeIncrease)
            {
                await Task.Yield();

                buffInfo.hostInfo.transform.localScale = Vector3.Lerp(buffInfo.hostInfo.transform.localScale, originalSize * sizeIncrease, .125f);
            }
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