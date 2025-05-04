using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragToSpawn : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private GameObject draggingInstance;
    private ItemSlotController slotController;

    private void Awake()
    {
        slotController = GetComponent<ItemSlotController>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (slotController == null || slotController.HasSpawned())
            return;

        Transform spawnParent = GameObject.Find("ItemSpawnRoot")?.transform;
        if (spawnParent == null)
        {
            Debug.LogWarning("[UIDrag] ItemSpawnRoot not found.");
            return;
        }

        Collider roomCollider = GameObject.Find("floor")?.GetComponent<Collider>();
        if (roomCollider == null)
        {
            Debug.LogWarning("[UIDrag] Floor collider not found.");
            return;
        }

        Vector3 spawnPos = CameraMapper.MappedMousePosition;
        GameObject prefab = slotController.modelPrefab;

        draggingInstance = Instantiate(prefab, spawnPos, Quaternion.identity, spawnParent);
        draggingInstance.name = prefab.name;

        var tracker = draggingInstance.GetComponent<ItemAutoDestroy>();
        if (tracker != null)
        {
            tracker.Init(slotController, roomCollider);

            var grid = Object.FindFirstObjectByType<FloorGrid>();
            if (grid != null)
                grid.roomCollider = roomCollider;
        }

        slotController.RegisterInstance(draggingInstance);
        slotController.SetSpawned(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 拖拽行为在 ItemAutoDestroy 中处理
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

                Vector3 pos = draggingInstance.transform.position;
                pos.y -= tracker.dropYOffset;
                draggingInstance.transform.position = pos;
            }
        }

        RoomManager.Instance?.RefreshCHIScore();
    }
}
