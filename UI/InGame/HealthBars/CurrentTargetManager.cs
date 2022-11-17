using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;
using System.Threading.Tasks;
public class CurrentTargetManager : MonoBehaviour
{
    // Start is called before the first frame update
    public PortraitBehavior targetPortrait;
    public GameManager gameManager;
    public ContainerTargetables targetManager;
    public KeyBindings binds;
    void GetDependencies()
    {
        binds = GMHelper.KeyBindings();
        if(GMHelper.GameManager() && GMHelper.TargetManager())
        {
            gameManager = GMHelper.GameManager();
            targetManager = GMHelper.TargetManager();
        }

        targetManager.OnSelectTarget += OnSelectTarget;
        targetManager.OnClearSelected += OnClearSelected;
        targetManager.OnClearFromEscape += OnClearFromEscape;

    }
    void Start()
    {
        GetDependencies();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnSelectTarget(GameObject target)
    {
        SetCanvasGroup(targetPortrait.GetComponent<CanvasGroup>(), true);
        targetPortrait.SetEntity(target.GetComponent<EntityInfo>());
        if (target == targetManager.currentHover)
        {
            targetPortrait.OnNewHoverTarget(null, target);
        }
    }

    public void OnClearFromEscape()
    {
        SetCanvasGroup(targetPortrait.GetComponent<CanvasGroup>(), false);
    }

    async public void OnClearSelected()
    {
        while (!Input.GetKeyUp(binds.keyBinds["Select"]))
        {
            await Task.Yield();
        }

        if (targetManager.selectedTargets.Count != 0) return;

        SetCanvasGroup(targetPortrait.GetComponent<CanvasGroup>(), false);
    }

    public void SetCanvasGroup(CanvasGroup group, bool active)
    {
        group.alpha = active ? 1 : 0f;
        group.interactable = active;
        group.blocksRaycasts = active;
    }


}
