using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
public class RaidInfo : MonoBehaviour
{
    // Start is called before the first frame update
    public EntityControlType raidControl;
    public List<GameObject> group;

    void Start()
    {
        foreach (Transform child in transform)
        {
            if (child.tag.Equals("Party"))
            {
                group.Add(child.gameObject);
            }
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
