using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class IsFacingCheck : MonoBehaviour
    {
        // Start is called before the first frame update
        public bool isActive;
        public GameObject entity1;
        public GameObject entity2;
        public bool entity1IsFacingEntity2;
        public bool entity2IsFacingEntity1;

        public float angleDifference1;
        public float angleDifference2;

        CharacterInfo charInfo1;
        CharacterInfo charInfo2;


        void Start()
        {
            if (entity1)
            {
                charInfo1 = entity1.GetComponentInChildren<CharacterInfo>();
            }
            if (entity2)
            {
                charInfo2 = entity2.GetComponentInChildren<CharacterInfo>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!isActive) return;
            entity1IsFacingEntity2 = charInfo1.IsFacing(entity2.transform.position);
            entity2IsFacingEntity1 = charInfo2.IsFacing(entity1.transform.position);

            angleDifference1 = charInfo1.AngleFromTarget(entity2.transform.position);
            angleDifference2 = charInfo2.AngleFromTarget(entity1.transform.position);
        }
    }
}

