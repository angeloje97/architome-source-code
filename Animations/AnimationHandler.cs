using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class AnimationHandler<E> where E : Enum
    {
        Animator animator;

        public AnimationHandler(Animator animator)
        {
            this.animator = animator;
        }

        public void SetTrigger(E trigger)
        {
            animator.SetTrigger(EnumString.GetValue(trigger));
        }

        public void ResetTrigger(E trigger)
        {
            animator.ResetTrigger(EnumString.GetValue(trigger));
        }

        public void SetBool(E boolean, bool value)
        {
            animator.SetBool(EnumString.GetValue(boolean), value);
        }

        public void SetInt(E name, int value)
        {
            animator.SetInteger(EnumString.GetValue(name), value);
        }

        public void SetFloat(E name, float value)
        {
            animator.SetFloat(EnumString.GetValue(name), value);
        }

    }
}
