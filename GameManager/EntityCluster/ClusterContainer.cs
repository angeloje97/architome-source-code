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
        StartCoroutine(OutOfRangeRoutine());
    }

    // Update is called once per frame
    void Update()
    {
        FollowAgents();
    }

    public IEnumerator OutOfRangeRoutine()
    {
        while(true)
        {
            yield return null;

            for(int i = 0; i < cluster.entities.Count; i++)
            {
                var agent = cluster.entities[i];
                yield return null;

                if (V3Helper.Distance(agent.transform.position, transform.position) > 6f)
                {
                    var clusterAgent = agent.GetComponentInChildren<EntityClusterAgent>();
                    HandleAgentExit(clusterAgent);
                    i--;
                }
            }
        }
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

        HandleAgentExit(otherAgent);
    }

    public void HandleAgentExit(EntityClusterAgent agent)
    {
        if(agent == null) { return; }
        agent.lastToEnter = false;
        var entity = agent.GetComponentInParent<EntityInfo>().gameObject;

        if (cluster.entities.Contains(entity))
        {
            agent.OnClusterExit?.Invoke(cluster, cluster.IndexOf(entity));
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
