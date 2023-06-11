using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.Networking.Types;

namespace Architome
{
    public class TransformTimeLineMax : TransformTimeLine
    {
        public List<float> values;
        Dictionary<Component, int> sourceIndex;

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

            if (!sourceIndex.ContainsKey(source))
            {
                var newIndex = values.Count;
                sourceIndex.Add(source, newIndex);
                values.Add(value);
            }
            else
            {
                values[sourceIndex[source]] = value;
            }

            base.Lerp(MaxValue());
        }


        public void SetComponent(Component component)
        {
            if(currentComponent != component)
            {
                currentComponent = component;
            }
        }
        

        float MaxValue()
        {
            var max = 0f;
            foreach(var value in values)
            {
                if(value > max)
                {
                    max = value;
                }
            }

            return max;
        }

    }
}
