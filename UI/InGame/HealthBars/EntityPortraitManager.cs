using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;
public class EntityPortraitManager : MonoBehaviour
{
    // Start is called before the first frame update


    public List<PortraitBehavior> memberPortraits;
    public PortraitBehavior currentTargetPortrait;
    
    void Start()
    {
        Invoke("SetPartyPortraits", .250f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPartyPortraits()
    {
        PartyInfo pInfo;
        if(GMHelper.GameManager() && GMHelper.GameManager().playableParties != null && GMHelper.GameManager().playableParties.Count == 1)
        {
            pInfo = GMHelper.GameManager().playableParties[0];
        }
        else
        {
            return;
        }
        if(memberPortraits == null) { return; }


        foreach(GameObject member in pInfo.members)
        {
            var i = pInfo.members.IndexOf(member);
            
            if(i < memberPortraits.Count)
            {
                memberPortraits[i].gameObject.SetActive(true);
                memberPortraits[i].SetEntity(member.GetComponent<EntityInfo>());
            }
        }
    }
}
