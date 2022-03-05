using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
public class DummyBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    EntityInfo entityInfo;

    [Range(0, 1)]
    public float targetHealth;

    public bool healWhenLow;
    public bool damageWhenHigh;

    public bool oscilateZ;
    public bool oscilateX;

    public float angle = 0;
    public float speed;

    public float angleX;
    public float angleZ;

    public void GetDependencies()
    {
        if(entityInfo == null)
        {
            if(gameObject.GetComponent<EntityInfo>())
            {
                entityInfo = gameObject.GetComponent<EntityInfo>();
            }
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetDependencies();
        HandleHealth();
        HandleOscilation();
    }
    public void HandleHealth()
    {
        if(healWhenLow)
        {

            if ((entityInfo.health / entityInfo.maxHealth) < targetHealth)
            {
                entityInfo.health = entityInfo.maxHealth;
            }
        }
        if(damageWhenHigh)
        {
            if((entityInfo.health/entityInfo.maxHealth) > targetHealth)
            {
                entityInfo.health = entityInfo.maxHealth * .25f;
            }
        }
    }
    public void HandleOscilation()
    {
        angle += Time.deltaTime;

        angle %= Mathf.PI * 2;

        angleX = Mathf.Cos(angle);
        angleZ = Mathf.Sin(angle);


        if(oscilateX) { transform.Translate(new Vector3(1, 0, 0) * angleX * speed * Time.deltaTime); }
        if(oscilateZ) { transform.Translate(new Vector3(0, 0, 1) * angleZ * speed * Time.deltaTime); }
    }

}
