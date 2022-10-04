using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pathfinding;
using UnityEngine;

namespace Architome.Tutorial
{
    public class TrailEmitter : MonoBehaviour
    {
        public static TrailEmitter activeTrailEmitter;

        public Trail trail;
        public Transform trailTarget;

        [Header("Properties")]
        public bool active;
        public bool intervalActive;
        public bool emitOnStart;
        public float interval = 1f;
        public float lifeTime = 5f;


        private void Start()
        {
            HandleEmitOnStart();
        }

        private void Awake()
        {
            activeTrailEmitter = this;
        }

        void HandleEmitOnStart()
        {
            if (!emitOnStart) return;

            active = true;
            HandleEmission();
        }

        public void SetTrail(Transform target)
        {
            trailTarget = target;
        }

        public void SetEmission(bool active)
        {
            this.active = active;

            HandleEmission();
        }

        async void HandleEmission()
        {
            
            if (interval <= 0) return;

            if (intervalActive) return;

            intervalActive = true;

            while (active)
            {
                SpawnEmission(trailTarget);
                await Task.Delay((int)(1000 * interval));
            }

            intervalActive = false;
        }

        void SpawnEmission(Transform target)
        {
            if (target == null) return;
            var newTrail = Instantiate(trail.gameObject, transform.position, transform.rotation);

            var trailBehavior = newTrail.GetComponent<Trail>();

            trailBehavior.SetTarget(target);
            trailBehavior.DestroySelf(lifeTime);
        }

    }
}
