using UnityEngine;

public class CameraMapper : MonoBehaviour
{
    public Camera[] cameras;
    private int currentIndex = 0;

    private static Vector3 _mappedMousePosition;
    public static Vector3 MappedMousePosition => _mappedMousePosition;

    private void Update()
    {
        // 实时更新映射的鼠标世界坐标（基于当前相机）
        _mappedMousePosition = GetWorldFromScreen(Input.mousePosition);
    }

    public void SwitchTo(int index)
    {
        currentIndex = index;
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].enabled = (i == index);
        }
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

        // 默认 Y=0 拖拽平面，你可以扩展为动态贴合目标物体
        Plane dragPlane = new Plane(Vector3.up, Vector3.zero);
        if (dragPlane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);

        return Vector3.zero;
    }
}
