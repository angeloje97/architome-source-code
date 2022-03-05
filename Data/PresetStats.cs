using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

[CreateAssetMenu(fileName = "New Preset Stats", menuName = "Preset Stats")]
public class PresetStats : ScriptableObject
{
    public new string name;
    public Role Role;
    public NPCType npcType;
    public bool canLevel;
    [SerializeField]
    private Stats stats;

    public Stats Stats()
    {
        return stats;
    }
}

