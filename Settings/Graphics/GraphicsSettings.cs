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
        public static void SaveGraphics()
        {
            SerializationManager.SaveConfig("Graphics", current);
        }

        [Serializable]
        public class Resolution
        {
            public float width;
            public float height;

            public override bool Equals(object obj)
            {
                if (obj.GetType() != typeof(Resolution)) return false;

                var otherResolution = (Resolution)obj;

                if (width == otherResolution.width && height == otherResolution.height) return true;

                return false;
            }
        }

        public GraphicsSettings()
        {
            qualityLevel = 3;
            resolution = new()
            {
                height = 1080,
                width = 1920,
            };
            maxFPS = 144;
            vsync = false;
        }

        public int qualityLevel;
        public Resolution resolution;
        public bool vsync;
        public float maxFPS = 144;
    }
}
