using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class MapAdjustments : MonoBehaviour
    {
        // Start is called before the first frame update

        public Transform background;
        public MapInfo mapInfo;
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void AdjustBackground(Vector3 position, Vector3 size)
        {
            if (background == null) { return; }
            position.y = background.transform.position.y;
            background.transform.position = position;
            background.transform.localScale = new Vector3(size.x * 10, size.y, size.z * 10);
            Bounds bound = new Bounds(position, size);

            var layeredGraph = AstarPath.active.data.layerGridGraph;

            layeredGraph.center = position;

            layeredGraph.SetDimensions((int)size.x * 5, (int)size.z * 5, 1);



        }


    }

}
