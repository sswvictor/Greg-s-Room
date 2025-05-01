using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragToSpawn : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject prefabToSpawn;
    public ItemSlotController slotController;

    private GameObject draggingInstance;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (slotController.HasSpawned()) return;

        Transform spawnParent = GameObject.Find("ItemSpawnRoot")?.transform;
        if (spawnParent == null)
        {
            Debug.LogWarning("[UIDragToSpawn] ItemSpawnRoot not found. Abort spawn.");
            return;
        }

        Collider roomCollider = GameObject.Find("Floor")?.GetComponent<Collider>();
        if (roomCollider == null)
        {
            Debug.LogWarning("[UIDragToSpawn] FloorGrid collider not found.");
            return;
        }

        Vector3 spawnPos = CameraMapper.MappedMousePosition;
        draggingInstance = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity, spawnParent);
        draggingInstance.name = prefabToSpawn.name;

        var tracker = draggingInstance.GetComponent<ItemAutoDestroy>();
        if (tracker != null)
        {
            tracker.Init(slotController, roomCollider);

            var grid = Object.FindFirstObjectByType<FloorGrid>();
            if (grid != null)
                grid.roomCollider = roomCollider;
        }

        slotController.RegisterInstance(draggingInstance);
        slotController.HideIcon();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 拖拽逻辑由 ItemAutoDestroy.Update() 管理
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

                // ✅ 直接自己根据 dropYOffset 修正Y，不依赖FloorGrid
                Vector3 pos = draggingInstance.transform.position;
                pos.y -= tracker.dropYOffset;
                draggingInstance.transform.position = pos;
            }
        }

        RoomManager.Instance?.RefreshCHIScore();
    }

}
