using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Architome
{
    public class Clicker : MonoBehaviour
    {
        // Start is called before the first frame update

        public int singleClicks;
        public int doubleClicks;

        public bool clicked;
        public bool holdClick;

        public float doubleClickTimer;

        private void Update()
        {
            OnClick();
            HoldClick();
        }

        async void OnClick()
        {
            if (!Input.GetKeyDown(KeyCode.Mouse0)) return;
            singleClicks++;


            if (clicked)
            {
                doubleClicks++;
                clicked = false;
                return;
            }

            clicked = true;

            float timer = doubleClickTimer;

            while (timer > 0)
            {
                timer -= Time.deltaTime;
                if (clicked == false)
                {
                    return;
                }
                await Task.Yield();

            }



            clicked = false;
        }

        void HoldClick()
        {
            if (holdClick != Input.GetKey(KeyCode.Mouse0))
            {
                holdClick = Input.GetKey(KeyCode.Mouse0);
            }
        }

    }
}
