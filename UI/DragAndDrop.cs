using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace Architome
{
    public class DragAndDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler, IPointerUpHandler
    {
        // Start is called before the first frame update
        public CanvasGroup canvasGroup;
        public Transform objectToDrag;
        public bool pivotDrag;
        public bool isDragging;


        private Vector3 objectStartPosition;
        private Vector3 startingPosition;
        private Vector3 endPosition;
        public bool startPositionSet;

        [Header("Drag and Drop Properties")]
        public float dragAlpha = 1f;
        public bool sameChildIndex;

        public Action<DragAndDrop, bool> OnDragChange;
        void Start()
        {
            if (objectToDrag == null)
            {
                objectToDrag = transform;
            }

            if (GetComponent<CanvasGroup>())
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
            else
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            var archSceneManager = ArchSceneManager.active;

            if (archSceneManager)
            {
                archSceneManager.AddListener(SceneEvent.OnLoadScene, () => {
                    OnEndDrag(null);
                }, this);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Input.GetKeyDown(KeyCode.Mouse0)) return;
            if (sameChildIndex) { return; }
            objectToDrag.SetAsLastSibling();


        }
        public void OnPointerUp(PointerEventData eventData)
        {
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!Input.GetKey(KeyCode.Mouse0)) return;
            isDragging = true;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = dragAlpha;
            OnDragChange?.Invoke(this, true);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            OnDragChange?.Invoke(this, false);
            isDragging = false;
            startPositionSet = false;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }

        public void OnDrop(PointerEventData eventData)
        {

        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!Input.GetKey(KeyCode.Mouse0)) return;
            HandlePivotDrag();
            HandleDrag();
        }

        

        void HandleDrag()
        {
            if (pivotDrag) { return; }

            if (isDragging)
            {
                var mousePos = Input.mousePosition;

                mousePos.x = Mathf.Clamp(mousePos.x, 0, Screen.width);
                mousePos.y = Mathf.Clamp(mousePos.y, 0, Screen.height);
                objectToDrag.position = Vector3.Lerp(objectToDrag.position, mousePos, .50f);

            }
        }
        void HandlePivotDrag()
        {
            if (!pivotDrag) { return; }
            if (!isDragging) { return; }
            if (!startPositionSet)
            {
                startPositionSet = true;
                startingPosition = Input.mousePosition;
                objectStartPosition = objectToDrag.position;
            }

            endPosition = Input.mousePosition;



            var position = objectStartPosition - V3Helper.Difference(startingPosition, endPosition);

            position.x = Mathf.Clamp(position.x, 0, Screen.width);
            position.y = Mathf.Clamp(position.y, 0, Screen.height);

            objectToDrag.position = Vector3.Lerp(objectToDrag.position, position, .50f);

        }
        void HandleUserInput()
        {
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                if (isDragging) { isDragging = false; }
                startPositionSet = false;
            }
        }

    }

}