using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System.Threading.Tasks;
using DungeonArchitect.Builders.Grid;
using DungeonArchitect;

namespace Architome
{
    public class RoomDAManager : MonoBehaviour
    {
        [Serializable]
        public class GridItemTransformData
        {
            [HideInInspector]
            public string name;
            public GameObject childTargetSample;
            public Transform parentTarget;
            public List<string> childTargetNames;
            public RoomObjectType objectType;


            public void OnValidate()
            {
                UpdateName();
                if (childTargetSample == null) return;
                if (childTargetNames == null) childTargetNames = new();

                if (!childTargetNames.Contains(childTargetSample.name))
                {
                    childTargetNames.Add(childTargetSample.name);
                }


                childTargetSample = null;
            }

            void UpdateName()
            {
                this.name = objectType.ToString();
            }

            public HashSet<string> nameMap
            {
                get
                {
                    var map = new HashSet<string>();

                    if (childTargetNames != null)
                    {
                        foreach (var target in childTargetNames)
                        {
                            map.Add(target);
                        }
                    }

                    return map;
                }
            }
        }

        [SerializeField] List<GridItemTransformData> transformDatas;
        [SerializeField] Transform bin;
        [SerializeField] GridDungeonConfig activeDungeonConfig;
        [SerializeField] RoomInfo activeRoom;

        [SerializeField] List<GameObject> parentToChildLayerMasks;

        [Header("Actions")]
        [SerializeField] bool update;
        [SerializeField] bool updateMap;
        //Properties
        Dictionary<string, Transform> transformMap
        {
            get
            {
                var map = new Dictionary<string, Transform>();

                if (transformDatas != null)
                {
                    foreach (var data in transformDatas)
                    {
                        if (data.childTargetNames != null)
                        {
                            foreach (var childName in data.childTargetNames)
                            {
                                if (map.ContainsKey(childName)) continue;
                                map.Add(childName, data.parentTarget);
                            }
                        }
                    }
                }

                return map;
            }
        }
        Dictionary<string, Transform> activeMap;
        
        private void OnValidate()
        {
            ValidateData();
            UpdateSelf();
            HandleUpdateMap();
        }

        void HandleUpdateMap()
        {
            if (!updateMap) return;
            updateMap = false;

            activeMap = transformMap;
        }

        void ValidateData()
        {
            if (transformDatas == null) return;
            foreach (var data in transformDatas)
            {
                data.OnValidate();
            }
        }

        void UpdateSelf()
        {
            if (!update) return;
            update = false;


            MapTransforms();
            UpdateLayers();
            UpdateSeed();
        }

        void MapTransforms()
        {
            if (bin == null) return;
            if (transformDatas == null) return;

            var transformMap = this.activeMap;

            if (transformMap == null) return;

            foreach (var binChild in bin.GetComponentsInChildren<Transform>())
            {
                if (!transformMap.ContainsKey(binChild.name)) continue;

                binChild.SetParent(transformMap[binChild.name]);
            }

        }

        void UpdateLayers()
        {
            if (parentToChildLayerMasks == null) return;

            foreach (var parent in parentToChildLayerMasks)
            {
                var layer = parent.layer;

                foreach (var trans in parent.GetComponentsInChildren<Transform>())
                {
                    trans.gameObject.layer = layer;
                    
                }
            }
        }

        void UpdateSeed()
        {
            if (activeRoom == null || activeDungeonConfig == null) return;

            activeRoom.seed = activeDungeonConfig.Seed.ToString();
        }
    }
}
