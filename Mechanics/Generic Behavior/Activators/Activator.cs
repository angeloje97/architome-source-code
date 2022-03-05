using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
public class Activator : MonoBehaviour
{
    // Start is called before the first frame update


    public Action<ActivatorData> OnActivate;

    public UnityEvent<ActivatorData> OnActivateUnity;

}

public class ActivatorData
{
    public GameObject gameObject;
    public Collider collider;
    public List<GameObject> gameObjects;
    public List<Collider> colliders;

}
