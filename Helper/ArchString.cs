using DungeonArchitect.Flow.Domains.Layout.Tasks;
using Language.Lua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Architome
{
    public delegate string StringModifier(string original);


    public static class ArchString
    {

        static string DefaultModifier(string original)
        {
            return original;
        }

        public static Dictionary<string, Regex> stringPatterns;
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


                    if(i > 0 && text[i-1].Equals(" "))
                    {
                        newText += text[i];
                        continue;
                    }
                    if (text[i].Equals(" "))
                    {
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
        public static string TitleToCamel(string text)
        {
            var newText = "";

            for (int i = 0; i < text.Length; i++)
            {
                if (i == 0)
                {
                    newText += $"{text[i]}".ToLower();
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

        #region String List

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
                if (list.Trim().Length > 0 && strings[i].Trim().Length > 0)
                {
                    list += ", ";
                }

                list += $"{strings[i]}";

            }

            return list;
        }
        public static string NextLineList(List<string> stringList, int extraSpace = 0, StringModifier modifier = null)
        {
            if (modifier == null)
            {
                modifier = DefaultModifier;
            }

            var result = new StringBuilder();

            foreach (var stringValue in stringList)
            {
                if (result.Length > 0 && stringValue.Length > 0)
                {
                    for (int i = 0; i < extraSpace+1; i++)
                    {
                        result.Append("\n");
                    }
                }

                result.Append(modifier(stringValue));
            }

            return result.ToString();
        }
        public static List<string> ModifyList(List<string> original,  StringModifier modifier)
        {
            var result = new List<string>();

            foreach(var str in original)
            {
                result.Add(modifier(str));
            }

            return result;
        }

        #endregion


        public static string Spacer(string line)
        {
            if (line.Length > 0)
            {
                return $" {line}";
            }

            return line;
        }
        



        static Regex StringRegex(string pattern)
        {
            stringPatterns ??= new();
            if (!stringPatterns.ContainsKey(pattern))
            {
                stringPatterns.Add(pattern, new Regex(pattern));
            }

            return stringPatterns[pattern];
        }

        public static string Replace(string input, string pattern, string output)
        {
            return StringRegex(pattern).Replace(input, output);
        }

        public static string NextLine(string line)
        {
            if (line.Length > 0)
            {
                line += "\n";
            }

            return line;
        }

        #region Floats and Numbers
        public static string FloatToSimple(float value, int decimalPlaces = 2)
        {
            var newText = $"{Mathg.Round(value, decimalPlaces)}";
            

            if (value > 100000f) //100 Thousand
            {
                newText = $"{Mathg.Round(value / 1000f, decimalPlaces)}k";
            }

            if (value > 100000000f) //100 Millions
            {
                newText = $"{Mathg.Round(value / 1000000f, decimalPlaces)}m";

            }

            if (value > 100000000000f) //100 Billions.
            {
                newText = $"{Mathg.Round(value / 1000000000f, decimalPlaces)}b";

            }


            return newText;
        }

        public static string FloatToTimer(float time)
        {
            string text = "";

            if (time >= 3600)
            {
                text += $"{(int)(time / 3600)}hr "; 
            }

            if (time >= 60)
            {
                var minutes = time % 3600;
                text += $"{(int)(minutes / 60)}min ";
            }

            if (time % 60 > 0)
            {
                text += $"{(int)(time % 60)}s";
            }


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

        public static string RoundedTime(int seconds, bool showMinuteSeconds = false)
        {
            var result = "";

            if (seconds > 3600)
            {
                result += $"{seconds / 3600}hr";

            }
            else if (seconds > 60)
            {
                result += $"{seconds / 60}m";
                if (showMinuteSeconds)
                {
                    result += $"{seconds%60}s";
                }
            }
            else
            {
                result += $"{seconds}s";
            }

            return result;
        }
        #endregion

        public static string GetLast(string source, int amount)
        {
            if(source.Length <= amount)
            {
                return source;
            }

            return source[^amount..];
        }

    }

}