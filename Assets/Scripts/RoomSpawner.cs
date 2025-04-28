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
        var instance = Instantiate(prefab, new Vector3(0, 0.5f, 0), Quaternion.identity, spawnParent);
        Debug.Log("[RoomSpawner] Spawned item under: " + spawnParent.name);
    }
}
