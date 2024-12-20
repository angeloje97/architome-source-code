using Architome.Assets.Source.Scripts.Dummy_Components;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class Defect : MonoBehaviour
    {
        public Transform source;
        public string sourceName;
        [Multiline] public string description;
        [Multiline(30)] public string stackTrace;



        public static void CreateIndicator(Transform target, string title = "Generic", Exception e = null)
        {
            var defectGameObject = new GameObject($"Defect from {target.gameObject}");

            defectGameObject.transform.position = new();
            var defect = defectGameObject.AddComponent<Defect>();
            defect.sourceName = target.gameObject.ToString();

            ArchAction.Delay(() => {
                if(target != null)
                {
                    defectGameObject.transform.SetParent(target);
                }
                else
                {
                    DefectManager.active.AddDefect(defect);
                }
            }, .625f);

            defect.source = target;
            defect.description = e.Message;


            if (e != null)
            {
                Debug.LogError($"{title}\n{target.gameObject}\n{e.StackTrace}");
                defect.stackTrace = e.StackTrace;
            }
        }
    }
}
