using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;

namespace Architome
{
    public enum eDamagePopUpType 
    {
        Once,
        ChainedLimited,
        ChainedUnlimited,
    }

    public class PopupTextManager : MonoBehaviour
    {

        public static PopupTextManager active;
        public Preferences preferences;



        [Serializable]
        public struct PopUp
        {
            public string name;
            public PopupText prefab;
            public Color color;
        }

        public PopupText defaultPopUp;
        public List<PopUp> popUps;
        public Dictionary<string, PopUp> popUpDict;

        [Serializable]
        public struct Colors
        {
            public Color general;
            public Color damage;
            public Color physicalDamage;
            public Color magicDamage;
            public Color trueDamage;
            public Color state;
            public Color stateImmune;
            public Color heal;

        }

        public Colors colors;


        private void Awake()
        {
            active = this;
        }

        void GetDependencies()
        {
            popUps ??= new();
            popUpDict = new();
            
            foreach (var popUp in popUps)
            {
                if (popUpDict.ContainsKey(popUp.name)) continue;
                popUpDict.Add(popUp.name, popUp);
            }
        }

        private void Start()
        {
            preferences = Preferences.active;
            GetDependencies();
        }

        PopUp PopupTextInfo(string name)
        { 
            if (popUpDict.ContainsKey(name))
            {
                return popUpDict[name];
            }
            return popUpDict["Default"];
        }

        public PopupText NewPopUp(string popUpName)
        {
            var popUp = PopupTextInfo(popUpName);

            var newPopUp = Instantiate(popUp.prefab.gameObject, transform).GetComponent<PopupText>();


            return newPopUp;
        }

        [Header("Properties")]
        public eDamagePopUpType damagePopUpType;
        public PopupText DamagePopUp(PopUpParameters parameters, DamageType damageType = DamageType.True)
        {
            if (!preferences.popUpPreferences.showDamage) return null;

            var color = colors.trueDamage;

            switch(damageType)
            {
                case DamageType.Physical:
                    color = colors.physicalDamage;
                    break;
                case DamageType.Magical:
                    color = colors.magicDamage;
                    break;
            }

            //var popUp = Instantiate(prefabs.damagePopUp, transform).GetComponent<PopupText>();
            var popUp = NewPopUp("Damage");

            parameters.boolean = PopupText.eAnimatorBool.HealthChange;
            if (damagePopUpType == eDamagePopUpType.ChainedLimited)
            {
                parameters.states = new() { PopupText.eAnimatorState.EnableRepeat };
            }

            else if(damagePopUpType == eDamagePopUpType.ChainedUnlimited)
            {
                parameters.states = new() { PopupText.eAnimatorState.EnableRepeatUnlimited };
            }

            parameters.color = color;

            popUp.SetPopUp(parameters);

            return popUp;
        }


        public void StateChangePopUp(PopUpParameters parameters)
        {
            if (!preferences.popUpPreferences.showCrowdControl) return;
            //var popUp = Instantiate(prefabs.stateChange, transform).GetComponent<PopupText>();
            var popUp = NewPopUp("State");
            parameters.boolean = PopupText.eAnimatorBool.StateChange;
            popUp.SetPopUp(parameters);

        }

        public void StateChangeImmunityPopUp(PopUpParameters parameters)
        {
            if (!preferences.popUpPreferences.showCrowdControl) return;

            var popUp = NewPopUp("StateImmune");
            parameters.boolean = PopupText.eAnimatorBool.StateImmunity;
            parameters.color = colors.stateImmune;
            parameters.text = $"{parameters.text} Immune";
            popUp.SetPopUp(parameters);
        }

        public void HealPopUp(PopUpParameters parameters )
        {
            var popUp = NewPopUp("Heal");

            parameters.color = colors.heal;
            parameters.boolean = PopupText.eAnimatorBool.HealthChange;
            popUp.SetPopUp(parameters);
        }

        

        public PopupText GeneralPopUp(PopUpParameters parameters = null)
        {
            if (!preferences.popUpPreferences.showGeneral) return null;
            var popUp = NewPopUp("Default");
            popUp.SetPopUp(parameters);

            ArchAction.Delay(() => { popUp.EndAnimation(); }, 3f);

            return popUp;
        }
    }

}