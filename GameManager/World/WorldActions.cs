using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome;
public class WorldActions : MonoBehaviour
{
    // Start is called before the first frame update
    public static WorldActions active;

    //events

    public Action<GameObject> OnWorldReviveB;
    public Action<GameObject> OnWorldReviveA;

    public void Awake()
    {
        active = this;
    }
    public void Restore(GameObject entity, float health = 1, float mana = 1)
    {
        var entityInfo = entity.GetComponent<EntityInfo>();
        if(entityInfo == null) { return; }

        entityInfo.health = health * entityInfo.maxHealth;
        entityInfo.mana = mana * entityInfo.maxMana;


        if(!entityInfo.isAlive)
        {
            entityInfo.isAlive = true;
        }

    }

    public void Revive(GameObject entity, Vector3 position = new Vector3(), float health = 1, float mana = 1)
    {
        var entityInfo = entity.GetComponent<EntityInfo>();
        if(entityInfo == null) { return; }

        OnWorldReviveB?.Invoke(entity);

        if(position != new Vector3())
        {
            entity.transform.position = position;
            entityInfo.currentRoom = entityInfo.CurrentRoom();
        }


        Restore(entity, health, mana);

        OnWorldReviveA?.Invoke(entity);


    }

    public void Kill(GameObject entity)
    {
        var entityInfo = entity.GetComponent<EntityInfo>();
        if(entityInfo == null) { return; }
        entityInfo.Die();
        entityInfo.health = 0;
        entityInfo.isAlive = false;
    }

    public void ReviveAtSpawnBeacon(GameObject entity)
    {
        var entityInfo = entity.GetComponent<EntityInfo>();
        var lastSpawnBeacon = GMHelper.WorldInfo().currentSpawnBeacon;

        if (entityInfo == null) { return; }
        if(lastSpawnBeacon == null) { return; }

        var randomPosition = lastSpawnBeacon.RandomPosition();

        Revive(entity, randomPosition, .25f, .25f);
        entityInfo.OnReviveThis?.Invoke(new(lastSpawnBeacon) { percentValue = .25f });

        lastSpawnBeacon.spawnEvents.OnSpawnEntity?.Invoke(entityInfo);
    }

    public GameObject SpawnEntity(GameObject entity, Vector3 position)
    {
        var collider = entity.GetComponent<Collider>();
        var character = entity.GetComponentInChildren<CharacterInfo>();
        var deltaHeight = -collider.bounds.center.y;
        var sizeOffset = collider.bounds.size.y;

        var positionOffset = -character.transform.localPosition.y;

        var node = AstarPath.active.GetNearest(position).node;

        if (true)
        {
            var newPosition = ((Vector3) node.position)  + new Vector3(0, positionOffset, 0);
            return Instantiate(entity, newPosition, new Quaternion());
        }
    }
}
