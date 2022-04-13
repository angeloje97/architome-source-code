using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New EntityFX Pack", menuName = "EntityFX")]
public class EntityFXPack : ScriptableObject
{
    // Start is called before the first frame update
    [Header("Audio")]
    public List<AudioClip> detectedPlayerSoud;
    public List<AudioClip> hurtSound;
    public List<AudioClip> levelUpSounds;
    public AudioClip footStepSound;

    [Header("Particle Effects")]
    public List<GameObject> leveUpParticles;
    public List<GameObject> reviveParticles;

    [Header("Phrases")]
    public List<string> deathPhrases;
    public List<string> detectedPlayerPhrases;

}
