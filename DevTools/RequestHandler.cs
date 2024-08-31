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

        [SerializeField] ArchButton button;

        public Dictionary<string, Type> typeKeys { get; private set; }
        public Dictionary<string, object> componentValues { get; private set; }

        [Header("Components")]
        public InputField defaultComponent;
        public Toggle booleanComponent;


        public void HandleRequest(Request request)
        {

            this.typeKeys = request.attributes;

            HandleComponentData();

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
                var boolComponent = UnityEngine.Object.Instantiate(booleanComponent, transform);

                boolComponent.onValueChanged.AddListener((bool newValue) => {
                    onValueChange?.Invoke(newValue);
                });

                return boolComponent.transform;
            }

            var defaultComponent = Instantiate(this.defaultComponent, transform);

            defaultComponent.onValueChanged.AddListener((string newValue) => {
                onValueChange?.Invoke(newValue);
            });

            return defaultComponent.transform;
        }

        public void HandleComponentData()
        {
            foreach (KeyValuePair<string, Type> typeKey in typeKeys)
            {
                var component = CreateComponent(typeKey.Value, (object newValue) => {
                    UpdateKey(typeKey.Key, newValue);
                });
            }
        }
    }
}