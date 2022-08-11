using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

namespace Architome
{
    public class AugmentCharge : AugmentType
    {
        public float maxSpeed = 100;
        public float maxAcceleration = 5000000;

        float heightOffSet = 0f;
        LayerMask groundLayer;

        public CatalystManager catalystManager;
        void Start()
        {
            GetDependencies();
        }

        async new void GetDependencies()
        {
            await base.GetDependencies();
            EnableCatalyst();
            catalystManager = CatalystManager.active;

            var collider = augment.entity.GetComponent<BoxCollider>();
            heightOffSet = collider.size.y / 2;
            var layerMasksData = LayerMasksData.active;

            groundLayer = layerMasksData.walkableLayer;
        }

        public override async void HandleNewCatlyst(CatalystInfo catalyst)
        {
            var chargeComponent = new GameObject("Charge Component");
            var chargeTarget = new GameObject("Charge Target");

            chargeComponent.transform.SetParent(catalystManager.transform);
            chargeTarget.transform.SetParent(catalystManager.transform);
            chargeComponent.transform.position = catalyst.transform.position;


            var destinationSetter = chargeComponent.AddComponent<AIDestinationSetter>();
            var seeker = chargeComponent.AddComponent<Seeker>();
            var path = chargeComponent.AddComponent<AIPath>();
            var offset = .5f;

            if (catalyst.target)
            {
                chargeTarget.transform.position = catalyst.target.transform.position;
                path.endReachedDistance = 1f;
            }
            else
            {
                chargeTarget.transform.position = catalyst.location;
                path.endReachedDistance = 0f;
            }

            destinationSetter.target = chargeTarget.transform;

            path.maxSpeed = maxSpeed;
            path.maxAcceleration = maxAcceleration;
            var endReachDistance = path.endReachedDistance;

            Predicate<object> predicate = (object o) 
                => V3Helper.Distance(chargeComponent.transform.position, chargeTarget.transform.position) <= offset + endReachDistance;

            while (!predicate.Invoke(null))
            {
                var groundPosition = V3Helper.GroundPosition(chargeComponent.transform.position, groundLayer, 2f, heightOffSet);
                augment.entity.transform.position = groundPosition;
                await Task.Yield();
            }

            Destroy(chargeComponent);
            Destroy(chargeTarget);


        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
