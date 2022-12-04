using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Architome.Enums;
using Architome;
public class ActionBarsInfo : MonoBehaviour
{
    public static ActionBarsInfo active;

    public List<ActionBarBehavior> actionBars;
    public List<ActionBarBehavior> miscActionBars;
    public ActionBarBehavior partyActionBar;
    public ActionBarBehavior passiveActionBar;


    public AbilityClickHandler currentAbilityClickHandler;

    //Private fields
    [SerializeField]
    private int defaultPartyMemberIndex;

    void Start()
    {
        //Invoke("SetPartyActionBars", .250f);
        GameManager.active.OnNewPlayableEntity += OnNewPlayableEntity;
    }

    private void Awake()
    {
        active = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool Busy()
    {
        return currentAbilityClickHandler != null;
    }

    public void OnNewPlayableEntity(EntityInfo newEntity, int index)
    {
        Debugger.InConsole(6493, $"New Member ({newEntity}) Index: {index}");

        var signature = newEntity.AbilityManager().Ability(AbilityType2.MainAbility);
        var party = newEntity.AbilityManager().Ability(AbilityType2.PartyAbility);
        var utility = newEntity.AbilityManager().Ability(AbilityType2.Utility);

        
        if(index == defaultPartyMemberIndex)
        {
            if(newEntity.AbilityManager().Ability(AbilityType2.PartyAbility))
            {
                partyActionBar.SetActionBar(newEntity.AbilityManager().Ability(AbilityType2.PartyAbility));
            }
        }

        if(actionBars.Count > index && signature != null)
        {
            actionBars[index].SetActionBar(signature);
        }

        if(miscActionBars.Count > index)
        {
            miscActionBars[index].SetActionBar(utility);
        }
    }

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
