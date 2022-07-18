using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Architome.Enums;

public class CharacterBodyParts : MonoBehaviour
{
    [Serializable]
    public class VectorBodyPart
    {
        public BodyPart part;
        public Transform target;
    }

    public List<VectorBodyPart> partPairs;
    public Sprite characterIcon;

    //public GameObject rightHand;
    //public GameObject rightThumb;
    //public GameObject leftHand;
    //public GameObject leftThumb;
    //public GameObject spine;
    //public GameObject hips;
    //public GameObject hipsAttachment;
    //public GameObject backAttachment;
    //public GameObject capeAttachment;

    private void OnValidate()
    {
        var enums = Enum.GetValues(typeof(BodyPart));

        if (partPairs == null)
        {
            partPairs = new();
        }

        foreach (BodyPart part in enums)
        {
            if (GetPair(part) == null)
            {
                partPairs.Add(new() { part = part });
            }
        }
    }

    public VectorBodyPart GetPair(BodyPart check)
    {
        foreach (var pair in partPairs)
        {
            if (pair.part == check)
            {
                return pair;
            }
        }

        return null;
    }

    public Transform BodyPartTransform(BodyPart check)
    {

        foreach (var pair in partPairs)
        {
            if (pair.part == check)
            {
                return pair.target;
            }
        }

        return transform;
    }
}
