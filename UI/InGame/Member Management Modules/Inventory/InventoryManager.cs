using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Architome;

public class InventoryManager : MonoBehaviour
{
    // Start is called before the first frame update
    public List<EntityInfo> playableEntities;
    public List<EntityInventoryUI> entityInventories;
    public List<Image> entityIcons;
    public Transform itemBin;

    public GameObject itemTemplate;

    public CanvasGroup canvasGroup;

    //Update Triggers
    private int entityCount = 0;
    void GetDependencies()
    {
        if(GMHelper.GameManager())
        {
            playableEntities = GMHelper.GameManager().playableEntities;
        }

        if (GetComponent<CanvasGroup>())
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        GMHelper.GameManager().OnNewPlayableEntity += OnNewPlayableEntity;
    }


    void Start()
    {
        GetDependencies();
    }

    

    // Update is called once per frame
    void Update()
    {
        HandleUpdateTriggers();
    }

    public void OnNewPlayableEntity(EntityInfo entity, int index)
    {
        UpdateInventories();
        UpdatePortrait();

        void UpdateInventories()
        {
            if (entityInventories.Count < index) return;

            entityIcons[index].sprite = playableEntities[index].entityPortrait;
        }

        void UpdatePortrait()
        {
            if (entityIcons.Count < index) return;

            entityInventories[index].SetEntity(entity);
        }
    }



    

    void HandleUpdateTriggers()
    {
        //HandleEntityCountTrigger();
        

        //void HandleEntityCountTrigger()
        //{
        //    if(entityCount != playableEntities.Count)
        //    {
        //        entityCount = playableEntities.Count;
        //        UpdatePortraits();
        //        UpdateInventories();
        //    }
        //}
    }
    void UpdatePortraits()
    {
        for (int i = 0; i < playableEntities.Count; i++)
        {
            if (i > entityIcons.Count) { break; }

            if (playableEntities[i].entityPortrait != null)
            {
                entityIcons[i].sprite = playableEntities[i].entityPortrait;
            }
        }
    }
    void UpdateInventories()
    {
        for (int i = 0; i < playableEntities.Count; i++)
        {
            if (i >= entityInventories.Count) { break; }

            entityInventories[i].entityInfo = playableEntities[i];
            entityInventories[i].entityInventory = playableEntities[i].Inventory();
            entityInventories[i].SetEntity(playableEntities[i]);
        }
    }


}
