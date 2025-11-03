using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Architome
{
    public class TransformTimeLineMax : TransformTimeLine
    {
        public List<float> values;
        Dictionary<Component, int> sourceIndex;
        int maxIndex = -1;

        public Component currentComponent;

        protected override void Start()
        {
            base.Start();
            values = new();
            sourceIndex = new();
        }

        public override void Lerp(float value)
        {
            if (currentComponent == null) return;
            var source = currentComponent;
            int index;

            if (!sourceIndex.ContainsKey(source))
            {
                var newIndex = values.Count;
                sourceIndex.Add(source, newIndex);
                values.Add(value);
                index = newIndex;
            }
            else
            {
                index = sourceIndex[source];
                values[sourceIndex[source]] = value;
            }

            values[index] = value;

            UpdateMaxIndex();

            base.Lerp(values[maxIndex]);

            void UpdateMaxIndex()
            {
                if (maxIndex == -1)
                {
                    maxIndex = index;
                }
                else
                {
                    if (value > values[maxIndex])
                    {
                        maxIndex = index;
                    }
                }
            }
        }


        public void SetComponent(Component component)
        {
            if(currentComponent != component)
            {
                currentComponent = component;
            }
        }
        

    }
}
