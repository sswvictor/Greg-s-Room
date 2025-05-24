using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class RoomData
{
    public GameObject roomPrefab;
    public List<GameObject> buttonPrefabs;
}

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    public List<RoomData> rooms;
    public Transform roomParent;
    public ItemBoxController itemBoxController;
    public GameObject transitionPanel;
    public float transitionDuration = 0.5f;
    public float maxScore = 60f;

    public TextMeshProUGUI chiScoreText;
    public ChiFillBar chiFillBar;

    private int currentIndex = -1;
    public GameObject currentRoom;

    public Transform doorMarker;
    public Transform windowMarker;

    private int currentRoomCHIScore = 0;
    public int totalCHIScore = 0;

    private void Awake()
    {
        Instance = this;

        if (chiScoreText == null)
        {
            GameObject chiTextGO = GameObject.Find("ChiScoreText");
            if (chiTextGO != null)
                chiScoreText = chiTextGO.GetComponent<TextMeshProUGUI>();
            else
                Debug.LogWarning("[RoomManager] ChiScoreText not found in scene.");
        }

        if (transitionPanel != null)
        {
            transitionPanel.SetActive(true);
            var cg = transitionPanel.GetComponent<CanvasGroup>();
            if (cg != null)
                cg.alpha = 1f;
        }
    }

    private void Start()
    {
        bool fromSummary = false;

        if (SummaryData.Instance != null)
        {
            fromSummary = true;
            currentIndex = SummaryData.Instance.nextRoomIndex;
            Debug.Log("[RoomManager] nextRoomIndex letto da SummaryData: " + currentIndex);
            Destroy(SummaryData.Instance.gameObject);
        }

        StartCoroutine(SwitchRoomCoroutine(fromSummary, false));
    }

    public void NextRoom()
    {
        StartCoroutine(SwitchRoomCoroutine(false, true));
    }

    private IEnumerator SwitchRoomCoroutine(bool fromSummary = false, bool goToSummary = false)
    {
        var cg = transitionPanel.GetComponent<CanvasGroup>();
        transitionPanel.SetActive(true);
        cg.alpha = 1f;
        yield return null;

        totalCHIScore += currentRoomCHIScore;
        if (currentRoom) Destroy(currentRoom);

        if (!fromSummary)
        {
            if (currentIndex == 0)
            {
                bool hasBasketball = false;
                Transform spawnRoot = GameObject.Find("ItemSpawnRoot")?.transform;
                if (spawnRoot != null)
                {
                    foreach (Transform child in spawnRoot)
                    {
                        if (child.name.Contains("Basketball") &&
                            child.GetComponent<ItemAutoDestroy>()?.isValidPlacement == true)
                        {
                            hasBasketball = true;
                            break;
                        }
                    }
                }
                currentIndex = hasBasketball ? 2 : 1;
            }
            else
            {
                currentIndex = 0;
            }
        }

        if (goToSummary)
        {
            if (SummaryData.Instance == null)
            {
                GameObject go = new GameObject("SummaryData");
                go.AddComponent<SummaryData>();
            }

            SummaryData.Instance.totalCHI = CHIScoreManager.Instance.CalculateTotalCHI();
            SummaryData.Instance.characterName = "Hippie Greg";
            SummaryData.Instance.characterSprite = null;
            SummaryData.Instance.nextRoomIndex = (currentIndex + 1) % rooms.Count;

            yield return new WaitForSeconds(0.2f);
            UnityEngine.SceneManagement.SceneManager.LoadScene("SummaryScene");
            yield break;
        }

        var room = rooms[currentIndex];
        Debug.Log($"[RoomManager] currentIndex = {currentIndex}");
        Debug.Log($"[RoomManager] rooms.Count = {rooms.Count}");
        currentRoom = Instantiate(room.roomPrefab, roomParent);
        doorMarker = currentRoom.transform.Find("DoorMarker");
        windowMarker = currentRoom.transform.Find("WindowMarker");

        var wallCtrl = currentRoom.GetComponent<WallVisibilityController>();
        if (wallCtrl != null) wallCtrl.ShowMainView();
        CameraMapper.Instance.SwitchTo(0);

        var spawnRootNew = currentRoom.transform.Find("ItemSpawnRoot");
        if (spawnRootNew != null)
            RoomSpawner.Instance.SetSpawnParent(spawnRootNew);

        itemBoxController.ShowButtons(room.buttonPrefabs);
        yield return new WaitForSeconds(0.2f);

        while (cg.alpha > 0f)
        {
            cg.alpha -= Time.deltaTime * 2;
            yield return null;
        }

        transitionPanel.SetActive(false);
        currentRoomCHIScore = 0;
        ResetCHIScore();
    }

    public int GetCurrentRoomCHI() => currentRoomCHIScore;
    public int GetTotalCHI() => totalCHIScore;

    public void ResetCHIScore()
    {
        if (chiScoreText != null)
            chiScoreText.text = "CHI Score: 0";

        chiFillBar?.UpdateBar(0, maxScore);
    }

    public void RefreshCHIScore()
    {
        Debug.Log("[CHI] RefreshCHIScore called!");

        if (chiScoreText == null)
        {
            Debug.LogWarning("[CHI] chiScoreText is NULL! Skip refresh.");
            return;
        }

        if (CHIScoreManager.Instance == null)
        {
            Debug.LogWarning("[CHI] CHIScoreManager.Instance is NULL! Skip refresh.");
            return;
        }
        currentRoomCHIScore = CHIScoreManager.Instance.CalculateTotalCHI();
        chiScoreText.text = $"CHI Score: {currentRoomCHIScore}";
        chiFillBar?.UpdateBar(currentRoomCHIScore, maxScore);
    }

    public int CurrentRoomIndex => currentIndex;

    public void LoadRoomFromQuickSave(QuickSaveData data)
    {
        StartCoroutine(LoadRoomFromSaveCoroutine(data));
    }

    private IEnumerator LoadRoomFromSaveCoroutine(QuickSaveData data)
    {
        currentIndex = data.roomIndex;
        if (currentRoom) Destroy(currentRoom);

        var room = rooms[currentIndex];
        currentRoom = Instantiate(room.roomPrefab, roomParent);

        var wallCtrl = currentRoom.GetComponent<WallVisibilityController>();
        if (wallCtrl != null) wallCtrl.ShowMainView();

        CameraMapper.Instance.SwitchTo(0);

        var spawnRoot = currentRoom.transform.Find("ItemSpawnRoot");
        if (spawnRoot == null)
        {
            Debug.LogError("[❌ RoomManager] ItemSpawnRoot not found!");
        }
        else
        {
            RoomSpawner.Instance.SetSpawnParent(spawnRoot);
        }

        itemBoxController.ShowButtons(room.buttonPrefabs);
        yield return new WaitForSeconds(0.1f);

        foreach (var item in data.placedItems)
        {
            GameObject prefab = FindPrefabByName(item.prefabName);
            if (prefab != null)
            {
                var go = Instantiate(prefab, item.position, item.rotation, spawnRoot);
                var tracker = go.GetComponent<ItemAutoDestroy>();
                var collider = go.GetComponent<Collider>();
                if (collider != null) collider.enabled = true;

                var slot = FindSlotForPrefab(item.prefabName);
                var roomCollider = currentRoom.GetComponentInChildren<Collider>();
                if (tracker != null)
                {
                    tracker.Init(slot, roomCollider);
                    PlacementType type = prefab.GetComponent<ItemType>()?.type ?? PlacementType.Floor;

                    if (type == PlacementType.Floor)
                    {
                        var grid = Object.FindFirstObjectByType<FloorGrid>();
                        if (grid != null)
                        {
                            grid.roomCollider = roomCollider;
                            tracker.floorGrid = grid;
                        }
                    }
                    else if (type == PlacementType.Wall)
                    {
                        var grid = roomCollider.GetComponentInChildren<WallGrid>();
                        if (grid != null)
                        {
                            grid.roomCollider = roomCollider;
                            tracker.wallGrid = grid;
                        }
                    }

                    tracker.StopDragging();
                    tracker.EnableDraggingOnClick();
                }
            }
        }

        var itemSlots = Object.FindObjectsOfType<ItemSlotController>();
        foreach (var slot in itemSlots)
        {
            string slotName = slot.modelPrefab?.name;
            var saved = data.itemBoxStatus.Find(e => e.prefabName == slotName);
            if (saved != null)
            {
                slot.SetSpawned(saved.hasSpawned);
            }
        }

        currentRoomCHIScore = data.currentRoomCHI;
        totalCHIScore = data.totalCHI;
        chiScoreText.text = $"CHI Score: {currentRoomCHIScore}";
        chiFillBar?.UpdateBar(currentRoomCHIScore, maxScore);
    }

    private ItemSlotController FindSlotForPrefab(string prefabName)
    {
        foreach (var slot in Object.FindObjectsOfType<ItemSlotController>())
        {
            if (slot.modelPrefab != null && slot.modelPrefab.name == prefabName)
            {
                return slot;
            }
        }
        return null;
    }

    private GameObject FindPrefabByName(string name)
    {
        foreach (var room in rooms)
        {
            foreach (var buttonPrefab in room.buttonPrefabs)
            {
                var slot = buttonPrefab.GetComponent<ItemSlotController>();
                if (slot != null && slot.modelPrefab != null && slot.modelPrefab.name == name)
                {
                    return slot.modelPrefab;
                }
            }
        }
        return null;
    }

    bool AlreadyHasBed(Transform spawnRoot)
    {
        if (spawnRoot == null) return false;
        foreach (var t in spawnRoot.GetComponentsInChildren<Transform>(true))
        {
            if (t.gameObject.name.Contains("Bed")) return true;
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && currentRoom != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(GetRoomCenter(), 0.2f);
        }
    }

    public Vector3 GetRoomCenter()
    {
        var floor = currentRoom?.GetComponentInChildren<FloorGrid>();
        if (floor != null && floor.roomCollider != null)
            return floor.roomCollider.bounds.center;

        return currentRoom != null ? currentRoom.transform.position : Vector3.zero;
    }
}
