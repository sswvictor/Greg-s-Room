using UnityEngine;

public enum FaceType { Floor, LeftWall, RightWall }

[System.Serializable]
public class GridFace
{
    public FaceType faceType;
    public Vector2 origin;
    public Vector2 rightDir;
    public Vector2 upDir;
    public int width;
    public int height;
    public float cellSize;

    public Vector2 GetCellCenter(int u, int v)
    {
        return origin + (rightDir * u + upDir * v) * cellSize;
    }
}
