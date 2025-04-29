using UnityEngine;
using UnityEngine.UI;

public class ChiManager : MonoBehaviour
{
    [Header("Chi Settings")]
    [SerializeField] private float maxChi = 60f;
    [SerializeField] private float chiPerItem = 10f;

    [Header("UI References")]
    [Tooltip("Assign the UI Image component used for the Chi fill bar.")]
    [SerializeField] private Image chiFillImage;

    private float currentChi = 0f;

    void Start()
    {
        Debug.Log("[ChiManager] Starting up. ChiFillImage assigned: " + (chiFillImage != null));
        
        if (chiFillImage != null)
        {
            Debug.Log($"[ChiManager] FillBar sprite assigned: {chiFillImage.sprite != null}, Material: {chiFillImage.material != null}");
            if (chiFillImage.sprite == null)
                Debug.LogError("[ChiManager] FillBar sprite is null! Please check Image component.");
        }
        
        UpdateChiUI(); // Initial UI update to show zero state
    }

    void OnEnable()
    {
        Debug.Log("[ChiManager] OnEnable - Subscribing to PlacementManager events");
        // Subscribe to the placement event
        PlacementManager.OnItemPlaced += HandleItemPlaced;
    }

    void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        PlacementManager.OnItemPlaced -= HandleItemPlaced;
    }

    private void HandleItemPlaced(GameObject placedItem)
    {
        // Increment Chi
        currentChi += chiPerItem;

        // Clamp Chi to the maximum value
        currentChi = Mathf.Clamp(currentChi, 0f, maxChi);

        Debug.Log($"[ChiManager] Item placed: {placedItem.name}. Current Chi: {currentChi}/{maxChi}");

        // Update the UI
        UpdateChiUI();
    }

    private void UpdateChiUI()
    {
        if (chiFillImage != null)
        {
            float fillAmount = currentChi / maxChi;
            chiFillImage.fillAmount = fillAmount;
            Debug.Log($"[ChiManager] Updating UI - Current Chi: {currentChi}, Max Chi: {maxChi}, Fill Amount: {fillAmount}");
        }
        else
        {
            Debug.LogError("[ChiManager] Chi Fill Image is not assigned in the Inspector. Please assign the FillBar Image component.");
        }
    }

    // Optional: Add methods to get current/max chi if needed elsewhere
    public float GetCurrentChi() => currentChi;
    public float GetMaxChi() => maxChi;
}
