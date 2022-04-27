using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class WeaponObject : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField] EquipmentSlot slot;
        [SerializeField] ParticleSystem particles;

        public bool savePosition;
        public bool toggleSheath;
        void Start()
        {
            var collider = GetComponent<Collider>();

            if (collider)
            {
                collider.enabled = false;
            }

            particles = GetComponentInChildren<ParticleSystem>();

            if (particles)
            {
                var entity = slot.entityInfo;

                
                var abilityManager = entity.AbilityManager();

                abilityManager.OnAbilityStart += OnAbilityStart;
                abilityManager.OnAbilityEnd += OnAbilityEnd;
                abilityManager.OnCatalystRelease += OnCatalystRelease;
            }
        }

        // Update is called once per frame
        void Update()
        {

            if (!slot) return;

            HandleSave();
            HandleSheath();
        }

        void OnAbilityStart(AbilityInfo ability)
        {
            if (!ability.vfx.activateWeaponParticles) return;
            ArchAction.Delay(() => particles.Play(true), .125f);


        }

        void OnCatalystRelease(AbilityInfo ability, CatalystInfo catalyst)
        {
            if (!ability.vfx.activateWeaponParticles) return;

            particles.Stop(true);
        }
        void OnAbilityEnd(AbilityInfo ability)
        {
            if (!ability.vfx.activateWeaponParticles) return;
            particles.Stop(true);
        }

        void HandleSheath()
        {
            if (!toggleSheath) return;
            toggleSheath = false;
            slot.toggleSheath = true;
        }

        void HandleSave()
        {
            if (!savePosition) return;
            savePosition = false;
            slot.savePosition = true;

        }

        public void SetWeapon(EquipmentSlot slot)
        {
            this.slot = slot;
        }
    }

}