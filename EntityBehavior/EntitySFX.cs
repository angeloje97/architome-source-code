using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;

public class EntitySFX : MonoBehaviour
{
    // Start is called before the first frame update
    public EntityInfo entityInfo;
    public AbilityManager abilityManager;
    public AudioManager soundEffects;

    public EntitySoundPack entitySoundPack;

    public struct SFXAudioSources
    {
        public AudioSource castingAudioSource;
        public AudioSource channelingAudioSource;
    }

    public SFXAudioSources sources;

    void GetDependencies()
    {
        if(GetComponentInParent<EntityInfo>())
        {
            entityInfo = GetComponentInParent<EntityInfo>();

            abilityManager = entityInfo.AbilityManager();
            soundEffects = GetComponent<AudioManager>();

            entityInfo.OnDamageTaken += OnDamageTaken;
            entityInfo.OnBuffApply += OnBuffyApply;

            abilityManager.OnCastRelease += OnCastRelease;

            abilityManager.OnCastStart += OnCastStart;
            abilityManager.OnCatalystRelease += OnCastEnd;
            abilityManager.OnCancelCast += OnCastEnd;
            
        }
    }

    void Start()
    {
        GetDependencies();
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDamageTaken(CombatEventData eventData)
    {

        var buff = eventData.buff;
        var catalyst = eventData.catalyst;

        if(catalyst)
        {
            var sounds = catalyst.effects.harmSounds;
            PlayRandomSound(sounds);
        }


    }

    public void OnCastRelease(AbilityInfo ability)
    {
        
    }
    

    public void OnCastStart(AbilityInfo ability)
    {
        if(ability.catalystInfo == null) { return; }
        var catalyst = ability.catalystInfo;
        if(catalyst.effects.castingSounds.Count == 0)
        {
            return;
        }

        
        var randomSound = catalyst.effects.castingSounds[Random.Range(0, catalyst.effects.castingSounds.Count)];

        sources.castingAudioSource = soundEffects.PlaySoundLoop(randomSound);


    }

    public void OnCastEnd(AbilityInfo ability)
    {
        if (sources.castingAudioSource == null) return;

        sources.castingAudioSource.Stop();

        sources.castingAudioSource = null;
    }

    public void OnBuffyApply(BuffInfo buff, EntityInfo source)
    {
        if(buff.buffSound)
        {
            soundEffects.PlaySound(buff.buffSound);
        }
    }

    public void PlayRandomSound(List<AudioClip> sounds)
    {
        if(sounds.Count == 0) { return; }
        soundEffects.PlaySound(sounds[Random.Range(0, sounds.Count)]);
    }

    
}
