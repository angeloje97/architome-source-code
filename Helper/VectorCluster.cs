using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    [Serializable]
    public class VectorCluster<T> where T : Component
    {

        public List<T> components;
        public Vector3 min, max;
        public Vector3 dimensions, midPoint, bottom, top, left, right, front, back;
        public float height, width, depth;
        public T heighest, lowest, leftMost, rightMost, frontMost, backMost;

        public VectorCluster(List<T> components, bool calculateImmediately = true)
        {
            this.components = components;
            if (calculateImmediately)
            {
                CalculateProperties();
            }

        }

        public void CalculateProperties()
        {
            min = V3Helper.PositiveInfinity();
            max = V3Helper.NegativeInfinity();

            foreach (var component in components)
            {
                var position = component.transform.position;

                if (position.x < min.x)
                {
                    min.x = position.x;
                    leftMost = component;
                }
                if(position.y < min.y)
                {
                    min.y = position.y;
                    lowest = component;
                }
                if (position.z < min.z)
                {
                    min.z = position.z;
                    backMost = component;
                }

                if (position.x > max.x)
                {
                    max.x = position.x;
                    rightMost = component;
                }
                if (position.y > max.y)
                {
                    max.y = position.y;
                    heighest = component;
                }
                if (position.z > max.z)
                {
                    max.z = position.z;
                    frontMost = component;
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
