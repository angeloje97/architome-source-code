using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

namespace Architome
{
    public class ArchChestAnimationHandler : MonoBehaviour
    {
        // Start is called before the first frame update
        [Serializable]
        public struct Info
        {
            public ArchChest archChest;
            public Transform lid;
            public Animator animator;
        }

        public Info info;

        void GetDependencies()
        {
            info.archChest = GetComponentInParent<ArchChest>();
            if (info.archChest)
            {
                info.archChest.events.OnOpen += OnOpen;
                info.archChest.events.OnClose += OnClose;
            }
        }

        private void Start()
        {
            GetDependencies();
        }

        private void OnValidate()
        {
            info.archChest = GetComponentInParent<ArchChest>();
        }

        public void OnOpen(ArchChest chest)
        {
            info.animator.SetBool("IsOpen", true);
        }


        public void OnClose(ArchChest chest)
        {
            info.animator.SetBool("IsOpen", false);
        }
    }

}