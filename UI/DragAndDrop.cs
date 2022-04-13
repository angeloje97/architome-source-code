using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class DragAndDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
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

    public void OnPointerDown(PointerEventData eventData)
    {
        
        if (sameChildIndex) { return; }
        objectToDrag.SetAsLastSibling();
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        //isDragging = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!Input.GetKey(KeyCode.Mouse0)) return;
        isDragging = true;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = dragAlpha;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
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
        HandlePivotDrag();
        HandleDrag();
    }

    void Start()
    {
        if(objectToDrag == null)
        {
            objectToDrag = transform;
        }

        if(GetComponent<CanvasGroup>())
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        else
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void HandleDrag()
    {
        if(pivotDrag) { return; }

        if (isDragging)
        {

            objectToDrag.position = Vector3.Lerp(objectToDrag.position, Input.mousePosition, .50f);

            //if (V3Helper.Abs(objectToDrag.localPosition) > localRadius)
            //{
            //    objectToDrag.localPosition *= localRadius / (V3Helper.Abs(objectToDrag.localPosition));
            //}
        }
    }
    void HandlePivotDrag()
    {
        if (!pivotDrag) { return; }
        if (!isDragging) { return; }
        if(!startPositionSet)
        {
            startPositionSet = true;
            startingPosition = Input.mousePosition;
            objectStartPosition = objectToDrag.position;
        }

        endPosition = Input.mousePosition;
        var position = objectStartPosition - V3Helper.Difference(startingPosition, endPosition);
        //objectToDrag.position = objectStartPosition - V3Helper.Difference(startingPosition, endPosition);

        objectToDrag.position = Vector3.Lerp(objectToDrag.position, position, .50f);

    }
    void HandleUserInput()
    {
        if(Input.GetKeyUp(KeyCode.Mouse0))
        {
            if (isDragging) { isDragging = false; }
            startPositionSet = false;
        }
    }

}
