using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace Architome
{
    public class ToolTipManager : MonoBehaviour
    {
        public static ToolTipManager active;
        [Serializable]
        struct ToolTips
        {
            public GameObject generalHeader;
            public GameObject general;
            public GameObject side;
        }

        public Transform sideToolBarPosition;
        

        [SerializeField] ToolTips tools;

        private void Awake()
        {
            active = this;
        }

        ToolTip ToolTip(GameObject toolTip)
        {
            if (toolTip == null) return null;

            var newToolTip = Instantiate(toolTip, transform).GetComponent<ToolTip>();

            transform.SetAsLastSibling();

            return newToolTip;

        }

        public ToolTip GeneralHeader()
        {
            return ToolTip(tools.generalHeader);
        }

        public ToolTip General()
        {
            return ToolTip(tools.general);
        }

        public ToolTip Side()
        {
            return ToolTip(tools.side);
        }
    }
}
