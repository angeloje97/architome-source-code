using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonArchitect;

namespace Architome
{
    public class PathTransformRule : TransformationRule
    {
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        public override void GetTransform(PropSocket socket, DungeonModel model, Matrix4x4 propTransform, System.Random random, out Vector3 outPosition, out Quaternion outRotation, out Vector3 outScale)
        {
            base.GetTransform(socket, model, propTransform, random, out outPosition, out outRotation, out outScale);
            
            Debugger.Environment(3254, $"{model} object");

        }

        void HandlePath(DungeonModel model)
        {
            var path = model.gameObject.GetComponent<PathInfo>();
            

        }
    }
}
