using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;
public class BodyPartFinder : MonoBehaviour
{
    // Start is called before the first frame update
    [Serializable]
    public struct BodyParts
    {
        public Sex sexTarget;
        public List<Transform> target;
    }

    public Transform generic;
    public Transform male;
    public Transform female;


    public List<BodyParts> bodyParts;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnValidate()
    {
        foreach (var bodyPart in bodyParts)
        {
            bodyPart.target.Clear();
            if (bodyPart.sexTarget == Sex.Male)
            {
                foreach (var render in male.GetComponentsInChildren<Renderer>(true))
                {
                    if (bodyPart.target.Contains(render.transform.parent)) continue;
                    bodyPart.target.Add(render.transform.parent);
                }
            }
            else if (bodyPart.sexTarget == Sex.Female)
            {
                foreach(var render in female.GetComponentsInChildren<Renderer>(true))
                {
                    if (bodyPart.target.Contains(render.transform.parent)) continue;
                    bodyPart.target.Add(render.transform.parent);
                }
            }
            else
            {
                foreach(var render in generic.GetComponentsInChildren<Renderer>(true))
                {
                    if (bodyPart.target.Contains(render.transform.parent)) continue;
                    bodyPart.target.Add(render.transform.parent);
                }
            }
        }
    }
}
