using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Architome
{
    public class SphereRaycasts : MonoBehaviour
    {
        private void OnDrawGizmosSelected()
        {
            //var directions = GetSphereDirections(20);
            var position = transform.position;
            var radius = 10f;

            V3Helper.SphereDirections(50, (Vector3 direction) => {
                var endPosition = position + (direction.normalized * radius);
                Debug.DrawLine(position, endPosition, Color.red);
            });
        }
    }
}
