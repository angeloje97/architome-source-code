using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class ArchString 
    {
        // Start is called before the first frame update
        public static string CamelToTitle(string text)
        {
            var newText = "";

            for (int i = 0; i < text.Length; i++)
            {
                if (i == 0)
                {
                    newText += $"{text[i]}".ToUpper();
                }
                else if($"{text[i]}" == $"{text[i]}".ToUpper())
                {
                    newText += $" {text[i]}";
                }
                else
                {
                    newText += text[i];
                }
            }

            return newText;
        }

        public static string TitleToCamel(string text)
        {
            var newText = "";

            for (int i = 0; i < text.Length; i++)
            {
                if (i == 0)
                {
                    newText += $"{text[i]}".ToUpper();
                    continue;
                }
                if (" "[0] == text[i])
                {
                    continue;
                }

                newText += text[i];
            }

            return newText;
        }

        public static string FloatToSimple(float value)
        {
            var newText = $"{Mathg.Round(value, 2)}";


            if (value > 1000f)
            {
                newText = $"{Mathg.Round(value / 1000f, 2)}k";
            }

            if (value > 1000000f)
            {
                newText = $"{Mathg.Round(value / 1000000f, 2)}m";
            }

            if (value > 1000000000f)
            {
                newText = $"{Mathg.Round(value / 1000000000, 2)}b";
            }


            return newText;
        }
    }

}