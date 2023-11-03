using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class ItemAuthentication : Authentication
    {
        public LogicType authenticationLogic;
        public List<ItemData> requiredItemsObtained;

        Dictionary<int, bool> values;

        public override void OnAuthenticationStart()
        {
            base.OnAuthenticationStart();
            UpdateValues();



        }

        void UpdateValues()
        {

        }

        public override bool Validated()
        {
            return base.Validated();
        }
    }
}
