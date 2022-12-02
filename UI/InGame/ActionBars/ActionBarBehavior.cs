using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Architome;
using UnityEngine.EventSystems;
using UltimateClean;

namespace Architome
{

    [RequireComponent(typeof(ToolTipElement))]
    public class ActionBarBehavior : MonoBehaviour
    {
        // Start is called before the first frame update
        public AbilityInfo abilityInfo { get; set; }
        public KeyBindings keyBindings;

        public Color darkenedColor;
        public Color halfDarkenedColor;

        public Image iconMain;
        public Image iconDark;

        //public Image characterPortrait;

        public TextMeshProUGUI coolDownTimer;
        public TextMeshProUGUI charges;
        public TextMeshProUGUI keyBindText;

        public bool isActive = false;

        public int actionBarNum;

        //Events
        public Action<AbilityInfo, int> OnChargeChange;
        public Action<AbilityInfo, AbilityInfo> OnAbilityChange;
        public Action<AbilityInfo> OnNewAbility;

        ToolTipElement toolTipHandler;
        public bool blockToolTip { get; set; }

        public void GetDependencies()
        {
            keyBindings = KeyBindings.active;

            ArchInput.active.OnAbilityKey += OnAbilityKey;

            if (keyBindings)
            {
                keyBindings.OnLoadBindings += OnLoadBindings;
            }

            OnAbilityChange += HandleAbilityChange;

            toolTipHandler = GetComponent<ToolTipElement>();

            if (toolTipHandler)
            {
                toolTipHandler.OnCanShowCheck += OnCanShowToolTipCheck;
            }
        }
        void Start()
        {
            GetDependencies();
            HandleEvents();
        }

        // Update is called once per frame
        void Update()
        {
            if (!isActive) { return; }
            HandleActionBars();
        }

        public void OnAbilityKey(int number)
        {
            if (!isActive) return;
            if (number != actionBarNum) return;

            if (abilityInfo.IsReady())
            {
                abilityInfo.Use();
                return;
            }

            abilityInfo.Recast();
        }
        public void HandleActionBars()
        {
            HandleCooldownTimer();
            HandleCharges();
            HandleFillAmount();

            void HandleCooldownTimer()
            {
                if(coolDownTimer == null) { return; }
                bool coolDownActive = abilityInfo.coolDown.charges != abilityInfo.coolDown.maxCharges;
                coolDownTimer.gameObject.SetActive(coolDownActive);
            
                if(coolDownActive)
                {
                    int timeRemaining = (int)(abilityInfo.coolDown.timer);

                    string text = "";
                    switch(timeRemaining)
                    {
                        case > 3600:
                            text = $"{timeRemaining /3600}h";
                            break;
                        case > 60:
                            text = $"{timeRemaining / 60}m";
                            text += $"{timeRemaining % 60}s";
                            break;
                        default:
                            text = $"{timeRemaining}s";
                            break;
                    }

                    coolDownTimer.text = ArchString.RoundedTime(timeRemaining, true);
                }

            
            }
            void HandleCharges()
            {
                if(!charges.text.Equals(abilityInfo.coolDown.charges.ToString()) && abilityInfo.coolDown.maxCharges > 1)
                {
                    charges.text = $"{abilityInfo.coolDown.charges}";
                }

                if (abilityInfo.coolDown.charges > 0)
                {
                    iconDark.color = halfDarkenedColor;
                }
                if(abilityInfo.coolDown.charges == 0)
                {
                    iconDark.color = darkenedColor;
                }
            }
            void HandleFillAmount()
            {
                if (iconMain.fillAmount != abilityInfo.coolDown.progress)
                {
                    iconMain.fillAmount = abilityInfo.coolDown.progress;
                }

            }
        }

        public async void HandleEvents()
        {
            var previousAbility = abilityInfo;

            while (this)
            {
                if (previousAbility != abilityInfo)
                {
                    OnAbilityChange?.Invoke(previousAbility, abilityInfo);
                    previousAbility = abilityInfo;
                    Debugger.Combat(6490, $"Ability Changed to {abilityInfo}");
                }
                await Task.Yield();
            }
        }

        void HandleAbilityChange(AbilityInfo before, AbilityInfo after)
        {
            if (before)
            {
                before.OnActiveChange -= OnAbilityActiveChange;
                before.OnChargesChange -= OnAbilityChargesChange;
            
            }

            if (after)
            {
                after.OnActiveChange += OnAbilityActiveChange;
                after.OnChargesChange += OnAbilityChargesChange;
            }
        }

        void OnCanShowToolTipCheck(ToolTipElement element)
        {
            if (abilityInfo == null)
            {
                element.checks.Add(false);
                return;
            }

            element.type = ToolTipType.GeneralHeader;
            element.data = abilityInfo.ToolTipData();
        }
        void OnAbilityActiveChange(AbilityInfo ability, bool active)
        {
            DisplayAbilityActive(active);
        }

        void OnAbilityChargesChange(AbilityInfo ability, int charges)
        {
            OnChargeChange?.Invoke(ability, charges);
        }
        public void SetImage(bool val)
        {
            if(iconMain)
            {
                iconMain.gameObject.SetActive(val);
            }

            if(iconDark)
            {
                iconDark.gameObject.SetActive(val);
            }

            if(coolDownTimer)
            {
                coolDownTimer.gameObject.SetActive(val);
            }

            if (abilityInfo && abilityInfo.coolDown.maxCharges > 1)
            {
                charges.gameObject.SetActive(true);
            }

            if (keyBindText)
            {
                keyBindText.text = keyBindings.keyBinds[$"Ability{actionBarNum}"].ToString().ToUpper();
                keyBindText.gameObject.SetActive(val);
            }
        }
        public void DisplayAbilityActive(bool val)
        {
            if (iconMain)
            {
                iconMain.gameObject.SetActive(val);
            }
        }

        public void OnLoadBindings(KeyBindings bindings)
        {
            SetKeyBindText();
        }
        public void SetKeyBindText()
        {
            keyBindText.text = keyBindings.keyBinds[$"Ability{actionBarNum}"].ToString().ToUpper();
        }

        public int SpriteIndex(KeyBindings bindings)
        {
            return bindings.SpriteIndex($"Ability{actionBarNum}");
        }
        public void SetActionBar(AbilityInfo abilityInfo)
        {
            Debugger.InConsole(8463, $"{abilityInfo}");

            //if (abilityInfo.catalyst == null) return;
            //var icon = abilityInfo.catalyst.GetComponent<CatalystInfo>().catalystIcon;

            //if (icon == null) return;

            if (abilityInfo.Icon() == null) return;

            this.abilityInfo = abilityInfo;
            iconMain.sprite = abilityInfo.abilityIcon;
            iconDark.sprite = abilityInfo.abilityIcon;
            iconDark.color = darkenedColor;
            OnNewAbility?.Invoke(abilityInfo);
            DisplayAbilityActive(abilityInfo.active);
            ArchAction.Delay(() =>
            {
                if (this.abilityInfo != abilityInfo) return;
                DisplayAbilityActive(abilityInfo.active);
            }, 1f);

            SetImage(true);

            isActive = true;
        }

        public void ResetBar()
        {
            abilityInfo = null;
            iconMain.sprite = null;
            iconDark.sprite = null;

            isActive = false;
            SetImage(false);
        }
        public void SetCharacterPortrait(EntityInfo entity)
        {
            //if (!characterPortrait) { return; }
            //if(!entity.entityPortrait) { return; }

            //characterPortrait.sprite = entity.entityPortrait;
            //characterPortrait.transform.parent.gameObject.SetActive(true);
        }
    }

}