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
            return;
        }

        Vector3 startPos = GetSpawnPositionFor(prefab);
        var instance = Instantiate(prefab, startPos, Quaternion.identity, spawnParent);
    }

    private Vector3 GetSpawnPositionFor(GameObject prefab)
    {
        if (prefab == null)
            return Vector3.zero;

        var type = prefab.GetComponent<ItemType>()?.type ?? PlacementType.Floor;

        if (type == PlacementType.Wall)
            return CameraMapper.MappedMousePositionXY + Vector3.forward * 0.01f;
        else if (type == PlacementType.Floor)
            return CameraMapper.MappedMousePositionXZ;
        else
            return GetWorldFromScreen(Input.mousePosition);
    }

    private Vector3 GetWorldFromScreen(Vector3 screenPos)
    {
        Camera cam = CameraMapper.Instance.GetCurrentCamera();
        Ray ray = cam.ScreenPointToRay(screenPos);
        Plane dragPlane = new Plane(Vector3.up, new Vector3(0, CameraMapper.Instance.ProjY, 0));
        if (dragPlane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);

        return Vector3.zero;
    }
} 
