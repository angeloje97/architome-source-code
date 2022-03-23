using System.Collections;
using System.Collections.Generic;
using Architome;
using UnityEngine;
using System;
using UnityEngine.UI;

public class SpellBookManager : MonoBehaviour
{
    // Start is called before the first frame update

    public List<EntityInfo> playableEntities;
    public List<Image> portraitIcons;
    public List<GameObject> spellSlots;

    public Transform bin;
    public GameObject abilityTemplate;
    //Events
    public Action<EntityInfo> OnNewEntity;


    void Start()
    {
        //Invoke("GetDependencies", .5f);
        GameManager.active.OnNewPlayableEntity += OnNewPlayableEntity;
    }


    public void OnNewPlayableEntity(EntityInfo newEntity, int index)
    {
        if(index >= portraitIcons.Count)
        {
            return;
        }

        playableEntities.Add(newEntity);

        if(newEntity.entityPortrait)
        {
            portraitIcons[index].sprite = newEntity.entityPortrait;
        }

        if(index == 0)
        {
            SetEntity(0);
        }
    }



    public void SetEntity(int num)
    {
        if(num >= playableEntities.Count) { return; }
        OnNewEntity?.Invoke(playableEntities[num].GetComponent<EntityInfo>());

        ClearBin();
        CreateItems(playableEntities[num]);
    }

    public void ClearBin()
    {
        GetComponentInParent<ModuleInfo>()?.DestroyBin();

        foreach(GameObject slot in spellSlots)
        {
            if(slot.GetComponentInChildren<AbilityInfoUI>())
            {
                Destroy(slot.GetComponentInChildren<AbilityInfoUI>().gameObject);
            }
        }
    }

    public void CreateItems(EntityInfo entity)
    {
        var abilities = entity.GetComponentInChildren<AbilityManager>();

        if(abilities == null) { return; }

        foreach(GameObject spellSlot in spellSlots)
        {
            var slot = spellSlot.GetComponent<SpellBookSlot>();
            var ability = abilities.Ability(slot.slotType);

            if(ability == null) { continue; }
            var newUI = Instantiate(abilityTemplate);
            newUI.GetComponent<AbilityInfoUI>().SetAbility(ability);
            slot.SetAbilityUI(newUI);
        }

    }
}
