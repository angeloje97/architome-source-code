using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.Settings
{
    [Serializable]
    public class GraphicsSettings
    {
        static GraphicsSettings current;
        public static GraphicsSettings Current
        {
            get
            {
                if(current == null)
                {
                    var graphicObj = SerializationManager.LoadConfig("Graphics");
                    
                    if(graphicObj == null)
                    {
                        current = new();

                        SaveGraphics();
                    }
                    else
                    {
                        current = (GraphicsSettings) graphicObj;
                    }
                }

                return current;
            }
        }

        public static void SetSettings(GraphicsSettings newSettings)
        {
            current = new(newSettings);
            SaveGraphics();
        }

        public static void ResetToDefault()
        {
            current = new();
        }

        public static void SaveGraphics()
        {
            SerializationManager.SaveConfig("Graphics", current);
        }

        [Serializable]
        public class Resolution
        {
            [HideInInspector] public string name;
            public int width;
            public int height;

            public bool Equals(Resolution otherResolution)
            {

                if (width == otherResolution.width && height == otherResolution.height) return true;

                return false;
            }

            public void OnValidate()
            {
                name = $"({width}px,{height}px)";
            }
        }

        public GraphicsSettings()
        {
            qualityLevel = 3;
            resolution = new()
            {
                width = Screen.width,
                height = Screen.height
            };
            maxFPS = 144;
            vsync = false;
            limitFPS = false;
            fullScreenMode = FullScreenMode.FullScreenWindow;
        }

        public int qualityLevel;
        public Resolution resolution;
        public FullScreenMode fullScreenMode;
        public bool vsync;
        public bool limitFPS;
        public int maxFPS = 144;

        public GraphicsSettings(GraphicsSettings copy)
        {
            foreach(var field in GetType().GetFields())
            {
                field.SetValue(this, field.GetValue(copy));
            }
        }
    }
}
