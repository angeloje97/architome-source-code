using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatalystAudio : MonoBehaviour
{
    // Start is called before the first frame update
    public CatalystInfo catalystInfo;
    public AudioManager audioManager;
    bool isActive;
    bool destroyCatalyst;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive) { return; }
        FollowCatalyst();
    }

    public void FollowCatalyst()
    {
        if(catalystInfo == null )
        {
            isActive = false;
            return;
        }
        transform.position = catalystInfo.transform.position;
    }

    public void Activate(CatalystInfo catalystInfo)
    {
        this.catalystInfo = catalystInfo;
        name = $"{catalystInfo} Audio";
        catalystInfo.OnCatalingRelease += OnCatalingRelease;
        catalystInfo.OnCatalystDestroy += OnCatalystDestroy;

        audioManager = gameObject.AddComponent<AudioManager>();
        audioManager.mixerGroup = GMHelper.Mixer().SoundEffect;
        audioManager.OnEmptyAudio += OnEmptyAudio;

        PlayCatalystReleaseSound();

        isActive = true;
    }

    public void PlayCatalystReleaseSound()
    {
        audioManager.PlayRandomSound(catalystInfo.effects.castReleaseSounds);
    }

    public void OnCatalingRelease(CatalystInfo sourceCatalyst, CatalystInfo cataling)
    {
        //audioManager.PlayRandomSound(cataling.effects.castReleaseSounds);
    }

    public void OnCatalystDestroy(CatalystDeathCondition deathCondition)
    {
        destroyCatalyst = true;
        isActive = false;

        catalystInfo.OnCatalystDestroy -= OnCatalystDestroy;
        catalystInfo.OnCatalingRelease -= OnCatalingRelease;

        HandleDestroy();
    }

    public void OnEmptyAudio(AudioManager audioManager)
    {
        HandleDestroy();
    }

    public void HandleDestroy()
    {
        if(!destroyCatalyst || audioManager.audioRoutineIsActive) { return; }

        
        Destroy(gameObject);
    }

}
