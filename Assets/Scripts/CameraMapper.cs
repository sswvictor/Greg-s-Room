using UnityEngine;

public class CameraMapper : MonoBehaviour
{
    public Camera[] cameras;
    public Canvas[] canvases;   // 要同步的Canvas数组
    private int currentIndex = 0;
    public float panel = -0.15f;

    private static Vector3 _mappedMousePosition;
    public static Vector3 MappedMousePosition => _mappedMousePosition;

    private void Update()
    {
        _mappedMousePosition = GetWorldFromScreen(Input.mousePosition);
    }

    public void SwitchTo(int index)
    {
        currentIndex = index;

        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].enabled = (i == index);
        }

        Camera activeCamera = cameras[currentIndex];

        // ✅ 同步Canvas
        foreach (var canvas in canvases)
        {
            if (canvas != null)
            {
                canvas.worldCamera = null;
                canvas.worldCamera = activeCamera;
            }
        }

        // ✅ 打印调试信息
        Debug.Log($"[CameraMapper] Switched to Camera {currentIndex}: {activeCamera.name}");
    }

    public void SwitchNext()
    {
        currentIndex = (currentIndex + 1) % cameras.Length;
        SwitchTo(currentIndex);
    }

    private Vector3 GetWorldFromScreen(Vector3 screenPos)
    {
        Camera cam = cameras[currentIndex];
        Ray ray = cam.ScreenPointToRay(screenPos);

        Plane dragPlane = new Plane(Vector3.up, new Vector3(0, panel, 0));
        if (dragPlane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);

        return Vector3.zero;
    }
}
