using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PromptInfo : ModuleInfo
{
    // Start is called before the first frame update

    public void EndOptions()
    {
        PlaySound(false);
        Destroy(gameObject);
    }
    void Start()
    {
        PlaySound(true);
    }

    public void PlaySound(bool open)
    {
        try
        {
            var inGameUI = GetComponentInParent<IGGUIInfo>();
            var audioManager = inGameUI.GetComponent<AudioManager>();

            var audioClip = open ? openSound : closeSound;
            if(audioClip != null)
            {
                audioManager.PlaySound(audioClip);
            }

        }
        catch { }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
