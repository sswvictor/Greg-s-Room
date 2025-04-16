using UnityEngine; 

public class FloorGrid : MonoBehaviour
{
    public int gridWidth = 12;
    public int gridHeight = 12;
    public float cellSize = 0.1f;
    public float floorX = 0.4f;
    public float floorZ = 0.4f;
    public float floorY = -0.33f;

    public GameObject highlightTilePrefab; // Assign a prefab with SpriteRenderer

    private Vector3 origin;
    private Vector3 rightDir;
    private Vector3 forwardDir;

    private GameObject highlightInstance;

    void Awake()
    {
        Vector3 center = new Vector3(floorX, floorY, floorZ);
        rightDir = transform.right.normalized;
        forwardDir = transform.forward.normalized;

        origin = center
               - rightDir * (gridWidth * 0.5f * cellSize)
               - forwardDir * (gridHeight * 0.5f * cellSize);
    }

    public bool TrySnapByEdge(Vector3 center, Vector3 size, out Vector3 snappedPos)
    {
        Vector3 extents = size * 0.5f;
        Vector3 corner = center + new Vector3(+extents.x, -extents.y, +extents.z);
        Vector3 toCorner = corner - origin;

        int i = Mathf.RoundToInt(Vector3.Dot(toCorner, rightDir) / cellSize);
        int j = Mathf.RoundToInt(Vector3.Dot(toCorner, forwardDir) / cellSize);

        int di = Mathf.CeilToInt(size.x / cellSize);
        int dj = Mathf.CeilToInt(size.z / cellSize);

        if (i < 0 || j < 0 || i + di > gridWidth || j + dj > gridHeight)
        {
            snappedPos = Vector3.zero;
            HideHighlight();
            return false;
        }

        Vector3 snappedCorner = origin + rightDir * i * cellSize + forwardDir * j * cellSize;
        Vector3 offset = center - corner;
        snappedPos = snappedCorner + offset;

        ShowHighlightArea(snappedCorner, di, dj);
        return true;
    }

    private void ShowHighlightArea(Vector3 corner, int w, int h)
    {
        if (highlightTilePrefab == null)
        {
            Debug.LogWarning("[HIGHLIGHT SPRITE] Missing highlightTilePrefab reference.");
            return;
        }

        if (highlightInstance == null)
        {
            highlightInstance = Instantiate(highlightTilePrefab);
            highlightInstance.name = "GridHighlightSprite";
        }

        Vector3 center = corner - new Vector3(w * 0.5f * cellSize, 0.01f, h * 0.5f * cellSize);

        highlightInstance.transform.position = center;
        highlightInstance.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        highlightInstance.transform.localScale = new Vector3(w * cellSize, h * cellSize, 0.05f); // Sprite 在 XY 面，躺下后 xy=地面尺寸，z=厚度

        highlightInstance.SetActive(true);

        var sr = highlightInstance.GetComponent<SpriteRenderer>();
        sr.color = new Color(0f, 0.8f, 1f, 0.6f);
        sr.sortingLayerName = "UI";
        sr.sortingOrder = 100;

        // Debug.Log($"[HIGHLIGHT ✅] corner={corner}, w={w}, h={h}, finalCenter={center}, finalScale={highlightInstance.transform.localScale}");
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
    forwardDir = transform.forward.normalized;

    Vector3 center = new Vector3(floorX, floorY, floorZ);
    Vector3 drawOrigin = center
                       - rightDir * (gridWidth * 0.5f * cellSize)
                       - forwardDir * (gridHeight * 0.5f * cellSize);
    drawOrigin.y = floorY;

    Gizmos.color = Color.yellow;

    for (int i = 0; i <= gridWidth; i++)
    {
        Vector3 start = drawOrigin + rightDir * i * cellSize;
        Vector3 end = start + forwardDir * gridHeight * cellSize;
        Gizmos.DrawLine(start, end);
    }

    for (int j = 0; j <= gridHeight; j++)
    {
        Vector3 start = drawOrigin + forwardDir * j * cellSize;
        Vector3 end = start + rightDir * gridWidth * cellSize;
        Gizmos.DrawLine(start, end);
    }
}
#endif

}