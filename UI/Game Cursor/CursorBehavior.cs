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
        Dictionary<string, CursorType> typesDict;

        ContainerTargetables targetManager;
        ClickableManager clickableManager;
        GameManager gameManager;

        bool useGameCursorCheck;

        public List<GameObject> targetObjects, clickableObjects;

        public Action<Vector3> WhileCursorMove;
        Vector3 currentMousePosition;
        

        EntityInfo playableEntity;

        bool clearingActive;

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

            if (gameManager)
            {
                gameManager.OnNewPlayableEntity += delegate (EntityInfo entity, int index) { 
                    playableEntity = entity;
                };
            }

        }
        void Start()
        {
            GetDependencies();
            
            UpdateCursorMap();
            SetGameCursor(useGameCursor);

        }

        void UpdateCursorMap()
        {
            typesDict = new();

            foreach (var type in types)
            {
                typesDict.Add(type.cursorName, type);
            }
        }

        void SetCursor(string cursorString)
        {
            //var target = types.Find(cursor => cursorString == cursor.cursorName);
            var target = typesDict[cursorString];
            var texture = target.cursorTexture;

            float xSpot = texture.width / 4f;
            float ySpot = texture.height / 4f;
            Vector2 hotSpot = new (xSpot, ySpot);

            Cursor.SetCursor(texture, hotSpot, CursorMode.Auto);
        }

        public void SetCursor(Texture2D texture)
        {
            Vector2 hotSpot = new(texture.width / 4f, texture.height / 4f);

            Cursor.SetCursor(texture, hotSpot, CursorMode.Auto);
        }

        

        private void Awake()
        {
            active = this;
            targetObjects = new();
            clickableObjects = new();
            ClearNulls();
        }


        // Update is called once per frame
        void Update()
        {
        }

        private void FixedUpdate()
        {
            if (currentMousePosition != Input.mousePosition)
            {
                currentMousePosition = Input.mousePosition;
                WhileCursorMove?.Invoke(currentMousePosition);
            }
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

        
        bool CanAttack(EntityInfo target)
        {
            if (playableEntity == null) return false;

            return playableEntity.CanAttack(target);
        }

        public void UpdateCursor(Predicate<object> canAttackCheck = null)
        {
            if (clickableObjects.Count > 0)
            {

                var clickable = clickableObjects[0].GetComponent<Clickable>();
                
                if (clickable && clickable.Interactable && clickable.options.Count > 0)
                {
                    var portal = clickable.GetComponent<PortalInfo>();
                    var presetTexture = clickable.MouseOverTexture();

                    if (presetTexture)
                    {
                        SetCursor(presetTexture);
                        return;
                    }

                    if (portal)
                    {
                        SetCursor("Portal");
                        return;
                    }
                    var workInfo = clickable.GetComponent<WorkInfo>();

                    if (workInfo)
                    {
                        SetCursor("Work");
                        return;
                    }
                }
            }

            if (canAttackCheck != null)
            {
                if (canAttackCheck(null))
                {
                    SetCursor("Attack");
                    return;
                }
            }


            HandleNullMouseOver();
            
        }

        async void ClearNulls()
        {
            if (clearingActive) return;
            clearingActive = true;
            while (this)
            {
                for (int i = 0; i < targetObjects.Count; i++)
                {
                    if (targetObjects[i] == null)
                    {
                        targetObjects.RemoveAt(i);
                        i--;
                    }
                }

                for (int i = 0; i < clickableObjects.Count; i++)
                {
                    if (clickableObjects[i] == null)
                    {
                        clickableObjects.RemoveAt(i);
                        i--;
                    }
                }

                await Task.Delay(2500);
            }
            clearingActive = false;
        }

        public void OnNewHoverTarget(EntityInfo previous, EntityInfo after)
        {
            
            if (previous && targetObjects.Contains(previous.gameObject))
            {
                targetObjects.Remove(previous.gameObject);
            }

            if (after && !targetObjects.Contains(after.gameObject))
            {
                targetObjects.Add(after.gameObject);
            }

            UpdateCursor(canAttackCheck: (object o) => {
                if (after == null) return false;
                return CanAttack(after);
            });
        }

        public void OnNewClickableHover(GameObject previous, GameObject after)
        {
            if (previous && clickableObjects.Contains(previous))
            {
                clickableObjects.Remove(previous);
            }

            if (after && !clickableObjects.Contains(after))
            {
                clickableObjects.Add(after);
            }

            UpdateCursor();

        }

        void HandleNullMouseOver()
        {
            SetCursor("Default");

        }
    }

}
