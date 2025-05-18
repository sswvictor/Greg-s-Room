using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class RoomData {
    public GameObject roomPrefab;
    public List<GameObject> buttonPrefabs; // ✅ 改为每个房间的按钮 prefab 列表
    // public List<Sprite> itemIcons;       // ❌ 已弃用
    // public List<GameObject> itemPrefabs; // ❌ 已弃用
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

        // ✅ 启动前先隐藏场景内容，确保只显示加载页
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
        // ✅ 避免中间显示空白，直接协程生成房间
        StartCoroutine(SwitchRoomCoroutine());
    }

    public void LoadNextRoom()
    {
        StartCoroutine(SwitchRoomCoroutine());
    }

    private IEnumerator SwitchRoomCoroutine()
    {
        var cg = transitionPanel.GetComponent<CanvasGroup>();
        transitionPanel.SetActive(true);

        // ✅ 确保加载遮罩不透明
        cg.alpha = 1f;
        yield return null;

        // 清除旧房间
        totalCHIScore += currentRoomCHIScore;
        Debug.Log($"[GLOBAL CHI] Added {currentRoomCHIScore} points. Total now = {totalCHIScore}");

        if (currentRoom)
            Destroy(currentRoom);

        // ✅ 特殊逻辑：room01 时根据是否放置 basketball 决定跳转
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
            currentIndex = 0; // ✅ 不管从 room1 还是 room2，统统跳回 room0
        }


        var room = rooms[currentIndex];
        currentRoom = Instantiate(room.roomPrefab, roomParent);

        // ✅ 初始化墙体显示为主视角（墙0/1显示，墙2/3压缩）
        var wallCtrl = currentRoom.GetComponent<WallVisibilityController>();
        if (wallCtrl != null)
            wallCtrl.ShowMainView();
        CameraMapper.Instance.SwitchTo(0); // 强制切换回主视角

        var spawnRootNew = currentRoom.transform.Find("ItemSpawnRoot");
        if (spawnRootNew != null)
            RoomSpawner.Instance.SetSpawnParent(spawnRootNew);
        else
            Debug.LogWarning("ItemSpawnRoot not found in current room!");

        // ✅ 改为使用按钮 prefab 列表生成
        itemBoxController.ShowButtons(room.buttonPrefabs);

        // 等待一帧，确保生成完成再开始淡出
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

    public int GetCurrentRoomCHI()
    {
        return currentRoomCHIScore;
    }

    public int GetTotalCHI()
    {
        return totalCHIScore;
    }


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

        Debug.Log("[CHI] Passed null checks. Start calculating...");

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

        if (currentRoom)
            Destroy(currentRoom);

        var room = rooms[currentIndex];
        currentRoom = Instantiate(room.roomPrefab, roomParent);

        var wallCtrl = currentRoom.GetComponent<WallVisibilityController>();
        if (wallCtrl != null)
            wallCtrl.ShowMainView();

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

        // ✅ 生成房间内的所有物体（问题来源区域）
        foreach (var item in data.placedItems)
        {
            GameObject prefab = FindPrefabByName(item.prefabName);
            if (prefab != null)
            {
                if (AlreadyHasBed(spawnRoot))
                {
                    Debug.LogWarning("[DEBUG] ItemSpawnRoot 中已经有一张床！");
                }
                if (spawnRoot == null)
                {
                    continue;
                }

                var go = Instantiate(prefab, item.position, item.rotation, spawnRoot);
                var tracker = go.GetComponent<ItemAutoDestroy>();
                var collider = go.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.enabled = true;
                    Debug.Log($"[QuickLoad] Enabled collider for {go.name}");
                }
                else
                {
                    Debug.LogWarning($"[QuickLoad ❌] {go.name} has no collider!");
                }

                var slot = FindSlotForPrefab(item.prefabName);   // ✅ 补上这句
                var roomCollider = currentRoom.GetComponentInChildren<Collider>();

                Debug.Log($"[QuickLoad] Instantiated {prefab.name} at {item.position:F3}");

                if (tracker != null)
                {
                    tracker.Init(slot, roomCollider);
                    Debug.Log($"[QuickLoad] Init tracker for {prefab.name}");

                    PlacementType type = prefab.GetComponent<ItemType>()?.type ?? PlacementType.Floor;

                    if (type == PlacementType.Floor)
                    {
                        var grid = Object.FindFirstObjectByType<FloorGrid>();
                        if (grid != null)
                        {
                            grid.roomCollider = roomCollider;
                            tracker.floorGrid = grid;
                            Debug.Log($"[QuickLoad] Assigned FloorGrid to {prefab.name}");
                        }
                        else
                        {
                            Debug.LogWarning("[QuickLoad ❌] FloorGrid not found");
                        }
                    }
                    else if (type == PlacementType.Wall)
                    {
                        var grid = roomCollider.GetComponentInChildren<WallGrid>();
                        if (grid != null)
                        {
                            grid.roomCollider = roomCollider;
                            tracker.wallGrid = grid;
                            Debug.Log($"[QuickLoad] Assigned WallGrid to {prefab.name}");
                        }
                        else
                        {
                            Debug.LogWarning("[QuickLoad ❌] WallGrid not found");
                        }
                    }

                    // tracker.CheckPositionImmediately();
                    tracker.StopDragging();
                    tracker.EnableDraggingOnClick();
                    // ✅ 再次保险设置一次，确保 collider 真的启用了（双保险）
                    collider = go.GetComponent<Collider>();
                    if (collider != null && !collider.enabled)
                    {
                        collider.enabled = true;
                        Debug.Log($"[QuickLoad] Forced enable of collider on {go.name}");
                    }
                    Debug.Log($"[QuickLoad ✅] Tracker ready for click-drag: {prefab.name}");
                }
                else
                {
                    Debug.LogWarning($"[QuickLoad ❌] Tracker is null on {prefab.name}");
                }

            }
        }


        // ✅ 恢复 ItemBox 中按钮状态
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

        // RefreshCHIScore();
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

        foreach (var t in spawnRoot.GetComponentsInChildren<Transform>(includeInactive: true))
        {
            if (t.gameObject.name.Contains("Bed"))  // or == "Bed"
            {
                Debug.Log("[DEBUG] 已经发现存在床对象: " + t.name);
                return true;
            }
        }

        return false;
    }


}
