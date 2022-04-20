using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class WeaponObject : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField] EquipmentSlot slot;

        public bool savePosition;
        public bool toggleSheath;
        void Start()
        {
            var collider = GetComponent<Collider>();

            if (collider)
            {
                collider.enabled = false;
            }
        }

        // Update is called once per frame
        void Update()
        {

            if (!slot) return;

            HandleSave();
            HandleSheath();
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