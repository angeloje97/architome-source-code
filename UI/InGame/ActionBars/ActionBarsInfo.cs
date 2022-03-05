using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Architome.Enums;
using Architome;
public class ActionBarsInfo : MonoBehaviour
{
    // Start is called before the first frame update


    public List<ActionBarBehavior> actionBars;
    public List<ActionBarBehavior> miscActionBars;
    public ActionBarBehavior partyActionBar;
    public ActionBarBehavior passiveActionBar;


    //Private fields
    [SerializeField]
    private int defaultPartyMemberIndex;

    void Start()
    {
        Invoke("SetPartyActionBars", .250f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public void SetMainActionBarsParty(AbilityInfo abilityInfo, int actionBarIndex)
    //{
    //    if(actionBars == null) { return; }
    //    if(actionBarIndex >= actionBars.Count) { return; }
    //    actionBars[actionBarIndex].SetActionBar(abilityInfo);
    //}

    //public void SetPartyPortrait(EntityInfo entity, int actionBarIndex)
    //{
    //    if(actionBars == null) { return; }
    //    if(actionBarIndex >= actionBars.Count) { return; }
    //    actionBars[actionBarIndex].SetCharacterPortrait(entity);
    //}

    public void SetPartyActionBars()
    {
        if(GMHelper.GameManager() && GMHelper.GameManager().playableParties != null && GMHelper.GameManager().playableParties.Count == 1)
        {
            PartyInfo pInfo = GMHelper.GameManager().playableParties[0];
            var members = pInfo.members;

            for(int i = 0; i < actionBars.Count; i++)
            {
                if (i >= members.Count) { break; }
                var abilities = members[i].GetComponent<EntityInfo>().AbilityManager();
                var signature = abilities.Ability(AbilityType2.MainAbility);
                               
                if(signature == null) { continue; }

                actionBars[i].SetActionBar(signature);
            }


            for(int i = 0; i < miscActionBars.Count; i++)
            {
                if (i >= members.Count) { break; }

                var abilities = members[i].GetComponent<EntityInfo>().AbilityManager();
                var utility = abilities.Ability(AbilityType2.Utility);

                if(utility == null) { continue; }

                miscActionBars[i].SetActionBar(utility);
            }

            var partyAbility = members[defaultPartyMemberIndex].GetComponent<EntityInfo>().AbilityManager().Ability(AbilityType2.PartyAbility);

            if(partyAbility!= null)
            {
                partyActionBar.SetActionBar(partyAbility);
            }

        }
    }
}
