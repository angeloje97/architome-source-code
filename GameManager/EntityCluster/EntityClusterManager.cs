using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;



namespace Architome
{
    [Serializable]
    public class EntityCluster
    {
        public GameObject clusterContainer;
        public List<GameObject> entities = new List<GameObject>();

        bool isActive;

        public EntityCluster()
        {
            isActive = true;
            TriggerChangeInterval();

        }
        public int IndexOf(GameObject entity)
        {
            return entities.IndexOf(entity);
        }

        public void HandleDeadEntity(GameObject entity)
        {
            if (!entities.Contains(entity)) { return; }
            entity.GetComponentInChildren<EntityClusterAgent>().OnClusterExit?.Invoke(this, IndexOf(entity));

            entities.Remove(entity);

            TriggerChange();

            if (entities.Count <= 1)
            {
                EntityClusterManager.active.HandleEmptyCluster(this);
            }


        }

        async void TriggerChangeInterval()
        {
            do
            {
                await Task.Delay(1000);
                TriggerChange();
            } while (isActive);
        }

        public void EndEntityCluster()
        {
            isActive = false;
        }

        public void TriggerChange()
        {
            foreach (var entity in entities)
            {
                if (entity == null) continue;
                var agent = entity.GetComponentInChildren<EntityClusterAgent>();
                if (agent == null) continue;
                agent.OnClusterChange?.Invoke(this, IndexOf(entity));
            }
        }
    }

    public class EntityClusterManager : MonoBehaviour
    {
        // Start is called before the first frame update
        public static EntityClusterManager active;


        public List<EntityCluster> entityClusters;
        public GameObject entityClusterContainer;


        ArchSceneManager sceneManager;


        void Awake()
        {
            active = this;
        }

        public void GetDependencies()
        {
            sceneManager = ArchSceneManager.active;
            if (sceneManager)
            {
                sceneManager.AddListener(SceneEvent.BeforeLoadScene, BeforeLoadScene, this);
            }
        }

        public void BeforeLoadScene(ArchSceneManager sceneManager)
        {
            foreach (var cluster in entityClusters)
            {
                HandleEmptyCluster(cluster);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void HandleAgent(EntityClusterAgent agent1, EntityClusterAgent agent2)
        {
            var entity1 = agent1.GetComponentInParent<EntityInfo>().gameObject;
            var entity2 = agent2.GetComponentInParent<EntityInfo>().gameObject;


            var cluster = Cluster(entity2);
            Debugger.InConsole(68123, $"{entity1}, {entity2}");

            if (cluster == null)
            {
                cluster = new EntityCluster();
                cluster.entities.Add(entity2);

                var newClusterContainer = Instantiate(entityClusterContainer, transform).GetComponent<ClusterContainer>();
                newClusterContainer.firstAgent = entity2;
                newClusterContainer.cluster = cluster;


                cluster.clusterContainer = newClusterContainer.gameObject;
                entityClusters.Add(cluster);

                agent2.OnClusterEnter?.Invoke(cluster, cluster.IndexOf(entity2));

            }

            cluster.entities.Add(entity1);

            agent1.OnClusterEnter?.Invoke(cluster, cluster.IndexOf(entity1));

        }



        public EntityCluster Cluster(GameObject entity)
        {
            foreach (EntityCluster cluster in entityClusters)
            {
                if (cluster.entities.Contains(entity))
                {
                    return cluster;
                }
            }

            return null;
        }

        public void HandleEmptyCluster(EntityCluster cluster)
        {
            //if (!entityClusters.Contains(cluster)) { return; }

            foreach (var entity in cluster.entities)
            {
                var clusterAgent = entity.GetComponentInChildren<EntityClusterAgent>();

                if (clusterAgent == null) continue;

                clusterAgent.OnClusterExit?.Invoke(cluster, cluster.entities.IndexOf(entity));
            }
            
            Destroy(cluster.clusterContainer);
            cluster.EndEntityCluster();

            if (!entityClusters.Contains(cluster)) return;
            entityClusters.Remove(cluster);
        }

        public void DeleteCluster()
        {

        }

        public int Position(GameObject entity)
        {
            var num = 0;

            foreach (EntityCluster cluster in entityClusters)
            {
                if (cluster.entities.Contains(entity))
                {
                    return cluster.entities.IndexOf(entity);
                }
            }

            return num;
        }

        public bool Contains(GameObject entity)
        {
            foreach (EntityCluster cluster in entityClusters)
            {
                if (cluster.entities.Contains(entity))
                {
                    return true;
                }
            }

            return false;
        }
    }

}