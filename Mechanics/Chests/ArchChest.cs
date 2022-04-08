using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.Events;
using Architome.Enums;



namespace Architome
{
    [RequireComponent(typeof(WorkInfo))]
    public class ArchChest : MonoBehaviour
    {
        [Serializable]
        public struct Info
        {
            [Header("Chest Properties")]
            public int level;
            public Rarity rarity;
            [Range(0, 3)]
            public int stars;
            public List<Item> items;
            public int maxChestSlots;

            [Header("Interactions")]
            public EntityInfo entityOpened;
            public WorkInfo station;
           
        }

        [Serializable]
        public struct Effects
        {
            public bool noLootTurnsOffEffects;

            [Header("Sound Effects")]
            public AudioClip onOpenSound;
            public AudioClip whileOpenSound;
            public AudioClip onCloseSound;
            public AudioClip whileClosedSound;


            [Header("Particle Effects")]
            public ParticleSystem onOpenParticle;
            public ParticleSystem whileOpenParticle;
            public ParticleSystem onCloseParticle;
            public ParticleSystem whileClosedParticle;


        }

        public Info info;
        public Effects effects;

        public struct Events
        {
            public Action<ArchChest> OnOpen;
            public Action<ArchChest> OnClose;
        }

        public Events events;

        public UnityEvent OnOpen;
        public UnityEvent OnClose;

        void GetDependencies()
        {
            info.station = GetComponent<WorkInfo>();
        }
        void Start()
        {
            GetDependencies();
        }
        public void Open()
        {

            if (!FindEntityThatOpened())
            {
                return;
            }

            events.OnOpen?.Invoke(this);
            OnOpen?.Invoke();
            ChestRoutine();

        }

        bool FindEntityThatOpened()
        {
            
            var entity = Entity.EntitiesWithinRange(transform.position, 5f).Find(entity => entity.Movement() && entity.Movement().Target() == transform);

            Debugger.InConsole(34589, $"{entity} opened {this}");
            if (entity == null) return false;

            info.entityOpened = entity;

            return true;
        }

        async void ChestRoutine()
        {
            await EntityBrowsing();
            Close();
        }

        public async Task EntityBrowsing()
        {
            if (info.entityOpened == null) return;
            if (info.entityOpened.Movement() == null) return;
            if (info.station == null) return;

            while (info.station.IsOfWorkStation(info.entityOpened.Target()))
            {
                await Task.Yield();
            }

        }

        public void Close()
        {
            events.OnClose?.Invoke(this);
            OnClose?.Invoke();
            info.entityOpened = null;
        }
    }
}