
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{

    public class EnumValue : Attribute
    {
        string value;
        public string Value => value;

        public EnumValue(string value)
        {
            this.value = value;
        }

        //Example

        enum Direction
        {
            [EnumValue("North")] N,
            [EnumValue("West")] W,
            [EnumValue("South")] S,
            [EnumValue("E")] E
        }

        void PrintDirection()
        {
            Console.WriteLine(EnumString.GetValue(Direction.N));
        }


    }

    public static class EnumString 
    {
        public static string GetValue(Enum value)
        {
            var type = value.GetType();

            var field = type.GetField(value.ToString());

            var attributes = field.GetCustomAttributes(typeof(EnumValue), false) as EnumValue[];

            if(attributes.Length > 0)
            {
                return attributes[0].Value;
            }

            return value.ToString();
        }
    }

}
