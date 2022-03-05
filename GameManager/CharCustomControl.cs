using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharCustomControl : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject objectToSpin;
    public float rotationValue;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(objectToSpin == null) { return; }
        HandleUserInputs();
    }

    void HandleUserInputs()
    {
        if(Input.GetKey(KeyCode.Mouse0))
        {
            HandleObjectRotating();
        }
    }

    void HandleObjectRotating()
    {
        rotationValue += -Input.GetAxis("Mouse X");
        objectToSpin.transform.rotation = Quaternion.Euler(0, rotationValue * 15, 0);
    }
}
