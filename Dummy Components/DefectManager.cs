using System.Collections;
using UnityEngine;

namespace Architome.Assets.Source.Scripts.Dummy_Components
{
    public class DefectManager : MonoBehaviour
    {
        public static DefectManager active;

        public void Start()
        {
            SingletonManger.HandleSingleton(typeof(DefectManager), gameObject, true, onSuccess: () => {
                active = this;
            });
        }

        public void AddDefect(Defect defect)
        {
            defect.transform.SetParent(transform);
        }
    }
}