using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome;
public class EntityClusterAgent : MonoBehaviour
{
    // Start is called before the first frame update
    public EntityInfo entityInfo;
    public bool lastToEnter;
    public Action<EntityCluster, int> OnClusterEnter;
    public Action<EntityCluster, int> OnClusterExit { get; set; }
    public Action<EntityCluster, int> OnClusterChange { get; set; }


    public EntityCluster currentCluster, previuosCluster;

    public void Awake()
    {
        if(GetComponentInParent<EntityInfo>())
        {
            entityInfo = GetComponentInParent<EntityInfo>();
            entityInfo.OnLifeChange += OnLifeChange;
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        if (EntityClusterManager.active == null) return;
        if(!entityInfo.isAlive) { return; }
        if(other.GetComponent<ClusterContainer>())
        {
            Debugger.InConsole(68121, $"{other.GetComponent<ClusterContainer>()}");
            var clusterContainer = other.GetComponent<ClusterContainer>();
            var clusterAgentEntity = GetComponentInParent<EntityInfo>().gameObject;

            if (clusterContainer.cluster.entities.Contains(clusterAgentEntity)) { return; }
            if (EntityClusterManager.active.Contains(entityInfo.gameObject)) { return; }
            clusterContainer.cluster.entities.Add(clusterAgentEntity);

            OnClusterEnter?.Invoke(clusterContainer.cluster, clusterContainer.cluster.IndexOf(entityInfo.gameObject));

            return;
        }

        if (!other.GetComponent<EntityClusterAgent>()) return;
        if (EntityClusterManager.active.Contains(entityInfo.gameObject)) { return; }
        Debugger.InConsole(68122, $"{other.GetComponent<EntityClusterAgent>()}");
        if (lastToEnter) { return; }

        var otherCluster = other.GetComponent<EntityClusterAgent>();
        if (!other.GetComponentInParent<EntityInfo>().isAlive) { return; }

        //other.GetComponent<EntityClusterAgent>().lastToEnter = true;

        EntityClusterManager.active.HandleAgent(this, otherCluster);
    }

    public void OnLifeChange(bool isAlive)
    {
        if(isAlive) { return; }
        if(!EntityClusterManager.active) { return; }
        if (EntityClusterManager.active.Cluster(entityInfo.gameObject) == null) { return; }

        var cluster = EntityClusterManager.active.Cluster(entityInfo.gameObject);
        cluster.HandleDeadEntity(entityInfo.gameObject);
    }
}
