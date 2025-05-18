using UnityEngine;

public class ItemAutoDestroy : MonoBehaviour
{
    private ItemSlotController originSlot;
    private Collider roomCollider;
    private FloorGrid floorGrid;
    private bool isDragging = true;

    private Vector3 cachedSize;
    private Collider selfCollider;
    
    // Reference to the feng shui visualizer
    private FengShuiVisualizer fengShuiVisualizer;

    [Tooltip("Drop Y-offset applied when mouse is released.")]
    public float dropYOffset = 0.1f;
    public bool isValidPlacement = false;


    public void Init(ItemSlotController slot, Collider room)
    {
        originSlot = slot;
        roomCollider = room;
    }

    void Start()
    {
        floorGrid = Object.FindFirstObjectByType<FloorGrid>();
        selfCollider = GetComponent<Collider>();
        
        // Find the feng shui visualizer
        fengShuiVisualizer = FindObjectOfType<FengShuiVisualizer>();

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
                
                // Show feng shui zones when dragging
                if (fengShuiVisualizer != null)
                {
                    string roomType = RoomManager.Instance?.GetCurrentRoomType() ?? "";
                    fengShuiVisualizer.ShowZonesForObject(gameObject, roomType);
                }
            }
            else
            {
                floorGrid?.HideHighlight();
                
                // Clear feng shui zones when not over valid grid
                fengShuiVisualizer?.ClearAllHighlights();
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
                
                // Update feng shui zones after rotation
                if (fengShuiVisualizer != null)
                {
                    string roomType = RoomManager.Instance?.GetCurrentRoomType() ?? "";
                    fengShuiVisualizer.ShowZonesForObject(gameObject, roomType);
                }
            }
            else
            {
                floorGrid?.HideHighlight();
                
                // Clear feng shui zones when not over valid grid
                fengShuiVisualizer?.ClearAllHighlights();
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
            snapped.y -= dropYOffset;
            transform.position = snapped;
        }
        else
        {
            floorGrid?.HideHighlight();
            
            // Clear feng shui zones
            if (fengShuiVisualizer != null)
            {
                fengShuiVisualizer.ClearAllHighlights();
            }
            
            originSlot.ClearInstance();
            originSlot.ShowIcon();
            transform.SetParent(null);
            RoomManager.Instance?.RefreshCHIScore();
            Destroy(gameObject);
            return;
        }

        CheckPositionImmediately();
        selfCollider.enabled = true;
        floorGrid?.HideHighlight();
        
        // Clear feng shui zones after placement
        if (fengShuiVisualizer != null)
        {
            fengShuiVisualizer.ClearAllHighlights();
        }
        
        RoomManager.Instance?.RefreshCHIScore();
    }

    public void StopDragging()
    {
        isDragging = false;
        selfCollider.enabled = true;
        floorGrid?.HideHighlight();
        
        // Clear feng shui zones
        if (fengShuiVisualizer != null)
        {
            fengShuiVisualizer.ClearAllHighlights();
        }
    }

   public void CheckPositionImmediately(){

    Vector3 pos = transform.position;
    Bounds bounds = roomCollider.bounds;

    float allowedMinY = bounds.min.y - 5f;
    float allowedMaxY = bounds.max.y + 5f;

    bool inside =
        pos.x >= bounds.min.x && pos.x <= bounds.max.x &&
        pos.z >= bounds.min.z && pos.z <= bounds.max.z &&
        pos.y >= allowedMinY && pos.y <= allowedMaxY;

    // Reset the isValidPlacement flag
    isValidPlacement = false;

    if (!inside)
    {
        originSlot.ClearInstance();
        originSlot.ShowIcon();

        if (FeedbackTextManager.Instance != null)
        {
            FeedbackTextManager.Instance.ShowMessage("Out of room bounds", Color.red);
        }

        transform.SetParent(null);
        RoomManager.Instance?.RefreshCHIScore();
        Destroy(gameObject);
    }
    else
    {
        Debug.Log($"[ItemAutoDestroy] Valid placement at position: {pos}.");

        if (floorGrid != null)
        {
            // Set placement validity based on grid highlight
            isValidPlacement = floorGrid.IsCurrentHighlightValid;

            // Feng shui feedback will be shown by the CHIScoreManager during RefreshCHIScore
            // However, still show basic invalid position feedback
            if (!isValidPlacement && FeedbackTextManager.Instance != null)
            {
                FeedbackTextManager.Instance.ShowMessage("Invalid position", Color.red);
            }
        }
    }
}

}
