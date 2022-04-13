using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;
using System.Linq;
using Architome.Enums;


namespace Architome
{
    public class CursorBehavior: MonoBehaviour
    {
        public bool useGameCursor;

        public static CursorBehavior active;


        
        [Serializable]
        public struct CursorType
        {
            public string cursorName;
            public Texture2D cursorTexture;
        }

        [SerializeField]
        List<CursorType> types;

        ContainerTargetables targetManager;
        ClickableManager clickableManager;
        GameManager gameManager;

        bool useGameCursorCheck;

        void GetDependencies()
        {
            targetManager = ContainerTargetables.active;
            clickableManager = ClickableManager.active;
            gameManager = GameManager.active;


            if (targetManager)
            {
            targetManager.OnNewHoverTarget += OnNewHoverTarget;

            }

            if (clickableManager)
            {
                clickableManager.OnNewClickableHover += OnNewClickableHover;
            }

        }
        void Start()
        {
            GetDependencies();
            SetGameCursor(useGameCursor);
        }

        void SetCursor(string cursorString)
        {
            var target = types.Find(cursor => cursorString == cursor.cursorName);

            var texture = target.cursorTexture;

            float xSpot = texture.width / 4f;
            float ySpot = texture.height / 4f;
            Vector2 hotSpot = new (xSpot, ySpot);

            Cursor.SetCursor(texture, hotSpot, CursorMode.ForceSoftware);
        }

        

        private void Awake()
        {
            active = this;
        }


        // Update is called once per frame
        void Update()
        {
        }

        public void SetGameCursor(bool value)
        {
            if (value)
            {
                SetCursor("Default");
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }

        //Events that will change the cursor image.

        public void OnNewHoverTarget(GameObject previous, GameObject after)
        {
            if (after == null)
            {
                HandleNullMouseOver();

                return;
            }

            var entity = after.GetComponent<EntityInfo>();

            if(entity == null)
            {                  
                HandleNullMouseOver();
            }


            if (gameManager.playableEntities.Count <= 0)
            {
                return;
            }

            if (gameManager.playableEntities[0].CanAttack(entity.gameObject))
            {
                SetCursor("Attack");
            }

        }

        public void OnNewClickableHover(GameObject previous, GameObject after)
        {

            if (after == null)
            {
                HandleNullMouseOver();
                return;
            }

            var clickable = after.GetComponent<Clickable>();

            HandlePortal();
            HandleWork();

            void HandlePortal()
            {
                if (!clickable.GetComponent<PortalInfo>()) return;
                SetCursor("Portal");
            }

            void HandleWork()
            {
                if (!clickable.GetComponent<WorkInfo>()) return;
                SetCursor("Work");
            }

        }

        void HandleNullMouseOver()
        {
            SetCursor("Default");

        }
    }

}
