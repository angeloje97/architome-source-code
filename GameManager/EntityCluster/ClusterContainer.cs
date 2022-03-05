using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome;
public class ClusterContainer : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject firstAgent;
    public EntityCluster cluster;

    private GameObject previousAgent;
    
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        FollowAgents();
    }

    
    void FollowAgent()
    {
        if(firstAgent != null)
        {
            transform.position = firstAgent.GetComponent<EntityInfo>().GraphicsInfo().transform.position;
        }
    }

    void FollowAgents()
    {
        
        var agents = cluster.entities;

        if(agents.Count == 0) { return; }

        var sum = new Vector3();

        foreach(var agent in agents)
        {
            sum += agent.transform.position;
        }

        var midPoint = sum / agents.Count;

        transform.position = midPoint;

    }

    public void OnTriggerExit(Collider other)
    {
        if (!other.GetComponent<EntityClusterAgent>()) { return; }

        var otherAgent = other.GetComponent<EntityClusterAgent>();

        otherAgent.lastToEnter = false;
        var entity = other.GetComponentInParent<EntityInfo>().gameObject;

        if (cluster.entities.Contains(entity))
        {
            otherAgent.OnClusterExit?.Invoke(cluster, cluster.IndexOf(entity));
            cluster.entities.Remove(entity);
        }

        cluster.TriggerChange();

        if (cluster.entities.Count == 1)
        {
            EntityClusterManager.active.HandleEmptyCluster(cluster);
        }

        
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!other.GetComponent<ClusterContainer>()) { return; }
    }


}
