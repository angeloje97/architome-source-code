using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatalystManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static CatalystManager active { get; private set; }
    void Start()
    {
        
    }

    private void Awake()
    {
        active = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
