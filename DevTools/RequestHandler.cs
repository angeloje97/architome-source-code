using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Architome.DevTools
{
    public class RequestHandler : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] ArchButton button;
        [SerializeField] SizeFitter sizeFitter;
        public Dictionary<string, Type> typeKeys { get; private set; }
        public Dictionary<string, object> componentValues { get; private set; }

        List<Transform> currentComponents;

        [Header("Components")]
        public InputField defaultComponent;
        public Toggle booleanComponent;


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