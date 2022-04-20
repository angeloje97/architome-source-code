using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
public class ParticleManager : MonoBehaviour
{
    // Start is called before the first frame update

    public async void PlayOnce(GameObject particle,  float time = 0f)
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

            await Task.Delay((int) (timeBetween * 1000));
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

    public ParticleSystem PlayParticle(GameObject particle)
    {
        if (particle.GetComponent<ParticleSystem>() == null) return null;

        var particleObject = Instantiate(particle, transform);


        var system = particleObject.GetComponent<ParticleSystem>();
        system.Play(true);

        return system;

    }

    
}
