using UnityEngine;

public class RoomSpawner : MonoBehaviour
{
    public static RoomSpawner Instance;

    private Transform spawnParent;

    private void Awake()
    {
        Instance = this;
    }

    public void SetSpawnParent(Transform parent)
    {
        spawnParent = parent;
    }

    public void Spawn(GameObject prefab)
    {
        if (spawnParent == null)
        {
            Debug.LogError("SpawnParent is null! Item will be created in Scene root.");
            return;
        }

        // ✅ 重新映射一次鼠标位置，确保最新
        Vector3 startPos = GetWorldFromScreen(Input.mousePosition);
        var instance = Instantiate(prefab, startPos, Quaternion.identity, spawnParent);
        Debug.Log("[RoomSpawner] Spawned item under: " + spawnParent.name);
    }

    private Vector3 GetWorldFromScreen(Vector3 screenPos)
    {
        Camera cam = CameraMapper.Instance.GetCurrentCamera();
        Ray ray = cam.ScreenPointToRay(screenPos);
        Plane dragPlane = new Plane(Vector3.up, new Vector3(0, CameraMapper.Instance.panel, 0));
        if (dragPlane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);

        return Vector3.zero;
    }
}