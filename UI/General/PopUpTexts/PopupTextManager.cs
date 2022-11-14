using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;

namespace Architome
{
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

        public PopupText NewPopUp(string popUpName, Transform target = null, string text = "")
        {
            var popUp = PopupTextInfo(popUpName);

            var newPopUp = Instantiate(popUp.prefab.gameObject, transform).GetComponent<PopupText>();

            if(target != null)
            {
                newPopUp.SetPopUp(target, text, popUp.color);
            }

            return newPopUp;


        }

        public void DamagePopUp(Transform target, string text, DamageType damageType = DamageType.True)
        {
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

            //var popUp = Instantiate(prefabs.damagePopUp, transform).GetComponent<PopupText>();
            var popUp = NewPopUp("Damage");
            popUp.SetAnimation(new() { healthChange = true });
            popUp.SetPopUp(target, text, color);

        }


        public void StateChangePopUp(Transform position, string text)
        {
            if (!preferences.popUpPreferences.showCrowdControl) return;
            //var popUp = Instantiate(prefabs.stateChange, transform).GetComponent<PopupText>();
            var popUp = NewPopUp("State");
            popUp.SetAnimation(new() { stateChange = true });
            popUp.SetPopUp(position, text, colors.state);

        }

        public void StateChangeImmunityPopUp(Transform target, string text)
        {
            if (!preferences.popUpPreferences.showCrowdControl) return;

            var popUp = NewPopUp("StateImmune");
            popUp.SetAnimation(new() { stateImmunity = true });
            popUp.SetPopUp(target, $"{text} Immune", colors.stateImmune);
        }

        public void HealPopUp(Transform target, string text)
        {
            var popUp = NewPopUp("Heal");
            popUp.SetAnimation(new() { healthChange = true });
            popUp.SetPopUp(target, text, colors.heal);
        }

        

        public PopupText GeneralPopUp(Transform target, string text, Color color, PopupText.PopUpParameters parameters = null)
        {
            if (!preferences.popUpPreferences.showGeneral) return null;
            var popUp = NewPopUp("Default");
            if (parameters != null)
            {
                popUp.SetAnimation(parameters);
            }
            popUp.SetPopUp(target, text, color);



            ArchAction.Delay(() => { popUp.EndAnimation(); }, 1f);

            return popUp;
        }
    }

}