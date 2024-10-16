using System;
using System.Collections.Generic;

namespace Architome.DevTools
{
    public class Toggles : DevTool
    {
        public class ToggleRequest: Request
        {
            #region Common Data
            bool currentState;

            Action<ToggleRequest> OnStateChange;
            #endregion

            #region Instantiation

            public ToggleRequest(string name, Action<bool> OnChangeState) : base(name)
            {
                this.OnStateChange += (request) => {
                    OnChangeState?.Invoke(currentState);
                };
            }

            public ToggleRequest(string name, Action<Request> OnStateChange) : base(name)
            {
                this.OnStateChange += (request) => OnStateChange?.Invoke(request);
            }
            #endregion

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

        #region Requests

        public readonly List<ToggleRequest> requests = new()
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

        #endregion
    }
}
