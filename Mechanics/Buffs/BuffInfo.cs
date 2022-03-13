using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System;
using Architome;
using UnityEngine.UI;

[Serializable]
public class BuffProperties
{
    public float value;
    public float aoeValue;
    public float valueContributionToBuff;
    public float valueContributionToBuffAOE;
    public float intervals;
    public float time;
    public float radius;
    public int stacksPerApplication;
    public bool selfBuffOnDestroy;
    public bool canStack;
    public bool reapplyResetsTimer;
    public bool reapplyResetsBuff;
}
public class BuffInfo : MonoBehaviour
{
    // Start is called before the first frame update
    public BuffsManager buffsManager;

    [Header("Source")]
    public GameObject sourceObject;
    public EntityInfo sourceInfo;
    public AbilityInfo sourceAbility;
    public CatalystInfo sourceCatalyst;

    [Header("Host")]
    public GameObject hostObject;
    public EntityInfo hostInfo;

    public BuffTargetType buffTargetType;
    public Sprite buffIcon;
    public int buffId;
    public float expireDelay = .125f;

    public BuffProperties properties;

    [Header("Buff Properties")]
    public DamageType damageType;
    public int stacks = 0;
    private bool buffTimeComplete = false;
    private bool cleansed = false;
    private bool depleted = false;
    public float buffTimer;

    [Header("Buff FX")]
    public Vector3 scalePortions;
    public GameObject buffParticles;
    public GameObject buffRadiusParticles;
    public AudioClip buffSound;


    //Events
    public  Action<BuffInfo> OnBuffCompletion;
    public  Action<BuffInfo> OnBuffCleanse;
    public Action<BuffInfo> OnBuffInterval;
    public Action<BuffInfo> OnBuffDeplete;
    public Action<BuffInfo> OnBuffEnd;
    public Action<BuffInfo, float, float> OnChangeValue;

    public Action<int> OnStack;

    //static variables

    public static List<BuffInfo> buffs;

    public void GetVessel()
    {
        if(GetComponentInParent<BuffsManager>())
        {
            buffsManager = GetComponentInParent<BuffsManager>();
            buffsManager.OnBuffStack += OnBuffStack;
            buffsManager.OnBuffTimerReset += OnBuffTimerReset;
            buffsManager.OnResetBuff += OnResetBuff;
        }
        if (GetComponentInParent<EntityInfo>())
        {
            hostInfo = GetComponentInParent<EntityInfo>();
            hostObject = hostInfo.gameObject;
            hostInfo.OnNewBuff?.Invoke(this, sourceInfo);
        }
        
    }
    public void SpawnParticle()
    {
        var buffRadius = properties.radius;
        if(buffParticles != null)
        {
            Instantiate(buffParticles, transform);
        }

        if(buffRadiusParticles)
        {
            if(scalePortions.x == 0)
            {
                scalePortions.x = buffRadius*2;
            }
            if(scalePortions.y == 0)
            {
                scalePortions.y = buffRadius*2;
            }
            if(scalePortions.z == 0)
            {
                scalePortions.z = buffRadius*2;
            }
            buffRadiusParticles.transform.localScale = scalePortions;
        }
    }

    private void Awake()
    {
        buffTimeComplete = false;
        buffTimer = properties.time;
        GetVessel();
        Invoke("SpawnParticle", .125f);
        StartCoroutine(BuffIntervalHandler());
    }
    private void Update()
    {
        HandleTimer();
    }

    public void OnBuffStack(BuffInfo buff)
    {
        if(buff != this) { return; }

        stacks++;
    }

    public void OnResetBuff(BuffInfo buff)
    {
        if(buff != this) { return; }
        Cleanse();
    }

    public void OnBuffTimerReset(BuffInfo buff)
    {
        if (buff != this) { return; }
        buffTimer = properties.time;
    }

    public void ChangeValue(float newValue)
    {
        OnChangeValue?.Invoke(this, properties.value, newValue);
        properties.value = newValue;
    }


    public void HandleTimer()
    {
        HandleBuffTimer();

        void HandleBuffTimer()
        {
            if(buffTimeComplete == true) { return; }

            if (buffTimer > 0)
            {
                buffTimer -= Time.deltaTime;
            }
            if (buffTimer <= 0)
            {
                buffTimer = 0;
                buffTimeComplete = true;
                OnBuffCompletion?.Invoke(this);
                StartCoroutine(Expire());
            }
        }
    }

    public void Deplete()
    {
        OnBuffDeplete?.Invoke(this);
        StartCoroutine(Expire());
    }
    public void Cleanse()
    {
        cleansed = true;
        OnBuffCleanse?.Invoke(this);
        StartCoroutine(Expire());
    }
    public void CompleteEarly()
    {
        buffTimer = 0;
    }
    public IEnumerator Expire()
    {
        OnBuffEnd?.Invoke(this);
        yield return new WaitForSeconds(expireDelay);
        if(transform.parent && GetComponentInParent<BuffsManager>())
        {
            if(GetComponentInParent<BuffsManager>().buffObjects.Contains(gameObject))
            {
                GetComponentInParent<BuffsManager>().buffObjects.Remove(gameObject);
            }
        }

        if(buffsManager)
        {
            buffsManager.OnBuffStack -= OnBuffStack;
            buffsManager.OnBuffTimerReset -= OnBuffTimerReset;
            buffsManager.OnResetBuff -= OnResetBuff;
        }

        Destroy(gameObject);
    }
    IEnumerator BuffIntervalHandler()
    {
        while(true)
        {
            if(sourceAbility == null) 
            { 
                yield return new WaitForSeconds(.125f);
                continue;
            }

            yield return new WaitForSeconds(properties.intervals);
            OnBuffInterval?.Invoke(this);

        }
    }
    public List<EntityInfo> EnemiesWithinRange()
    {

        var entitiesWithinRange = Entity.EntitiesWithinRange(hostObject, properties.radius);


        for(int i = 0; i < entitiesWithinRange.Count; i++)
        {
            if(sourceInfo.CanAttack(entitiesWithinRange[i].gameObject))
            {
                continue;
            }
            else
            {
                entitiesWithinRange.RemoveAt(i);
                i--;
            }
        }

        return entitiesWithinRange;
    }
    public List<EntityInfo> AlliesWithinRange()
    {
        var allyList = Entity.EntitiesWithinRange(hostObject, properties.radius);

        for (int i = 0; i < allyList.Count; i++)
        {
            if(sourceInfo.CanHelp(allyList[i].gameObject))
            {
                continue;
            }
            else
            {
                allyList.RemoveAt(i);
                i--;
            }
        }


        return allyList;
    }

    public void HandleTargetHealth(EntityInfo target, float val, BuffTargetType targettingType)
    {
        var combatData = new CombatEventData(this, sourceInfo, val);

        HandleDamage();
        HandleHealing();

        void HandleDamage()
        {
            if(targettingType == BuffTargetType.Assist) { return; }
            target.Damage(combatData);
        }

        void HandleHealing()
        {
            if(targettingType == BuffTargetType.Harm) { return; }
            target.Heal(combatData);
        }
    }
}
