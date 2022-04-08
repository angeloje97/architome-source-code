using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class CatalystAudio : MonoBehaviour
    {
        // Start is called before the first frame update
        public CatalystInfo catalystInfo;
        public AudioManager audioManager;
        bool isActive;
        bool destroyCatalyst;

        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (!isActive) { return; }
            FollowCatalyst();
        }

        public void FollowCatalyst()
        {
            if (catalystInfo == null)
            {
                isActive = false;
                return;
            }
            transform.position = catalystInfo.transform.position;
        }

        public void Activate(CatalystInfo catalystInfo)
        {
            this.catalystInfo = catalystInfo;
            transform.position = catalystInfo.transform.position;
            name = $"{catalystInfo} Audio";


            catalystInfo.OnCatalingRelease += OnCatalingRelease;
            catalystInfo.OnCatalystDestroy += OnCatalystDestroy;
            catalystInfo.OnHeal += OnHeal;
            catalystInfo.OnDamage += OnDamage;
            catalystInfo.OnAssist += OnAssist;

            audioManager = gameObject.AddComponent<AudioManager>();
            audioManager.mixerGroup = GMHelper.Mixer().SoundEffect;
            audioManager.OnEmptyAudio += OnEmptyAudio;

            PlayCatalystReleaseSound();

            isActive = true;
        }

        public void PlayCatalystReleaseSound()
        {
            audioManager.PlayRandomSound(catalystInfo.effects.castReleaseSounds);
        }

        public void OnCatalingRelease(CatalystInfo sourceCatalyst, CatalystInfo cataling)
        {
            //audioManager.PlayRandomSound(cataling.effects.castReleaseSounds);
        }

        public void OnDamage(GameObject target)
        {
            audioManager.PlayRandomSound(catalystInfo.effects.harmSounds);
        }

        public void OnHeal(GameObject target)
        {

            audioManager.PlayRandomSound(catalystInfo.effects.healSounds);
        }

        public void OnAssist(GameObject target)
        {

            audioManager.PlayRandomSound(catalystInfo.effects.assistSounds);
        }

        public void OnHit(GameObject target)
        {

        }

        public void OnCatalystDestroy(CatalystDeathCondition deathCondition)
        {
            destroyCatalyst = true;
            isActive = false;

            PlayDestroySound();

            catalystInfo.OnCatalystDestroy -= OnCatalystDestroy;
            catalystInfo.OnCatalingRelease -= OnCatalingRelease;
            catalystInfo.OnHeal -= OnHeal;
            catalystInfo.OnDamage -= OnDamage;
            catalystInfo.OnAssist -= OnAssist;

            HandleDestroy();

            void PlayDestroySound()
            {
                if (catalystInfo.effects.destroySounds.Count == 0) return;

                audioManager.PlayRandomSound(catalystInfo.effects.destroySounds);
            }
        }

        public void OnEmptyAudio(AudioManager audioManager)
        {
            HandleDestroy();
        }

        public void HandleDestroy()
        {
            if (!destroyCatalyst || audioManager.audioRoutineIsActive) { return; }


            Destroy(gameObject);
        }

    }
}

