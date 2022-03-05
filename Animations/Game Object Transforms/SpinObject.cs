using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinObject : MonoBehaviour
{
    // Start is called before the first frame update
    public float spinSpeed;
    public float angle = 0;
    public Vector3 spinAxis;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //angle += spinSpeed * Time.deltaTime;
        //transform.localRotation = Quaternion.Euler(spinAxis * angle);

        transform.Rotate(spinAxis * spinSpeed*Time.deltaTime);
    }
}
