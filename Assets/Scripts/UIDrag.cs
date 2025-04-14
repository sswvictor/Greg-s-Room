using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragToSpawn : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject prefabToSpawn;
    public ItemSlotController slotController;
    public Collider roomCollider; // 单个房间范围 Collider

    private GameObject draggingInstance;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

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
        // 拖动逻辑交给拖出的物体自己处理
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggingInstance != null)
        {
            var tracker = draggingInstance.GetComponent<ItemAutoDestroy>();
            if (tracker != null)
            {
                tracker.CheckPositionImmediately(); // 主动检测第一次是否在房间内
            }
        }
    }
}