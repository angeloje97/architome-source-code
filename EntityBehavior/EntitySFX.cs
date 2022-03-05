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
            abilityManager.OnCatalystRelease += OnCatalystRelease;
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

    public void OnCatalystRelease(AbilityInfo ability)
    {
        //var catalyst = ability.catalystInfo;

        //if (catalyst.effects != null && catalyst.effects.castReleaseSounds != null)
        //{
        //    var sounds = catalyst.effects.castReleaseSounds;

        //    PlayRandomSound(sounds);
        //}
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
