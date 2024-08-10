using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.DevTools
{
    public class DevToolsManager : MonoBehaviour
    {
        public static DevToolsManager active;

        public void Awake()
        {
            SingletonManger.HandleSingleton(this.GetType(), gameObject, true, true, () => {
                active = this;
            });
        }
    }
}
