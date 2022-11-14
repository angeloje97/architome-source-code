using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
public class EntityAnimationEvents : MonoBehaviour
{
    // Start is called before the first frame update
    Animator anim;

    public AbilityInfo ability;
    public bool abilityIsActive;

    public struct Events
    {
        public Action OnHit;
        public Action OnShoot;
        public Action OnFootR;
        public Action OnFootL;
        public Action OnWeaponSwitch;
        public Action OnDeath;
        public Action OnLand;
    }

    public Events events;

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
        Debugger.Combat(6759, $"Hit Event Invoked");
        events.OnHit?.Invoke();
    }

    public void Shoot()
    {
        events.OnShoot?.Invoke();
    }

    public void FootR()
    {
        events.OnFootR?.Invoke();
    }

    public void FootL()
    {

        events.OnFootL?.Invoke();
    }

    public void Land()
    {
        events.OnLand?.Invoke();
    }

    public void WeaponSwitch()
    {
        events.OnWeaponSwitch?.Invoke();
    }

    public void EndCast()
    {
        if(ability == null) { return; }

        ability.EndCast();

        ability = null;
    }

    public void Death()
    {
        events.OnDeath?.Invoke();
    }

    public void SetAbility(AbilityInfo ability)
    {
        this.ability = ability;
        abilityIsActive = true;
    }



}
