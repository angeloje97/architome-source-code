using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Architome
{
    public class AbilityPipeLine : MonoBehaviour
    {
        // Start is called before the first frame update
        public AbilityInfo currentAbility;
        public List<AbilityInfo> abilityQueue;
        public bool isActive;
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }


        async public void CastAbility(AbilityInfo ability)
        {
            if (isActive) return;

            isActive = true;
            currentAbility = ability;

            await HandleCasting();

            await HandleChanneling();

            isActive = false;

        }

        async Task HandleCasting()
        {
            var timer = 0f;
            var time = currentAbility.castTime;

            while (timer < time)
            {
                await Task.Yield();
                timer += Time.deltaTime;
            }
        }

        async Task HandleChanneling()
        {
            var timer = currentAbility.channel.time;

            while (timer > 0)
            {
                await Task.Yield();
                timer -= Time.deltaTime;
            }

        }

        public void DeactivateAbility()
        {
            isActive = false;
            currentAbility = null;
        }
    }

}
