using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Architome
{
    [Serializable]
    public struct FieldMarkUp<T>
    {
        public T sourceObj;
        [TextArea(5, 10)]
        public string source;
        [TextArea(5, 10)]
        public string output;

        public int maxRecursions;
        int recursions;

        List<string> variableNames;
        List<int> openIndeces;
        List<int> closedIndeces;

        public UnityEvent<string> OnGenerateOutput;


        public void Validate(T obj)
        {
            sourceObj = obj;
            variableNames = new();
            openIndeces = new();
            closedIndeces = new();

            output = InterperateString(source);

            OnGenerateOutput?.Invoke(output);
        }

        string InterperateString(string str, bool fromRecursion = false)
        {
            if (!fromRecursion)
            {
                recursions = 0;
            }

            StringBuilder builder = new();
            for (int i = 0; i < str.Length; i++)
            {
                var (validationName, validationEndIndex) = FieldName(str, '<', '>', i);
                var (fieldName, fieldEndIndex) = FieldName(str,'{', '}', i);



                if (validationName.Length > 0 && !validationName.EndsWith("/"))
                {
                    var (innerString, innerStringEndIndex) = StringUntil(str, $"<{validationName}/>", i, validationName.Length + 2);
                    if (BooleanField(validationName))
                    {
                        recursions++;
                        if(recursions <= maxRecursions)
                        {
                            var interpretedString = InterperateString(innerString, true);
                            builder.Append(interpretedString);
                        }
                        else
                        {
                            builder.Append(innerString);
                        }
                        recursions--;
                    }


                    i = innerStringEndIndex;

                }

                if (fieldName.Length > 0)
                {
                    builder.Append(FieldNameValue(fieldName));
                    i = fieldEndIndex;
                }



                if (i >= str.Length) break;
                builder.Append(str[i]);
            }

            return builder.ToString();
        }

        (string, int) FieldName(string source, char opening, char close, int start)
        {
            if (!source[start].Equals(opening))
            {
                return ("", start);
            }

            openIndeces.Add(start);
            start += 1;
            
            var builder = new StringBuilder();

            for(int i = start; i < source.Length; i++)
            {
                if (source[i].Equals(close))
                {
                    closedIndeces.Add(i);
                    var variableName = builder.ToString();
                    variableNames.Add(variableName);

                    if(variableName.Length == 0)
                    {
                        variableName = "NULL";
                    }

                    return (variableName, i + 1);
                }
                builder.Append(source[i]);
            }

            return ("", start);
        }

        (string, int) StringUntil(string source, string endString, int start, int skip)
        {
            var builder = new StringBuilder();
            var finalPos = start;
            bool foundString = false;

            for(int i = start + skip; i < source.Length; i++)
            {
                for(int j = 0; j < endString.Length; j++)
                {
                    if (i + j >= source.Length) return ("", start);
                    if (!endString[j].Equals(source[i + j])) break;
                    if (j == endString.Length - 1) foundString = true;
                    finalPos = i + j + 1;
                }
                if (foundString) break;

                builder.Append(source[i]);
            }

            if (foundString)
            {
                return (builder.ToString(), finalPos);
            }
            else
            {
                return ("", start);
            }
        }

        bool BooleanField(string fieldName)
        {
            var value = FieldNameValue(fieldName);
            
            if (value.GetType() != typeof(bool)) return false;

            return (bool)value;
        }

        object FieldNameValue(string fieldName)
        {
            bool negation = false;

            if (fieldName[0] == '!')
            {
                negation = true;
                fieldName = fieldName[1..];
            }

            var field = typeof(T).GetField(fieldName);
            if(field != null)
            {
                var value = field.GetValue(sourceObj);

                if(value.GetType() == typeof(bool))
                {
                    var boolValue = (bool)value;
                    if (negation)
                    {
                        return !boolValue;
                    }

                    return boolValue;
                }

                return value.ToString();
            }

            return "NULL";
        }



    }
}
