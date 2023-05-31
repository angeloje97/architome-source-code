
using Architome.Settings.Keybindings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.Tutorial
{
    public class RotateCameraListener : EventListener
    {
        [Header("Rotate Camera Properties")]
        public CameraAnchor anchor;
        public float targetAmount, currentAmount;
        void Start()
        {
            GetDependencies();
            HandleStart();
        }

        public override void StartEventListener()
        {
            base.StartEventListener();

            anchor.OnAngleChange += OnCameraAngleChange;

            OnSuccessfulEvent += (EventListener listener) => {
                anchor.OnAngleChange -= OnCameraAngleChange;
            };
        }

        void OnCameraAngleChange(float before, float after)
        {
            var difference = Mathf.Abs(after - before);

            currentAmount += difference;

            if (currentAmount > targetAmount)
            {
                CompleteEventListener();
            }
        }

        public override string Directions()
        {
            var rotatorIndex = keyBindData.SpriteIndex(KeybindSetType.Party, "CameraRotator");

            var result = new List<string>() {
                base.Directions(),
                $"Rotate the camera by holding <sprite={rotatorIndex}> and moving the mouse",
            };

            return ArchString.NextLineList(result);
        }

        public override string Tips()
        {
            var stringList = new List<string>() {
                base.Tips() 
            };



            return ArchString.NextLineList(stringList);
        }
    }
}
