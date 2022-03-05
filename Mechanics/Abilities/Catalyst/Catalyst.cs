using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

[CreateAssetMenu(fileName = "New Catalyst", menuName = "Catalyst")]
public class Catalyst : Item
{
    public new string name;
    public float range;
    public float castTime;
    public float speed;
    public CatalystType catalystType;
    public ParticleSystem tail;
    public ParticleSystem hit;
}
