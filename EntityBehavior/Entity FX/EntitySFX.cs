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
    public AudioManager voice;

    public AudioSource castingSource;

    void GetDependencies()
    {
        entityInfo = GetComponentInParent<EntityInfo>();

        if (entityInfo)
        {
            abilityManager = entityInfo.AbilityManager();
            soundEffects = GetComponent<AudioManager>();

            entityInfo.OnDamageTaken += OnDamageTaken;
            entityInfo.OnBuffApply += OnBuffyApply;

            abilityManager.OnCastStart += OnCastStart;
            abilityManager.OnCastRelease += OnCastEnd;

            abilityManager.OnAbilityStart += OnAbilityStart;
            abilityManager.OnAbilityEnd += OnAbilityEnd;
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
    public void OnLevelUp()
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


    void OnCastStart(AbilityInfo ability)
    {
        if (ability.catalystInfo == null) { return; }
        var catalyst = ability.catalystInfo;

        soundEffects.PlayRandomSound(catalyst.effects.startCastSounds);

        PlayCastingLoop();

        void PlayCastingLoop()
        {
            if (catalyst.effects.castingSounds.Count == 0)
            {
                return;
            }

            var randomSound = catalyst.effects.castingSounds[Random.Range(0, catalyst.effects.castingSounds.Count)];

            castingSource = soundEffects.PlaySoundLoop(randomSound, catalyst.castTime);
        }
    }


    void OnCastEnd(AbilityInfo ability)
    {
        if (castingSource == null) return;

        castingSource.Stop();

        foreach (var clip in ability.catalystInfo.effects.castingSounds)
        {
            if (soundEffects.AudioSourceFromClip(clip))
            {
                soundEffects.AudioSourceFromClip(clip).Stop();
            }
        }

        castingSource = null;
        if (ability.catalystInfo == null) return;
        
    }
    

    public void OnAbilityStart(AbilityInfo ability)
    {
        
    }


    public void OnAbilityEnd(AbilityInfo ability)
    {
        
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
