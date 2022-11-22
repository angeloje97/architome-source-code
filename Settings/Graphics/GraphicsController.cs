using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;


namespace Architome.Settings
{
    public class GraphicsController : MonoBehaviour
    {
        public static bool initiated = false;
        public RenderPipelineAsset[] qualityLevels;
        public Dropdown quality;
        public Dropdown resolution;

        public List<GraphicsSettings.Resolution> resolutionPresets;

        public GraphicsSettings settings;
        void Start()
        {
            GetDependencies();
            HandleInitiation();
        }



        void GetDependencies()
        {
            settings = GraphicsSettings.Current;
        }

        void HandleInitiation()
        {
            if (initiated) return;
            initiated = true;


        }


        void Update()
        {
        
        }
    }
}
