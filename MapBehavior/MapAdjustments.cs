using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

namespace Architome
{
    public class MapAdjustments : MonoBehaviour
    {
        public static MapAdjustments active;

        public Transform background;
        public MapInfo mapInfo;
        public MapEntityGenerator entityGenerator;

        public Action<MapAdjustments, float> WhileLoading { get; set; }
        void GetDependencies()
        {
            entityGenerator = MapEntityGenerator.active;

            if (entityGenerator)
            {
                entityGenerator.OnEntitiesGenerated += OnEntitiesGenerated;
            }
        }
        void Start()
        {
            GetDependencies();
        }

        private void Awake()
        {
            active = this;
        }

        // Update is called once per frame
        void Update()
        {

        }

        async public Task AdjustBackground(Vector3 position, Vector3 size)
        {
            if (background == null) { return; }
            position.y = background.transform.position.y;
            background.transform.position = position;
            background.transform.localScale = new Vector3(size.x * 5, 1, size.z * 5);
            Bounds bound = new Bounds(position, size);

            var layeredGraph = AstarPath.active.data.layerGridGraph;

            layeredGraph.center = position;

            layeredGraph.SetDimensions((int)(size.x), (int)( size.z), 1);

            float timer = 0f;
            foreach (var progress in AstarPath.active.ScanAsync())
            {
                await Task.Yield();
                WhileLoading?.Invoke(this, progress.progress);
                timer += Time.deltaTime;
            }

            Debugger.InConsole(54382, $"A star pathfinding project scan took {timer} seconds");
            
        }

        void OnEntitiesGenerated(MapEntityGenerator entityGenerator)
        {
            //NudgeEntities(entityGenerator.entityList, entityGenerator.miscEntities);
        }

        void NudgeEntities(Transform entityList, Transform miscEntities)
        {
            var entities = entityList.GetComponentsInChildren<EntityInfo>().ToList();
            var originalPositions = new List<Vector3>();

            foreach (var entity in entities)
            {
                originalPositions.Add(entity.transform.position);
                entity.transform.position += new Vector3(1, 0, 1);

            }

            ArchAction.Delay(() => {
                for (int i = 0; i < entities.Count; i++)
                {
                    if (originalPositions.Count <= i)
                    {
                        break;
                    }

                    entities[i].transform.position = originalPositions[i];
                }
            }, .250f);
            

        }


    }

}
