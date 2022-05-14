using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Architome
{
    public class MenuCamera : MonoBehaviour
    {
        MainMenuUI menuUI;

        public float smoothening;

        Transform target;

        bool isMoving;

        public bool _isMoving { get { return isMoving; } }

        public void GetDependencies()
        {
            menuUI = MainMenuUI.active;

            if (menuUI)
            {
                menuUI.OnOpenMenu += OnOpenMenu;
            }
        }

        void Start()
        {
            GetDependencies();
        }


        async void OnOpenMenu(MenuModule module)
        {
            var target = module.cameraPosition;


            if (target != null)
            {
                this.target = target;
            }
            else
            {
                this.target = menuUI.defaultCameraPosition;
            }


            if (isMoving) return;
            isMoving = true;



            while (transform.position != target.transform.position &&
                transform.rotation != target.transform.rotation)
            {
                transform.SetPositionAndRotation(Vector3.Lerp(transform.position, target.transform.position, 1 / smoothening), 
                                                Quaternion.Lerp(transform.rotation, target.transform.rotation, 1 / smoothening));

                await Task.Yield();
            }

            isMoving = false;

        }

    }
}
