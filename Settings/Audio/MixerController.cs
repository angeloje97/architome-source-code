using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Linq;
using System.Threading.Tasks;

namespace Architome.Settings
{
    public class MixerController : ArchSettings
    {
        [SerializeField] AudioSetting currentSetting;
        [SerializeField] AudioMixer audioMixer;
        [SerializeField] AudioSetting.Mixer tempMixer;

        public Dictionary<string, SliderController> mixerDict;

        [Header("Components")]
        public Transform sliderParents;



        public List<NavBar> navBarHandler;

        
        public static void SetMixerVolume(AudioMixer mixer, string sliderName, float percent)
        {
            var newValue = Mathf.Log10(percent) * 20;
            Debugger.UI(5439, $"{newValue}");
            mixer.SetFloat(sliderName, newValue);
        }


        void Start()
        {
            GetDependencies();
            UpdateMixers();
            HandleSliders();
            HandleDirtyConflicts();
        }

        public override void HandleLeaveDirty()
        {
            RevertChanges();
        }

        void UpdateMixers()
        {
            if (currentSetting.mixer == null)
            {
                var shards = new List<AudioSetting.Mixer.Shard>();

                foreach(var name in AudioSetting.gameMixerNames)
                {
                    shards.Add(new(name, .50f));
                }

                AudioSetting.SaveCurrentSettings();
                currentSetting.mixer = new(shards);
            }

            CopyMixerValues();
        }



        public void RevertChanges()
        {
            CopyMixerValues();

            foreach(var shard in tempMixer.shards)
            {
                if (!mixerDict.ContainsKey(shard.alias)) continue;
                mixerDict[shard.alias].SetValue(shard.volume);
            }

            dirty = false;
        }

        public void ResetToDefault()
        {
            foreach(var shard in tempMixer.shards)
            {
                if (!mixerDict.ContainsKey(shard.alias)) continue;
                mixerDict[shard.alias].SetValue(.50f);
                currentSetting.mixer.shardDict[shard.alias].volume = .50f;
            }
            ApplyMixerValues();
            dirty = false;

        }


        void CopyMixerValues()
        {
            tempMixer = new(currentSetting.mixer.ShardsCopy());

        }

        public void ApplyMixerValues()
        {
            currentSetting.mixer = new(tempMixer.ShardsCopy());

            AudioSetting.SaveCurrentSettings();
            dirty = false;

        }

        public void HandleSliders()
        {
            if (sliderParents == null) return;

            mixerDict = new();
            foreach (var sliderController in sliderParents.GetComponentsInChildren<SliderController>())
            {
                var name = sliderController.Label();
                Debugger.UI(5966, $"Slider name is {name}");
                if (name == "") continue;
                mixerDict.Add(name, sliderController);

                var value = tempMixer.shardDict[name].volume;
                sliderController.SetValue(value);

                sliderController.OnValueChange += (SliderController controller, float value) => {
                    HandleSliderChange(name, value);
                };

            }
        }



        void GetDependencies()
        {
            currentSetting = AudioSetting.CurrentSettings;
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void HandleSliderChange(string name, float value)
        {
            if (!mixerDict.ContainsKey(name)) return;
            dirty = true;


            SetMixerVolume(audioMixer, name, value);
            tempMixer.SetMixer(name, value);
        }
    }
}
