using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    [Serializable]
    public class VectorCluster
    {
        public List<Transform> transforms;
        public Vector3 min, max;
        public Vector3 dimensions, midPoint, bottom, top, left, right, front, back;
        public float height, width, depth;
        public Transform heighest, lowest, leftMost, rightMost, frontMost, backMost;

        public VectorCluster(List<Transform> transforms, bool calculateImmediately = true)
        {
            this.transforms = transforms;
            if (calculateImmediately)
            {

                CalculateProperties();
            }
        }

        public void CalculateProperties()
        {
            min = V3Helper.PositiveInfinity();
            max = V3Helper.NegativeInfinity();

            foreach (var trans in transforms)
            {

                if (trans.position.x < min.x)
                {
                    min.x = trans.position.x;
                    leftMost = trans;
                }
                if(trans.position.y < min.y)
                {
                    min.y = trans.position.y;
                    lowest = trans;
                }
                if (trans.position.z < min.z)
                {
                    min.z = trans.position.z;
                    backMost = trans;
                }

                if (trans.position.x > max.x)
                {
                    max.x = trans.position.x;
                    rightMost = trans;
                }
                if (trans.position.y > max.y)
                {
                    max.y = trans.position.y;
                    heighest = trans;
                }
                if (trans.position.z > max.z)
                {
                    max.z = trans.position.z;
                    frontMost = trans;
                }
            }

            dimensions = max - min;
            width = dimensions.x;
            height = dimensions.y;
            depth = dimensions.z;

            midPoint = (max + min) / 2;
            bottom = new Vector3(midPoint.x, min.y, midPoint.z);
            top = new Vector3(midPoint.x, max.y, midPoint.z);
            left = new Vector3(min.x, midPoint.y, midPoint.z);
            right = new Vector3(max.x, midPoint.y, midPoint.z);
            front = new Vector3(midPoint.x, midPoint.y, max.z);
            back = new Vector3(midPoint.x, midPoint.y, min.z);

        }

    }
}
