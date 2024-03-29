using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RandomGen
{
    public static string RandomString(int length)
    {
        var result = "";

        var string1 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var string2 = "123456789";
        var string3 = string1.ToLower();

        string totalString = string1 + string2 + string3;

        for(int i = 0; i < length; i++)
        {
            result = result + $"{totalString[Random.Range(0, totalString.Length)]}";
        }
        return result;
    }
}
