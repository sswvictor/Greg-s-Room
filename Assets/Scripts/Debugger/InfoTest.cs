using UnityEngine;

public class RoomInfoPrinter : MonoBehaviour
{

    void Start()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("=== [Room Info] ===");
            Debug.Log("World Position: " + transform.position);
            Debug.Log("World Rotation (Euler): " + transform.rotation.eulerAngles);
            Debug.Log("World Scale: " + transform.lossyScale);

            if (TryGetComponent(out Collider col))
            {
                Debug.Log("Collider.bounds.size (world): " + col.bounds.size);
            }

            if (TryGetComponent(out Renderer rend))
            {
                Debug.Log("Renderer.bounds.size (world): " + rend.bounds.size);
            }

            Debug.Log("World Right: " + transform.right);
            Debug.Log("World Up: " + transform.up);
            Debug.Log("World Forward: " + transform.forward);
        } 
    }
}
