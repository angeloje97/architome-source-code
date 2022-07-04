using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Mathg
{
    public static float Round(float value, int digits)
    {
        float mult = Mathf.Pow(10.0f,(float)digits);
        return Mathf.Round(value * mult) / mult;
    }
}
