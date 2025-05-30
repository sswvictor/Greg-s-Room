using UnityEngine;

public class FloorGrid : MonoBehaviour
{
    public int gridWidth = 12;
    public int gridHeight = 12;
    public float cellSize = 0.1f;
    public float floorX = 0.4f;
    public float floorZ = 0.4f;
    public float floorY = -0.33f;

    public float GetFloorY()
    {
        return floorY;
    }


    public GameObject highlightTilePrefab; 
    public Collider roomCollider; 

    private Vector3 origin;
    private Vector3 rightDir;
    private Vector3 forwardDir;

    private GameObject highlightInstance;

    public bool IsCurrentHighlightValid { get; private set; }

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

        Vector3 snappedCorner = origin + rightDir * i * cellSize + forwardDir * j * cellSize;
        Vector3 offset = center - corner;
        snappedPos = snappedCorner + offset;
        snappedPos.y = center.y;

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
            highlightInstance.name = "GridHighlightSprite";
        }

        Vector3 center = corner - new Vector3(w * 0.5f * cellSize, 0.01f, h * 0.5f * cellSize);
        highlightInstance.transform.position = center;
        highlightInstance.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
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

            bool zValid = highlightBounds.min.z >= roomBounds.min.z - epsilon &&
                        highlightBounds.max.z <= roomBounds.max.z + epsilon;

            isValid = xValid && zValid;
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
                if (col.CompareTag("Wall")|| col.CompareTag("OtherObj"))
                {
                    isValid = false;
                    break;
                }

                Bounds ob = col.bounds;
                float topY = ob.max.y;

                Vector3 p = highlightInstance.transform.position;
                p.y = topY + 0.01f;
                highlightInstance.transform.position = p;
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

    public float GetHighlightYOffset()
    {
        if (highlightInstance == null || !highlightInstance.activeSelf)
            return 0f;

        return highlightInstance.transform.position.y - floorY;
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
