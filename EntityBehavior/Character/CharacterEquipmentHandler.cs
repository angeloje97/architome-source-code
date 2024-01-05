using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace Architome
{
    public class CharacterEquipmentHandler : EntityProp
    {
        // Start is called before the first frame update
        CharacterInfo character;
        ArchitomeCharacter archiChar;

        public float startingTimer;

        public override void GetDependencies()
        {

            character = GetComponent<CharacterInfo>();
            archiChar = GetComponentInChildren<ArchitomeCharacter>();

            if (character)
            {
                character.OnChangeEquipment += OnChangeEquipment;
            }

        }

        public override void  EUpdate()
        {
            if (startingTimer > 0)
            {
                startingTimer -= Time.deltaTime;
            }

            if (startingTimer < 0)
            {
                startingTimer = 0;
            }
        }

        public void OnChangeEquipment(EquipmentSlot slot, Equipment previous, Equipment after)
        {
            ArchAction.Yield(() => {
                if (entityInfo == null) return;
                entityInfo.UpdateCurrentStats(); });
            UpdateModel();

            if (startingTimer > 0)
            {
                entityInfo.RestoreFull();
            }
        }

        public void UpdateModel()
        {
            if (archiChar == null) return;
            var originalValues = archiChar.originalParts;
            var equipments = GetComponentsInChildren<EquipmentSlot>().Where(slot => slot.equipment != null).Select(slot => slot.equipment).ToList();

            var materialValue = archiChar.currentMaterial;

            //OriginalValues
            foreach (Vector2 current in originalValues)
            {
                archiChar.SetPart((int)current.x, (int)current.y, materialValue);
            }

            foreach (var equipment in equipments)
            {
                if (!Item.IsEquipment(equipment)) continue;
                var values = equipment.equipmentOverRide;

                foreach (var value in values)
                {
                    archiChar.SetPart((int)value.x, (int)value.y, (int) value.z);
                }
            }
        }
    }

}