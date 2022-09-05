using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class SystemAudio : MonoBehaviour
    {
        public static SystemAudio active;

        [Serializable]
        public class NotificationEffect
        {
            [HideInInspector] public string name;
            public Notification type;
            public List<AudioClip> audioClips;

            public NotificationEffect(Notification type)
            {
                this.type = type;

                name = type.ToString();
            }
        }

        [SerializeField] List<NotificationEffect> effects;
        [SerializeField] AudioManager audioManager;
        

        Dictionary<Notification, NotificationEffect> effectsMap;

        [Header("Actions")]
        [SerializeField] bool update;
        [SerializeField] bool updateNames;

        private void OnValidate()
        {
            HandleUpdate();
            HandleUpdateNames();
        }
        private void Awake()
        {
            active = this;
            UpdateMaps();
        }


        void HandleUpdate()
        {
            if (!update) return;
            update = false;

            if (effects == null) effects = new();
            foreach (Notification notification in Enum.GetValues(typeof(Notification)))
            {
                bool addEffect = true;

                for (int i = 0; i < effects.Count; i++)
                {
                    if (effects[i].type == notification)
                    {
                        if (!addEffect)
                        {
                            effects.RemoveAt(i);
                            i--;
                            continue;
                        }

                        addEffect = false;
                    }

                }

                if (addEffect)
                {
                    effects.Add(new(notification));
                }
            }
        }

        void HandleUpdateNames()
        {
            if (!updateNames) return;
            updateNames = false;

            if (effects == null) return;
            foreach (var effect in effects)
            {
                effect.name = effect.type.ToString();
            }
        }

        void PlayNotificationSound(Notification type)
        {
            if (audioManager == null) return;
            if (effectsMap == null) return;
            if (!effectsMap.ContainsKey(type)) return;
            var audioClips = effectsMap[type].audioClips;

            if (audioClips == null || audioClips.Count == 0) return;

            var randomClip = ArchGeneric.RandomItem(audioClips);

            audioManager.PlayAudioClip(randomClip);

        }

        void UpdateMaps()
        {
            effectsMap = new();

            foreach (var effect in effects)
            {
                if (effectsMap.ContainsKey(effect.type)) continue;
                effectsMap.Add(effect.type, effect);
            }
        }

        public static void PlayNotification(Notification type)
        {
            if (!active) return;
            active.PlayNotificationSound(type);
        }

    }
}
