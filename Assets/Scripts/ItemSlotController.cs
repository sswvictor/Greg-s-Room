using UnityEngine;
using UnityEngine.UI;

public class ItemSlotController : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public Image iconImage;

    private GameObject spawnedInstance;

    public bool HasSpawned() => spawnedInstance != null;
    public void HideIcon() => iconImage.enabled = false;
    public void ShowIcon() => iconImage.enabled = true;

    public void RegisterInstance(GameObject go)
    {
        spawnedInstance = go;
    }

    public void ClearInstance()
    {
        spawnedInstance = null;
    }
}
