using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Architome
{
    public class TargetAgent : EntityProp
    {
        ContainerTargetables targetManager;
        new void GetDependencies()
        {
            base.GetDependencies();

            targetManager = ContainerTargetables.active;
        }
        void Start()
        {
            GetDependencies();
        }

        // Update is called once per frame
        private void OnMouseEnter()
        {
            targetManager.AddMouseOver(entityInfo);
        }

        private void OnMouseExit()
        {
            targetManager.RemoveMouseOver(entityInfo);
           
        }
    }

}
