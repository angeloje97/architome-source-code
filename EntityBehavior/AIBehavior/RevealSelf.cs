using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
public class RevealSelf : MonoBehaviour
{
    // Start is called before the first frame update
    public EntityInfo entityInfo;
    public GraphicsInfo graphicsInfo;
    public CharacterInfo characterInfo;
    public AIBehavior behavior;

    public ContainerTargetables targetManager;
    public DifficultyModifications difficulty;
    public LayerMasksData layerMasks;

    public float hidingInterval;

    public bool dynamicGraphics;
    public bool dynamicCharacter;

    public RoomInfo currentRoom;

    void GetDependencies()
    {
        if(GetComponentInParent<EntityInfo>())
        {
            entityInfo = GetComponentInParent<EntityInfo>();

            if(entityInfo.GraphicsInfo())
            {
                graphicsInfo = entityInfo.GraphicsInfo();
            }

            if (entityInfo.CharacterInfo())
            {
                characterInfo = entityInfo.CharacterInfo();
            }


            entityInfo.OnPlayerLineOfSight += OnPlayerLineOfSight;
            entityInfo.OnPlayerOutOfRange += OnPlayerOutOfRange;
            entityInfo.OnPlayerLOSBreak += OnplayerLOSBreak;
            entityInfo.OnCurrentShowRoom += OnCurrentShowRoom;
            entityInfo.OnRoomChange += OnRoomChange;

            entityInfo.partyEvents.OnAddedToParty += OnAddedToParty;
        }

        if(GetComponentInParent<AIBehavior>())
        {
            behavior = GetComponentInParent<AIBehavior>();
        }

        if(GMHelper.TargetManager())
        {
            targetManager = GMHelper.TargetManager();
        }

        if(GMHelper.Difficulty())
        {
            difficulty = GMHelper.Difficulty();
        }


        layerMasks = LayerMasksData.active;
    }

    public void OnAddedToParty(PartyInfo party)
    {
        dynamicGraphics = false;
        dynamicCharacter = false;

        entityInfo.ShowEntity(true);
    }

    void Start()
    {
        GetDependencies();
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void OnPlayerLineOfSight(EntityInfo player, PartyInfo party)
    {
        //HandleVisibility();
    }

    public void OnPlayerOutOfRange(EntityInfo player, PartyInfo party)
    {
        //HandleVisibility();
    }

    public void OnplayerLOSBreak(EntityInfo player, PartyInfo party)
    {
        //HandleVisibility();
    }

    public void OnCurrentShowRoom(RoomInfo info, bool val)
    {
        if (entityInfo == null) return;
        if (entityInfo.gameObject == null) return;
        if (Entity.IsPlayer(entityInfo)) { return; }

        entityInfo.ShowEntity(val);
    }

    public void OnRoomChange(RoomInfo previous, RoomInfo current)
    {
        
        HandleNonPlayer();
        HandlePlayer();
       


        void HandleNonPlayer()
        {
            if(!entityInfo.isAlive) { return; }
            if (Entity.IsPlayer(entityInfo)) { return; }

            if(current != null)
            {
                entityInfo.ShowEntity(current.isRevealed);
            }

        }
        
        void HandlePlayer()
        {
            if (!Entity.IsPlayer(entityInfo)) { return; }
            if(previous == null) { return; }
        }

    }

    void HandleVisibility()
    {
        if(!dynamicGraphics && dynamicCharacter) { return; }
        HandleGraphics();
        HandleCharacter();

        void HandleGraphics()
        {
            if(!dynamicGraphics) { return; }
            if(graphicsInfo == null) { return; }

            ShowGraphics(PlayerHasLineOfSight() && CurrentRoomIsRevealed());
           
        }

        void HandleCharacter()
        {
            if (!dynamicCharacter) { return; }
        }
    }

    void ShowGraphics(bool val)
    {
        foreach(Transform child in graphicsInfo.transform)
        {
            if(child.gameObject.activeSelf != val)
            {
                child.gameObject.SetActive(val);
            }
        }
    }




    public bool PlayerHasLineOfSight()
    {
        Collider[] entities = Physics.OverlapSphere(entityInfo.transform.position, difficulty.settings.playerDetectionRange, layerMasks.entityLayerMask);

        for(int i = 0; i < entities.Length; i++)
        {
            var current = entities[i].GetComponent<EntityInfo>();

            if(current == null) { continue; }

            if (!Player.IsPlayer(current)) { continue; }

            if(current.LineOfSight().HasLineOfSight(entityInfo.gameObject))
            {
                return true;
            }
        }

        return false;
    }

    public bool CurrentRoomIsRevealed()
    {
        if(entityInfo.currentRoom == null)
        {
            if(entityInfo.CurrentRoom())
            {
                entityInfo.currentRoom = entityInfo.CurrentRoom();
            }
            else
            {
                return true;
            }
        }

        return entityInfo.currentRoom.isRevealed;

    }
}
