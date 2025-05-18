// ✅ QuickSaveSystem.cs
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuickSaveData
{
    public int roomIndex;
    public int currentRoomCHI;
    public int totalCHI;

    public List<PlacedItemData> placedItems = new();
    public List<ItemSlotStatus> itemBoxStatus = new();
}

[System.Serializable]
public class PlacedItemData
{
    public string prefabName;
    public Vector3 position;
    public Quaternion rotation;
    public bool isValid;
}

[System.Serializable]
public class ItemSlotStatus
{
    public string prefabName;
    public bool hasSpawned;
}

public static class QuickSaveSystem
{
    private const string SAVE_KEY = "quick_save_json";

    public static void Save()
    {
        var data = new QuickSaveData();
        data.roomIndex = RoomManager.Instance.CurrentRoomIndex;
        data.currentRoomCHI = RoomManager.Instance?.GetCurrentRoomCHI() ?? 0;
        data.totalCHI = RoomManager.Instance?.totalCHIScore ?? 0;


        Transform spawnRoot = GameObject.Find("ItemSpawnRoot")?.transform;
        if (spawnRoot != null)
        {
            foreach (Transform child in spawnRoot)
            {
                var tracker = child.GetComponent<ItemAutoDestroy>();
                if (tracker != null)
                {
                    data.placedItems.Add(new PlacedItemData
                    {
                        prefabName = child.name.Replace("(Clone)", ""),
                        position = child.position,
                        rotation = child.rotation,
                        isValid = tracker.isValidPlacement
                    });
                }
            }
        }

        var itemSlots = Object.FindObjectsOfType<ItemSlotController>();
        foreach (var slot in itemSlots)
        {
            if (slot.modelPrefab != null)
            {
                data.itemBoxStatus.Add(new ItemSlotStatus
                {
                    prefabName = slot.modelPrefab.name,
                    hasSpawned = slot.HasSpawned()
                });
            }
        }

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.SetInt("has_quick_save", 1);
        PlayerPrefs.Save();

        Debug.Log("[QUICK SAVE ✅] Saved quick data.");
    }

    public static bool HasSave() => PlayerPrefs.GetInt("has_quick_save", 0) == 1;

    public static void Load()
    {
        if (!HasSave())
        {
            Debug.LogWarning("[QUICK LOAD ⚠️] No quick save found.");
            return;
        }

        string json = PlayerPrefs.GetString(SAVE_KEY);
        QuickSaveData data = JsonUtility.FromJson<QuickSaveData>(json);
        RoomManager.Instance.LoadRoomFromQuickSave(data);
    }
}
