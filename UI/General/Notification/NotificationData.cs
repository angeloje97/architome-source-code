using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class NotificationData
    {
        public NotificationType type;
        public string name;
        public string description;
        

        public NotificationData(NotificationType type)
        {
            this.type = type;
        }
    }
}
