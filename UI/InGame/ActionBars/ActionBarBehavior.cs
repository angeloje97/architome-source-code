using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Architome;
public class ActionBarBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    public AbilityInfo abilityInfo;
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
    public Action<AbilityInfo> OnNewAbility;

    //EventHandlers
    int chargeCheck;
    public void GetDependencies()
    {
        if (GMHelper.GameManager() && GMHelper.GameManager().keyBinds)
        {
            keyBindings = GMHelper.GameManager().keyBinds;
        }

        ArchInput.active.OnAbilityKey += OnAbilityKey;
    }
    void Start()
    {
        GetDependencies();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive) { return; }
        HandleActionBars();
        HandleEvents();
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
                int timeRemaining = (int)(abilityInfo.coolDown.progressTimer);

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

                coolDownTimer.text = text;
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

    public void HandleEvents()
    {
        if(chargeCheck != abilityInfo.coolDown.charges)
        {
            chargeCheck = (int) abilityInfo.coolDown.charges;
            OnChargeChange?.Invoke(abilityInfo, chargeCheck);
        }
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

        //if(keyBindText)
        //{
        //    keyBindText.text = keyBindings.keyBinds[keyBind].ToString().ToUpper();
        //    keyBindText.gameObject.SetActive(val);
        //}
        
    }
    public void SetActionBar(AbilityInfo abilityInfo)
    {
        Debugger.InConsole(8463, $"{abilityInfo}");
        //if (abilityInfo.catalyst == null) return;
        //var icon = abilityInfo.catalyst.GetComponent<CatalystInfo>().catalystIcon;

        //if (icon == null) return;

        if (abilityInfo.abilityIcon == null) return;

        this.abilityInfo = abilityInfo;
        iconMain.sprite = abilityInfo.abilityIcon;
        iconDark.sprite = abilityInfo.abilityIcon;
        iconDark.color = darkenedColor;
        OnNewAbility?.Invoke(abilityInfo);

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
