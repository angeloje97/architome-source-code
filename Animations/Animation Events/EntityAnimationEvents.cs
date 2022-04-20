using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class EntityAnimationEvents : MonoBehaviour
{
    // Start is called before the first frame update
    Animator anim;

    public AbilityInfo ability;
    public bool abilityIsActive;

    public void GetDependencies()
    {
        if(GetComponent<Animator>())
        {
            anim = GetComponent<Animator>();
        }

    }
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Hit()
    {

    }

    public void Shoot()
    {

    }

    public void FootR()
    {
    }

    public void FootL()
    {
    }

    public void Land()
    {
    }

    public void WeaponSwitch()
    {
    }

    public void EndCast()
    {
        if(ability == null) { return; }

        ability.EndCast();

        ability = null;
    }

    public void Death()
    {

    }

    public void SetAbility(AbilityInfo ability)
    {
        this.ability = ability;
        abilityIsActive = true;
    }



}
