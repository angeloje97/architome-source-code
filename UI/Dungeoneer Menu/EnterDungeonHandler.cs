using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

namespace Architome
{
    [RequireComponent(typeof(ToolTipElement))]
    public class EnterDungeonHandler : MonoBehaviour
    {
        [Header("Components")]
        public ArchButton archButton;
        public ToolTipElement toolTipHandler;

        DungeoneerManager dungeoneerManager;
        DungeonTable dungeonTable;

        [Header("Properties")]
        [SerializeField] bool valid;
        [SerializeField] bool fullParty;
        [SerializeField] bool selectedDungeon;


        void Start()
        {
            GetDependencies();
            ArchAction.Delay(UpdateValidity, .50f);
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        void GetDependencies()
        {
            dungeoneerManager = DungeoneerManager.active;
            dungeonTable = DungeonTable.active;

            if (dungeonTable)
            {
                dungeonTable.OnSelectedDungeonChange += HandleSelectDungeonChange;
            }

            if (dungeoneerManager)
            {
                dungeoneerManager.OnSetSelectedMembers += OnSetSelectedMembers;

            }

            if (toolTipHandler)
            {
                toolTipHandler.OnCanShowCheck += OnCanShowToolTip;
            }
        }

        void HandleSelectDungeonChange(Dungeon before, Dungeon after)
        {
            selectedDungeon = after != null;

            UpdateValidity();
        }

        void OnCanShowToolTip(ToolTipElement element)
        {
            if (valid)
            {
                element.checks.Add(false);
                return;
            }

            var invalidReason = new List<string>();

            if (!fullParty)
            {
                invalidReason.Add("- Requires a full party that's ready.");
            }

            if (!selectedDungeon)
            {
                invalidReason.Add("- Requires a dungeon to be selected.");
            }

            element.data = new()
            {
                name = "Cannot Start Dungeon",
                description = ArchString.NextLineList(invalidReason),
            };
        }

        void OnSetSelectedMembers(DungeoneerManager manager, List<EntityInfo> members)
        {

            fullParty = members.Count == 5;
            UpdateValidity();
        }

        void UpdateValidity()
        {
            valid = true;

            if (!fullParty)
            {
                valid = false;
            }

            if (!selectedDungeon)
            {
                valid = false;
            }

            archButton.SetButton(valid);
        }
    }
}
