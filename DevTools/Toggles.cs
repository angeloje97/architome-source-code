using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.DevTools
{
    public class Toggles : DevTool
    {
        public class Request
        {
            public string name;
            bool currentState;

            Action<Request> OnStateChange;
            public Dictionary<string, Type> attributes;


            public Request(string name, Action<bool> OnChangeState)
            {
                this.OnStateChange += (request) => {
                    OnChangeState?.Invoke(currentState);
                };
            }

            public Request(string name, Action<Request> OnStateChange)
            {
                this.OnStateChange += (request) => OnStateChange?.Invoke(request);
            }

            public void SetState(bool state)
            {
                this.currentState = state;
                OnStateChange?.Invoke(this);
            }

            public void ToggleState()
            {
                SetState(!currentState);
            }
        }

        public List<Request> functions = new()
        {
            new("Spawn Entity", (bool state) => {

                }){
                    attributes = {
                        { "EntityID", typeof(int) },
                        { "Level", typeof(int) },
                }},

            new("Spawn Item", (bool state) => { 
            }) 
            { 
                attributes = new()
                {
                    { "Item ID", typeof(int) },
                    { "Amount", typeof(int) }
                }    
            },

            new("Damage", (bool state) => {

                }){
                    attributes = {
                        { "Value", typeof(int) },
                }},

            new("Heal", (bool state) => {

                }){
                    attributes = {
                        { "Value", typeof(int) },
                }},

            new("Give EXP", (bool state) => {

                }){
                    attributes = {
                        { "Value", typeof(int) },
                }},
        };
    }
}
