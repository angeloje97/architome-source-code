using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class SpellBookManager : MonoBehaviour
    {
        public EntitySpellBook spellBookTemplate;
        public Transform entityPortraitsParent;
        public ModuleInfo module;

        public List<EntitySpellBook> spellBooks;

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
            var newSpellBook = Instantiate(spellBookTemplate.gameObject, transform).GetComponent<EntitySpellBook>();
            var iconTemplate = World.active.prefabsUI.icon;

            var newIcon = Instantiate(iconTemplate, entityPortraitsParent).GetComponent<Icon>();

            newSpellBook.SetEntity(entity);

            if (spellBooks == null)
            {
                spellBooks = new();
            }

            spellBooks.Add(newSpellBook);

            newIcon.SetIcon(new()
            {
                data = newSpellBook,
                sprite = entity.PortraitIcon()
            });

            newIcon.OnSelectIcon += OnSelectEntityIcon;
        }

        void OnSelectEntityIcon(Icon icon)
        {
            if (icon.data.GetType() != typeof(EntitySpellBook)) return;

            foreach (var spellBook in spellBooks)
            {
                var canvas = spellBook.GetComponent<CanvasGroup>();

                ArchUI.SetCanvas(canvas, (EntitySpellBook)icon.data == spellBook);
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
