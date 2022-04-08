using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;

namespace Architome
{
    public class WorldPopupTextManager : MonoBehaviour
    {

        public static WorldPopupTextManager active;
        public Preferences preferences;

        [Serializable]
        public struct Prefabs
        {
            public GameObject damagePopUp;
            public GameObject healPopUp;
            public GameObject stateChange;
            public GameObject stateChangeImmune;
            public GameObject generalNotification;
        }

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

        public Prefabs prefabs;

        private void Awake()
        {
            active = this;
        }

        private void Start()
        {
            preferences = Preferences.active;
        }

        // Update is called once per frame

        public void DamagePopUp(Transform target, string text, DamageType damageType = DamageType.True)
        {
            if (prefabs.damagePopUp == null) return;
            if (!preferences.popUpPreferences.showDamage) return;

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

            Instantiate(prefabs.damagePopUp, transform).GetComponent<WorldPopUpText>().SetPopUp(target, text, color);
        }


        public void StateChangePopUp(Transform position, string text)
        {
            if (prefabs.stateChange == null) return;
            if (!preferences.popUpPreferences.showCrowdControl) return;
            Instantiate(prefabs.stateChange, transform).GetComponent<WorldPopUpText>().SetPopUp(position, text, colors.state);
        }

        public void StateChangeImmunityPopUp(Transform target, string text)
        {
            if (prefabs.stateChangeImmune == null) return;
            if (!preferences.popUpPreferences.showCrowdControl) return;

            Instantiate(prefabs.stateChangeImmune, transform).GetComponent<WorldPopUpText>().SetPopUp(target, $"{text} Immune", colors.stateImmune);
        }

        public void HealPopUp(Transform target, string text)
        {
            if (prefabs.healPopUp == null) return;

            Instantiate(prefabs.healPopUp, transform).GetComponent<WorldPopUpText>().SetPopUp(target, text, colors.heal);
        }

        public void GeneralPopUp(Transform target, string text)
        {
            if (prefabs.generalNotification == null) return;
            if (!preferences.popUpPreferences.showGeneral) return;
            Instantiate(prefabs.generalNotification, transform).GetComponent<WorldPopUpText>().SetPopUp(target, text, colors.general);
        }
    }

}