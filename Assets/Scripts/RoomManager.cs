using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomData {
    public GameObject roomPrefab;
    public List<Sprite> itemIcons;
    public List<GameObject> itemPrefabs;
}

public class RoomManager : MonoBehaviour
{
    public List<RoomData> rooms;
    public Transform roomParent;
    public ItemBoxController itemBoxController;
    public GameObject transitionPanel;
    public float transitionDuration = 0.5f;

    private int currentIndex = -1;
    private GameObject currentRoom;

    public void LoadNextRoom()
    {
        StartCoroutine(SwitchRoomCoroutine());
    }

    private IEnumerator SwitchRoomCoroutine()
    {
        // 淡入过渡动画
        var cg = transitionPanel.GetComponent<CanvasGroup>();
        transitionPanel.SetActive(true);
        while (cg.alpha < 1f)
        {
            cg.alpha += Time.deltaTime * 2;
            yield return null;
        }

        // 移除旧房间
        if (currentRoom)
            Destroy(currentRoom);

        // 切换房间数据
        currentIndex = (currentIndex + 1) % rooms.Count;
        var room = rooms[currentIndex];

        // 实例化新房间
        currentRoom = Instantiate(room.roomPrefab, roomParent);

        // 找出新房间中的 ItemSpawnRoot 并设置给 RoomSpawner
        var spawnRoot = currentRoom.transform.Find("ItemSpawnRoot");
        if (spawnRoot != null)
            RoomSpawner.Instance.SetSpawnParent(spawnRoot);
        else
            Debug.LogWarning("ItemSpawnRoot not found in current room!");

        // 更新 itemBox 图标按钮
        itemBoxController.ShowItems(room.itemIcons, room.itemPrefabs);

        // 等一小段再淡出
        yield return new WaitForSeconds(0.2f);
        while (cg.alpha > 0f)
        {
            cg.alpha -= Time.deltaTime * 2;
            yield return null;
        }

        transitionPanel.SetActive(false);
    }
}
