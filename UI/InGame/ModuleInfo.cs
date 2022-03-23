using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;


[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(CanvasGroup))]
public class ModuleInfo : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerExitHandler
{
    // Start is called before the first frame update
    public CanvasGroup canvasGroup;
    
    public bool isActive;
    public bool isHovering;
    public TextMeshProUGUI title;

    public Transform itemBin;

    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioSource audioSource;

    public Action<bool> OnActiveChange;


    public void GetDependencies()
    {
        if(GetComponent<CanvasGroup>() == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        else
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if(audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = GMHelper.Mixer().UI;
            audioSource.playOnAwake = false;
        }
    }

    void Start()
    {
        GetDependencies();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        if (!eventData.pointerDrag ||
            !eventData.pointerDrag.GetComponent<ItemInfo>() ||
            itemBin == null) { return; }

        eventData.pointerDrag.transform.SetParent(itemBin);
        transform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.SetAsLastSibling();
    }

    public void DestroyBin()
    {
        if(itemBin)
        {
            foreach(Transform child in itemBin)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void Close()
    {
        SetActive(false);
    }

    public void OnValidate()
    {
        if(canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();  
        }

        if(audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        SetActive(isActive, false);
    }

    public void SetActive(bool val, bool playSound = true)
    {
        if(canvasGroup)
        {
            canvasGroup.interactable = val;
            canvasGroup.blocksRaycasts = val;
            canvasGroup.alpha = val ? 1 : 0;
        }
        isActive = val;

        OnActiveChange?.Invoke(val);
        if(playSound)
        {
            if(openSound && closeSound)
            {
                audioSource.clip = val ? openSound : closeSound;
                audioSource.Play();

            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
