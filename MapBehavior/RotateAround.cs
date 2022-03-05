using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RotateAround : MonoBehaviour
{
    public GameObject roomToSpawn;
    public Transform roomAnchor;
    public Vector3 locationToSpawn;
    public Quaternion angle;
    public Vector3 pivotPoint;
    // Start is called before the first frame update
    void Start()
    {
        if(roomAnchor == null) { return; }
        var newObject = Instantiate(roomToSpawn, roomAnchor.transform.position, roomAnchor.transform.rotation).transform;
        newObject.transform.position = new Vector3(newObject.transform.position.x +15, 0, newObject.transform.position.z + 30);
        newObject.RotateAround(roomAnchor.transform.position, new Vector3(0,1,0), 90f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
