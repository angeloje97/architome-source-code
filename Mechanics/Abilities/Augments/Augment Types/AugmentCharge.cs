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
        [Header("Charge Properties")]
        public GameObject pathfindingAgent;
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

        public override void HandleNewCatlyst(CatalystInfo catalyst)
        {
            catalyst.OnCatalystDestroy += OnCatalystDestroy;
            
        }

        public override string Description()
        {
            var result = "";

            result += $"When the catalyst is destroyed, the caster charges to the target location.";

            return result; 
        }

        public async void OnCatalystDestroy(CatalystDeathCondition condition)
        {
            if (pathfindingAgent == null) return;
            var catalyst = condition.GetComponent<CatalystInfo>();

            var chargeComponent = Instantiate(pathfindingAgent, catalystManager.transform);
            var chargeTarget = new GameObject("Charge Target");

            chargeComponent.transform.SetParent(catalystManager.transform);
            chargeTarget.transform.SetParent(catalystManager.transform);
            chargeTarget.transform.position = catalyst.transform.position;
            chargeComponent.transform.position = augment.entity.transform.position;
            chargeComponent.transform.LookAt(chargeTarget.transform);


            var destinationSetter = chargeComponent.GetComponent<AIDestinationSetter>();
            var seeker = chargeComponent.GetComponent<Seeker>();
            var path = chargeComponent.GetComponent<AIPath>();
            var offset = 2f;

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

            Debugger.Combat(3895, $"Distance between charge target is {V3Helper.Distance(chargeComponent.transform.position, chargeTarget.transform.position)}");

            Predicate<object> predicate = (object o)
                => V3Helper.Distance(chargeComponent.transform.position, chargeTarget.transform.position) <= offset + endReachDistance;


            while (!predicate.Invoke(null))
            {
                var groundPosition = V3Helper.GroundPosition(chargeComponent.transform.position, groundLayer, 2f, heightOffSet + .2f);
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
