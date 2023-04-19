using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Architome
{
    public class AStarManifester : MonoBehaviour
    {
        [SerializeField] Transform targetBin;
        [SerializeField] AstarPath aStarPath;


        [Header("Actions")]
        public bool updatePathfinding;

        private void OnValidate()
        {
            if (!updatePathfinding) return;
            updatePathfinding = false;

            UpdatePathfinding();
        }

        public void UpdatePathfinding()
        {
            if (targetBin == null) return;
            if (aStarPath == null) return;

            var layeredGridGraph = aStarPath.data.layerGridGraph;
            if (layeredGridGraph == null) return;
            var vectorCluster = new VectorCluster<Transform>(targetBin.GetComponentsInChildren<Transform>().ToList());
            

            layeredGridGraph.center = vectorCluster.bottom;
            layeredGridGraph.SetDimensions((int)(vectorCluster.width), (int)(vectorCluster.depth), 1);

            aStarPath.Scan();
        }


    }
}
