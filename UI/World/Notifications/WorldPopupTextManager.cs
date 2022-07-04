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

            var popUp = Instantiate(prefabs.damagePopUp, transform).GetComponent<WorldPopUpText>();
            popUp.SetPopUp(target, text, color);
            popUp.SetAnimation(new() { healthChange = true });

        }


        public void StateChangePopUp(Transform position, string text)
        {
            if (prefabs.stateChange == null) return;
            if (!preferences.popUpPreferences.showCrowdControl) return;
            var popUp = Instantiate(prefabs.stateChange, transform).GetComponent<WorldPopUpText>();

            popUp.SetPopUp(position, text, colors.state);
            popUp.SetAnimation(new() { stateChange = true });

        }

        public void StateChangeImmunityPopUp(Transform target, string text)
        {
            if (prefabs.stateChangeImmune == null) return;
            if (!preferences.popUpPreferences.showCrowdControl) return;

            var popUp = Instantiate(prefabs.stateChangeImmune, transform).GetComponent<WorldPopUpText>();
            popUp.SetPopUp(target, $"{text} Immune", colors.stateImmune);
            popUp.SetAnimation(new() { stateImmunity = true });
        }

        public void HealPopUp(Transform target, string text)
        {
            if (prefabs.healPopUp == null) return;

            var popUp = Instantiate(prefabs.healPopUp, transform).GetComponent<WorldPopUpText>();
            popUp.SetPopUp(target, text, colors.heal);
            popUp.SetAnimation(new() { healthChange = true });
        }

        public void GeneralPopUp(Transform target, string text)
        {
            if (prefabs.generalNotification == null) return;
            if (!preferences.popUpPreferences.showGeneral) return;
            var popUp = Instantiate(prefabs.generalNotification, transform).GetComponent<WorldPopUpText>();
            popUp.SetPopUp(target, text, colors.general);

            ArchAction.Delay(() => { popUp.EndAnimation(); }, 1f);
        }
    }

}