using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    [RequireComponent(typeof(Canvas))]
    public class PersistantCanvas : MonoBehaviour
    {
        public static PersistantCanvas active;


        private void Awake()
        {
            if (active)
            {
                Destroy(gameObject);
                return;
            }

            active = this;
        }

        private void Start()
        {
            if (active != this) return;
            ArchGeneric.DontDestroyOnLoad(gameObject);

        }
    }
}
