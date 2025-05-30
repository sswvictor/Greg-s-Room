using UnityEngine;

public class WallGrid : MonoBehaviour
{
    public int gridWidth = 12;
    public int gridHeight = 12;
    public float cellSize = 0.1f;
    public float wallX = 0.0f;
    public float wallY = 0.0f;
    public float wallZ = 5.95f;

    public float GetWallZ()
    {
        return wallZ;
    }

    public GameObject highlightTilePrefab; 
    public Collider roomCollider;  

    private Vector3 origin;
    private Vector3 rightDir;
    private Vector3 upDir;

    private GameObject highlightInstance;

    public bool IsCurrentHighlightValid { get; private set; }

    void Awake()
    {
        Vector3 center = new Vector3(wallX, wallY, wallZ);

        rightDir = transform.right.normalized;
        upDir = transform.up.normalized;
        origin = center
            + rightDir * (-gridWidth * 0.5f * cellSize)
            + upDir * (-gridHeight * 0.5f * cellSize);
    }


    public bool TrySnapByEdge(Vector3 center, Vector3 size, out Vector3 snappedPos)
    {
        Vector3 halfRight = rightDir * (size.x * 0.5f);
        Vector3 halfUp = upDir * (size.y * 0.5f);
        Vector3 corner = center + halfRight + halfUp;

        Vector3 toCorner = corner - origin;

        int i = Mathf.RoundToInt(Vector3.Dot(toCorner, rightDir) / cellSize);
        int j = Mathf.RoundToInt(Vector3.Dot(toCorner, upDir) / cellSize);

        int di = Mathf.CeilToInt(size.x / cellSize);
        int dj = Mathf.CeilToInt(size.y / cellSize);

        Vector3 snappedCorner = origin + rightDir * i * cellSize + upDir * j * cellSize;
        Vector3 offset = center - corner;
        snappedPos = snappedCorner + offset;
        snappedPos.z = center.z;

        ShowHighlightArea(snappedCorner, di, dj);
        return true;
    }


private void ShowHighlightArea(Vector3 corner, int w, int h)
{
    if (highlightTilePrefab == null)
    {
        return;
    }

    if (highlightInstance == null)
    {
        highlightInstance = Instantiate(highlightTilePrefab);
        highlightInstance.name = "WallHighlight";
    }

    Vector3 center = corner
        + rightDir * (-w * 0.5f * cellSize)
        + upDir * (-h * 0.5f * cellSize)
        - transform.forward * 0.01f;

    highlightInstance.transform.position = center;
    highlightInstance.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    highlightInstance.transform.localScale = new Vector3(w * cellSize, h * cellSize, 0.05f);
    highlightInstance.SetActive(true);

    bool isValid = true;

    if (roomCollider != null)
    {
        Bounds highlightBounds = highlightInstance.GetComponent<Renderer>().bounds;
        Bounds roomBounds = roomCollider.bounds;
        const float epsilon = 0.1f;

        bool xValid = highlightBounds.min.x >= roomBounds.min.x - epsilon &&
                      highlightBounds.max.x <= roomBounds.max.x + epsilon;

        bool yValid = highlightBounds.min.y >= roomBounds.min.y - epsilon &&
                      highlightBounds.max.y <= roomBounds.max.y + epsilon;

        isValid = xValid && yValid;
    }

    if (isValid)
    {
        Collider[] overlapping = Physics.OverlapBox(
            highlightInstance.transform.position,
            highlightInstance.transform.localScale * 0.5f,
            highlightInstance.transform.rotation
        );

        foreach (var col in overlapping)
        {
            if (col == null || col.isTrigger) continue;
            if (col.gameObject == highlightInstance) continue;
            if (col == roomCollider) continue;

            isValid = false;
            break;
        }
    }

    IsCurrentHighlightValid = isValid;

    var sr = highlightInstance.GetComponent<SpriteRenderer>();
    sr.color = isValid
        ? new Color(0f, 0.8f, 0.3f, 0.8f)
        : new Color(1f, 0f, 0f, 0.8f);


    sr.sortingLayerName = "UI";
    sr.sortingOrder = 100;
}


    public void HideHighlight()
    {
        if (highlightInstance != null)
            highlightInstance.SetActive(false);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        rightDir = transform.right.normalized;
        upDir = transform.up.normalized;

        Vector3 center = new Vector3(wallX, wallY, wallZ);
        Vector3 drawOrigin = center
                           - rightDir * (gridWidth * 0.5f * cellSize)
                           - upDir * (gridHeight * 0.5f * cellSize);
        drawOrigin.z = wallZ;

        Gizmos.color = Color.cyan;

        for (int i = 0; i <= gridWidth; i++)
        {
            Vector3 start = drawOrigin + rightDir * i * cellSize;
            Vector3 end = start + upDir * gridHeight * cellSize;
            Gizmos.DrawLine(start, end);
        }

        for (int j = 0; j <= gridHeight; j++)
        {
            Vector3 start = drawOrigin + upDir * j * cellSize;
            Vector3 end = start + rightDir * gridWidth * cellSize;
            Gizmos.DrawLine(start, end);
        }
    }
#endif
}
