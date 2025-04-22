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
    
    public GameObject tilePrefab;
    private GameObject[,] tileGrid; 


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
        highlightInstance.transform.localScale = new Vector3(w * cellSize, h * cellSize, 0.05f);

        highlightInstance.SetActive(true);

        var sr = highlightInstance.GetComponent<SpriteRenderer>();
        sr.color = new Color(0f, 0.8f, 1f, 0.6f);
        sr.sortingLayerName = "UI";
        sr.sortingOrder = 100;
    }

    public void HideHighlight()
    {
        if (highlightInstance != null)
            highlightInstance.SetActive(false);
    }

    public void ShowColoredGrid(){

        if (tilePrefab == null)
        {
            Debug.LogWarning("tilePrefab not assigned");
            return;
        }

        if (tileGrid == null)
            tileGrid = new GameObject[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                if (tileGrid[x, z] == null)
                {
                    GameObject tile = Instantiate(tilePrefab, transform);
                    tile.name = $"Tile_{x}_{z}";
                    tile.transform.position = origin 
                        + rightDir * (x + 0.5f) * cellSize 
                        + forwardDir * (z + 0.5f) * cellSize
                        + Vector3.up * 0.015f;

                    tile.transform.rotation = Quaternion.Euler(90, 0, 0);
                    tile.transform.localScale = new Vector3(cellSize, cellSize, 1f);

                    tileGrid[x, z] = tile;
                }

                var renderer = tileGrid[x, z].GetComponentInChildren<SpriteRenderer>();
                renderer.color = (x < gridWidth / 2) 
                    ? new Color(0f, 1f, 0f, 0.4f) 
                    : new Color(1f, 0f, 0f, 0.4f);

                tileGrid[x, z].SetActive(true);
            }
        }
    }


    public void HideColoredGrid()
    {
        if (tileGrid == null) return;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                if (tileGrid[x, z] != null)
                    tileGrid[x, z].SetActive(false);
            }
        }
    }

    public bool IsPositionInRedZone(Vector3 worldPosition)
    {
        Vector3 toCorner = worldPosition - origin;

        int i = Mathf.FloorToInt(Vector3.Dot(toCorner, rightDir) / cellSize);

        //if the cell is in the right half of the grid, is in the red zone
        return i >= gridWidth / 2;
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
