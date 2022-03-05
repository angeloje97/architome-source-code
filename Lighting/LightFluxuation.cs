using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFluxuation : MonoBehaviour
{
    // Start is called before the first frame update
    public Light mainLight;
    
    public float center;
    public float range;
    public float oscillationSpeed;

    public float oscillationValue;
    public float angle;
    
    void Start()
    {
        center = mainLight.intensity;
    }

    // Update is called once per frame
    void Update()
    {
        Oscillate();
    }

    void Oscillate()
    {
        angle += Time.deltaTime * oscillationSpeed;
        oscillationValue = Mathf.Sin(angle)*range;
        mainLight.intensity = center + oscillationValue;
    }
}
