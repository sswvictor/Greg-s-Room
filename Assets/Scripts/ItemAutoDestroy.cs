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
    }

    public void PrepareDragging()
    {
        if (selfCollider == null)
            selfCollider = GetComponent<Collider>();

        if (floorGrid == null)
            floorGrid = Object.FindFirstObjectByType<FloorGrid>();

        isDragging = true;
        selfCollider.enabled = true;
        cachedSize = selfCollider.bounds.size;
        selfCollider.enabled = false;

        floorGrid?.ShowColoredGrid();
        Debug.Log("✅ Chiamata a ShowColoredGrid fatta");

        Debug.Log("🔍 PrepareDragging() chiamato");
        Debug.Log($"[PREPARE DRAGGING] cachedSize = {cachedSize}");
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

    public void TryPlace()
    {
        isDragging = false;

        Vector3 center = transform.position;
        Vector3 snapped = Vector3.zero;

        Debug.Log($"TryPlace - center: {center}");

        if (floorGrid != null && floorGrid.TrySnapByEdge(center, cachedSize, out snapped))
        {
            transform.position = snapped;

            BedLogic logic = GetComponent<BedLogic>();
            if (logic != null)
            {
                Debug.Log("Call ShowFeedback()");
                logic.ShowFeedback();
            }
        }
        else
        {
            Debug.LogWarning("Snap failed. Obhect destroyed.");
            floorGrid?.HideHighlight();
            originSlot.ClearInstance();
            originSlot.ShowIcon();
            Destroy(gameObject);
            return;
        }

        CheckPositionImmediately();
        selfCollider.enabled = true;
        floorGrid?.HideColoredGrid();

    }

    public void StopDragging()
    {
        isDragging = false;
        selfCollider.enabled = true;
        floorGrid?.HideHighlight();
        floorGrid?.HideColoredGrid();
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
