using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace Architome
{
    public class CatalystManager : MonoBehaviour
    {
        // Start is called before the first frame update
        public static CatalystManager active { get; private set; }
        public CatalystAudio catalystAudio;
        public Transform defectiveCatalysts;
        void Start()
        {

        }

        private void Awake()
        {
            active = this;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public GameObject CatalystAudioManager()
        {
            if (catalystAudio== null) return null;

            return Instantiate(catalystAudio.gameObject, transform);
            
        }

        public struct MoveCatalystJob : IJobParallelForTransform
        {
            public NativeArray<float3> positions;
            public NativeArray<float3> directions;
            public NativeArray<float> speeds;
            public void Execute(int index, TransformAccess transform)
            {
                var nextPosition = positions[index] + (directions[index] * speeds[index]);
                positions[index] = nextPosition;

            }
        }

    }

}