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
    public float maxScore = 20f;

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

        currentIndex = (currentIndex + 1) % rooms.Count;
        var room = rooms[currentIndex];

        currentRoom = Instantiate(room.roomPrefab, roomParent);

        // ✅ 初始化墙体显示为主视角（墙0/1显示，墙2/3压缩）
        var wallCtrl = currentRoom.GetComponent<WallVisibilityController>();
        if (wallCtrl != null)
            wallCtrl.ShowMainView();
        CameraMapper.Instance.SwitchTo(0); // 强制切换回主视角

        var spawnRoot = currentRoom.transform.Find("ItemSpawnRoot");
        if (spawnRoot != null)
            RoomSpawner.Instance.SetSpawnParent(spawnRoot);
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
}
