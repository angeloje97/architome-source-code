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
        [SerializeField] ItemSlotHandler itemSlotHandler;
        [SerializeField] PopupTextManager popUpManager;

        ItemInfo worldItemInfo;

        ItemInfo currentItemInfo;


        private void Start()
        {
            GetDependenciesWorld();
            GetDependenciesUI();
        }

        void GetDependenciesUI()
        {
            itemSlotHandler = GetComponent<ItemSlotHandler>();
            if (itemSlotHandler == null) return;

            HandleCantInsertIntoSlot();
        }
        void HandleCantInsertIntoSlot()
        {
            var notifications = NotificationManager.active;
            itemSlotHandler.OnCantInsertToSlot += async delegate (InventorySlot slot, ItemInfo item, string reason)
            {
                var notification = await notifications.CreateNotification(new(NotificationType.Warning) {
                    name = item.item.itemName,
                    description = reason,
                    dismissable = true
                });
            };
        }
        void GetDependenciesWorld()
        {
            if (!worldItemInfo)
            {
                worldItemInfo = GetComponent<ItemInfo>();
            }
            if (!worldItemInfo) return;
            if (worldItemInfo.isUI) return;
            popUpManager = PopupTextManager.active;


            HandleStart();
            //worldItemInfo.OnPickUp += OnItemPickedUp;
            HandleItemPickUp(worldItemInfo);

        }


        async void HandleStart()
        {
            while (worldItemInfo.item == null)
            {
                await Task.Yield();
            }

            HandleItemFX(worldItemInfo, ItemEvent.OnDrop);
        }

        void HandleItemPickUp(ItemInfo item)
        {
            var world = World.active;
            var rarityProperty = world.RarityProperty(item.item.rarity);

            item.AddListener(ItemEvent.OnPickUp, (ItemInfoEventData data) =>
            {
                var entityPickedUp = data.entityInteracted;
                var info = data.info;
                var soundEffect = entityPickedUp.SoundEffect();
                if (soundEffect == null) return;

                audioManager = soundEffect;

                HandlePopUpText();
                HandleItemFX(info, ItemEvent.OnPickUp);
            }, this);

            void HandlePopUpText()
            {
                var stacks = item.currentStacks;
                var text = $"{item.item.itemName}";


                if (stacks > 1)
                {
                    text += $" ({stacks})";
                }

                var popUp = popUpManager.GeneralPopUp(transform, text, rarityProperty.color, new()
                {
                    boolean = PopupText.eAnimatorBools.HealthChange
                });

                popUp.SetOffset(new(0, 60, 0));
            }
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

        public void HandleItemFX(ItemInfo info, ItemFX.Effect fx)
        {
            Debugger.UI(7612, $"Playing effect for {info} on {fx.trigger}");
            HandleAudio(fx);
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
