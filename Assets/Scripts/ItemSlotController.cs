using UnityEngine;
using UnityEngine.UI;

public class ItemSlotController : MonoBehaviour
{
    [Header("要生成的物体 prefab")]
    public GameObject modelPrefab;

    [Header("图标对象（子物体）")]
    public GameObject iconObject;

    private GameObject spawnedInstance;

    public void OnClick()
    {
        if (HasSpawned() || modelPrefab == null) return;

        RoomSpawner.Instance.Spawn(modelPrefab);
        SetSpawned(true);
    }

    public void RegisterInstance(GameObject obj)
    {
        spawnedInstance = obj;
    }

    public void ClearInstance()
    {
        spawnedInstance = null;
    }

    public void SetSpawned(bool state)
    {
        if (!state)
        {
            ClearInstance();
            ShowIcon();   // ✅ 恢复图标
        }
        else
        {
            HideIcon();   // ✅ 隐藏图标
        }
    }

    public void HideIcon()
    {
        if (iconObject != null)
            iconObject.SetActive(false);
    }

    public void ShowIcon()
    {
        if (iconObject != null)
            iconObject.SetActive(true);
    }

    public bool HasSpawned()
    {
        return spawnedInstance != null;
    }

    public GameObject GetSpawnedInstance()
    {
        return spawnedInstance;
    }
}
