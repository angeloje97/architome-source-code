using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class Defect : MonoBehaviour
    {
        public Transform source;
        [Multiline] public string description;
        [Multiline(30)] public string stackTrace;


        public static void CreateIndicator(Transform target, string title = "Generic", Exception e = null)
        {
            var defectGameObject = new GameObject($"Defect: {title}");
            if(target != null)
            {
                defectGameObject.transform.SetParent(target);
            }



             
            defectGameObject.transform.position = new();
            var defect = defectGameObject.AddComponent<Defect>();
            defect.source = target;
            defect.description = e.Message;

            Debug.LogError($"{target.gameObject}\n{e.StackTrace}");

            if (e != null)
            {
                defect.stackTrace = e.StackTrace;
            }
        }
    }
}
