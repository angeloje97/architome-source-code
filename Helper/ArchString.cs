using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public static class ArchString 
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
                    if (i > 0 && char.IsNumber(text[i]) && char.IsNumber(text[i - 1]))
                    {
                        newText += $"{text[i]}";
                        continue;
                    }


                    newText += $" {text[i]}";
                }
                else
                {
                    newText += text[i];
                }
            }

            return newText;
        }

        public static string StringList(List<string> strings)
        {
            if (strings == null || strings.Count == 0)
            {
                return "";
            }

            if (strings.Count == 1)
            {
                return strings[0];
            }

            var list = "";

            for (int i = 0; i < strings.Count; i++)
            {
                if (i == 0)
                {
                    list += strings[i];
                    continue;
                }

                list += $", {strings[i]}";
            }

            return list;
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

        public static string Spacer(string line)
        {
            if (line.Length > 0)
            {
                return $" {line}";
            }

            return line;
        }

        public static string NextLineList(List<string> stringList)
        {
            var result = "";
            for (int i = 0; i < stringList.Count; i++)
            {
                var item = stringList[i];

                if (i == 0)
                {
                    result += item;
                    continue;
                }

                if (item.Length > 0)
                {

                    if (result.Length > 0 && item.Length > 0 && i != stringList.Count - 1)
                    {
                        result += "\n";
                    }

                    result += $"{item}";
                }
            }

            return result;
        }

        public static string NextLine(string line)
        {
            if (line.Length > 0)
            {
                line += "\n";
            }

            return line;
        }

        public static string FloatToSimple(float value)
        {
            var newText = $"{Mathg.Round(value, 2)}";


            if (value > 100000f) //100 Thousand
            {
                newText = $"{Mathg.Round(value / 1000f, 2)}k";
            }

            if (value > 100000000f) //100 Millions
            {
                newText = $"{Mathg.Round(value / 1000000f, 2)}m";
            }

            if (value > 100000000000f) //100 Billions.
            {
                newText = $"{Mathg.Round(value / 1000000000, 2)}b";
            }

            return newText;
        }

        public static string FloatToTimer(float time)
        {
            string text = "";

            if (time > 3600)
            {
                text += $"{(int)(time / 3600)}hr "; 
            }

            if (time > 60)
            {
                var minutes = time % 3600;
                text += $"{(int)(minutes / 60)}min ";
            }

            text += $"{(int)(time % 60)}s";

            //switch(time)
            //{
            //    case > 3600:
            //        text = $"{(int) (time/3600)}h";
            //        break;
            //    case > 60:
            //        text = $"{(int) (time / 60)}m";
            //        break;
            //    default:
            //        text = $"{(int) time}s";
            //        break;
            //}

            return text;
        }

    }

}