using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StayOnGround : MonoBehaviour
{
    // Start is called before the first frame update
    public float heightOffSet;
    
    void GetDependencies()
    {
        if(!GMHelper.LayerMasks())
        {
            Destroy(this);
            
        }


    }
    void Start()
    {
        GetDependencies();
    }

    // Update is called once per frame
    void Update()
    {
        Stay();
    }

    public void Stay()
    {
        var ray = new Ray(transform.position, Vector3.down);

        if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, GMHelper.LayerMasks().walkableLayer))
        {
            transform.position = hit.transform.position + new Vector3(0, heightOffSet, 0);
        }
    }
}
