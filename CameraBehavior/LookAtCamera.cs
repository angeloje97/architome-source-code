using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{

    Camera currentCamera;

    void Start()
    {
        GetDependencies();        
    }

    void GetDependencies()
    {
        currentCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        MirrorCamera();
    }

    void MirrorCamera()
    {
        if (currentCamera == null) return;
        var cameraRotation = currentCamera.transform.rotation;

        transform.rotation = cameraRotation;
    }
}
