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

        GameObject prefab = slotController.modelPrefab;
        if (prefab == null)
        {
            Debug.LogWarning("[UIDrag] Prefab is null.");
            return;
        }

        PlacementType type = prefab.GetComponent<ItemType>()?.type ?? PlacementType.Floor;
        Collider roomCollider = FindTargetCollider(type);
        if (roomCollider == null)
        {
            Debug.LogWarning("[UIDrag] Room collider not found.");
            return;
        }

        Vector3 spawnPos = (type == PlacementType.Floor)
            ? CameraMapper.MappedMousePositionXZ
            : CameraMapper.MappedMousePositionXY;

        draggingInstance = Instantiate(prefab, spawnPos, Quaternion.identity, spawnParent);
        draggingInstance.name = prefab.name;

        var tracker = draggingInstance.GetComponent<ItemAutoDestroy>();
        if (tracker != null)
        {
            tracker.Init(slotController, roomCollider);

            if (type == PlacementType.Floor)
            {
                var grid = Object.FindFirstObjectByType<FloorGrid>();
                if (grid != null)
                    grid.roomCollider = roomCollider;
            }
           else if (type == PlacementType.Wall)
            {
                var grid = roomCollider.GetComponentInChildren<WallGrid>();
                if (grid != null)
                    grid.roomCollider = roomCollider;
                    tracker.wallGrid = grid;
            }

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
                if (!tracker.isValidPlacement)
                {
                    Debug.Log("[UIDrag] 拖拽结果非法，销毁物体");
                    slotController.ClearInstance();
                    slotController.ShowIcon();
                    Destroy(draggingInstance);
                    return;
                }
                tracker.StopDragging();

                Vector3 center = draggingInstance.transform.position;
                Vector3 size = draggingInstance.GetComponent<Collider>().bounds.size;
                Vector3 snapped = Vector3.zero;

                PlacementType type = tracker.GetComponent<ItemType>()?.type ?? PlacementType.Floor;

                if (type == PlacementType.Floor)
                {
                    var floorGrid = Object.FindFirstObjectByType<FloorGrid>();
                    if (floorGrid != null && floorGrid.TrySnapByEdge(center, size, out snapped))
                    {
                        float halfY = size.y * 0.5f;
                        snapped.y = floorGrid.GetFloorY() + halfY;
                        draggingInstance.transform.position = snapped;
                    }
                    else
                    {
                        floorGrid?.HideHighlight();
                        slotController.ClearInstance();
                        slotController.ShowIcon();
                        draggingInstance.transform.SetParent(null);
                        RoomManager.Instance?.RefreshCHIScore();
                        Destroy(draggingInstance);
                        return;
                    }

                    floorGrid?.HideHighlight();
                }
                else if (type == PlacementType.Wall)
                {
                    var wallGrid = tracker.wallGrid;
                    if (wallGrid != null && wallGrid.TrySnapByEdge(center, size, out snapped))
                    {
                        float halfZ = size.z * 0.5f;
                        snapped.z = wallGrid.GetWallZ() - halfZ; // ✅ correct snap against wall face
                        draggingInstance.transform.position = snapped;
                    }
                    else
                    {
                        wallGrid?.HideHighlight();
                        slotController.ClearInstance();
                        slotController.ShowIcon();
                        draggingInstance.transform.SetParent(null);
                        RoomManager.Instance?.RefreshCHIScore();
                        Destroy(draggingInstance);
                        return;
                    }

                    wallGrid?.HideHighlight();
                }

            }
        }

        RoomManager.Instance?.RefreshCHIScore();

    }

    private Collider FindTargetCollider(PlacementType type)
    {
        if (type == PlacementType.Floor)
        {
            GameObject floorGO = GameObject.Find("floor");
            return floorGO?.GetComponent<Collider>();
        }

        if (type == PlacementType.Wall)
        {
            string[] wallNames = { "wall0", "wall1", "wall2", "wall3" };
            Vector3 mouse = CameraMapper.MappedMousePositionXY;
            float minDelta = float.MaxValue;
            Collider closest = null;

            foreach (string name in wallNames)
            {
                GameObject wall = GameObject.Find(name);
                if (wall == null) continue;

                if (wall.transform.localScale.y < 0.2f) continue;

                Collider col = wall.GetComponent<Collider>();
                if (col == null || !col.enabled) continue;

                Vector3 forward = wall.transform.forward;
                float delta;

                if (Mathf.Abs(forward.z) > Mathf.Abs(forward.x))
                {
                    delta = Mathf.Abs(wall.transform.position.z - mouse.z);
                }
                else
                {
                    delta = Mathf.Abs(wall.transform.position.x - mouse.x);
                }

                Debug.Log($"[WALL SELECT] {name}: delta = {delta:F3}");

                if (delta < minDelta)
                {
                    minDelta = delta;
                    closest = col;
                }
            }

            if (closest != null)
                Debug.Log($"[UIDrag] Selected wall = {closest.gameObject.name}");

            return closest;
        }

        return null;
    }
}
