using UnityEngine;

public class GridFaceRenderer : MonoBehaviour
{
    public GridFace grid;

    private void OnDrawGizmos()
    {
        if (grid == null) return;

        for (int u = 0; u < grid.width; u++)
        {
            for (int v = 0; v < grid.height; v++)
            {
                Vector2 center = grid.GetCellCenter(u, v);

                // 画格子边框
                Vector2 p0 = center - (grid.rightDir + grid.upDir) * grid.cellSize / 2f;
                Vector2 p1 = p0 + grid.rightDir * grid.cellSize;
                Vector2 p2 = p1 + grid.upDir * grid.cellSize;
                Vector2 p3 = p0 + grid.upDir * grid.cellSize;

                Gizmos.color = Color.red;
                Gizmos.DrawLine(p0, p1);
                Gizmos.DrawLine(p1, p2);
                Gizmos.DrawLine(p2, p3);
                Gizmos.DrawLine(p3, p0);
            }
        }
    }
}
