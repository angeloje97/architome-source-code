using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class CatalystManager : MonoBehaviour
    {
        // Start is called before the first frame update
        public static CatalystManager active { get; private set; }
        public CatalystAudio catalystAudio;
        void Start()
        {

        }

        private void Awake()
        {
            active = this;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public GameObject CatalystAudioManager()
        {
            if (catalystAudio== null) return null;

            return Instantiate(catalystAudio.gameObject, transform);
        }
    }

}