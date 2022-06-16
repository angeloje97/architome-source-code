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
        }

        [SerializeField] ToolTips tools;

        private void Awake()
        {
            active = this;
        }

        public ToolTip GeneralHeader()
        {
            if (tools.generalHeader == null) return null;
            transform.SetAsLastSibling();
            var toolTip = Instantiate(tools.generalHeader, transform).GetComponent<ToolTip>();

            return toolTip;

        }

        public ToolTip General()
        {
            if (tools.general == null) return null;

            transform.SetAsLastSibling();

            var toolTip = Instantiate(tools.general, transform).GetComponent<ToolTip>();

            return toolTip;
        }
    }
}
