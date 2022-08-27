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

            var center = Center();
            center.y = -50;
            var size = Size();



            layeredGridGraph.center = center;
            layeredGridGraph.SetDimensions((int)(size.x), (int)(size.z), 1);

            aStarPath.Scan();


            Vector3 Center()
            {
                var trans = targetBin.GetComponentsInChildren<Transform>().ToList();

                return V3Helper.MidPoint(trans);
            }

            Vector3 Size()
            {
                var trans = targetBin.GetComponentsInChildren<Transform>().ToList();



                return V3Helper.Dimensions(trans);
            }
        }


    }
}
