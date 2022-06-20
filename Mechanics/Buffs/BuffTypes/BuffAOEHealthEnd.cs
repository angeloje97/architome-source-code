using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class BuffAOEHealthEnd : BuffType
    {

        public AOEType aoeType;

        public bool heals;
        public bool damages;

        new void GetDependencies()
        {
            base.GetDependencies();

            if (buffInfo)
            {
                buffInfo.OnBuffCompletion += OnBuffCompletion;
                buffInfo.OnBuffCleanse += OnBuffCleanse;
            }
        }
        void Start()
        {
            GetDependencies();
            //if (GetComponent<BuffInfo>())
            //{
            //    buffInfo = GetComponent<BuffInfo>();
            //    buffInfo.OnBuffCompletion += OnBuffCompletion;
            //    buffInfo.OnBuffCleanse += OnBuffCleanse;
            //}
        }

        public override string Description()
        {
            buffInfo = GetComponent<BuffInfo>();

            var result = $"At the end of the buff, all targets within a {buffInfo.properties.radius} meter radius will ";

            if (heals)
            {
                result += $"heal allies for {value}";

                if (damages)
                {
                    result += " and ";
                }
            }

            if (damages)
            {
                result += $"damage enemies for {value}";
            }

            result += $" that will {ArchString.CamelToTitle(aoeType.ToString())} across all targets.\n";

            return result;
        }

        public override string GeneralDescription()
        {
            var result = "";



            return result;
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
            if (!damages) return;

            var enemies = buff.EnemiesWithinRange();
            var value = ProcessedValue(buff, enemies.Count);

            foreach (var i in enemies)
            {
                buffInfo.HandleTargetHealth(i, value, BuffTargetType.Harm);
            }
        }

        public void HandleAssist(BuffInfo buff)
        {
            if (!heals) return;

            var allies = buff.AlliesWithinRange();
            var value = ProcessedValue(buff, allies.Count);
            foreach (var i in allies)
            {
                buffInfo.HandleTargetHealth(i, value, BuffTargetType.Assist);
            }
        }

        public float ProcessedValue(BuffInfo buff, int count)
        {
            
            switch (aoeType)
            {
                case AOEType.Distribute:
                    return value / count;
                case AOEType.Multiply:
                    return value * count;
                default:
                    return value;
            }
        }
    }
}

