using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class StructureHider : MonoBehaviour
    {
        // Start is called before the first frame update
        public List<EntityInfo> playableEntities;
        public List<GameObject> squares;
        public List<GameObject> structures;

        public GameObject squarePrefab;
        
        void GetDependencies()
        {
            GMHelper.GameManager().OnNewPlayableEntity += OnNewPlayableEntity;
        }
        void Start()
        {
            playableEntities = new();
            GetDependencies();
        }

        // Update is called once per frame
        void Update()
        {
            //CheckStructure();
            CheckStructuresSquares();
        }

        void CheckStructuresSquares()
        {
            for (int i = 0; i < playableEntities.Count; i++)
            {
                var distance = V3Helper.Distance(playableEntities[i].transform.position, transform.position);
                squares[i].transform.LookAt(playableEntities[i].transform);
                squares[i].transform.localScale = new Vector3(1, 1, distance);
            }
        }

        void CheckStructure()
        {
            if (CameraManager.Main == null) return;
            if (playableEntities.Count == 0) return;

            List<GameObject> rayCastHits = new();

            foreach (var entity in playableEntities)
            {
                var distance = V3Helper.Distance(entity.transform.position, CameraManager.Main.transform.position);
                var direction = V3Helper.Direction(entity.transform.position, CameraManager.Main.transform.position);

                var ray = new Ray(CameraManager.Main.transform.position, direction);
                RaycastHit hit;
                if (Physics.Raycast(CameraManager.Main.transform.position, direction, out hit, distance, GMHelper.LayerMasks().structureLayerMask))
                {
                    rayCastHits.Add(hit.transform.gameObject);
                }
            }

            structures = rayCastHits;

        }

        void OnTrigger(Collider other, bool entered)
        {
            if (other.transform.gameObject.layer == GMHelper.LayerMasks().structureLayerMask)
            {
                if (entered)
                {
                    structures.Add(other.transform.gameObject);
                }
                else
                {
                    structures.Remove(other.transform.gameObject);
                }
            }
        }



        public void OnNewPlayableEntity(EntityInfo entity, int index)
        {
            playableEntities.Add(entity);
            var newSquare = Instantiate(squarePrefab, transform); 
            squares.Add(newSquare);
            newSquare.transform.GetChild(0).gameObject.AddComponent<TriggerActivator>().OnTrigger += OnTrigger;
        }
    }

}