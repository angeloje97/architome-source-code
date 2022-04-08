using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

namespace Architome
{
    public class CombatMeterManager : MonoBehaviour
    {
        // Start is called before the first frame update
        public GameObject combatMeterPrefab;

        public List<CombatMeter> combatMeters;

        public float highestValue;
        public bool isUpdating;

        void Start()
        {
            GameManager.active.OnNewPlayableEntity += OnNewPlayableEntity;
            GameManager.active.OnNewPlayableParty += OnNewPlayableParty;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnNewPlayableEntity(EntityInfo entity, int index)
        {
            if (combatMeterPrefab == null) return;
            if (entity == null) return;

            var combatMeter = Instantiate(combatMeterPrefab, transform).GetComponent<CombatMeter>();

            if (combatMeter != null)
            {
                combatMeters.Add(combatMeter);
                combatMeter.SetEntity(entity);
            }
        }

        public void OnNewPlayableParty(PartyInfo party, int index)
        {
            if (index != 0) return;
            party.events.OnCombatChange += OnCombatChange;
        }

        async public void UpdateMeters(bool ignoreActiveMeters = false)
        {
            if (isUpdating)
            {
                return;
            }

            isUpdating = true;
            if (ignoreActiveMeters)
            {
                combatMeters = combatMeters.OrderBy(meter => meter.value).ToList();
            }
            else
            {
                combatMeters = combatMeters.OrderBy(meter => meter.value).ThenBy(meter => meter.isActive).ToList();
            }

            highestValue = combatMeters.Last().value;

            await Task.Delay(250);

            foreach (var meter in combatMeters)
            {
                meter.transform.SetAsFirstSibling();
            }


            isUpdating = false;
        }

        public void OnCombatChange(bool isInCombat)
        {
            if (!isInCombat)
            {
                ArchAction.Delay(() => UpdateMeters(true), .5f);
                return;
            }
            foreach (var meter in combatMeters)
            {
                if (meter.entity.isInCombat) continue;
                meter.ResetMeter();
            }
        }
    }

}