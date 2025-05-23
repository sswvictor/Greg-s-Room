using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class RoomData
{
    public GameObject roomPrefab;
    public List<GameObject> buttonPrefabs; // ✅ 改为每个房间的按钮 prefab 列表


    // public GameObject cutscenePrefab;  // ✅ 新增字段
    // public List<Sprite> itemIcons;       // ❌ 已弃用
    // public List<GameObject> itemPrefabs; // ❌ 已弃用
}

[System.Serializable]
public class CutsceneEntry
{
    public string itemName;             // 例如 "Basketball_Prefab"
    public GameObject cutscenePrefab;   // 对应的动画 prefab
}

public class RoomHistory {
    public bool unlocked;
    public bool finished;
    public List<string> placedItemNames;
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

    public Dictionary<int, RoomHistory> roomHistories = new();

    // public KeyObjectSelectionPanel keyObjectPanel;



    public Vector3 GetDoorPosition()
    {
        return doorMarker != null ? doorMarker.position : Vector3.zero;
    }

    public Vector3 GetWindowPosition()
    {
        return windowMarker != null ? windowMarker.position : Vector3.zero;
    }

    private int currentRoomCHIScore = 0;
    public int totalCHIScore = 0;

    public List<CutsceneEntry> cutsceneMapping;
    private bool hasStarted = false;


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

        // ✅ 初始化 roomHistories 字典
        if (roomHistories == null)
            roomHistories = new Dictionary<int, RoomHistory>();

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
        if (!hasStarted && transitionPanel != null)
        {
            var cg = transitionPanel.GetComponent<CanvasGroup>();
            transitionPanel.SetActive(true);

            if (cg != null)
            {
                cg.alpha = 1f;
                yield return new WaitForSeconds(0.3f);
                cg.alpha = 0f;
            }

            transitionPanel.SetActive(false);
            hasStarted = true;
        }

        totalCHIScore += currentRoomCHIScore;
        Debug.Log($"[GLOBAL CHI] Added {currentRoomCHIScore} points. Total now = {totalCHIScore}");

        UpdateRoomHistoryIfNeeded();

        if (currentRoom)
            Destroy(currentRoom);

        // ✅ 关键物体选择逻辑
        Transform spawnRoot = GameObject.Find("ItemSpawnRoot")?.transform;
        string[] keyObjects = { "Bed_Prefab", "Basketball_Prefab", "Frame_Prefab" };
        List<string> keyItemsThisRoom = new();

        if (spawnRoot != null)
        {
            foreach (Transform child in spawnRoot)
            {
                var item = child.GetComponent<ItemAutoDestroy>();
                if (item != null && item.isValidPlacement)
                {
                    string itemName = child.name.Replace("(Clone)", "");
                    if (keyObjects.Contains(itemName))
                        keyItemsThisRoom.Add(itemName);
                }
            }
        }

        if (keyItemsThisRoom.Count == 1)
        {
            PlayerPrefs.SetString("next_key_object", keyItemsThisRoom[0]);
        }
        else if (keyItemsThisRoom.Count > 1)
        {
            GameObject panelGO = GameObject.Find("KeyObjectSelectionPanel");
            if (panelGO != null)
            {
                var panel = GameObject.Find("KeyObjectSelectionPanel")?.GetComponent<KeyObjectSelectionPanel>();

                if (panel != null)
                {
                    panel.Show(keyItemsThisRoom);
                    yield break;
                }
                else
                {
                    Debug.LogWarning("[RoomManager] ❌ 找到 GameObject 但没有 KeyObjectSelectionPanel 脚本！");
                }
            }
            else
            {
                Debug.LogWarning("[RoomManager] ❌ 找不到 KeyObjectSelectionPanel 物体！");
            }
        }

        // ✅ 播放关键物体关联的 cutscene（若存在）
        string chosenKeyObject = PlayerPrefs.GetString("next_key_object", "");
        if (!string.IsNullOrEmpty(chosenKeyObject) && hasStarted)
        {
            GameObject selectedCutscene = null;
            foreach (var entry in cutsceneMapping)
            {
                if (entry.itemName == chosenKeyObject && entry.cutscenePrefab != null)
                {
                    selectedCutscene = entry.cutscenePrefab;
                    Debug.Log($"[RoomManager] 播放关键物体 Cutscene：{chosenKeyObject}");
                    break;
                }
            }

            PlayerPrefs.DeleteKey("next_key_object"); // ✅ 播放后清除
            if (selectedCutscene != null)
            {
                yield return PlayCutscene(selectedCutscene);
            }
        }


        // ✅ 接着进入下一个房间
        currentIndex = (currentIndex + 1) % rooms.Count;
        var room = rooms[currentIndex];
        currentRoom = Instantiate(room.roomPrefab, roomParent);
        doorMarker = currentRoom.transform.Find("DoorMarker");
        windowMarker = currentRoom.transform.Find("WindowMarker");

        if (doorMarker == null) Debug.LogWarning("[RoomManager] ❌ DoorMarker not found!");
        if (windowMarker == null) Debug.LogWarning("[RoomManager] ❌ WindowMarker not found!");

        var wallCtrl = currentRoom.GetComponent<WallVisibilityController>();
        if (wallCtrl != null) wallCtrl.ShowMainView();

        CameraMapper.Instance.SwitchTo(0);

        var spawnRootNew = currentRoom.transform.Find("ItemSpawnRoot");
        if (spawnRootNew != null)
            RoomSpawner.Instance.SetSpawnParent(spawnRootNew);
        else
            Debug.LogWarning("ItemSpawnRoot not found in current room!");

        itemBoxController.ShowButtons(room.buttonPrefabs);
        yield return new WaitForSeconds(0.1f);

        currentRoomCHIScore = 0;
        ResetCHIScore();
    }


    public void LoadRoomByIndex(int index, bool reset = true)
    {
        StartCoroutine(SwitchToRoomCoroutine(index, reset));
    }

    private IEnumerator SwitchToRoomCoroutine(int newIndex, bool reset)
    {
        if (reset && currentRoom != null)
        {
            Destroy(currentRoom);
            foreach (var item in FindObjectsOfType<ItemAutoDestroy>())
            {
                Destroy(item.gameObject);
            }
        }

        currentIndex = newIndex;
        Debug.Log($"[RoomSwitcher] Index = {currentIndex}");

        // ✅ 实例化新房间
        var room = rooms[currentIndex];

        currentRoom = Instantiate(room.roomPrefab, roomParent);
        doorMarker = currentRoom.transform.Find("DoorMarker");
        windowMarker = currentRoom.transform.Find("WindowMarker");

        if (doorMarker == null)
            Debug.LogWarning("[RoomManager] ❌ DoorMarker not found!");

        if (windowMarker == null)
            Debug.LogWarning("[RoomManager] ❌ WindowMarker not found!");

        var wallCtrl = currentRoom.GetComponent<WallVisibilityController>();
        if (wallCtrl != null)
            wallCtrl.ShowMainView();

        CameraMapper.Instance.SwitchTo(0);

        var roomCollider = currentRoom.GetComponentInChildren<Collider>();
        if (roomCollider == null)
        {
            Debug.LogError("[RoomManager ❌] roomCollider is NULL after Instantiate!");
            Debug.Log($"[DEBUG] Room instantiated = {currentRoom.name}");
            foreach (var col in currentRoom.GetComponentsInChildren<Collider>(true))
            {
                Debug.Log($"[DEBUG] Found collider on: {col.gameObject.name}");
            }
        }
        else
        {
            Debug.Log($"[RoomManager ✅] Found roomCollider = {roomCollider.name}");
        }


        var spawnRootNew = currentRoom.transform.Find("ItemSpawnRoot");
        if (spawnRootNew != null)
            RoomSpawner.Instance.SetSpawnParent(spawnRootNew);
        else
            Debug.LogWarning("ItemSpawnRoot not found in current room!");

        var grid = Object.FindFirstObjectByType<FloorGrid>();
        if (grid != null)
        {
            grid.roomCollider = roomCollider;
            Debug.Log($"[RoomManager ✅] FloorGrid.roomCollider = {roomCollider.name}");
        }
        else
        {
            Debug.LogWarning("[RoomManager ❌] FloorGrid not found!");
        }


        itemBoxController.ShowButtons(room.buttonPrefabs);

        yield return new WaitForSeconds(0.1f); // 轻微延迟更平滑
        CameraMapper.Instance.SwitchTo(0);
        currentRoomCHIScore = 0;
        UpdateRoomHistoryIfNeeded();
        ResetCHIScore();

    }

    public void UpdateRoomHistoryIfNeeded()
    {
        if (roomHistories == null)
            roomHistories = new Dictionary<int, RoomHistory>();

        if (!roomHistories.ContainsKey(currentIndex))
        {
            roomHistories[currentIndex] = new RoomHistory
            {
                unlocked = true,
                finished = CheckCompletionCriteria(),
                placedItemNames = GetPlacedItemNamesFromCurrentRoom()
            };
        }
        else
        {
            var history = roomHistories[currentIndex];
            history.unlocked = true;

            var newlyPlaced = GetPlacedItemNamesFromCurrentRoom();
            foreach (var item in newlyPlaced)
            {
                if (!history.placedItemNames.Contains(item))
                    history.placedItemNames.Add(item);
            }

            history.finished = CheckCompletionCriteria();
        }
    }

    private List<string> GetPlacedItemNamesFromCurrentRoom()
    {
        List<string> names = new();

        Transform root = GameObject.Find("ItemSpawnRoot")?.transform;
        if (root == null) return names;

        foreach (Transform child in root)
        {
            var item = child.GetComponent<ItemAutoDestroy>();
            if (item != null && item.isValidPlacement)
            {
                string name = child.name.Replace("(Clone)", "");
                names.Add(name);
            }
        }

        return names;
    }

    private bool CheckCompletionCriteria()
    {
        var placed = GetPlacedItemNamesFromCurrentRoom();
        // ✅ 示例：放了床 + 桌子 + 灯算完成
        string[] keyItems = { "Bed_Prefab", "Nightstand_Prefab", "Frame_Prefab" };

        foreach (var item in keyItems)
        {
            if (!placed.Contains(item))
                return false;
        }

        return true;
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

    private IEnumerator PlayCutscene(GameObject cutscenePrefab)
    {
        if (cutscenePrefab == null)
            yield break;

        var instance = Instantiate(cutscenePrefab, GameObject.Find("CanvasRoot")?.transform, false); // 挂到 UI Canvas 下
        var director = instance.GetComponent<CutsceneDirector>();

        instance.SetActive(true);
        if (director != null)
        {
            director.TransitionToRoom(1f);
            yield return new WaitForSeconds(director.transitionDuration);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        Destroy(instance);  // 播完即销毁
    }

    //Ricky's new code
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && currentRoom != null)
        {
            Vector3 center = GetRoomCenter();
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(center, 0.2f);
        }
    }

    public Vector3 GetRoomCenter()
    {
        var floor = currentRoom?.GetComponentInChildren<FloorGrid>();
        if (floor != null && floor.roomCollider != null)
        {
            return floor.roomCollider.bounds.center;
        }

        return currentRoom != null ? currentRoom.transform.position : Vector3.zero;
    }



}
