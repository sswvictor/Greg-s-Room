using UnityEngine;

public class ItemPositionDebugger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("<color=lime>=== DEBUG INFO ===</color>");
            Debug.Log($"[Transform] position = {transform.position:F4}");

            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Bounds b = col.bounds;
                Debug.Log($"[Collider] bounds.center = {b.center:F4}, size = {b.size:F4}");
            }
            else
            {
                Debug.LogWarning("[Collider] Not found on object.");
            }

            Renderer r = GetComponent<Renderer>();
            if (r != null)
            {
                Bounds b = r.bounds;
                Debug.Log($"[Renderer] bounds.center = {b.center:F4}, size = {b.size:F4}");
            }
            else
            {
                Debug.LogWarning("[Renderer] Not found on object.");
            }

            Debug.Log("<color=lime>=================</color>");
        }
    }
}
