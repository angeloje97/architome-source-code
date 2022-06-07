using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Architome
{
    public class CharacterEquipmentHandler : EntityProp
    {
        // Start is called before the first frame update
        CharacterInfo character;
        ArchitomeCharacter archiChar;

        new void GetDependencies()
        {
            base.GetDependencies();

            character = GetComponent<CharacterInfo>();
            archiChar = GetComponentInChildren<ArchitomeCharacter>();

            if (character)
            {
                character.OnChangeEquipment += OnChangeEquipment;
            }

        }
        void Start()
        {
            GetDependencies();
        }

        // Update is called once per frame

        public void OnChangeEquipment(EquipmentSlot slot, Equipment previous, Equipment after)
        {
            ArchAction.Yield(() => {
                if (entityInfo == null) return;
                entityInfo.UpdateCurrentStats(); });
            UpdateModel();
        }

        public void UpdateModel()
        {
            if (archiChar == null) return;
            var originalValues = archiChar.originalParts;
            var equipments = GetComponentsInChildren<EquipmentSlot>().Where(slot => slot.equipment != null).Select(slot => slot.equipment).ToList();


            //OriginalValues
            foreach (Vector2 current in originalValues)
            {
                archiChar.SetPart((int)current.x, (int)current.y);
            }

            foreach (var equipment in equipments)
            {
                var values = equipment.equipmentOverRide;

                foreach (var value in values)
                {
                    archiChar.SetPart((int)value.x, (int)value.y);
                }
            }
        }
    }

}