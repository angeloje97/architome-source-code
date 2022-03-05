using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Stats
{
    // Start is called before the first frame update
    public int Level;
    public int Vitality;
    public int Strength;
    public int Dexterity;
    public int Wisdom;

    public float attackSpeed;
    public float attackDamage;
    public float coolDownReduction;
    public float criticalStrikeChance;
    public float criticalDamage;
    public float damageReduction;
    public float magicResist;
    public float armor;
    public float manaRegen;
    public float healthRegen;
    public float outOfCombatRegenMultiplier;
    public float healingReceivedMultiplier;
    public float damageMultiplier;
    public float damageTakenMultiplier;
    public float movementSpeed;
    public float haste;

    public float experience;
    public float experienceReq;

    public void ZeroOut()
    {
        Vitality = 0;
        Strength = 0;
        Dexterity = 0;
        Wisdom = 0;

        attackSpeed = 0;
        attackDamage = 0;
        coolDownReduction = 0;
        criticalStrikeChance = 0;
        criticalDamage = 0;
        damageReduction = 0;
        magicResist = 0;
        armor = 0;
        manaRegen = 0;
        healthRegen = 0;
        outOfCombatRegenMultiplier = 0;
        healingReceivedMultiplier = 0;
        damageMultiplier = 0;
        damageTakenMultiplier = 0;
        movementSpeed = 0;
        haste = 0;
    }
    public Stats Sum(Stats s1, Stats s2)
    {
        Stats s3 = new Stats();


        s3.Level = s1.Level + s2.Level;
        s3.Vitality = s1.Vitality + s2.Vitality;
        s3.Strength = s1.Strength + s2.Strength;
        s3.Dexterity = s1.Dexterity + s2.Dexterity;
        s3.Wisdom = s1.Wisdom + s2.Wisdom;
        
        s3.attackSpeed = s1.attackSpeed + s2.attackSpeed;
        s3.attackDamage = s1.attackDamage + s2.attackDamage;
        s3.coolDownReduction = s1.coolDownReduction + s2.coolDownReduction;
        s3.criticalStrikeChance = s1.criticalStrikeChance + s2.criticalStrikeChance;
        s3.criticalDamage = s1.criticalDamage + s2.criticalDamage;
        s3.damageReduction = s1.damageReduction + s2.damageReduction;
        s3.magicResist = s1.magicResist + s2.magicResist;
        s3.armor = s1.armor + s2.armor;
        s3.manaRegen = s1.manaRegen + s2.manaRegen;
        s3.healthRegen = s1.healthRegen + s2.manaRegen;
        s3.outOfCombatRegenMultiplier = s1.outOfCombatRegenMultiplier + s2.outOfCombatRegenMultiplier;
        s3.healingReceivedMultiplier = s1.healingReceivedMultiplier + s2.healingReceivedMultiplier;
        s3.damageMultiplier = s1.damageMultiplier + s2.damageMultiplier;
        s3.damageTakenMultiplier = s1.damageTakenMultiplier + s2.damageTakenMultiplier;
        s3.movementSpeed = s1.movementSpeed + s2.movementSpeed;
        s3.haste = s1.haste + s2.haste;

        return s3;
    }

    public void UpdateCoreStats()
    {
        Vitality = (Level * 5) + 5;
        Strength = (Level * 5) + 5;
        Dexterity = (Level * 5) + 5;
        Wisdom = (Level * 5) + 5;
    }

    public Stats UpdateRequiredToLevel()
    {
        var experienceMultiplier = GMHelper.Difficulty().settings.experienceMultiplier;

        experienceReq = Level * experienceMultiplier;

        return this;
    }
}
