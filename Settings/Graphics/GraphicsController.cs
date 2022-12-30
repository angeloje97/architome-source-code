using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using TMPro;
using SharpNav.Crowds;

namespace Architome.Settings
{
    public class GraphicsController : ArchSettings
    {
        public static bool initiated = false;
        public RenderPipelineAsset[] qualityLevels;

        public List<GraphicsSettings.Resolution> resolutionPresets;
        public List<int> fpsOptions;

        public GraphicsSettings settings;

        public GraphicsSettings tempSettings;

        [Header("Components")]
        public TMP_Dropdown quality;
        public TMP_Dropdown resolution;
        public TMP_Dropdown fullScreenMode;
        public SelectionSliderLoopable targetFps;
        public Toggle vsync;
        public Toggle limitFps;
        void Start()
        {
            GetDependencies();
            HandleInitiation();
            UpdateFromSettings();
            HandleDirtyConflicts();
        }

        [SerializeField] bool update;
        private void OnValidate()
        {
            if (!update) return;
            update = false;

            resolutionPresets ??= new();

            foreach(var resolution in resolutionPresets)
            {
                resolution.OnValidate();
            }
            UpdateFromSettings();
        }
        void Update()
        {
        
        }


        void GetDependencies()
        {
            settings = GraphicsSettings.Current;
            tempSettings = new(settings);
        }

        void UpdateFromSettings()
        {
            UpdateQualityLevels();
            UpdateResolutions();
            UpdateFullScreenMode();
            UpdateFlags();
            UpdateFps();

            tempSettings = new(settings);



            void UpdateQualityLevels()
            {
                if (quality == null) return;

                var options = new List<TMP_Dropdown.OptionData>();

                foreach (var name in QualitySettings.names)
                {
                    options.Add(new(name));
                }
                quality.options = options;

                quality.SetValueWithoutNotify(settings.qualityLevel);
            }

            void UpdateResolutions()
            {
                if (resolution == null) return;
                var options = new List<TMP_Dropdown.OptionData>();
                int selectedIndex = 0;

                for (int i = 0; i < resolutionPresets.Count; i++)
                {
                    var resolution = resolutionPresets[i];
                    options.Add(new(resolution.name));

                    if (settings.resolution.Equals(resolution))
                    {
                        selectedIndex = i;
                    }
                }

                resolution.options = options;
                resolution.SetValueWithoutNotify(selectedIndex);

            }

            void UpdateFullScreenMode()
            {
                if (fullScreenMode == null) return;
                var options = new List<TMP_Dropdown.OptionData>();
                int count = 0;
                int selectedIndex = 0;
                foreach(FullScreenMode mode in Enum.GetValues(typeof(FullScreenMode)))
                {
                    options.Add(new(ArchString.CamelToTitle(mode.ToString())));

                    if(mode == settings.fullScreenMode)
                    {
                        selectedIndex = count;
                    }

                    count++;
                }

                fullScreenMode.options = options;
                fullScreenMode.SetValueWithoutNotify(selectedIndex);
            }

            void UpdateFps()
            {
                if (targetFps == null) return;
                if (fpsOptions == null) return;
                var options = new List<string>();

                int count = 0;
                int selectedIndex = 0;
                foreach(var frame in fpsOptions)
                {
                    if(frame == settings.maxFPS)
                    {
                        selectedIndex = count;
                    }

                    options.Add($"{frame} fps");

                    count++;
                }

                targetFps.SetOptions(options);
                targetFps.index = selectedIndex;

                targetFps.gameObject.SetActive(settings.limitFPS);
            }
            void UpdateFlags()
            {
                if (vsync) vsync.isOn = settings.vsync;
                if (limitFps) limitFps.isOn = settings.limitFPS;
            }

        }

        void HandleInitiation()
        {
            if (initiated) return; 
            initiated = true;
            Apply();
        }

        
        public void Apply()
        {
            GraphicsSettings.SetSettings(tempSettings);
            settings = GraphicsSettings.Current;
            dirty = false;

            QualitySettings.SetQualityLevel(settings.qualityLevel);
            QualitySettings.renderPipeline = qualityLevels[settings.qualityLevel];
            QualitySettings.vSyncCount = settings.vsync ? 1 : 0;

            var maxFps = settings.limitFPS ? settings.maxFPS : 1000;

            Screen.SetResolution(settings.resolution.width, settings.resolution.height, settings.fullScreenMode, maxFps);
        }

        void Revert()
        {
            tempSettings = new(settings);
            dirty = false;
        }

        public void ResetToDefault()
        {
            GraphicsSettings.ResetToDefault();
            settings = GraphicsSettings.Current;
            tempSettings = new(settings);
            Apply();
            UpdateFromSettings();
        }

        public override void HandleChooseApply()
        {
            base.HandleChooseApply();
            Apply();
        }

        public override void HandleLeaveDirty()
        {
            base.HandleLeaveDirty();
            Revert();
        }

        public async void UpdateScreen(bool unsafeUpdate = false)
        {
            int fps = tempSettings.limitFPS ? tempSettings.maxFPS : 1000;
            var currentScreenMode = Screen.fullScreenMode;
            Screen.SetResolution(tempSettings.resolution.width, tempSettings.resolution.height, tempSettings.fullScreenMode, fps);
            if (!unsafeUpdate) return;
            var promptHandler = PromptHandler.active;
            if (promptHandler == null) return;

            await promptHandler.GeneralPrompt(new() {
                title = "Settings",
                question = "Would you like to keep these settings?",
                blocksScreen = true,
                options = new()
                {
                    new("Keep Changes", (OptionData option) => {
                        Apply();
                    }),
                    new("Cancel", (OptionData option) => {
                        tempSettings.fullScreenMode = currentScreenMode;
                        Screen.SetResolution(tempSettings.resolution.width, tempSettings.resolution.height, currentScreenMode, fps);
                        UpdateFromSettings();
                    }) 
                    {
                        isEscape = true,
                        autoPick = true,
                        autoPickTimer = 10,
                    },
                }
            });

        }

        void UpdateTargetFPS()
        {
            if (targetFps == null) return;
            targetFps.gameObject.SetActive(tempSettings.limitFPS);
        }

        //Controller Actions
        public void ChangeQualityLevel(int level)
        {
            tempSettings.qualityLevel = level;
            dirty = true;
        }

        public void ChangeResolution(int resolutionIndex)
        {
            tempSettings.resolution = resolutionPresets[resolutionIndex];
            UpdateScreen();
            dirty = true;

        }

        public void ChangeFullScreenmode(int index)
        {
            var fullScreenMode = (FullScreenMode) Enum.GetValues(typeof(FullScreenMode)).GetValue(index);

            tempSettings.fullScreenMode = fullScreenMode;
            UpdateScreen(true);

        }

        public void SetLimitFPS(bool limit)
        {
            tempSettings.limitFPS = limit;
            UpdateTargetFPS();
            UpdateScreen();
        }

        public void SetVsync(bool vsync)
        {
            tempSettings.vsync = vsync;
        }

        public void SetMaxFps(int index)
        {
            tempSettings.maxFPS = fpsOptions[index];
        }


    }
}
