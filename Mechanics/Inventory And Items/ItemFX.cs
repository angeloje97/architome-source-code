using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class ItemFX : ScriptableObject
    {
        public List<Effect> effects;

        private void OnValidate()
        {
            foreach (var effect in effects)
            {
                if (effect.volume == 0)
                {
                    effect.volume = 1f;
                }
            }
        }

        [Serializable]
        public class Effect
        {
            public ItemEvent trigger;

            public AudioClip audioClip;
            public List<AudioClip> randomAudioClips;
            public float volume = 1f;

        }
    }
}
