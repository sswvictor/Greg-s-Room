// ✅ ItemAutoDestroy.cs（保持 HashSet 封装，但嵌入类内部）
using UnityEngine;
using System.Collections.Generic;

public class ItemAutoDestroy : MonoBehaviour
{
    private ItemSlotController originSlot;
    private Collider roomCollider;
    public FloorGrid floorGrid;
    public WallGrid wallGrid;
    private bool isDragging = true;

    private Vector3 cachedSize;
    private Collider selfCollider;

    public bool isValidPlacement = false;

    // ✅ 内部封装销毁列表
    private static readonly HashSet<string> destroyIfInvalid = new()
    {
        "Bed_Prefab",
        "Bookshelf_Prefab",
        "Couch_Prefab",
        "Nightstand_Prefab",
        "Basketball_Prefab",
        "Frame_Prefab"
    };

    private bool ShouldDestroy(string name)
    {
        return destroyIfInvalid.Contains(name);
    }

    // public void AssignFloorGrid(FloorGrid grid)
    // {
    //     floorGrid = grid;
    // }


    public void Init(ItemSlotController slot, Collider room)
    {
        originSlot = slot;
        roomCollider = room;
    }

    void Start()
    {
        floorGrid = Object.FindFirstObjectByType<FloorGrid>();
        selfCollider = GetComponent<Collider>();

        if (selfCollider == null)
        {
            Debug.LogError($"[ItemAutoDestroy ❌] {name} has NO collider attached!");
            return;
        }

        if (cachedSize == Vector3.zero)
        {
            selfCollider.enabled = true;
            cachedSize = selfCollider.bounds.size;

            // ❗ 只有是拖拽生成的物体（isDragging 为 true）才临时关闭 collider
            if (isDragging)
            {
                selfCollider.enabled = false;
                Debug.Log($"[INIT SIZE] {name} -> size = {cachedSize:F3}, collider TEMP disabled for drag");
            }
            else
            {
                Debug.Log($"[INIT SIZE] {name} -> size = {cachedSize:F3}, collider REMAINS enabled for click");
            }
        }
    }


    void Update()
    {
        if (!isDragging) return;
        if (GamePauseState.IsPaused) return;

        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = GetMappedMousePosition();
            transform.position = mousePos;

            Vector3 center = transform.position;
            Vector3 snapped;
            if (TrySnapByEdge(center, cachedSize, out snapped))
                transform.position = snapped;
            else
                HideHighlight();
        }

        if (Input.GetMouseButtonDown(1))
        {
            transform.Rotate(0f, 90f, 0f, Space.Self);

            selfCollider.enabled = true;
            cachedSize = selfCollider.bounds.size;
            selfCollider.enabled = false;

            Vector3 center = transform.position;
            Vector3 snapped;
            if (TrySnapByEdge(center, cachedSize, out snapped))
                transform.position = snapped;
            else
                HideHighlight();
        }
    }

    void OnMouseDown()
    {
        if (GamePauseState.IsPaused)
        {
            Debug.Log("[MouseDown] Ignored due to pause");
            return;
        }

        Debug.Log($"[MouseDown ✅] {gameObject.name} clicked!");

        isDragging = true;

        if (selfCollider == null)
            selfCollider = GetComponent<Collider>();

        selfCollider.enabled = true;
        cachedSize = selfCollider.bounds.size;
        selfCollider.enabled = false;
    }



    void OnMouseUp()
    {
        isDragging = false;

        Vector3 center = transform.position;
        Vector3 snapped = Vector3.zero;

        if (TrySnapByEdge(center, cachedSize, out snapped))
        {
            PlacementType type = GetPlacementType();

            if (type == PlacementType.Floor && floorGrid != null)
            {
                float modelHalfHeight = cachedSize.y * 0.5f;
                snapped.y = floorGrid.GetFloorY() + modelHalfHeight;
            }
            else if (type == PlacementType.Wall && wallGrid != null)
            {
                float modelHalfDepth = cachedSize.z * 0.5f;
                snapped.z = wallGrid.GetWallZ() - modelHalfDepth;
            }

            transform.position = snapped;
        }
        else
        {
            HideHighlight();
            originSlot.ClearInstance();
            originSlot.ShowIcon();
            transform.SetParent(null);
            RoomManager.Instance?.RefreshCHIScore();
            Destroy(gameObject);
            return;
        }

        CheckPositionImmediately();
        selfCollider.enabled = true;
        HideHighlight();
        RoomManager.Instance?.RefreshCHIScore();
    }

    public void StopDragging()
    {
        isDragging = false;

        if (selfCollider == null)
            selfCollider = GetComponent<Collider>();

        selfCollider.enabled = true;
        HideHighlight();
    }




    private void HideHighlight()
    {
        floorGrid?.HideHighlight();
        wallGrid?.HideHighlight();
    }

    private bool TrySnapByEdge(Vector3 center, Vector3 size, out Vector3 snapped)
    {
        PlacementType type = GetPlacementType();
        if (type == PlacementType.Floor && floorGrid != null)
            return floorGrid.TrySnapByEdge(center, size, out snapped);
        if (type == PlacementType.Wall && wallGrid != null)
            return wallGrid.TrySnapByEdge(center, size, out snapped);

        snapped = center;
        return false;
    }

    private Vector3 GetMappedMousePosition()
    {
        PlacementType type = GetPlacementType();
        if (type == PlacementType.Floor)
            return CameraMapper.MappedMousePositionXZ;
        if (type == PlacementType.Wall)
            return CameraMapper.MappedMousePositionXY;
        return CameraMapper.MappedMousePosition;
    }

    private PlacementType GetPlacementType()
    {
        return GetComponent<ItemType>()?.type ?? PlacementType.Floor;
    }

    private Bounds GetActiveColliderBounds()
    {
        PlacementType type = GetPlacementType();

        if (type == PlacementType.Floor && floorGrid?.roomCollider != null)
        {
            Debug.Log("[CheckBounds] Using FloorGrid bounds.");
            return floorGrid.roomCollider.bounds;
        }

        if (type == PlacementType.Wall && wallGrid?.roomCollider != null)
        {
            Debug.Log("[CheckBounds] Using WallGrid bounds.");
            return wallGrid.roomCollider.bounds;
        }

        Debug.Log("[CheckBounds] Using fallback bounds.");
        return roomCollider?.bounds ?? new Bounds(transform.position, Vector3.one);
    }

    // public void AssignWallGrid(WallGrid grid)
    // {
    //     wallGrid = grid;
    // }

    public WallGrid GetWallGrid()
    {
        return wallGrid;
    }

    public FloorGrid GetFloorGrid()
    {
        return floorGrid;
    }

    public void EnableDraggingOnClick()
    {
        isDragging = false;  // ✅ 准备下一次点击重新拖
        if (selfCollider == null)
            selfCollider = GetComponent<Collider>();

        selfCollider.enabled = true;  // ✅ 确保后续点击能触发 OnMouseDown()
    }


    public void CheckPositionImmediately()
    {
        Vector3 pos = transform.position;
        Bounds bounds = GetActiveColliderBounds();

        bool inside;

        if (GetPlacementType() == PlacementType.Wall)
        {
            inside =
                pos.x >= bounds.min.x && pos.x <= bounds.max.x &&
                pos.y >= bounds.min.y && pos.y <= bounds.max.y &&
                Mathf.Abs(pos.z - bounds.center.z) <= 12f;
        }
        else // Floor
        {
            inside =
                pos.x >= bounds.min.x && pos.x <= bounds.max.x &&
                pos.z >= bounds.min.z && pos.z <= bounds.max.z &&
                Mathf.Abs(pos.y - bounds.center.y) <= 12f;
        }

        isValidPlacement = false;

        if (!inside)
        {
            originSlot.ClearInstance();
            originSlot.ShowIcon();

            if (FeedbackTextManager.Instance != null && floorGrid != null)
            {
                string name = gameObject.name.Replace("(Clone)", "");
                if (ShouldDestroy(name))
                {
                    FeedbackTextManager.Instance.ShowMessage("What the hell", Color.red);
                }
            }

            transform.SetParent(null);
            RoomManager.Instance?.RefreshCHIScore();
            Destroy(gameObject);
        }
        else
        {
            Debug.Log($"[ItemAutoDestroy] Valid placement at position: {pos}.");

            if (floorGrid != null || wallGrid != null)
            {
                isValidPlacement = (floorGrid?.IsCurrentHighlightValid ?? false) || (wallGrid?.IsCurrentHighlightValid ?? false);

                if (FeedbackTextManager.Instance != null)
                {
                    string name = gameObject.name.Replace("(Clone)", "");
                    if (ShouldDestroy(name))
                    {
                        FeedbackTextManager.Instance.ShowMessage(
                            isValidPlacement ? "Damn bro, you nailed it" : "What the hell",
                            isValidPlacement ? Color.green : Color.red
                        );
                    }
                }
            }
        }
    }
}