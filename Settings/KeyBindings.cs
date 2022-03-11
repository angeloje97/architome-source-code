using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyBindings : MonoBehaviour
{

    public static KeyBindings active;

    public void Awake()
    {
        active = this;
    }

    public Dictionary<string, KeyCode> keyBinds = new Dictionary<string, KeyCode>()
    {
        ["Ability1"] = KeyCode.Q,
        ["Ability2"] = KeyCode.W,
        ["Ability3"] = KeyCode.E,
        ["Ability4"] = KeyCode.R,
        ["Ability5"] = KeyCode.T,
        ["Ability6"] = KeyCode.A,
        ["Ability7"] = KeyCode.S,
        ["Ability8"] = KeyCode.D,
        ["Ability9"] = KeyCode.F,
        ["Ability10"] = KeyCode.G,


        ["AlternateAction1"] = KeyCode.Alpha1,
        ["AlternateAction2"] = KeyCode.Alpha2,
        ["AlternateAction3"] = KeyCode.Alpha3,
        ["AlternateAction4"] = KeyCode.Alpha4,
        ["AlternateAction5"] = KeyCode.Alpha5,

        ["PartyAbility"] = KeyCode.X,

        ["NextCamera"] = KeyCode.Tab,
        ["ToggleFreeCam"] = KeyCode.Y,

        ["SelectMultiple"] = KeyCode.LeftShift,
        ["SelectObject"] = KeyCode.Mouse0,
        ["Select"] = KeyCode.Mouse0,

        ["Action"] = KeyCode.Mouse1,

        ["Escape"] = KeyCode.Escape
    };


    public static Dictionary<string, KeyCode> keyBindsDefault = new Dictionary<string, KeyCode>()
    {
        ["Ability1"] = KeyCode.Q,
        ["Ability2"] = KeyCode.W,
        ["Ability3"] = KeyCode.E,
        ["Ability4"] = KeyCode.R,
        ["Ability5"] = KeyCode.T,
        ["Ability6"] = KeyCode.A,
        ["Ability7"] = KeyCode.S,
        ["Ability8"] = KeyCode.D,
        ["Ability9"] = KeyCode.F,
        ["Ability10"] = KeyCode.G,


        ["AlternateAction1"] = KeyCode.Alpha1,
        ["AlternateAction2"] = KeyCode.Alpha2,
        ["AlternateAction3"] = KeyCode.Alpha3,
        ["AlternateAction4"] = KeyCode.Alpha4,
        ["AlternateAction5"] = KeyCode.Alpha5,

        ["PartyAbility"] = KeyCode.X,

        ["NextCamera"] = KeyCode.Tab,
        ["ToggleFreeCam"] = KeyCode.Y,

        ["SelectMultiple"] = KeyCode.LeftShift,
        ["SelectObject"] = KeyCode.Mouse0,
        ["Select"] = KeyCode.Mouse0,

        ["Action"] = KeyCode.Mouse1,

        ["Escape"] = KeyCode.Escape
    };
}
