using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class AugmentTaskActivator : MonoBehaviour
    {
        public int amountActivated;

        public List<int> validTransmissions;


        public void IncrementActivated()
        {
            amountActivated++;
        }

        public bool ValidAugment(AugmentTask augment)
        {
            if (validTransmissions == null) return false;
            return validTransmissions.Contains(augment.validReceiver);

        }
    }
}
