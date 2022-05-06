using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class KeyBindings : MonoBehaviour
{

    public static KeyBindings active;
    public string testKey;
    public List<NamedKeycode> keycodes;

    public void Awake()
    {
        active = this;
        Dictionary<string, KeyCode> keys = new Dictionary<string, KeyCode>();
        foreach (var key in keycodes)
        {
            keys.Add(key.name, key.keycode);
        }
    }
    [Serializable]
    public struct NamedKeycode
    {
        public string name;
        public KeyCode keycode;
    }



    private void OnValidate()
    {
        keycodes.Clear();

        foreach (KeyValuePair<string, KeyCode> binding in keyBinds)
        {
            keycodes.Add(new() { name = binding.Key, keycode = binding.Value });
        }

        if (testKey.Length == 0) return;
        
        KeyCode parsedKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), testKey);


        Debugger.InConsole(0452, $"{parsedKeyCode}");
    }


    public Dictionary<string, KeyCode> keyBinds = new Dictionary<string, KeyCode>()
    {
        ["Ability0"] = KeyCode.Q,
        ["Ability1"] = KeyCode.W,
        ["Ability2"] = KeyCode.E,
        ["Ability3"] = KeyCode.R,
        ["Ability4"] = KeyCode.T,
        ["Ability5"] = KeyCode.A,
        ["Ability6"] = KeyCode.S,
        ["Ability7"] = KeyCode.D,
        ["Ability8"] = KeyCode.F,
        ["Ability9"] = KeyCode.G,
        ["Ability10"] = KeyCode.X,


        ["AlternateAction0"] = KeyCode.Alpha1,
        ["AlternateAction1"] = KeyCode.Alpha2,
        ["AlternateAction2"] = KeyCode.Alpha3,
        ["AlternateAction3"] = KeyCode.Alpha4,
        ["AlternateAction4"] = KeyCode.Alpha5,


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
        ["Ability0"] = KeyCode.Q,
        ["Ability1"] = KeyCode.W,
        ["Ability2"] = KeyCode.E,
        ["Ability3"] = KeyCode.R,
        ["Ability4"] = KeyCode.T,
        ["Ability5"] = KeyCode.A,
        ["Ability6"] = KeyCode.S,
        ["Ability7"] = KeyCode.D,
        ["Ability8"] = KeyCode.F,
        ["Ability9"] = KeyCode.G,
        ["Ability10"] = KeyCode.X,


        ["AlternateAction0"] = KeyCode.Alpha1,
        ["AlternateAction1"] = KeyCode.Alpha2,
        ["AlternateAction2"] = KeyCode.Alpha3,
        ["AlternateAction3"] = KeyCode.Alpha4,
        ["AlternateAction4"] = KeyCode.Alpha5,


        ["NextCamera"] = KeyCode.Tab,
        ["ToggleFreeCam"] = KeyCode.Y,

        ["SelectMultiple"] = KeyCode.LeftShift,
        ["SelectObject"] = KeyCode.Mouse0,
        ["Select"] = KeyCode.Mouse0,

        ["Action"] = KeyCode.Mouse1,

        ["Escape"] = KeyCode.Escape
    };
}
