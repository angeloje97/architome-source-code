using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class ContainerMouseOvers : MonoBehaviour
    {
        // Start is called before the first frame update
        public List<GameObject> mouseOvers;
        void Start()
        {
            StartCoroutine(MouseOverRoutine());
        }

        // Update is called once per frame
        void Update()
        {
        }

        IEnumerator MouseOverRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(.25f);

                mouseOvers = Mouse.AllMouseOvers();
            }
        }
    }

}