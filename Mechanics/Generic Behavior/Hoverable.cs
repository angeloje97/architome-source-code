using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Hoverable : MonoBehaviour
{
    // Start is called before the first frame update
    // Update is called once per frame

    public bool isHovering = false;
    public Action<bool> OnHoverChange;

    private void Start()
    {
        Physics.queriesHitTriggers = true;
    }

    void Update()
    {
        
    }

    public void OnMouseEnter()
    {
        isHovering = true;
        OnHoverChange?.Invoke(isHovering);
    }

    public void OnMouseExit()
    {
        isHovering = false;
        OnHoverChange?.Invoke(isHovering);
    }
}
