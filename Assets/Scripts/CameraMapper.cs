using UnityEngine;

public class CameraMapper : MonoBehaviour
{
    public Camera[] cameras;
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
