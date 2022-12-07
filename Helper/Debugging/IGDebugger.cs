

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Architome.Debugging
{



    public class IGDebugger : MonoBehaviour
    {
        public static List<LogData> unCaughtLogs;
        static bool globalEnableDebug;
        static bool globalPopUpError;
        [SerializeField] ModuleInfo module;

        bool listening { get; set; }

        public List<LogType> logTypesShown;

        public int maxLogs = 20;


        public bool enableDebug;
        public bool popUpError;
        public bool testErrorOnStart;
        bool enableDebugCheck;

        public string searchFilter;

        public Action<LoggedItem> OnNewLog;
        public Action<LoggedItem> OnDuplicatedLog;

        [Serializable]
        public struct Components
        {
            public Toggle enableDebug;
            public Toggle popUpError;
        }


        [Header("Prefabs")]
        public LoggedItem logItemPrefab;
        public Transform logParent;

        public Components components;

        public List<LogSetting> logSettings;
        public Dictionary<LogType, LogSetting> logSettingDict;
        
        ArchSceneManager sceneManager;

        public Action<LogData> OnSelectLogData;
        public Action<LogData> OnNewData { get; set; }

        public Action<LoggedItem> OnStackedLog;


        public Dictionary<string, LogData> logHistory;

        void Start()
        {
            if (module == null) return;

            CreateDictionary();
            GetDependencies();
            logHistory = new();
            HandleTestErrorOnStart();
            HandleUncaughtLogs();
            
        }


        void Update()
        {
            Debug.developerConsoleVisible = false;
            if (!gameObject.activeInHierarchy) return;
            if (enableDebug != enableDebugCheck)
            {
                enableDebugCheck = enableDebug;
                HandleEnableChange();
            }
        }

        private void OnValidate()
        {
            UpdateComponents();


            logSettings ??= new();

            foreach (var setting in logSettings)
            {
                setting.UpdateSelf();
            }
        }
        void GetDependencies()
        {
            module.OnActiveChange += OnModuleActiveChange;
            enableDebug = globalEnableDebug;
            popUpError = globalPopUpError;

            var canvas = GetComponent<Canvas>();
            sceneManager = ArchSceneManager.active;

            UpdateComponents();
        }
        async void HandleUncaughtLogs()
        {
            unCaughtLogs ??= new();

            while (sceneManager.isLoading)
            {
                await Task.Yield();
            }

            foreach(var log in unCaughtLogs)
            {
                CreateLog(log);
            }

            unCaughtLogs = new();
        }
        void HandleTestErrorOnStart()
        {
            if (!testErrorOnStart) return;
            ArchAction.Delay(() => { TestError(); }, 10);
        }

        void CreateDictionary()
        {
            logSettingDict = new();
            foreach(var setting in logSettings)
            {
                logSettingDict.Add(setting.type, setting);
            }
        }

        void UpdateComponents()
        {

            components.enableDebug.isOn = enableDebug;
            components.enableDebug.SetIsOnWithoutNotify(enableDebug);
            components.popUpError.isOn = popUpError;
            components.popUpError.SetIsOnWithoutNotify(popUpError);

        }

        void HandleEnableChange()
        {
            if (enableDebug)
            {
                Application.logMessageReceived += Log;

            }
            else
            {
                Application.logMessageReceived -= Log;
            }
        }

        public void SetListen(Toggle toggle)
        {
            enableDebug = toggle.isOn;
            globalEnableDebug = toggle.isOn;

            if(!enableDebug && popUpError)
            {
                popUpError = false;
                UpdateComponents();
            }
        }

        public void SetPopUpError(Toggle toggle)
        {
            popUpError = toggle.isOn;
            globalPopUpError = toggle.isOn;

            if (popUpError && !enableDebug)
            {
                enableDebug = true;
                UpdateComponents();
            }
        }

        public void SelectLogData(LogData data)
        {
            OnSelectLogData?.Invoke(data);
        }

        

        void OnModuleActiveChange(bool isActive)
        {

            if (isActive)
            {
                listening = true;
                UpdateText();
            }
            else
            {
                listening = false;
            }
        }

        async void Log(string logString, string stackTrace, LogType type)
        {


            var logData = new LogData(logString, stackTrace, type);

            unCaughtLogs.Add(logData);

            while (this && sceneManager.isLoading)
            {
                await Task.Yield();
            }

            if (this == null) return;

            unCaughtLogs = new();

            CreateLog(logData);
        }

        void CreateLog(LogData logData)
        {
            if (popUpError)
            {
                if (logData.type == LogType.Error || logData.type == LogType.Exception)
                {
                    if (!module.isActive)
                    {
                        module.SetActive(true);
                    }
                }
            }

            if (logHistory.ContainsKey(logData.log))
            {
                var originalLog = logHistory[logData.log];

                bool exists = false;

                if (originalLog.itemHost != null)
                {
                    OnStackedLog?.Invoke(originalLog.itemHost);
                    exists = true;
                }

                originalLog.Increment();
                if (exists) return;
            }
            else
            {
                logHistory.Add(logData.log, logData);
            }

            var logItem = Instantiate(logItemPrefab.gameObject, logParent).GetComponent<LoggedItem>();
            OnNewLog?.Invoke(logItem);
            logItem.SetLogData(logData, logSettingDict[logData.type].color);
            logItem.SetDebugger(this);

            
        }

        void UpdateText()
        {
            UpdateFilter();
        }

        public void Clear()
        {
            foreach(var loggedItem in GetComponentsInChildren<LoggedItem>())
            {
                loggedItem.RemoveSelf();
            }

            logHistory = new();
        }

        public void HandleChangeFilter(TMP_InputField inputField)
        {
            searchFilter = inputField.text;
            UpdateFilter();
        }

        void UpdateFilter()
        {



        }

        public void TestError()
        {
            Debug.LogException(new("Testing exception"));
        }


        [Serializable]
        public class LogData
        {
            public LoggedItem itemHost;
            public string log, stack;
            public LogType type;
            public bool taken;
            public bool duplicate;
            public int amount;

            public Action<LogData> OnIncrement;

            public void Increment()
            {
                amount++;
                OnIncrement?.Invoke(this);
            }

            public void SetLogItem(LoggedItem loggedItem)
            {
                itemHost = loggedItem;
            }

            public LogData(string log, string stack, LogType type)
            {
                amount = 1;
                this.log = log;
                this.stack = stack;
                this.type = type;
            }
        }

        [Serializable]
        public class LogSetting
        {
            [HideInInspector] public string name;
            public LogType type;
            public Color color;

            public void UpdateSelf()
            {
                name = type.ToString();
            }
        }

    }
}
