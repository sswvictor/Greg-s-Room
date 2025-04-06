using UnityEngine;
using UnityEngine.EventSystems;

public class SmartDraggable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private Vector3 initialPosition;
    private RectTransform rectTransform;
    private Canvas canvas;

    [Header("区域引用")]
    public RectTransform roomArea;
    public RectTransform itemBoxArea;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        initialPosition = rectTransform.localPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.SetAsLastSibling(); // 确保在顶层
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPos
        );
        rectTransform.localPosition = localPos;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;

        bool inItemBox = RectTransformUtility.RectangleContainsScreenPoint(itemBoxArea, eventData.position, uiCam);

        if (inItemBox)
        {
            // 放在物品框：回归原位
            rectTransform.localPosition = initialPosition;
        }
        // 否则放哪儿就哪儿，什么都不做
    }
}
