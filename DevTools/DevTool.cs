using Architome.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.DevTools
{
    public class DevTool : MonoBehaviour
    {
        [SerializeField] protected List<GameState> _availableStates;

        public virtual List<GameState> availableStates
        {
            get
            {
                if(_availableStates == null)
                {
                    _availableStates = new();

                    foreach(GameState state in Enum.GetValues(typeof(GameState)))
                    {
                        _availableStates.Add(state);
                    }
                }

                return _availableStates;
            }
        }
    }

    public class Request
    {
        public string name;
        public Dictionary<string, Type> attributes;


        public Request(string name)
        {
            this.name = name;
        }
    }
}