using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class RunSceneWithButton : MonoBehaviour
{
    // Start is called before the first frame update

    Button button;
    public string sceneName;
    void Start()
    {
        button = gameObject.GetComponent<Button>();

        button.onClick.AddListener(StartScene);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
