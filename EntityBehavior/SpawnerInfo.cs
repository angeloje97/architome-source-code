using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class SpawnerInfo : EntityInfo
    {
        // Start is called before the first frame update
        [Header("Spawner Properties")]
        public Transform spawnPoint;
        public float spawnRadius;
        public float spawnHeight;
        public bool isActive;


        private void Start()
        {
            
        }

        private void Update()
        {
            
        }

        public Vector3 RandomPosition()
        {
            if(spawnPoint == null)
            {
                spawnPoint = transform;
            }
            
            float x = spawnPoint.position.x + Random.Range(0, spawnRadius);
            float z = spawnPoint.position.z + Random.Range(0, spawnRadius);
            float y = spawnPoint.position.y + Random.Range(-spawnHeight, spawnHeight);

            return new Vector3(x, y, z);

        }
    }
}
