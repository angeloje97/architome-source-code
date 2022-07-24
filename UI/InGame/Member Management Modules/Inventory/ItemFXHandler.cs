using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class ItemFXHandler : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField] AudioManager audioManager;
        [SerializeField] ParticleManager particleManager;

        public ItemInfo worldItemInfo;

        ItemInfo currentItemInfo;


        private void Start()
        {
            GetDependencies();
        }

        
        void GetDependencies()
        {
            if (!worldItemInfo)
            {
                worldItemInfo = GetComponent<ItemInfo>();
            }
            if (!worldItemInfo) return;
            if (worldItemInfo.isUI) return;


            HandleStart();
            worldItemInfo.OnPickUp += OnItemPickedUp;
        }

        async void HandleStart()
        {
            while (worldItemInfo.item == null)
            {
                await Task.Yield();
            }

            HandleItemFX(worldItemInfo, ItemEvent.OnDrop);
        }

        void OnItemPickedUp(ItemInfo info, EntityInfo entityPickedUp)
        {
            var soundEffect = entityPickedUp.SoundEffect();
            if (soundEffect == null) return;

            audioManager = soundEffect;

            HandleItemFX(info, ItemEvent.OnPickUp);
        }
        

        public void HandleItemFX(ItemInfo info, ItemEvent trigger)
        {
            //Debugger.UI(1953, $"Item: {info} event: {trigger}");
            if (info.item == null) return;
            var itemEffect = info.item.effects;
            if (itemEffect == null) return;
            if (itemEffect.effects == null) return;
            Debugger.UI(1953, $"Item: {info} event: {trigger}");


            foreach (var effect in itemEffect.effects)
            {
                if (effect.trigger != trigger) continue;
                HandleAudio(effect);
            }
            
        }

        void HandleAudio(ItemFX.Effect fx)
        {
            if (audioManager == null) return;
            HandleMainAudio();
            HandleRandomAudio();

            void HandleMainAudio()
            {
                if (fx.audioClip == null) return;

                audioManager.PlaySound(fx.audioClip, fx.volume);
            }

            void HandleRandomAudio()
            {
                if (fx.randomAudioClips == null || fx.randomAudioClips.Count == 0) return;

                var randomClip = ArchGeneric.RandomItem(fx.randomAudioClips);

                audioManager.PlaySound(randomClip, fx.volume);
            }
        }


        public void Reveal()
        {
            if (audioManager == null) return;
            var world = World.active;
            if (world == null) return;
            var rarity = world.RarityProperty(worldItemInfo.item.rarity);

            if (rarity.revealAudio)
            {
                audioManager.PlaySound(rarity.revealAudio);
            }

            if (rarity.lingerSound)
            {
                var audioSource = audioManager.PlaySoundLoop(rarity.lingerSound);

                
            }

        }
    }
}
