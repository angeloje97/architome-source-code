using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Architome
{
    [Serializable]
    public class AudioSetting
    {
        public static readonly List<string> gameMixerNames = new List<string>() {
            "Master",
            "Sound Effect",
            "Ambience",
            "Music",
            "Voice",
            "UI"
        };

        static AudioSetting currentSettings;



        public static AudioSetting CurrentSettings
        {
            get
            {
                if (currentSettings == null)
                {
                    currentSettings = (AudioSetting) SerializationManager.LoadConfig("Audio");

                    if (currentSettings == null)
                    {
                        currentSettings = new();
                        SaveCurrentSettings();
                        
                    }
                }

                return currentSettings;
            }

            private set
            {
                currentSettings = value;
            }
        }
        
        public static void SaveCurrentSettings()
        {
            SerializationManager.SaveConfig("Audio", currentSettings);
        }

        [Serializable]
        public class Mixer
        {

            [Serializable]
            public class Shard
            {
                public string name, alias;
                public float volume;

                public Shard(string alias, float volume)
                {
                    this.alias = alias;
                    this.volume = volume;

                    name = $"{alias} : {volume}";
                }
            }

            public List<Shard> shards;
            public Dictionary<string, Shard> shardDict;


            public List<Shard> ShardsCopy()
            {
                var result = new List<Shard>();
                foreach (var shard in shards)
                {
                    result.Add(new(shard.alias, shard.volume));
                }

                return result;
            }
            public Mixer(List<Shard> shards)
            {
                this.shards = shards;
                UpdateShardDict();
            }

            public Mixer(Mixer other)
            {
                foreach (var field in typeof(Mixer).GetFields())
                {
                    field.SetValue(this, field.GetValue(other));
                }

                UpdateShardDict();
            }

            void UpdateShardDict()
            {
                shardDict = new();
                foreach(var shard in shards)
                {
                    shardDict.Add(shard.alias, shard);
                }
            }

            public void SetMixer(string alias, float value)
            {
                shardDict[alias].volume = value;
            }
        }

        public Mixer mixer;



    }
}
