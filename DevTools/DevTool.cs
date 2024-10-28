﻿using Architome.Enums;
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
        public Dictionary<string, object> parameters;

        protected Action<Request> baseAction;
        

        public Request(string name)
        {
            this.name = name;
        }

        public Request(string name, Action action)
        {
            this.name = name;
            this.baseAction += (request) => action();
        }

        public Request(string name, Action<Request> action)
        {
            this.name = name;
            this.baseAction += action;
        }

        public virtual void Invoke(Dictionary<string, object> parameters)
        {
            this.parameters = parameters;
            baseAction?.Invoke(this);
        }

        public virtual void Invoke()
        {
            this.parameters = null;
            baseAction?.Invoke(this);
        }


    }
}