using UnityEngine;

public class MouseRaycastDebugger : MonoBehaviour
{
    public CameraMapper mapper;  // 拖入你的 CameraMapper 实例
    public bool drawDebugRay = true;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Camera cam = (mapper != null) ? mapper.cameras[mapperCurrentIndex()] : Camera.main;

            if (cam == null)
            {
                Debug.LogWarning("[RAYCAST DEBUG] ❌ No valid camera found.");
                return;
            }

            Vector3 screenPos = Input.mousePosition;
            Ray ray = cam.ScreenPointToRay(screenPos);

            Debug.Log($"[RAYCAST DEBUG] 👆 Mouse clicked at screen {screenPos}, using camera: {cam.name}");

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                Collider col = hit.collider;
                Debug.Log($"[RAYCAST DEBUG ✅] Hit object: {col.name}, worldPos: {hit.point:F3}, layer: {LayerMask.LayerToName(col.gameObject.layer)}, colliderEnabled: {col.enabled}");
            }
            else
            {
                Debug.Log($"[RAYCAST DEBUG ❌] No object hit. Ray origin: {ray.origin:F3}, direction: {ray.direction.normalized:F3}");
            }

            if (drawDebugRay)
                Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);
        }
    }

    // 用反射读取 CameraMapper 的私有字段 currentIndex
    private int mapperCurrentIndex()
    {
        var field = typeof(CameraMapper).GetField("currentIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (int)field.GetValue(mapper);
    }
}
