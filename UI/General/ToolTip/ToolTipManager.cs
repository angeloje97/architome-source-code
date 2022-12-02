using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace Architome
{
    public enum ToolTipType
    {
        GeneralHeader,
        General,
        Side,
        Label,
    }
    public class ToolTipManager : MonoBehaviour
    {
        public static ToolTipManager active;
        [Serializable]
        struct ToolTips
        {
            public GameObject generalHeader;
            public GameObject general;
            public GameObject side;
            public GameObject label;
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

        public ToolTip ToolTip(ToolTipType type)
        {
            switch (type)
            {
                case ToolTipType.Side:
                    return Side();
                case ToolTipType.Label:
                    return Label();
                case ToolTipType.GeneralHeader:
                    return GeneralHeader();
                default:
                    return General();
                    
            }
        }

        public ToolTip GeneralHeader()
        {
            return ToolTip(tools.generalHeader);
        }

        public ToolTip General()
        {
            return ToolTip(tools.general);
        }

        public ToolTip Label()
        {
            return ToolTip(tools.label);
        }

        public ToolTip Side()
        {
            return ToolTip(tools.side);
        }
    }
}
