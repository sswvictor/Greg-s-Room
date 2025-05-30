using UnityEngine;

public class MouseRaycastDebugger : MonoBehaviour
{
    public CameraMapper mapper; 
    public bool drawDebugRay = true;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Camera cam = (mapper != null) ? mapper.cameras[mapperCurrentIndex()] : Camera.main;

            if (cam == null)
            {
                return;
            }

            Vector3 screenPos = Input.mousePosition;
            Ray ray = cam.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                Collider col = hit.collider;
                Debug.Log($"[RAYCAST DEBUG] Hit object: {col.name}, worldPos: {hit.point:F3}, layer: {LayerMask.LayerToName(col.gameObject.layer)}, colliderEnabled: {col.enabled}");
            }

            if (drawDebugRay)
                Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);
        }
    }

    private int mapperCurrentIndex()
    {
        var field = typeof(CameraMapper).GetField("currentIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (int)field.GetValue(mapper);
    }
}
