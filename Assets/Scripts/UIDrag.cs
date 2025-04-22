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
        if (tracker != null)
        {
            tracker.Init(slotController, roomCollider);
            tracker.PrepareDragging(); // simulate the start of dragging
        }

        slotController.RegisterInstance(draggingInstance);
        slotController.HideIcon();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // This part is handled in ItemAutoDestroy
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggingInstance != null)
        {
            var tracker = draggingInstance.GetComponent<ItemAutoDestroy>();
            if (tracker != null)
            {
                tracker.TryPlace(); // simulate the release and try to place the object
            }
        }
    }
}
