using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;

public class CurrentTargetManager : MonoBehaviour
{
    // Start is called before the first frame update
    public PortraitBehavior targetPortrait;
    public GameManager gameManager;
    public ContainerTargetables targetManager;
    void GetDependencies()
    {
        if(GMHelper.GameManager() && GMHelper.TargetManager())
        {
            gameManager = GMHelper.GameManager();
            targetManager = GMHelper.GameManager().targetManager;
        }
    }
    void Start()
    {
        GetDependencies();
    }

    // Update is called once per frame
    void Update()
    {
        if(targetPortrait == null) { return; }
        UpdateTargetPortrait();
    }

    void UpdateTargetPortrait()
    {
        if(targetManager.selectedTargets != null && targetManager.selectedTargets.Count > 0)
        {

            targetPortrait.gameObject.SetActive(true);
            if (targetPortrait.entity != targetManager.selectedTargets[0].GetComponent<EntityInfo>())
            {
                targetPortrait.SetEntity(targetManager.selectedTargets[0].GetComponent<EntityInfo>());
            }
        }
        else
        {
            targetPortrait.ResetEntity();
            targetPortrait.gameObject.SetActive(false);
        }
    }


}
