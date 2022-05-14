using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Architome
{
    public class DataStructs
    {
    }

    [Serializable]
    public struct Float3
    {
        public float x, y, z;

        public Float3(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public Vector3 ToVector()
        {
            return new Vector3(x, y, z);
        }
    }

    [Serializable]
    public struct Float2
    {
        public float x;
        public float y;

        public Float2(Vector2 vector)
        {
            x = vector.x;
            y = vector.y;
        }

        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }
    }

    [Serializable]
    public struct String2
    {
        public string x, y;

        public String2(string x, string y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
