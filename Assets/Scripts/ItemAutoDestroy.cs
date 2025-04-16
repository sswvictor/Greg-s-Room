using UnityEngine;

public class ItemAutoDestroy : MonoBehaviour
{
    private ItemSlotController originSlot;
    private Collider roomCollider;
    private FloorGrid floorGrid;
    private bool isDragging = true;

    private Vector3 cachedSize;
    private Collider selfCollider;

    public void Init(ItemSlotController slot, Collider room)
    {
        originSlot = slot;
        roomCollider = room;
    }

    void Start()
    {
        floorGrid = Object.FindFirstObjectByType<FloorGrid>();
        selfCollider = GetComponent<Collider>();

        if (cachedSize == Vector3.zero)
        {
            selfCollider.enabled = true;
            cachedSize = selfCollider.bounds.size;
            selfCollider.enabled = false;
            Debug.Log($"[INIT SIZE] cachedSize = {cachedSize}");
        }
    }

    void Update()
    {
        if (!isDragging) return;

        if (Input.GetMouseButton(0))
        {
            transform.position = CameraMapper.MappedMousePosition;

            Vector3 center = transform.position;
            Vector3 snapped;
            if (floorGrid != null && floorGrid.TrySnapByEdge(center, cachedSize, out snapped))
            {
                transform.position = snapped;
            }
            else
            {
                floorGrid?.HideHighlight();
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            transform.Rotate(0f, 90f, 0f, Space.Self);

            // 重新计算尺寸并刷新投影
            selfCollider.enabled = true;
            cachedSize = selfCollider.bounds.size;
            selfCollider.enabled = false;

            Vector3 center = transform.position;
            Vector3 snapped;
            if (floorGrid != null && floorGrid.TrySnapByEdge(center, cachedSize, out snapped))
            {
                transform.position = snapped;
            }
            else
            {
                floorGrid?.HideHighlight();
            }
        }
    }

    void OnMouseDown()
    {
        isDragging = true;
        selfCollider.enabled = true;
        cachedSize = selfCollider.bounds.size;
        selfCollider.enabled = false;
    }

    void OnMouseUp()
    {
        isDragging = false;

        Vector3 center = transform.position;
        Vector3 snapped = Vector3.zero;

        if (floorGrid != null && floorGrid.TrySnapByEdge(center, cachedSize, out snapped))
        {
            transform.position = snapped;
        }
        else
        {
            floorGrid?.HideHighlight();
            originSlot.ClearInstance();
            originSlot.ShowIcon();
            Destroy(gameObject);
            return;
        }

        CheckPositionImmediately();
        selfCollider.enabled = true;
        floorGrid?.HideHighlight();
    }

    public void StopDragging()
    {
        isDragging = false;
        selfCollider.enabled = true;
        floorGrid?.HideHighlight();
    }

    public void CheckPositionImmediately()
    {
        Vector3 pos = transform.position;
        Bounds bounds = roomCollider.bounds;

        float allowedMinY = bounds.min.y - 5f;
        float allowedMaxY = bounds.max.y + 5f;

        bool inside =
            pos.x >= bounds.min.x && pos.x <= bounds.max.x &&
            pos.z >= bounds.min.z && pos.z <= bounds.max.z &&
            pos.y >= allowedMinY && pos.y <= allowedMaxY;

        if (!inside)
        {
            originSlot.ClearInstance();
            originSlot.ShowIcon();
            Destroy(gameObject);
        }
    }
}