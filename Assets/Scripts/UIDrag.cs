using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragToSpawn : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject prefabToSpawn;
    public ItemSlotController slotController;
    public Collider roomCollider;

    private GameObject draggingInstance;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (slotController.HasSpawned()) return;

        draggingInstance = Instantiate(prefabToSpawn);
        draggingInstance.name = prefabToSpawn.name;

        var tracker = draggingInstance.GetComponent<ItemAutoDestroy>();
        tracker.Init(slotController, roomCollider);

        slotController.RegisterInstance(draggingInstance);
        slotController.HideIcon();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 拖拽逻辑由实例自己处理
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggingInstance != null)
        {
            var tracker = draggingInstance.GetComponent<ItemAutoDestroy>();
            if (tracker != null)
            {
                tracker.CheckPositionImmediately();
                tracker.StopDragging();
            }
        }
    }
}
