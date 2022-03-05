using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    // Start is called before the first frame update
    

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MirrorCamera();
    }

    void MirrorCamera()
    {
        var cameraRotation = Camera.main.transform.rotation;


        transform.rotation = cameraRotation;
    }
}
