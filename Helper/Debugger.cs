using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debugger : MonoBehaviour
{
    // Start is called before the first frame update
    static bool debugging = true;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void InConsole(int id, string sentence)
    {
        if(debugging)
        {
            Debug.Log($"{id}: {sentence}");
        }
    }
}
