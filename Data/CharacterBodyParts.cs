using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Architome.Enums;

namespace Architome
{
    public class CharacterBodyParts : MonoBehaviour
    {
        public EntityInfo entity;
        BodyPartCatalystHandler catalystHandler;
        [Serializable]
        public class VectorBodyPart
        {
            public BodyPart part;
            public Transform target;
        }

        public List<VectorBodyPart> partPairs;
        public Dictionary<BodyPart, Transform> partDict;
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

        private void Awake()
        {
            CreateDictionary();
        }

        private void Start()
        {
            GetDepedencies();
        }

        void CreateDictionary()
        {
            if (partPairs == null) return;
            partDict = new();
            foreach (var pair in partPairs)
            {
                if (partDict.ContainsKey(pair.part)) continue;
                partDict.Add(pair.part, pair.target);
            }
        }

        void GetDepedencies()
        {
            entity = GetComponentInParent<EntityInfo>();
            
            if(entity)
            {
                if (characterIcon)
                {
                    entity.infoEvents.OnNullPortraitCheck += (EntityInfo info) => {
                        info.SetPortrait(characterIcon);
                    };
                }
            }

            catalystHandler = new(this);
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
            return partDict[check];
            //foreach (var pair in partPairs)
            //{
            //    if (pair.part == check)
            //    {
            //        return pair.target;
            //    }
            //}

            //return transform;
        }
    }

}
