using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class BuffStackEffects : BuffType
    {

        public List<StackEffect> stackEffects;

        [SerializeField]
        public class StackEffect
        {
            public List<GameObject> buffsOnStack;
            public int stackTrigger;
            public bool applied;
        }

        [SerializeField] bool update;

        void Start()
        {
            GetDependencies();
        }
        new void GetDependencies()
        {
            base.GetDependencies();

            foreach (var effect in stackEffects)
            {
                effect.applied = false;
            }

            buffInfo.OnStack += OnBuffStack;

            OnBuffStack(buffInfo, buffInfo.stacks, buffInfo.properties.value);
        }
        private void OnValidate()
        {
            if (!update) return;
            update = false;

            if (stackEffects == null) return;

            foreach (var stackEffect in stackEffects)
            {
                if (stackEffect.buffsOnStack == null) continue;

                for (int i = 0; i < stackEffect.buffsOnStack.Count; i++)
                {
                    var current = stackEffect.buffsOnStack[i];

                    if (current == null)
                    {
                        stackEffect.buffsOnStack.RemoveAt(i);
                        i--;
                        continue;
                    }

                    var info = current.GetComponent<BuffInfo>();
                    if (info == null)
                    {
                        stackEffect.buffsOnStack.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        public override string Description()
        {
            var result = "";

            if (buffInfo.sourceAbility)
            {
                return FaceDescription(buffInfo.sourceAbility.value);
            }


            return result;
        }
        public override string GeneralDescription()
        {
            var result = "";

            var listString = new List<string>();

            foreach (var stackEffect in stackEffects)
            {
                
                var buffNames = new List<string>();

                foreach (var buff in stackEffect.buffsOnStack)
                {
                    var info = buff.GetComponent<BuffInfo>();

                    buffNames.Add(info.name);
                }

                var buffNamesString = ArchString.StringList(buffNames);

                if (buffNamesString.Length > 0)
                {
                    listString.Add($"{stackEffect.stackTrigger} Stack(s): {buffNamesString}");
                }
            }

            var stacksDescription = ArchString.NextLineList(listString);

            if (stacksDescription.Length > 0)
            {
                result = $"Buff Effects on Stack: \n{stacksDescription}";
            }

            return result;
        }
        public override string FaceDescription(float theoreticalValue)
        {
            var result = "";

            var listString = new List<string>();

            foreach (var stackEffect in stackEffects)
            {

                var buffDescriptionList = new List<string>();

                foreach (var buff in stackEffect.buffsOnStack)
                {
                    var info = buff.GetComponent<BuffInfo>();

                    buffDescriptionList.Add(info.TypeDescriptionFace(theoreticalValue));
                }

                var buffDescription = ArchString.StringList(buffDescriptionList);

                if (buffDescription.Length > 0)
                {
                    listString.Add($"{stackEffect.stackTrigger} Stack(s): {buffDescription}");
                }
            }

            var stacksDescription = ArchString.NextLineList(listString);

            if (stacksDescription.Length > 0)
            {
                result = $"Buff Effects on Stack: \n{stacksDescription}";
            }

            return result;
        }
        void OnBuffStack(BuffInfo buff, int stacks, float value)
        {
            foreach (var stackEffect in stackEffects)
            {
                if (stackEffect.stackTrigger != stacks) continue;
                ApplyStackEffect(stackEffect);
            }
        }
        void ApplyStackEffect(StackEffect effect)
        {
            if (effect.applied) return;

            var manager = buffInfo.hostInfo.Buffs();

            foreach (var buff in effect.buffsOnStack)
            {
                manager.ApplyBuff(new(buff, buffInfo));
            }

            effect.applied = true;
        }
        void Update()
        {
        
        }
    }
}
