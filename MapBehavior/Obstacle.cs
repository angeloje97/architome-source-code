using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    public class Obstacle : MonoBehaviour
    {
        Collider[] colliders;
        MapAdjustments adjustments;
        Vector3 currentPosition;
        Vector3 currentPositionCheck;
        Vector3 lastPositionBeforeDisabled;
        [SerializeField] float updateInterval = 1f;
        [SerializeField] float currentTime;

        bool isDisabled;
        bool isDestroyed;



        void GetDependencies()
        {
            adjustments = MapAdjustments.active;
            colliders = GetComponentsInChildren<Collider>();
        }
        void Start()
        {
            GetDependencies();
            UpdateObstacle();
        }

        private void OnDestroy()
        {
            isDestroyed = true;
            transform.Translate(Vector3.down * 1000);
            UpdateObstacle(3f, 3);
        }

        private void OnEnable()
        {
            isDisabled = false;
            transform.position = lastPositionBeforeDisabled;
            UpdateObstacle();
        }

        private async void OnDisable()
        {
            if (isDisabled)
            {
                gameObject.SetActive(false);
                return;
            }

            await Task.Yield();

            if (isDestroyed) return;
            if (this == null) return;
            gameObject.SetActive(true);


            lastPositionBeforeDisabled = currentPosition;
            var farPosition = transform.position;
            farPosition.y -= 1000;
            transform.position = farPosition;
            currentPosition = transform.position;
            UpdateObstacle();

            await Task.Yield();
            isDisabled = true;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if(currentTime > 0f)
            {
                currentTime -= Time.deltaTime;
                return;
            }

            currentTime = updateInterval;

            currentPosition = transform.position;

            if(currentPosition != currentPositionCheck)
            {
                UpdateObstacle();
            }
        }

        

        void UpdateObstacle(float sizeMultiplier = 1f, int iterations = 1)
        {
            if (adjustments == null) return;
            var bounds = new List<Bounds>();
            //await adjustments.AdjustAroundCollider(colliders);
            foreach(var collider in colliders)
            {
                bounds.Add(collider.bounds);
                var previousBounds = collider.bounds;
                previousBounds.center = currentPositionCheck;
                previousBounds.size *= sizeMultiplier;
                bounds.Add(previousBounds);
            };


            adjustments.AdjustAroundBounds(bounds, iterations);


            currentPositionCheck = currentPosition;
        }
    }
}
