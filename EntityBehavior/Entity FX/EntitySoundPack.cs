using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Entity Sound Pack", menuName = "Entity Sound Pack")]
public class EntitySoundPack : ScriptableObject
{
    // Start is called before the first frame update
    public AudioClip hurtSound;
    public AudioClip footStepSound;
}
