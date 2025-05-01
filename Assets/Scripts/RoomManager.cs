using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class RoomData {
    public GameObject roomPrefab;
    public List<Sprite> itemIcons;
    public List<GameObject> itemPrefabs;
}

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    public List<RoomData> rooms;
    public Transform roomParent;
    public ItemBoxController itemBoxController;
    public GameObject transitionPanel;
    public float transitionDuration = 0.5f;

    public TextMeshProUGUI chiScoreText;
    public ChiFillBar chiFillBar;  // ✅ 用于更新进度条

    private int currentIndex = -1;
    private GameObject currentRoom;

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
    }

    public void LoadNextRoom()
    {
        StartCoroutine(SwitchRoomCoroutine());
    }

    private IEnumerator SwitchRoomCoroutine()
    {
        var cg = transitionPanel.GetComponent<CanvasGroup>();
        transitionPanel.SetActive(true);
        while (cg.alpha < 1f)
        {
            cg.alpha += Time.deltaTime * 2;
            yield return null;
        }

        totalCHIScore += currentRoomCHIScore;
        Debug.Log($"[GLOBAL CHI] Added {currentRoomCHIScore} points. Total now = {totalCHIScore}");

        if (currentRoom)
            Destroy(currentRoom);

        currentIndex = (currentIndex + 1) % rooms.Count;
        var room = rooms[currentIndex];

        currentRoom = Instantiate(room.roomPrefab, roomParent);

        var spawnRoot = currentRoom.transform.Find("ItemSpawnRoot");
        if (spawnRoot != null)
            RoomSpawner.Instance.SetSpawnParent(spawnRoot);
        else
            Debug.LogWarning("ItemSpawnRoot not found in current room!");

        itemBoxController.ShowItems(room.itemIcons, room.itemPrefabs);

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

        chiFillBar?.UpdateBar(0, 9f); // ✅ 同步进度条清零
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

        chiFillBar?.UpdateBar(currentRoomCHIScore, 9f); // ✅ 动态更新进度条
    }
}
