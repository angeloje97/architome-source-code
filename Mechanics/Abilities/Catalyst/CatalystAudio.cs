using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Architome.Enums;
using System.Threading.Tasks;

namespace Architome
{
    [RequireComponent(typeof(AudioManager))]
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

        private void OnValidate()
        {
            audioManager = GetComponent<AudioManager>();
        }
        // Update is called once per frame

        public void Activate(CatalystInfo catalystInfo)
        {
            this.catalystInfo = catalystInfo;
            catalystInfo.catalystAudio = audioManager;
            transform.position = catalystInfo.transform.position;
            name = $"{catalystInfo} Audio";


            catalystInfo.OnCatalystDestroy += OnCatalystDestroy;

            audioManager.OnEmptyAudio += OnEmptyAudio;


            transform.SetParent(catalystInfo.transform);

            isActive = true;
        }


        public async void OnCatalystDestroy(CatalystDeathCondition deathCondition)
        {
            destroyCatalyst = true;
            isActive = false;


            transform.SetParent(CatalystManager.active.transform);
            catalystInfo.OnCatalystDestroy -= OnCatalystDestroy;

            await StopLoops();
            HandleDestroy();

            //ArchAction.Delay(() => {
            //    //destroyCatalyst = true;
            //    //isActive = false;

            //    StopLoops();
            //    //transform.SetParent(CatalystManager.active.transform);

            //    HandleDestroy();
            
            //}, .25f);
        }

        public async Task StopLoops()
        {
            var effects = catalystInfo.effects.catalystsEffects;

            foreach (var effect in effects)
            {
                if (!effect.loops) continue;
                if (!effect.audioClip) continue;

                var source = audioManager.AudioSourceFromClip(effect.audioClip);

                if (source) source.Stop();
            }

            await audioManager.UntilAllDone();
        }

        public void OnEmptyAudio(AudioManager audioManager)
        {
            HandleDestroy();
        }

        public void HandleDestroy()
        {
            if (!destroyCatalyst || audioManager.audioRoutineIsActive) { return; }
            if (this == null) return;


            Destroy(gameObject);
        }

    }
}

