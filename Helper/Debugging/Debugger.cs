using SharpNav.Crowds;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public enum ALogType
    {
        General,
        Combat,
        Environment,
        UI,
        Social,
        System,
        OutSource,
    }
    public class Debugger : MonoBehaviour
    {

        public static Dictionary<ALogType, bool> logDict = new()
        {
            { ALogType.General, false },
            { ALogType.Combat, false },
            { ALogType.Environment, false },
            { ALogType.UI, false },
            { ALogType.Social, false },
            { ALogType.System, false },
            { ALogType.OutSource, false },
        };

        public static bool Status(ALogType logType)
        {
            return logDict[logType];
        }

        public static void Set(ALogType logType, bool status)
        {
            logDict[logType] = status;
        }

        public static void Toggle(ALogType logType)
        {
            logDict[logType] = !logDict[logType];
        }


        void Start()
        {
        
        }


        // Update is called once per frame
        void Update()
        {
        
        }

        public static void InConsole(int id, string sentence)
        {
            if (!logDict[ALogType.General]) return;
            Debug.Log($"{id}: {sentence}");
            
        }

        public static void Combat(int id, string sentence)
        {
            if (!logDict[ALogType.Combat]) return;
            Debug.Log($"Combat {id}: {sentence}");
        }

        public static void Environment(int id, string sentence)
        {
            if (!logDict[ALogType.Environment]) return;
            Debug.Log($"Environment {id}: {sentence}");
        }

        public static void UI(int id, string sentence)
        {
            if (!logDict[ALogType.UI]) return;

            Debug.Log($"UI: {id} : {sentence}");
        }
    
        public static void Social(int id, string sentence)
        {
            if (!logDict[ALogType.Social]) return;
            Debug.Log($"Social: {id} : {sentence}");
        }


        public static void System(int id, string sentence)
        {
            if (!logDict[ALogType.System]) return;

            Debug.Log($"Social: {id} : {sentence}");
        }

        public static void OutSource(int id, string sentence)
        {
            if (!logDict[ALogType.OutSource]) return;
            Debug.Log($"OutSource: {id} : {sentence}");
        }

    }

}