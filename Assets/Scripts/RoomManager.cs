using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using UnityEngine.SceneManagement;


[System.Serializable]
public class RoomData
{
    public GameObject roomPrefab;
    public List<GameObject> buttonPrefabs; 
}

[System.Serializable]
public class CutsceneEntry
{
    public string itemName;          
    
    [Header("CHI Score Conditional Cutscenes")]
    public VideoClip highCHIVideo;   
    public VideoClip lowCHIVideo;  
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

    public string endSceneName = "SummaryScene";   
    [Header("Video Player - MVP")]
    public VideoPlayer videoPlayer;     
    public RawImage videoDisplay;    
    public GameObject videoCanvas;     

    public GameObject gregStatusPanelPrefab;  


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
        }

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
        StartCoroutine(SwitchRoomCoroutine());
        BGMManager.Instance?.PlayRoomBGM(0);

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

        UpdateRoomHistoryIfNeeded();

        if (currentRoom)
            Destroy(currentRoom);

        if (currentIndex != 2)
        { 
            Transform spawnRoot = GameObject.Find("ItemSpawnRoot")?.transform;
            string[] keyObjects = null;

            if (currentIndex == 0)
                keyObjects = new[] { "DeskComputer_Prefab", "Basketball_Prefab", "Poster_Prefab" };
            else if (currentIndex == 1)
                keyObjects = new[] { "Weed_Prefab", "Weights_Prefab", "Couch_Prefab" };
            else if (currentIndex == 2)
                keyObjects = new[] { "DeskComputer_Prefab", "Kallax_Prefab", "TV_Prefab" };
            else
                keyObjects = new string[0];               
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
                        // RefreshKeyObjectChoice(RoomManager.Instance.CurrentRoomIndex, selected);
                        yield break;
                    }
                }
            }

        }




        currentIndex++;

        if (currentIndex >= rooms.Count)
        {
            GameSummary.totalCHI = totalCHIScore;
            GameSummary.roomIndices.Clear();
            GameSummary.roomKeys.Clear();
            GameSummary.roomIcons.Clear();
            GameSummary.roomTexts.Clear();

            string[] keyObjectsList = { "DeskComputer_Prefab", "Basketball_Prefab", "Frame_Prefab", "Weed_Prefab", "Weights_Prefab", "Couch_Prefab", "TV_Prefab", "Kallax_Prefab" };

            foreach (var kv in roomHistories)
            {
                int index = kv.Key;
                var history = kv.Value;

                string chosen = history.placedItemNames.Find(p => keyObjectsList.Contains(p));
                if (chosen == null) continue;

                GameSummary.roomIndices.Add(index);
                GameSummary.roomKeys.Add(chosen);

                GameObject buttonPrefab = FindButtonPrefabByKeyObject(chosen); 

                Sprite icon = null;
                if (buttonPrefab != null)
                {
                    var image = buttonPrefab.GetComponentInChildren<UnityEngine.UI.Image>();
                    if (image != null)
                        icon = image.sprite;
                }
                GameSummary.roomIcons.Add(icon);

                string summary = chosen switch
                {
                    "Basketball_Prefab" => "You followed your passion for sports.",
                    "DeskComputer_Prefab" => "You are born to be a coder.",
                    "Frame_Prefab" => "Memories guided your decisions.",
                    _ => "You made a mysterious choice."
                };
                GameSummary.roomTexts.Add(summary);
            }

            UnityEngine.SceneManagement.SceneManager.LoadScene("SummaryScene");
            BGMManager.Instance?.PlayRoomBGM(3);
            yield break;
        }

        string chosenKeyObject = PlayerPrefs.GetString("next_key_object", "");
        if (chosenKeyObject != null)
            RefreshKeyObjectChoice(currentIndex-1, chosenKeyObject);

        if (!string.IsNullOrEmpty(chosenKeyObject) && hasStarted)
        {
            VideoClip selectedVideo = null; 

            float chiPercentage = (currentRoomCHIScore / maxScore) * 100f;
            bool isHighCHI = chiPercentage >= 50f;

            foreach (var entry in cutsceneMapping)
            {
                if (entry.itemName == chosenKeyObject)
                {

                    if (isHighCHI && entry.highCHIVideo != null)
                    {
                        selectedVideo = entry.highCHIVideo;
                        Debug.Log($"[RoomManager] Playing HIGH CHI video cutscene: {chosenKeyObject}");
                    }
                    else if (!isHighCHI && entry.lowCHIVideo != null)
                    {
                        selectedVideo = entry.lowCHIVideo;
                        Debug.Log($"[RoomManager] Playing LOW CHI video cutscene: {chosenKeyObject}");
                    }
                    else
                    {
                        Debug.LogWarning($"[RoomManager] No video assigned for {chosenKeyObject} with {(isHighCHI ? "HIGH" : "LOW")} CHI score");
                    }
                    break;
                }
            }

            PlayerPrefs.DeleteKey("next_key_object");
            if (selectedVideo != null)
            {
                yield return PlayCutscene(selectedVideo);
            }

            yield return ShowGregStatusPanel(chosenKeyObject);
        }





        BGMManager.Instance?.PlayRoomBGM(currentIndex);

        // currentIndex = (currentIndex + 1) % rooms.Count;

        var room = rooms[currentIndex];
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
        yield return new WaitForSeconds(0.1f);

        currentRoomCHIScore = 0;
        ResetCHIScore();
    }

    private Sprite GetGregSpriteFromPlayScene(string chosenKeyObject)
    {
        if (currentIndex == 0 || currentIndex >= 3 || string.IsNullOrEmpty(chosenKeyObject))
            return null;

        string middle;

        if (currentIndex == 1)
        {
            middle = chosenKeyObject.Replace("_Prefab", "");

        }
        else
        {
            if (roomHistories.TryGetValue(0, out var prevHistory) && prevHistory.placedItemNames.Count >= 1)
            {
                string prevKey = prevHistory.placedItemNames[0];
                middle = $"{chosenKeyObject.Replace("_Prefab", "")}_{prevKey.Replace("_Prefab", "")}";
            }
            else
            {
                middle = chosenKeyObject.Replace("_Prefab", "");
            }
        }

        string level = (currentRoomCHIScore >= maxScore * 0.5f) ? "High" : "Low";
        string path = $"Gregs_ResumeScreen/Greg_{middle}_{level}";
        GameSummary.finalGregSpritePath = path;

        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite == null)
            Debug.LogWarning($"[GregStatus] Fail to find：{path}");

        return sprite;
    }




    private IEnumerator ShowGregStatusPanel(string chosenKeyObject)
    {
        if (currentIndex != 1 && currentIndex != 2)
            yield break;

        if (gregStatusPanelPrefab == null)
        {
            yield break;
        }

        GameObject panel = Instantiate(gregStatusPanelPrefab, GameObject.Find("CanvasRoot")?.transform, false);
        panel.SetActive(true);

        var img = panel.transform.Find("GregImage")?.GetComponent<UnityEngine.UI.Image>();
        if (img != null)
            img.sprite = GetGregSpriteFromPlayScene(chosenKeyObject);

        var chiText = panel.transform.Find("CHIScoreText")?.GetComponent<TextMeshProUGUI>();
        if (chiText != null)
            chiText.text = $"CHI Score: {GetCurrentRoomCHI()}";

        bool proceed = false;
        var btn = panel.transform.Find("ContinueButton")?.GetComponent<UnityEngine.UI.Button>();
        if (btn != null)
            btn.onClick.AddListener(() => proceed = true);

        yield return new WaitUntil(() => proceed);
        Destroy(panel);
    }




    private GameObject FindButtonPrefabByKeyObject(string prefabName)
    {
        foreach (var room in rooms)
        {
            foreach (var buttonPrefab in room.buttonPrefabs)
            {
                var slot = buttonPrefab.GetComponent<ItemSlotController>();
                if (slot != null && slot.modelPrefab != null && slot.modelPrefab.name == prefabName)
                {
                    return buttonPrefab;
                }
            }
        }
        return null;
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
        var room = rooms[currentIndex];

        currentRoom = Instantiate(room.roomPrefab, roomParent);
        doorMarker = currentRoom.transform.Find("DoorMarker");
        windowMarker = currentRoom.transform.Find("WindowMarker");


        var wallCtrl = currentRoom.GetComponent<WallVisibilityController>();
        if (wallCtrl != null)
            wallCtrl.ShowMainView();

        CameraMapper.Instance.SwitchTo(0);

        var roomCollider = currentRoom.GetComponentInChildren<Collider>();
        if (roomCollider == null)
        {
            Debug.LogError("[RoomManager] roomCollider is NULL after Instantiate!");
        }
        else
        {
            Debug.Log($"[RoomManager] Found roomCollider = {roomCollider.name}");
        }

        BGMManager.Instance?.PlayRoomBGM(currentIndex);


        var spawnRootNew = currentRoom.transform.Find("ItemSpawnRoot");
        if (spawnRootNew != null)
            RoomSpawner.Instance.SetSpawnParent(spawnRootNew);

        var grid = Object.FindFirstObjectByType<FloorGrid>();
        if (grid != null)
        {
            grid.roomCollider = roomCollider;
        }
        else
        {
            Debug.LogWarning("[RoomManager] FloorGrid not found!");
        }


        itemBoxController.ShowButtons(room.buttonPrefabs);

        yield return new WaitForSeconds(0.1f); 
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


    public static void RefreshKeyObjectChoice(int roomIndex, string selectedKeyObject)
    {
        if (!RoomManager.Instance.roomHistories.TryGetValue(roomIndex, out var history))
            return;

        string[] keyObjects = { "DeskComputer_Prefab", "Basketball_Prefab", "Frame_Prefab", "Weed_Prefab", "Weights_Prefab", "Couch_Prefab", "TV_Prefab", "Kallax_Prefab" };
        List<string> newList = new();

        foreach (var item in history.placedItemNames)
        {
            // if (item == selectedKeyObject || !keyObjects.Contains(item))
            if (item == selectedKeyObject)
            {
                newList.Add(item);
            }
        }

        history.placedItemNames = newList;
        Debug.Log($"[LIFE CHOICE] Room {roomIndex} now only keeps: {string.Join(", ", newList)}");
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

        if (chiScoreText == null)
        {
            return;
        }

        if (CHIScoreManager.Instance == null)
        {
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

        if (currentRoom)
            Destroy(currentRoom);

        var room = rooms[currentIndex];
        currentRoom = Instantiate(room.roomPrefab, roomParent);

        var wallCtrl = currentRoom.GetComponent<WallVisibilityController>();
        if (wallCtrl != null)
            wallCtrl.ShowMainView();

        CameraMapper.Instance.SwitchTo(0);

        var spawnRoot = currentRoom.transform.Find("ItemSpawnRoot");
        if (spawnRoot != null)
        {
            RoomSpawner.Instance.SetSpawnParent(spawnRoot);
        }

        itemBoxController.ShowButtons(room.buttonPrefabs);
        yield return new WaitForSeconds(0.1f);


        BGMManager.Instance?.PlayRoomBGM(currentIndex);

        foreach (var item in data.placedItems)
        {
            GameObject prefab = FindPrefabByName(item.prefabName);
            if (prefab != null)
            {
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
                }
                else
                {
                    Debug.LogWarning($"[QuickLoad] {go.name} has no collider!");
                }

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
                        else
                        {
                            Debug.LogWarning("[QuickLoad] FloorGrid not found");
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
                        else
                        {
                            Debug.LogWarning("[QuickLoad] WallGrid not found");
                        }
                    }

                    // tracker.CheckPositionImmediately();
                    tracker.StopDragging();
                    tracker.EnableDraggingOnClick();
                    collider = go.GetComponent<Collider>();
                    if (collider != null && !collider.enabled)
                    {
                        collider.enabled = true;
                    }
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
            if (t.gameObject.name.Contains("Bed")) 
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerator PlayCutscene(VideoClip videoClip) 
    {
        if (videoClip == null || videoPlayer == null || videoCanvas == null)
        {
            yield return new WaitForSeconds(3f);
            yield break;
        }

        GameObject videoCanvasInstance = Instantiate(videoCanvas);
        
        RawImage displayImage = videoCanvasInstance.GetComponentInChildren<RawImage>();
        if (displayImage != null && videoPlayer.targetTexture != null)
        {
            displayImage.texture = videoPlayer.targetTexture;
        }
        
        videoPlayer.clip = videoClip;
        videoPlayer.Play();
        
        
        yield return new WaitForSeconds((float)videoPlayer.clip.length);
        
        videoPlayer.Stop();
        if (videoCanvasInstance != null)
        {
            Destroy(videoCanvasInstance);
        }
    
    }

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
