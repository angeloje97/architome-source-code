using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class SpellBookManager : MonoBehaviour
    {
        public SpellBookPage pageTemplate;
        public Transform entityPortraitsParent;
        public ModuleInfo module;

        public Transform pagesParent;
        public List<SpellBookPage> pages;

        void Start()
        {
            GetDependencies();
        }

        void GetDependencies()
        {
            var gameManager = GameManager.active;
            gameManager.OnNewPlayableEntity += OnNewPlayableEntity;
            gameManager.OnNewPlayableParty += OnNewPlayableParty;


            module = GetComponentInParent<ModuleInfo>();
        }

        public async void OnNewPlayableParty(PartyInfo party, int index)
        {
            await Task.Yield();

            foreach (Transform child in entityPortraitsParent)
            {
                var icon = child.GetComponent<Icon>();
                if (icon == null) continue;

                OnSelectEntityIcon(icon);
                break;
            }
        }

        public void OnNewPlayableEntity(EntityInfo entity, int index)
        {
            var newPage = Instantiate(pageTemplate, pagesParent);
            var iconTemplate = World.active.prefabsUI.icon;

            var newIcon = Instantiate(iconTemplate, entityPortraitsParent).GetComponent<Icon>();

            newPage.SetEntity(entity);

            pages ??= new();

            pages.Add(newPage);

            newIcon.SetIcon(new()
            {
                data = newPage,
                sprite = entity.PortraitIcon()
            });

            newIcon.OnSelectIcon += OnSelectEntityIcon;
        }

        void OnSelectEntityIcon(Icon icon)
        {
            if (icon.data.GetType() != typeof(SpellBookPage)) return;

            foreach (var page in pages)
            {
                var canvas = page.GetComponent<CanvasGroup>();

                ArchUI.SetCanvas(canvas, (SpellBookPage)icon.data == page);
            }
        }
    }
}
