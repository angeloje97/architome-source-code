using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
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

        TransformAccessArray nativeTransforms;
        public Transform[] transforms;
        public NativeArray<Vector3> directions;
        public NativeArray<float> speeds;
        public HashSet<CatalystInfo> catalysts;
        public Action<CatalystInfo, int> OnRemoveCatalyst;
        public Action<CatalystManager> BeforeCreateJob;
        public int catalystsCount { get; set; }

        void Start()
        {

        }

        private void Awake()
        {
            if(active && active != this)
            {
                Destroy(gameObject);
                return;
            }

            active = this;
            catalysts = new();
        }

        // Update is called once per frame
        void Update()
        {

            if (catalystsCount == 0) return;
            directions = new NativeArray<Vector3>(catalystsCount, Allocator.TempJob);
            speeds = new NativeArray<float>(catalystsCount, Allocator.TempJob);
            transforms= new Transform[catalystsCount];

            BeforeCreateJob?.Invoke(this);

            MoveCatalystJob moveCatalysts = new MoveCatalystJob
            {
                deltaTime = World.deltaTime,
                directions = directions,
                speeds = speeds,
            };

            nativeTransforms = new TransformAccessArray(transforms, 3);
            JobHandle jHandle = moveCatalysts.Schedule(nativeTransforms);
            jHandle.Complete();

            directions.Dispose();
            speeds.Dispose();
            nativeTransforms.Dispose();
        }

        public int AddCatalyst(CatalystInfo catalyst)
        {
            if (catalysts.Contains(catalyst)) return -1;
            catalysts.Add(catalyst);
            catalystsCount = catalysts.Count;
            return catalystsCount - 1;
        }

        public void RemoveCatalyst(CatalystInfo catalyst, int index)
        {
            if (!catalysts.Contains(catalyst)) return;
            catalysts.Remove(catalyst);
            catalystsCount = catalysts.Count;
            OnRemoveCatalyst?.Invoke(catalyst, index);
            
        }

        public GameObject CatalystAudioManager()
        {
            if (catalystAudio== null) return null;

            return Instantiate(catalystAudio.gameObject, transform);
            
        }

        struct MoveCatalystJob : IJobParallelForTransform
        {
            public NativeArray<Vector3> directions;
            public NativeArray<float> speeds;

            public float deltaTime;
            public void Execute(int index, TransformAccess transform)
            {
                transform.position += (directions[index] * speeds[index])*deltaTime;
                
                
            }
        }

    }

}