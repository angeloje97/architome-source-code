using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarToScreen : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject entity;
    public Camera mainCamera;
    void Start()
    {
        Architome.ArchAction.Delay(() => { mainCamera = MainCamera3.active; }, .125f);
    }

    // Update is called once per frame
    void Update()
    {
        if (entity == null) return;
        if (mainCamera == null) return;
        FollowCamera();
    }

    public void FollowCamera()
    {
        var position = mainCamera.WorldToScreenPoint(entity.transform.position);
        position.z = 0;
        //transform.position = position;
    }
}
