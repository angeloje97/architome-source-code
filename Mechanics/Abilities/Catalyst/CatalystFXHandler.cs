using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Architome
{
    public class CatalystFXHandler : CatalystProp
    {
        // Start is called before the first frame update

        new void GetDependencies()
        {
            base.GetDependencies();

            if (catalyst)
            {
                catalyst.OnCatalystDestroy += OnCatalystDestroy;
                catalyst.OnDamage += OnDamage;
                catalyst.OnHeal += OnHeal;
            }
        }
        void Start()
        {
            GetDependencies();

        }
        void OnCatalystDestroy(CatalystDeathCondition deathCondition)
        {
            if (catalyst == null) return;
            HandleCollapse();
            HandleDestroyParticles();

            async void HandleCollapse()
            {
                try
                {
                    if (!catalyst.effects.collapseOnDeath) return;

                    while (transform.localScale != new Vector3())
                    {
                        await Task.Yield();
                        if (gameObject == null) break;
                        transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(), .125f);
                    }
                }
                catch
                {

                }
            }

            async void HandleDestroyParticles()
            {
                var destroyParticles = catalyst.effects.destroyParticle;

                if (destroyParticles == null) return;

                var particles = Instantiate(destroyParticles);
                particles.transform.position = transform.position;

                var particleSystem = particles.GetComponent<ParticleSystem>();
                

                if (particleSystem == null) Destroy(particles);


                particleSystem.Play(true);

                await Task.Delay((int)(particleSystem.main.duration * 1000));

                Destroy(particles.gameObject);


                
            }
        }

        public void OnDamage(GameObject target)
        {

        }

        public void OnHeal(GameObject target)
        {

        }

        // Update is called once per frame
    }

}
