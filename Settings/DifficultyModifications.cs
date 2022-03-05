using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;
public class DifficultyModifications : MonoBehaviour
{
    // Start is called before the first frame update
    public Difficulty gameDifficulty;
    [SerializeField]
    private bool startUpdate;
    public List<DifficultySet> difficultySets;
    public DifficultySet settings;


    public void DetermineSettings()
    {
        settings = difficultySets[((int) gameDifficulty) - 1];
    }

    void Start()
    {
        DetermineSettings();
    }

    public void OnValidate()
    {
        if(startUpdate)
        {
            startUpdate = false;
            DetermineSettings();
        }
     
    }

    // Update is called once per frame
    void Update()
    {
        
    }




}

[Serializable]
public class DifficultySet
{
    public float tankThreatMultiplier = 9f;
    public float healThreatMultiplier = .0625f;
    public float tankHealthMultiplier=2.5f;
    public float npcDetectionRange = 10f;
    public float playerDetectionRange = 45f;
    public float experienceMultiplier = 300f;
}
