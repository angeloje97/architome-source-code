using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Architome
{
    public class DungeonPreviewHandler : MonoBehaviour
    {
        public CanvasGroup canvasGroup;
        public Image background;
        public TextMeshProUGUI dungeonName;
        public TextMeshProUGUI recommendedLevel;

        DungeonTable dungeonTable;
        Dungeon currentlySelected;

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
            dungeonTable = DungeonTable.active;

            if (dungeonTable)
            {
                dungeonTable.OnSelectedDungeonChange += HandleSelectDungeonChange;
            }
        }

        void HandleSelectDungeonChange(Dungeon before, Dungeon after)
        {
            currentlySelected = after;

            UpdatePreview();
        }

        void UpdatePreview()
        {
            ArchUI.SetCanvas(canvasGroup, currentlySelected != null);
            if (!currentlySelected) return;

            var set = currentlySelected.dungeonInfo.set;

            background.sprite = set.background;
            dungeonName.text = set.dungeonSetName;
            recommendedLevel.text = $"Highest Level: {currentlySelected.RecommendedLevel()}";
        }
    }
}
