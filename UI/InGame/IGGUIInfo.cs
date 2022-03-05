using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IGGUIInfo : MonoBehaviour
{
    // Start is called before the first frame update
    public List<GameObject> properties;
    public List<GameObject> modules;
    
    public void GetProperties()
    {
        properties = new List<GameObject>();

        foreach(Transform child in transform)
        {
            properties.Add(child.gameObject);
        }
    }
    void Start()
    {
        GetProperties();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public ActionBarsInfo ActionBarInfo()
    {
        foreach(GameObject property in properties)
        {
            if(property.GetComponent<ActionBarsInfo>()) { return property.GetComponent<ActionBarsInfo>(); }
        }

        return null;
    }

    public void ToggleModule(int index)
    {
        if(index >= modules.Count) { return; }
        if(modules[index].GetComponent<CanvasGroup>() == null) { return; }

        var moduleInfo = modules[index].GetComponent<ModuleInfo>();
        moduleInfo.SetActive(!moduleInfo.isActive);


    }

    public void SetModule(int index, bool val)
    {
        if(index >= modules.Count) { return; }

        if (!modules[index].GetComponent<ModuleInfo>()) { return; }

        modules[index].GetComponent<ModuleInfo>().SetActive(val);
    }

    public void SetModules(bool val)
    {
        foreach(var i in modules)
        {
            if(i.GetComponent<ModuleInfo>() && !i.GetComponent<ModuleInfo>().isActive) { continue; }
            var num = modules.IndexOf(i);

            SetModule(num, val);
        }
    }

    public bool ModulesActive()
    {
        foreach(GameObject module in modules)
        {
            if(module.GetComponent<CanvasGroup>() && module.GetComponent<CanvasGroup>().interactable == true)
            {
                return true;
            }
        }
        return false;
    }
}
