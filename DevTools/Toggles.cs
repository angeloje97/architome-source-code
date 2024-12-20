using System;
using System.Collections.Generic;

namespace Architome.DevTools
{
    public class Toggles : DevTool
    {
        public override DevToolType type
        {
            get
            {
                return DevToolType.Toggle;
            }
        }

       
        #region Requests

        public List<ToggleRequest> requests
        {
            get
            {
                return new()
                {
                    new("Test", (bool val) => {

                        Debugger.System(1043, "Testing Toggle");
                    })
                    //new("Spawn Entity", (bool state) => {

                    //    }){
                    //        attributes = {
                    //            { "EntityID", typeof(int) },
                    //            { "Level", typeof(int) },
                    //    }},

                    //new("Spawn Item", (bool state) => {
                    //})
                    //{
                    //    attributes = new()
                    //    {
                    //        { "Item ID", typeof(int) },
                    //        { "Amount", typeof(int) }
                    //    }
                    //},

                    //new("Damage", (bool state) => {

                    //    }){
                    //        attributes = {
                    //            { "Value", typeof(int) },
                    //    }},

                    //new("Heal", (bool state) => {

                    //    }){
                    //        attributes = {
                    //            { "Value", typeof(int) },
                    //    }},

                    //new("Give EXP", (bool state) => {

                    //    }){
                    //        attributes = {
                    //            { "Value", typeof(int) },
                    //    }},
                };
            }
        }

        #endregion
    }

    #region Toggle Requests
    public class ToggleRequest : Request
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

        #region Setting States
        public void SetState(bool state, Dictionary<string, object> parameters)
        {
            this.parameters = parameters;
            this.currentState = state;
            OnStateChange?.Invoke(this);
        }

        public void ToggleState(Dictionary<string, object> parameters)
        {
            SetState(!currentState, parameters);
        }
        #endregion
    }
    #endregion

}
