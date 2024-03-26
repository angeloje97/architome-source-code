using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Architome
{

    public enum SpriteAssetType
    {
        General, KeyBindings, Flat, Classes
    }
    public class SpriteAssetData : MonoBehaviour
    {
        public static SpriteAssetData active;
        [SerializeField] TMP_SpriteAsset general, keybindings, flat, classes;
        Dictionary<SpriteAssetType, TMP_SpriteAsset> spriteDictionary;



        private void Awake()
        {
            active = this;
            CreateDictionary();
        }

        void CreateDictionary()
        {
            spriteDictionary = new()
            {
                {SpriteAssetType.General, general },
                {SpriteAssetType.KeyBindings, keybindings },
                {SpriteAssetType.Flat, flat },
                {SpriteAssetType.Classes, classes }
            };
        }

        public TMP_SpriteAsset SpriteAsset(SpriteAssetType type)
        {
            return spriteDictionary[type];
        }
    }
}
