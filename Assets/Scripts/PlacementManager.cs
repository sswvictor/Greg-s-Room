using System;
using UnityEngine;

public static class PlacementManager
{
    // Event triggered when an item is successfully placed.
    // Passes the GameObject of the placed item.
    public static event Action<GameObject> OnItemPlaced;

    /// <summary>
    /// Call this method after an item has been successfully placed and validated.
    /// </summary>
    /// <param name="placedItem">The GameObject that was successfully placed.</param>
    public static void NotifyItemPlaced(GameObject placedItem)
    {
        OnItemPlaced?.Invoke(placedItem);
        Debug.Log($"[PlacementManager] Item placed: {placedItem.name}");
    }

    // Prepared for future expansion:
    // public struct PlacedItemData {
    //     public GameObject ItemObject;
    //     public Vector3 Position;
    //     public float ChiValue;
    //     // Add other relevant metadata
    // }
    // public static event Action<PlacedItemData> OnItemPlacedData;
    // public static void NotifyItemPlaced(PlacedItemData data) { ... }
}
