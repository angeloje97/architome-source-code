using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DungeonArchitect;

namespace Architome
{
    public class AStarManifester : MonoBehaviour
    {
        [SerializeField] Transform targetBin;
        [SerializeField] AstarPath aStarPath;

        public Vector3 boundPosition, boundSize;

        [Header("Actions")]
        public bool updatePathfinding;
        public bool checkBounds;

        private void OnValidate()
        {

            UpdatePathfinding();
            CheckBounds();
        }

        public void UpdatePathfinding()
        {
            if (!updatePathfinding) return;
            updatePathfinding = false;
            if (targetBin == null) return;
            if (aStarPath == null) return;

            var layeredGridGraph = aStarPath.data.layerGridGraph;
            if (layeredGridGraph == null) return;
            var vectorCluster = new VectorCluster<Transform>(targetBin.GetComponentsInChildren<Transform>().ToList());
            

            layeredGridGraph.center = vectorCluster.bottom;
            layeredGridGraph.SetDimensions((int)(vectorCluster.width), (int)(vectorCluster.depth), 1);

            aStarPath.Scan();
        }

        void CheckBounds()
        {
            if (!checkBounds) return;
            checkBounds = false;

            Bounds bounds = new Bounds(boundPosition, boundSize);
            var layeredGraph = aStarPath.data.layerGridGraph;

            boundPosition = layeredGraph.center;
            boundSize = new Vector3(layeredGraph.width, 1, layeredGraph.depth);

            AstarPath.active.UpdateGraphs(bounds);
        }


    }
}
