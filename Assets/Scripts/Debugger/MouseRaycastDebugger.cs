using UnityEngine;

public class MouseRaycastDebugger : MonoBehaviour
{
    public CameraMapper mapper;  // 拖入你的 CameraMapper 实例

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Camera cam = (mapper != null) ? mapper.cameras[mapperCurrentIndex()] : Camera.main;

            if (cam == null)
            {
                Debug.LogWarning("[RAYCAST DEBUG] No valid camera found.");
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                Debug.Log($"[RAYCAST DEBUG] Mouse clicked on: {hit.collider.name} (layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)})");
            }
            else
            {
                Debug.Log("[RAYCAST DEBUG] Mouse clicked on nothing.");
            }
        }
    }

    // 用反射或扩展方法获取当前索引（因为 currentIndex 是 private）
    private int mapperCurrentIndex()
    {
        var field = typeof(CameraMapper).GetField("currentIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (int)field.GetValue(mapper);
    }
}
