using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class Defect : MonoBehaviour
    {
        
        [Multiline] public string description;


        public static void CreateIndicator(Transform target, string title = "Generic", string description = "NA")
        {
            var defectGameObject = new GameObject($"Defect: {title}");

            defectGameObject.transform.SetParent(target);
            defectGameObject.transform.position = new();

            var defect = defectGameObject.AddComponent<Defect>();
            defect.description = description;
        }
    }
}
