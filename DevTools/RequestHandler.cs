using Architome.Enums;
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
        [SerializeField] ArchButton button; //For Action Requests
        [SerializeField] Toggle toggle; // For Toggle Request
        [SerializeField] SizeFitter sizeFitter;
        public Dictionary<string, Type> typeKeys { get; private set; }
        public Dictionary<string, object> componentValues { get; private set; }

        List<Transform> currentComponents;

        [Header("Components")]
        public TMP_InputField defaultComponent;
        public Toggle booleanComponent;
        public Slider rangeComponent;
        public TextMeshProUGUI rangeLabel;
        public TMP_Dropdown dropDown;

        Request request;
        #endregion

        #region Initiation

        public async Task HandleRequest(Request request)
        {

            this.typeKeys = request.attributes;


            this.request = request;

            await HandleComponentData();

        }

        public Transform CreateComponent(Type type, Action<object> onValueChange)
        {
            #region Bool Handle
            if (type == typeof(bool))
            {
                var boolComponent = Instantiate(booleanComponent, transform);

                boolComponent.enabled = false;

                boolComponent.onValueChanged.AddListener((bool newValue) => {
                    onValueChange?.Invoke(newValue);
                });


                return boolComponent.transform;

            }
            #endregion

            #region Range Handle

            if (type == typeof(IntRange) || type == typeof(FloatRange))
            {
                var slider = Instantiate(rangeComponent, transform);
                var label = Instantiate(rangeLabel, transform);

                if(type == typeof(IntRange))
                {
                    slider.wholeNumbers = true;

                }
                else
                {

                }

                slider.value = slider.minValue;

                slider.onValueChanged.AddListener((newValue) => {
                    var valueText = slider.wholeNumbers ? $"{(int)newValue}" : $"{(float)newValue}";
                    onValueChange?.Invoke(newValue);
                });

                return slider.transform;
            }
            #endregion

            #region Enum Handle
            if (type.IsEnum)
            {
                var newDropDown = Instantiate(dropDown, transform);
                List<TMP_Dropdown.OptionData> options = new();

                var enumValues = Enum.GetValues(type);

                foreach(Enum item in enumValues){
                    options.Add(new(item.ToString()));
                }

                newDropDown.options = options;

                dropDown.onValueChanged.AddListener((int index) => {
                    onValueChange?.Invoke(enumValues.GetValue(index));
                });


                return newDropDown.transform;
            }

            #endregion;

            var defaultComponent = Instantiate(this.defaultComponent, transform);

            defaultComponent.onValueChanged.AddListener((string newValue) => {
                onValueChange?.Invoke(newValue);
            });

            return defaultComponent.transform;
        }

        public Transform CreateRequestType()
        {
            var requestType = request.sourceType;

            if(requestType == DevToolType.Toggle)
            {
                var toggle = Instantiate(this.toggle, transform);

                toggle.onValueChanged.AddListener((bool newVal) => {
                    var toggleRequest = (ToggleRequest)request;
                    
                    toggleRequest.SetState(newVal, componentValues);
                });

                return toggle.transform;
            }
            else
            {
                var actionButton = Instantiate(button, transform);

                actionButton.OnClick += (button) => {
                    request.Invoke(componentValues);
                };

                return actionButton.transform;
            }
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

            CreateRequestType();

            foreach (KeyValuePair<string, Type> typeKey in typeKeys)
            {
                var component = CreateComponent(typeKey.Value, (object newValue) => {
                    UpdateKey(typeKey.Key, newValue);
                });

                currentComponents.Add(component);
            }

            await sizeFitter.AdjustToSize(3);
        }


        #endregion

        #region Live Actions
        public void UpdateKey(string key, object value)
        {
            if (!componentValues.ContainsKey(key))
            {
                componentValues.Add(key, value);
                return;
            }

            componentValues[key] = value;
        }

        #endregion
    }
}