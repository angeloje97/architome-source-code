using JetBrains.Annotations;
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
        Error,
    }
    public static class Debugger
    {

        public static Dictionary<ALogType, bool> logDict = new()
        {
            { ALogType.General, false},
            { ALogType.Combat, false },
            { ALogType.Environment, false },
            { ALogType.UI, true },
            { ALogType.Social, false },
            { ALogType.System, true },
            { ALogType.OutSource, false },
            { ALogType.Error, false },
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

        public static void InConsole(int id, string sentence)
        {
            Log(id, sentence, ALogType.General);
        }

        public static void Combat(int id, string sentence)
        {
            Log(id, sentence, ALogType.Combat);
        }

        public static void Environment(int id, string sentence)
        {
            Log(id, sentence, ALogType.Environment);
        }

        public static void UI(int id, string sentence)
        {
            Log(id, sentence, ALogType.UI);
        }
    
        public static void Social(int id, string sentence)
        {
            Log(id, sentence, ALogType.Social);
        }


        public static void System(int id, string sentence)
        {
            Log(id, sentence, ALogType.System);
        }
        public static void OutSource(int id, string sentence)
        {
            Log(id, sentence, ALogType.OutSource);
        }

        public static void Verify(string sentence, int id, ALogType logType)
        {
            Log(id, sentence, logType, "[VERIFY]");
        }

        public static void System(Action action)
        {
            if (!logDict[ALogType.System]) return;
            action();
        }



        public static void Error(int id, string sentence)
        {
            if (!logDict[ALogType.Error]) return;
            Debug.LogError(sentence);
        }

        public static void InvokeCheck(ALogType type, Action action)
        {
            if (!logDict[type]) return;
            action();
        }

        

        static void Log(int id, string sentence, ALogType type, string prefix = "")
        {
            if (!logDict[type]) return;
            Debug.Log($"{prefix}[{type}]({id}): {sentence}");
        }


    }

}