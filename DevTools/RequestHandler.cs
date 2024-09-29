using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Architome
{
    #region Structs
    public struct FloatRange
    {
        public float min;
        public float max;
    }

    public struct IntRange
    {
        public int min;
        public int max;
    }
    #endregion
}

namespace Architome.DevTools
{
    

    public class RequestHandler : MonoBehaviour
    {
        #region Common Data
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] ArchButton button;
        [SerializeField] SizeFitter sizeFitter;
        public Dictionary<string, Type> typeKeys { get; private set; }
        public Dictionary<string, object> componentValues { get; private set; }

        List<Transform> currentComponents;

        [Header("Components")]
        public InputField defaultComponent;
        public Toggle booleanComponent;
        public Slider rangeComponent;
        public TextMeshProUGUI rangeLabel;
        #endregion

        


        public async Task HandleRequest(Request request)
        {

            this.typeKeys = request.attributes;

            await HandleComponentData();

            button.OnClick += (button) => {
                request.Invoke(componentValues);
            }; 
        }

        public void UpdateKey(string key, object value)
        {
            if (!componentValues.ContainsKey(key))
            {
                componentValues.Add(key, value);
                return;
            }

            componentValues[key] = value;
        }

        public Transform CreateComponent(Type type, Action<object> onValueChange)
        {
            if (type == typeof(bool))
            {
                var boolComponent = Instantiate(booleanComponent, transform);

                boolComponent.onValueChanged.AddListener((bool newValue) => {
                    onValueChange?.Invoke(newValue);
                });

                return boolComponent.transform;
            }

            if(type == typeof(IntRange) || type == typeof(FloatRange))
            {
                var slider = Instantiate(rangeComponent, transform);
                var label = Instantiate(rangeLabel, transform);

                if(type == typeof(IntRange))
                {
                    slider.wholeNumbers = true;
                }

                slider.onValueChanged.AddListener((newValue) => {
                    var valueText = slider.wholeNumbers ? $"{(int)newValue}" : $"{(float)newValue}";
                    onValueChange?.Invoke(newValue);
                });

                return slider.transform;
            }

            var defaultComponent = Instantiate(this.defaultComponent, transform);

            defaultComponent.onValueChanged.AddListener((string newValue) => {
                onValueChange?.Invoke(newValue);
            });

            return defaultComponent.transform;
        }

        public async Task HandleComponentData()
        {
            if(currentComponents == null)
            {
                currentComponents = new();
            }
            else
            {
                for(int i = 0; i < currentComponents.Count; i++)
                {
                    Destroy(currentComponents[i].gameObject);
                    currentComponents.RemoveAt(i);
                    i--;
                }
            }

            foreach (KeyValuePair<string, Type> typeKey in typeKeys)
            {
                var component = CreateComponent(typeKey.Value, (object newValue) => {
                    UpdateKey(typeKey.Key, newValue);
                });

                currentComponents.Add(component);
            }

            await sizeFitter.AdjustToSize(3);
        }
    }
}