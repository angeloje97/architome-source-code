using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Serialization;

namespace Architome
{
    public class GroupFormationSaveHandler : MonoBehaviour
    {
        GroupFormationBehavior formationBehavior;
        void Start()
        {
            GetDependencies();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        void GetDependencies()
        {
            formationBehavior = GetComponent<GroupFormationBehavior>();
            var archSceneManager = ArchSceneManager.active;

            if (formationBehavior)
            {
                formationBehavior.OnSetGroup += HandleSetGroup;
            }

            if (archSceneManager)
            {
                archSceneManager.AddListener(SceneEvent.BeforeLoadScene, BeforeLoadScene, this);
            }
        }

        void BeforeLoadScene(ArchSceneManager sceneManager)
        {
            SaveGroupFormation();
        }

        void SaveGroupFormation()
        {
            SaveSystem.Operate((SaveGame save) => {
                var members = formationBehavior.members;
                for(int i = 0; i < members.Count; i++)
                {
                    var member = members[i];
                    var spot = formationBehavior.memberSpots[i];

                    save.UI.GroupFormation.SaveEntity(member, spot.transform.localPosition);
                }
            });
        }

        void HandleSetGroup(GroupFormationBehavior formationBehavior, List<EntityInfo> members)
        {
            SaveSystem.Operate((SaveGame save) => {

                for(int i = 0; i < members.Count; i++)
                {
                    var spotTrans = formationBehavior.memberSpots[i].transform;
                    var member = members[i];


                    save.UI.GroupFormation.LoadSpotData(member, spotTrans);
                }
            });

            formationBehavior.UpdateFormation(true);
        }
    }
}
