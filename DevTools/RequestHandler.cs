using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Architome.DevTools
{
    public class RequestHandler : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] ComponentTypes componentTypes;
        [SerializeField] ArchButton button;

        public void HandleRequest(Request request)
        {
            var componentData = new ComponentData(request.attributes, transform);

            componentTypes.HandleComponentData(componentData);

            button.OnClick += (button) => {
                request.Invoke(componentData.componentValues);
            }; 
        }

    }
    [Serializable]
    public class ComponentTypes
    {
        public InputField defaultComponent;
        public Toggle booleanComponent;


        public Transform CreateComponent(Type type, Transform targetParent, Action<object> onValueChange)
        {
            if (type == typeof(bool))
            {
                var boolComponent = UnityEngine.Object.Instantiate(booleanComponent, targetParent);

                boolComponent.onValueChanged.AddListener((bool newValue) => {
                    onValueChange?.Invoke(newValue);
                });

                return boolComponent.transform;
            }

            var defaultComponent = UnityEngine.Object.Instantiate(this.defaultComponent, targetParent);

            defaultComponent.onValueChanged.AddListener((string newValue) => {
                onValueChange?.Invoke(newValue);
            });

            return defaultComponent.transform;
        }

        public void HandleComponentData(ComponentData data)
        {
            foreach (KeyValuePair<string, Type> typeKey in data.typeKeys)
            {
                var component = CreateComponent(typeKey.Value, data.targetParent, (object newValue) => {
                    data.UpdateKey(typeKey.Key, newValue);
                });
            }
        }
    }

    public class ComponentData
    {
        public Transform targetParent;


        public Dictionary<string, Type> typeKeys { get; private set; }
        public Dictionary<string, object> componentValues { get; private set; }

        public ComponentData(Dictionary<string, Type> typeKeys, Transform targetParent)
        {
            this.typeKeys = typeKeys;
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

    }
}