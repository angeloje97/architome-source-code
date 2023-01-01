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

        ArchitomeCharacter archCharacter;
        [SerializeField] bool isMale;
        [SerializeField] bool isFemale;

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

            if(isMale && isFemale)
            {
                isFemale = false;
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
            archCharacter = GetComponent<ArchitomeCharacter>();

            
            if(entity)
            {
                if (characterIcon)
                {
                    entity.infoEvents.OnNullPortraitCheck += (EntityInfo info) => {
                        info.SetPortrait(characterIcon);
                    };
                }

                entity.infoEvents.AddOneTrue(InfoEvents.EventType.IsFemaleCheck, HandleFemaleCheck);
                entity.infoEvents.AddOneTrue(InfoEvents.EventType.IsMaleCheck, HandleMaleCheck);
            }

            catalystHandler = new(this);
        }

        void HandleFemaleCheck(EntityInfo entity, object data, List<bool> checks)
        {
            if (archCharacter)
            {
                if (archCharacter.isFemale)
                {
                    checks.Add(true);
                    return;
                }
            }

            if (isFemale) checks.Add(true);
        }
        void HandleMaleCheck(EntityInfo entity, object data, List<bool> checks)
        {
            if(archCharacter && archCharacter.isMale)
            {
                checks.Add(true);
                return;
            }

            if(isMale) checks.Add(true);
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
