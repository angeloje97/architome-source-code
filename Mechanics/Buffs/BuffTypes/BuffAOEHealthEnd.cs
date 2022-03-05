using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class BuffAOEHealthEnd : MonoBehaviour
    {


        // Start is called before the first frame update
        public BuffInfo buffInfo;
        public AOEType aoeType;

        public bool isDamage;
        public bool isHealing;

        void Start()
        {
            if (GetComponent<BuffInfo>())
            {
                buffInfo = GetComponent<BuffInfo>();
                buffInfo.OnBuffCompletion += OnBuffCompletion;
                buffInfo.OnBuffCleanse += OnBuffCleanse;
            }
        }

        public void OnBuffCompletion(BuffInfo buff)
        {
            HandleHarm(buff);
            HandleAssist(buff);
        }

        public void OnBuffCleanse(BuffInfo buff)
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        public void HandleHarm(BuffInfo buff)
        {
            if (!isDamage) { return; }

            var enemies = buff.EnemiesWithinRange();
            var value = ProcessedValue(buff, enemies.Count);

            foreach (var i in enemies)
            {
                buffInfo.HandleTargetHealth(i, value, BuffTargetType.Harm);
            }
        }

        public void HandleAssist(BuffInfo buff)
        {
            if (!isHealing) { return; }

            var allies = buff.AlliesWithinRange();
            var value = ProcessedValue(buff, allies.Count);
            foreach (var i in allies)
            {
                buffInfo.HandleTargetHealth(i, value, BuffTargetType.Assist);
            }
        }

        public float ProcessedValue(BuffInfo buff, int count)
        {
            var aoeValue = buff.properties.aoeValue;
            switch (aoeType)
            {
                case AOEType.Distribute:
                    return aoeValue / count;
                case AOEType.Multiply:
                    return aoeValue * count;
                default:
                    return aoeValue;
            }
        }
    }
}

