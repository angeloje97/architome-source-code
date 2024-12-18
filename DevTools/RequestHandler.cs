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
    [Serializable]
    public struct FloatRange
    {
        public float min;
        public float max;

        float minCheck, maxCheck;

        public FloatRange(float min, float max)
        {
            this.min = min;
            this.max = max;
            minCheck = min;
            maxCheck = max;
        }

        public FloatRange(IntRange intRange) : this(intRange.min, intRange.max) { }

        public void ClampValues()
        {
            if (minCheck != min)
            {
                if (min > max)
                {
                    min = max;
                }
                minCheck = min;
            }

            if (maxCheck != max)
            {
                if (max < min)
                {
                    max = min;
                }
                maxCheck = max;
            }
        }

        public void ClampValues(FloatRange restrictions)
        {
            if (min < restrictions.min)
            {
                min = restrictions.min;
            }

            if (max > restrictions.max)
            {
                max = restrictions.max;
            }

            ClampValues();
        }
    }

    [Serializable]
    public struct IntRange
    {
        public int min, max;

        int minCheck, maxCheck;

        public IntRange(int min, int max)
        {
            this.min = min;
            this.max = max;
            minCheck = min;
            maxCheck = max;
        }

        public IntRange(FloatRange floatRange) : this((int) floatRange.min, (int)floatRange.max) { }

        public void ClampValues()
        {
            if(minCheck != min)
            {
                if(min > max)
                {
                    min = max;
                }
                minCheck = min;
            }

            if(maxCheck != max)
            {
                if(max < min)
                {
                    max = min;
                }
                maxCheck = max;
            }
        }

        public void ClampValues(IntRange restrictions)
        {
            if(min < restrictions.min)
            {
                min = restrictions.min;
            }

            if(max > restrictions.max)
            {
                max = restrictions.max;
            }

            ClampValues();
        }

        public override string ToString()
        {
            return $"[{min}, {max}";
        }
    }
    #endregion
}

namespace Architome.DevTools
{
    #region Request Handler
    public class RequestHandler : MonoBehaviour
    {
        #region Common Data
        [SerializeField] ArchButton button; //For Action Requests
        [SerializeField] Toggle toggle; // For Toggle Request
        [SerializeField] SizeFitter sizeFitter;
        public Dictionary<string, Type> typeKeys { get; private set; }
        public Dictionary<string, object> componentValues { get; private set; }
        public Dictionary<string, GameObject> attributeGameObjects;

        List<Transform> currentComponents;

        [Header("Components")]
        public TMP_InputField defaultComponent;
        public Toggle booleanComponent;
        public RangeSlider rangeComponent;
        public TextMeshProUGUI rangeLabel;
        public TMP_Dropdown dropDown;

        Request request;
        #endregion

        #region Initiation

        public void HandleRequest(Request request)
        {
            typeKeys = request.attributes;

            typeKeys ??= new();

            CreateComponentValues();

            this.request = request;

            HandleComponentData();

            request.onSetRequestHandler?.Invoke(this);
        }

        void CreateComponentValues()
        {
            componentValues = new();

            foreach(var key in typeKeys)
            {
                componentValues.Add(key.Key, "");
            }
        }

        public Transform CreateComponent(Type type, Action<object> onValueChange)
        {
            try
            {
                #region Bool Handle
                if (type == typeof(bool))
                {
                    var boolComponent = Instantiate(booleanComponent, transform);

                    boolComponent.enabled = false;

                    boolComponent.onValueChanged.AddListener((bool newValue) => {
                        onValueChange?.Invoke(newValue);
                    });

                    onValueChange?.Invoke(boolComponent.enabled);


                    return boolComponent.transform;

                }
                #endregion

                #region Range Handle

                if (type == typeof(IntRange) || type == typeof(FloatRange))
                {
                    var wholeNumbers = type == typeof(IntRange);
                    var slider = Instantiate(rangeComponent, transform);
                    var label = Instantiate(rangeLabel, transform);

                    slider.SetWholeNumbers(wholeNumbers);

                    slider.onValueChange += (floatRange) => {
                        object value = wholeNumbers ? new IntRange(floatRange) : floatRange;

                        onValueChange?.Invoke(value);
                    };

                    return slider.transform;
                }
                #endregion

                #region Enum Handle
                if (type.IsEnum)
                {
                    var newDropDown = Instantiate(dropDown, transform);
                    List<TMP_Dropdown.OptionData> options = new();

                    var enumValues = Enum.GetValues(type);


                    Enum firstEnum = (Enum) enumValues.GetValue(0);

                    Debugger.UI(5017, $"First enum is {firstEnum}");

                    ArchUI.SetDropDown((object newValue) => {
                        onValueChange?.Invoke(newValue);
                    }, newDropDown, type);

                    onValueChange?.Invoke(firstEnum);

                    return newDropDown.transform;
                }

                #endregion;

                var defaultComponent = Instantiate(this.defaultComponent, transform);

                defaultComponent.onValueChanged.AddListener((string newValue) => {
                    onValueChange?.Invoke(newValue);
                });

                return defaultComponent.transform;

            }
            catch (Exception e)
            {
                Defect.CreateIndicator(transform, $"Cannot create component of type: {type}", e);
            }

            return null;
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

        public void HandleComponentData()
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

            attributeGameObjects = new();

            foreach (KeyValuePair<string, Type> typeKey in typeKeys)
            {
                var component = CreateComponent(typeKey.Value, (object newValue) =>
                {
                    UpdateKey(typeKey.Key, newValue);
                });

                attributeGameObjects.Add(typeKey.Key, component.gameObject);

                component.gameObject.name = typeKey.Key;

                currentComponents.Add(component);

                if(request.onCreateComponentForAttribute != null && request.onCreateComponentForAttribute.ContainsKey(typeKey.Key))
                {
                    request.onCreateComponentForAttribute[typeKey.Key].Invoke(component.gameObject);
                }
            }

            ArchAction.Delay(async () => {
                Debugger.UI(5014, $"Waiting Size Fitter {this}");
                await sizeFitter.AdjustToSize(3);
            }, 1f);
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

    #endregion

}