using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debugger : MonoBehaviour
{
    // Start is called before the first frame update
    static bool debugging = false;
    static bool debugCombat = false;
    static bool debugEnvironment = false;
    static bool debugUI = false;
    static bool debugSocial = false;
    static bool debugOutSource = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void InConsole(int id, string sentence)
    {
        if(debugging)
        {
            Debug.Log($"{id}: {sentence}");
        }
    }

    public static void Combat(int id, string sentence)
    {
        if (!debugCombat) return;
        Debug.Log($"Combat {id}: {sentence}");
    }

    public static void Environment(int id, string sentence)
    {
        if (!debugEnvironment) return;
        Debug.Log($"Environment {id}: {sentence}");
    }

    public static void UI(int id, string sentence)
    {
        if (!debugUI) return;

        Debug.Log($"UI: {id} : {sentence}");
    }
    
    public static void Social(int id, string sentence)
    {
        if (!debugSocial) return;

        Debug.Log($"Social: {id} : {sentence}");
    }

    public static void OutSource(int id, string sentence)
    {
        if (!debugOutSource) return;
        Debug.Log($"OutSource: {id} : {sentence}");
    }

}
