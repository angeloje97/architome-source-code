using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Architome
{
    public class ParticleManager : MonoBehaviour
    {
        // Start is called before the first frame update

        public async void PlayOnce(GameObject particle, float time = 0f)
        {
            if (particle.GetComponent<ParticleSystem>() == null) return;
            var pSystem = Instantiate(particle, transform).GetComponent<ParticleSystem>();

            pSystem.Play(true);

            var duration = time > 0 ? time : pSystem.main.duration;

            await Task.Delay((int)(duration) * 1000);

            pSystem.Stop();

            await Task.Delay(1000);

            Destroy(pSystem.gameObject);
        }



        public async void PlayFor(GameObject particle, int amount, float liveTime = 0, float interval = 0)
        {
            if (particle.GetComponent<ParticleSystem>() == null) return;

            var pSystem = Instantiate(particle, transform).GetComponent<ParticleSystem>();

            float duration = liveTime > 0 ? liveTime : pSystem.main.duration;
            float timeBetween = interval > 0 ? interval : 1;

            while (amount > 0)
            {
                pSystem.Play();
                amount--;
                await Task.Delay((int)(duration * 1000));

                pSystem.Stop();

                await Task.Delay((int)(timeBetween * 1000));
            }

            Destroy(pSystem.gameObject);
        }

        public async void PlayOnceAt(GameObject particle, Transform target = null, float time = 0f)
        {
            if (particle.GetComponent<ParticleSystem>() == null) return;
            var currentTarget = target != null ? target : transform;
            var pSystem = Instantiate(particle, transform).GetComponent<ParticleSystem>();



            var duration = time > 0 ? time : pSystem.main.duration;

            while (duration > 0)
            {
                await Task.Yield();
                duration -= Time.deltaTime;
                pSystem.transform.position = currentTarget.position;
            }

            pSystem.Stop();

            await Task.Delay(1000);

            Destroy(pSystem.gameObject);

        }

        public async void PlayOnceAt(GameObject particleObj, Vector3 position, Vector3 rotation = new(), float time = 0f)
        {
            if (particleObj.GetComponent<ParticleSystem>() == null) return;

            var particle = Instantiate(particleObj, position, Quaternion.Euler(rotation));

            var pSystem = particle.GetComponent<ParticleSystem>();

            var maxDuration = pSystem.main.duration;

            pSystem.Play(true);

            await Task.Delay((int)(maxDuration * 1000));

            pSystem.Stop(true);

            await Task.Delay(1000);

            Destroy(particle);
        }

        public async void PlayOnceAt(GameObject particleObj, Vector3 position, Quaternion rotation = new(), float time = 0f)
        {
            if (particleObj.GetComponent<ParticleSystem>() == null) return;

            var particle = Instantiate(particleObj, position, rotation);

            var pSystem = particle.GetComponent<ParticleSystem>();

            var maxDuration = pSystem.main.duration;

            pSystem.Play(true);

            await Task.Delay((int)(maxDuration * 1000));

            pSystem.Stop(true);

            await Task.Delay(1000);

            Destroy(particle.gameObject);
        }

        public (ParticleSystem, GameObject) Play(GameObject particle, bool autoStop = false)
        {
            if (particle.GetComponentInChildren<ParticleSystem>() == null) return (null, null);
            

            var particleObject = Instantiate(particle, transform);


            var system = particleObject.GetComponentInChildren<ParticleSystem>();
            system.Play(true);

            if (autoStop)
            {
                ArchAction.Delay(() => { system.Stop(true); }, system.main.duration);

                ArchAction.Delay(() => { Destroy(particleObject); }, system.main.duration + 2f);
            }


            return (system, particleObject);

        }

        public void ManifestRadius(GameObject particleObj, float radius)
        {
            var particleSystems = particleObj.GetComponentsInChildren<ParticleSystem>();

            foreach (var system in particleSystems)
            {
                if (system.shape.enabled)
                {
                    var shape = system.shape;

                    shape.radius = radius;
                }
            }
        }


    }

}