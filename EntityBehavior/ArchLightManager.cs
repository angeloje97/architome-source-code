using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Architome.Enums;

namespace Architome
{
    public class ArchLightManager : MonoBehaviour
    {
        public Light LightExplosion(Transform sender, Color color, float intensity = 5f, float falloff = 3f)
        {
            var lightObject = new GameObject();
            lightObject.transform.SetParent(sender);
            lightObject.transform.localPosition = new();

            var light = lightObject.AddComponent<Light>();

            light.type = LightType.Point;

            light.intensity = intensity;
            light.range = 10;
            light.color = color;

            FallOff(light, falloff);

            return light;
        }

        public Light Light(Transform sender, Color color, float intensity, float range = 5f)
        {
            var lightObject = new GameObject();
            lightObject.transform.SetParent(sender);
            lightObject.transform.localPosition = new();

            var light = lightObject.AddComponent<Light>();

            light.type = LightType.Point;

            light.intensity = intensity;
            light.range = 10;
            light.color = color;


            return light;
        }

        async void FallOff(Light light, float fallOffSpeed)
        {
            while (light.range > 0)
            {
                await Task.Yield();
                light.range -= (Time.deltaTime * fallOffSpeed);
            }

            Destroy(light);
        }
    }
}
