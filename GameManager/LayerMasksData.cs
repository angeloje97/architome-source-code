using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerMasksData : MonoBehaviour
{
    
    public static LayerMasksData active { get; private set; }

    public LayerMask structureLayerMask;
    public LayerMask wallLayer;
    public LayerMask entityLayerMask;
    public LayerMask walkableLayer;

    private void Awake()
    {
        active = this;
    }
}
