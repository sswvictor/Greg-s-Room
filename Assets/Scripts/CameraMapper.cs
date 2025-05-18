// ✅ CameraMapper.cs（支持 Floor / Wall / 自定义平面投影）
using UnityEngine;

public class CameraMapper : MonoBehaviour
{
    public static CameraMapper Instance;
    public Camera[] cameras;
    public Canvas[] canvases;
    private int currentIndex = 0;

    [Header("Plane高度（地面）")]
    public float ProjY = -0.15f; // Y轴吸附高度（可调）

    [Header("墙面位置")]
    public float ProjZ = 2.0f;   // Z轴正墙（XY投影）
    public float ProjX = -2.0f;  // X轴侧墙（YZ投影）

    private static Vector3 _mappedMousePosition;
    public static Vector3 MappedMousePosition => _mappedMousePosition;

    public static Vector3 MappedMousePositionXZ { get; private set; } = Vector3.zero;
    public static Vector3 MappedMousePositionXY { get; private set; } = Vector3.zero;
    public static Vector3 MappedMousePositionYZ { get; private set; } = Vector3.zero;

    private void Awake() => Instance = this;

    private void Update()
    {
        Camera cam = cameras[currentIndex];
        Vector3 screenPos = Input.mousePosition;
        Ray ray = cam.ScreenPointToRay(screenPos);

        Plane yPlane = new Plane(Vector3.up, new Vector3(0, ProjY, 0));
        Plane zPlane = new Plane(Vector3.forward, new Vector3(0, 0, ProjZ));
        Plane xPlane = new Plane(Vector3.right, new Vector3(ProjX, 0, 0));

        if (yPlane.Raycast(ray, out float yEnter))
            MappedMousePositionXZ = ray.GetPoint(yEnter);

        if (zPlane.Raycast(ray, out float zEnter))
            MappedMousePositionXY = ray.GetPoint(zEnter);

        if (xPlane.Raycast(ray, out float xEnter))
            MappedMousePositionYZ = ray.GetPoint(xEnter);

        _mappedMousePosition = MappedMousePositionXZ; // 默认是 XZ 平面（floor）
    }

    public void SwitchTo(int index)
    {
        currentIndex = index;
        for (int i = 0; i < cameras.Length; i++)
            cameras[i].enabled = (i == index);

        Camera activeCamera = cameras[currentIndex];
        foreach (var canvas in canvases)
        {
            if (canvas != null)
            {
                canvas.worldCamera = null;
                canvas.worldCamera = activeCamera;
            }
        }
        Debug.Log($"[CameraMapper] Switched to Camera {currentIndex}: {activeCamera.name}");
    }

    public void SwitchNext()
    {
        currentIndex = (currentIndex + 1) % cameras.Length;
        SwitchTo(currentIndex);
    }

    public Camera GetCurrentCamera() => cameras[currentIndex];

    public static Vector3 GetMouseProjectionOnPlane(Vector3 normal, Vector3 pointOnPlane)
    {
        if (Instance == null) return Vector3.zero;
        Camera cam = Instance.cameras[Instance.currentIndex];
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane customPlane = new Plane(normal, pointOnPlane);
        return customPlane.Raycast(ray, out float enter) ? ray.GetPoint(enter) : Vector3.zero;
    }
} 
