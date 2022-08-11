using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Architome
{
    public class PathfinderOpt : MonoBehaviour
    {
        // Start is called before the first frame update
        public AstarPath path;
        public List<Transform> rooms;
        public Transform midPoint;
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        public Vector3 MidPoint()
        {
            return V3Helper.MidPoint(rooms);
        }
    }

}