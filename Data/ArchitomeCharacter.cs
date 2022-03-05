using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

public class ArchitomeCharacter : MonoBehaviour
{
    // Start is called before the first frame update

    public Sex sex;
    public bool isMale = true;
    public bool isFemale = false;
    public GameObject maleBody;
    public GameObject femaleBody;

    public bool created = false;
    public bool basePartsSet = false;
    public bool isHighlighted = false;

    public int currentMaterial = 0;
    public List<Material> materials;
    public Shader originalShader;
    public Shader highlightShader;

    public List<Vector2> originalParts;

    public List<List<GameObject>> bodyParts;
    public List<GameObject> head;                       //0
    public List<GameObject> helmets;
    public List<GameObject> eyeBrows;
    public List<GameObject> facialHair;
    public List<GameObject> torso;                      //4
    public List<GameObject> upperArmRight;
    public List<GameObject> upperArmLeft;
    public List<GameObject> lowerArmRight;
    public List<GameObject> lowerArmLeft;               //8
    public List<GameObject> handRight;
    public List<GameObject> handLeft;
    public List<GameObject> hips;
    public List<GameObject> legsRight;                  //12
    public List<GameObject> legsLeft;

    [Header("Non Binary")]

    public List<GameObject> headCoveringBaseHair;
    public List<GameObject> headCoveringNoFacialHair;
    public List<GameObject> headCoveringNoHair;         //16
    public List<GameObject> hair;
    public List<GameObject> helmetAttachMent;
    public List<GameObject> backAttachment;
    public List<GameObject> shoulderRight;              //20
    public List<GameObject> shoulderLeft;
    public List<GameObject> elbowRight;
    public List<GameObject> elbowLeft;
    public List<GameObject> hipsAttachment;             //24
    public List<GameObject> kneeAttachmentRight;
    public List<GameObject> kneeAttachmentLeft;
    public List<GameObject> extraElfEars;               //27


    void GetDependencies()
    {
        bodyParts = new List<List<GameObject>>();
        bodyParts.Add(head);                        // 0
        bodyParts.Add(helmets);
        bodyParts.Add(eyeBrows);
        bodyParts.Add(facialHair);
        bodyParts.Add(torso);                       //4
        bodyParts.Add(upperArmRight);
        bodyParts.Add(upperArmLeft);
        bodyParts.Add(lowerArmRight);
        bodyParts.Add(lowerArmLeft);                //8
        bodyParts.Add(handRight);
        bodyParts.Add(handLeft);
        bodyParts.Add(hips);
        bodyParts.Add(legsRight);                   //12
        bodyParts.Add(legsLeft);

        bodyParts.Add(headCoveringBaseHair);
        bodyParts.Add(headCoveringNoFacialHair);
        bodyParts.Add(headCoveringNoHair);          //16
        bodyParts.Add(hair);
        bodyParts.Add(helmetAttachMent);
        bodyParts.Add(backAttachment);
        bodyParts.Add(shoulderRight);               //20
        bodyParts.Add(shoulderLeft);
        bodyParts.Add(elbowRight);
        bodyParts.Add(elbowLeft);
        bodyParts.Add(hipsAttachment);              //24
        bodyParts.Add(kneeAttachmentRight);
        bodyParts.Add(kneeAttachmentLeft);
        bodyParts.Add(extraElfEars);

        GetOriginalShader();

        void GetOriginalShader()
        {
            if(AllChildren().Count > 0)
            {
                originalShader = AllChildren()[0].GetComponent<Renderer>().material.shader;
            }
        }
    }
    public void SetOriginalParts()
    {
        originalParts= new List<Vector2>();
        foreach(List<GameObject> bodyPart in bodyParts)
        {
            var i = bodyParts.IndexOf(bodyPart);
            if(bodyPart.Count > 0)
            {
                int j = ActivePartIndex(bodyPart[0]);
                originalParts.Add(new Vector2(i, j));
            }
        }
    }
    void Start()
    {
        GetDependencies();

        if(!created)
        {
            SetDefault();
            SetDefaultMaterial();
        }
        if(!basePartsSet)
        {
            SetOriginalParts();
            basePartsSet = true;
        }

        UpdateSex();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void HideAll()
    {
        if(bodyParts == null) { return;}
        foreach(List<GameObject> i in bodyParts)
        {
            foreach(GameObject j in i)
            {
                foreach(Transform child in j.transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }
    public void SetDefault()
    {
        if(bodyParts == null) { return; }
        HideAll();
        foreach(List<GameObject> i in bodyParts)
        {
            foreach (GameObject j in i)
            {
                if (j.transform.childCount > 0)
                {
                    j.transform.GetChild(0).gameObject.SetActive(true);
                }
            }
        }
        SetSex(Sex.Male);
    }
    public void ToggleSex()
    {
        if(!maleBody || !femaleBody) { return; }
        isMale = !isMale;
        isFemale = !isMale;

        UpdateSex();
    }
    void UpdateSex()
    {
        if(isMale && isFemale)
        {
            isMale = true;
            isFemale = false;
        }

        if (isMale)
        {
            maleBody.SetActive(true);
            femaleBody.SetActive(false);
        }
        else if (isFemale)
        {
            femaleBody.SetActive(true);
            maleBody.SetActive(false);
        }
    }
    public void SetSex(Sex sex)
    {
        if(!maleBody || !femaleBody) { return; }
        switch(sex)
        {
            case Sex.Male:
                isMale = true;
                isFemale = false;
                break;
            case Sex.Female:
                isMale = false;
                isFemale = true;
                break;
        }

        UpdateSex();
    }
    public void SetPart(int bodyPartNum, int partNum)
    {
        if (bodyParts.Count <= bodyPartNum) { return; }
        List<GameObject> bodyPartObject = bodyParts[bodyPartNum];

        if(bodyPartObject[0].transform.childCount <= partNum) { partNum = 0; }
        if(partNum < 0) { partNum = bodyPartObject[0].transform.childCount - 1; }
        
        foreach (GameObject part in bodyPartObject)
        {
            if(ActivePart(part)) { ActivePart(part).SetActive(false); }
        }

        foreach (GameObject part in bodyPartObject)
        {
            if (part.transform.childCount > partNum)
            {
                part.transform.GetChild(partNum).gameObject.SetActive(true);
            }
            
        }
    }
    public void NextPart(int bodyPartNum)
    {
        if (bodyParts.Count <= bodyPartNum) { return; }
        List<GameObject> bodyPartObject = bodyParts[bodyPartNum];
        
        foreach(GameObject bodyPart in bodyPartObject)
        {
            var activePart = ActivePartIndex(bodyPart);
            Debugger.InConsole(2495, $"{activePart}");
            activePart++;

            SetPart(bodyPartNum, activePart);
            break;
        }
    }
    public void PreviousPart(int bodyPartNum)
    {
        if (bodyParts.Count <= bodyPartNum) { return; }
        List<GameObject> bodyPartObject = bodyParts[bodyPartNum];

        foreach (GameObject bodyPart in bodyPartObject)
        {
            var activePart = ActivePartIndex(bodyPart);
            activePart--;

            SetPart(bodyPartNum, activePart);
        }
    }
    public GameObject ActivePart(GameObject bodyPart)
    {
        foreach (Transform child in bodyPart.transform)
        {
            if (child.gameObject.activeSelf == true)
            {
                return child.gameObject;
            }
        }

        return null;
    }
    public int ActivePartIndex(GameObject bodyPart)
    {
        var index = 0;
        foreach(Transform part in bodyPart.transform)
        {
            
            if(part.gameObject.activeSelf == true)
            {
                return index;
            }
            index++;
        }

        return 0;
    }
    public void SetDefaultMaterial()
    {
        SetMaterial(0);
    }
    public void SetMaterial(int material)
    {
        Material current = materials[material];
        foreach(List<GameObject> bodyPart in bodyParts)
        {
            foreach(GameObject part in bodyPart)
            {
                foreach(Transform child in part.transform)
                {
                    if(child.GetComponent<Renderer>())
                    {
                        var rend = child.GetComponent<Renderer>();
                        rend.material = current;
                    }
                    
                }
            }
        }
    }
    public void Highlight(bool val)
    {
        isHighlighted = val;
        UpdateHighlight();
    }
    public void ToggleHighlight()
    {
        isHighlighted = !isHighlighted;

        UpdateHighlight();
    }
    public void UpdateHighlight()
    {
        if(!highlightShader || !originalShader) { return; }

        foreach (GameObject child in AllChildren())
        {
            if (child.activeSelf == true)
            {
                if(child.GetComponent<Renderer>())
                {
                    var rend = child.GetComponent<Renderer>();
                    rend.material.shader = isHighlighted ? highlightShader : originalShader;
                }
                
            }
        }
    }
    
    public List<GameObject> AllChildren()
    {
        List<GameObject> children = new List<GameObject>();
        foreach(List<GameObject> bodyPart in bodyParts)
        {
            foreach(GameObject part in bodyPart)
            {
                foreach(Transform child in part.transform)
                {
                    
                    children.Add(child.gameObject);
                }
            }
        }

        return children;
    }
    public void NextMaterial()
    {
        currentMaterial++;

        if(currentMaterial >= materials.Count)
        {
            currentMaterial = 0;
        }
        

        SetMaterial(currentMaterial);
    }
    public void PreviousMaterial()
    {
        currentMaterial--;

        if (currentMaterial < 0)
        {
            currentMaterial = materials.Count - 1;
        }

        SetMaterial(currentMaterial);
    }
    public void LoadValues()
    {
        foreach(Vector2 original in originalParts)
        {
            SetPart((int)original.x, (int)original.y);
        }
    }

}
