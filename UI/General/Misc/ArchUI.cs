using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Architome
{
    public static class ArchUI
    {
        #region Canvas
        public static async void SetCanvas(this CanvasGroup canvas, bool val, float totalTime = 0f)
        {
            if (canvas == null) return;

            var targetAlpha = val ? 1f : 0f;
            var startingAlpha = canvas.alpha;

            await ArchCurve.Smooth((float lerpValue) => {
                canvas.alpha = Mathf.Lerp(startingAlpha, targetAlpha, lerpValue);
            }, CurveType.EaseInOut, totalTime);


            canvas.alpha = targetAlpha;
            canvas.interactable = val;
            canvas.blocksRaycasts = val;
        }

        public static void SetCanvases(List<CanvasGroup> canvasGroups, bool val, float totalTime = 0f)
        {
            foreach (var canvas in canvasGroups)
            {
                SetCanvas(canvas, val, totalTime);
            }
        }

        public static async Task SetCanvasAsync(this CanvasGroup canvas, bool val, float totalTime)
        {
            if (canvas == null) return;

            var targetAlpha = val ? 1f : 0f;
            var startingAlpha = canvas.alpha;


            await ArchCurve.Smooth((float lerpValue) => {
                canvas.alpha = Mathf.Lerp(startingAlpha, targetAlpha, lerpValue);
            }, CurveType.EaseInOut, totalTime);

            canvas.interactable = val;
            canvas.blocksRaycasts = val;
        }

        #endregion

        #region DropDown

        public static Action<E> SetDropDown<E>(Action<E> action, TMP_Dropdown dropDown, E defaultValue) where E : Enum
        {
            var enums = Enum.GetValues(typeof(E));
            var options = new List<TMP_Dropdown.OptionData>();
            dropDown.ClearOptions();
            int defaultIndex = 0;
            int index = 0;
            foreach (E value in enums)
            {
                if (defaultValue.Equals(value))
                {
                    defaultIndex = index;
                }
                options.Add(new()
                {
                    text = EnumString.GetValue(value),
                });
                index++;
            }

            dropDown.options = options;
            dropDown.value = defaultIndex;


            dropDown.onValueChanged.AddListener((int newValue) =>
            {
                action((E)enums.GetValue(newValue));
            });


            return (E updatedValue) =>
            {
                index = 0;

                foreach (E value in enums)
                {
                    if (updatedValue.Equals(value))
                    {
                        dropDown.SetValueWithoutNotify(index);
                    }

                    index++;
                }
            };
        }

        public static void SetDropDown(Action<object> onChange, TMP_Dropdown dropDown, Type type)
        {
            if (!type.IsEnum) return;

            var values = Enum.GetValues(type);

            var options = new List<TMP_Dropdown.OptionData>();
            dropDown.ClearOptions();

            foreach (Enum value in values)
            {
                options.Add(new()
                {
                    text = EnumString.GetValue(value)
                });
            }

            dropDown.options = options;


            dropDown.onValueChanged.AddListener((int index) => {
                onChange?.Invoke(values.GetValue(index));
            });
        }

        public static Action<E> SetDropDown<E>(TMP_Dropdown dropDown, E defaultValue) where E: Enum
        {
            return SetDropDown((E ignore) => { }, dropDown, defaultValue);
        }

        public static Action<FieldInfo> SetDropDownData<C>(Action<FieldInfo> onChange, TMP_Dropdown dropDown, int defaultOption = 0)
        {
            var fields = typeof(C).GetFields();

            var options = new List<TMP_Dropdown.OptionData>();

            foreach(var field in fields)
            {
                options.Add(new() { text = field.Name });
            }


            dropDown.options = options;
            dropDown.value = defaultOption;

            dropDown.onValueChanged.AddListener((int newIndex) => {
                onChange(fields[newIndex]);
            });

            return (FieldInfo info) => {
                var index = 0;
                foreach(var field in fields)
                {
                    if(info == field)
                    {
                        dropDown.SetValueWithoutNotify(index);
                    }
                    index++;
                }
            };
        }

        public static Action<FieldInfo> SetDropDownData<C>(TMP_Dropdown dropDown, int defaultOption = 0)
        {
            return SetDropDownData<C>((FieldInfo ignore) => { }, dropDown, defaultOption);
        }
        #endregion

        #region Check Boxes
        public static void SetToggle(Action<bool> onChange, Toggle toggle, bool defaultValue)
        {
            toggle.isOn = defaultValue;
            toggle.onValueChanged.AddListener((bool isChecked) => {
                onChange(isChecked);
            });
        }
        #endregion

        #region Input Field

        public static void SetInputField(Action<string> onChange, TMP_InputField inputField, string defaultValue = "", string regexString = ".*")
        {
            inputField.text = defaultValue;

            var currentValue = defaultValue;

            inputField.onValueChanged.AddListener((string newValue) => {
                if(!Regex.IsMatch(newValue, regexString))
                {
                    inputField.SetTextWithoutNotify(currentValue);
                    return;
                }

                currentValue = newValue;
                onChange(newValue);
            });
        }

        #endregion

        public static async Task FixLayoutGroups(GameObject target, bool controlCanvas = false, float delay = 0f) // Needs multiple iterations for some reason
        {

            var canvasGroup = target.GetComponent<CanvasGroup>();

            if (canvasGroup && controlCanvas)
            {
                SetCanvas(canvasGroup, false);
            }

            await Task.Delay((int)(delay * 1000));

            var layoutGroups = target.GetComponentsInChildren<HorizontalOrVerticalLayoutGroup>();
            var contentFitters = target.GetComponentsInChildren<ContentSizeFitter>();

            var layoutGroup = target.GetComponent<LayoutGroup>();
            var contentSizeFitter = target.GetComponent<ContentSizeFitter>();




            
            if(layoutGroup) layoutGroup.enabled = false;
            if(contentSizeFitter) contentSizeFitter.enabled = false;

            for (int i = 0; i < 4; i++) //Toggling components asynchronously
            {
                foreach(var group in layoutGroups) { group.enabled = false; }
                foreach(var fitter in contentFitters) { fitter.enabled = false; }
                

                await Task.Delay(25);

                foreach (var group in layoutGroups) { group.enabled = true; }
                foreach (var fitter in contentFitters) { fitter.enabled = true; }
                
            }

            if(layoutGroup) layoutGroup.enabled = true;
            if(contentSizeFitter) contentSizeFitter.enabled = true;


            if (canvasGroup && controlCanvas)
            {
                SetCanvas(canvasGroup, true);
            }
        }

        public static void SetText(TextMeshProUGUI tmp, string str, SpriteAssetType type)
        {
            var spriteAsset = SpriteAssetData.active.SpriteAsset(type);
            tmp.spriteAsset = spriteAsset;
            tmp.text = str;
        }

    }
}
