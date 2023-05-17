using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Architome
{
    public class Obstacle : MonoBehaviour
    {
        Collider[] colliders;
        MapAdjustments adjustments;
        Vector3 currentPosition;
        Vector3 currentPositionCheck;

        float updateIntervals;

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
            foreach(var collider in colliders)
            {
                collider.enabled = false;
            }
            UpdateObstacle();
        }

        private void Update()
        {
            if(updateIntervals > 0f)
            {
                updateIntervals -= Time.deltaTime;
                return;
            }

            updateIntervals = 1f;

            currentPosition = transform.position;

            if(currentPosition != currentPositionCheck)
            {
                currentPositionCheck = currentPosition;
                UpdateObstacle();
            }
        }

        void UpdateObstacle()
        {
            currentPositionCheck = currentPosition;
            if (adjustments == null) return;
            adjustments.AdjustAroundCollider(colliders);
        }
    }
}
