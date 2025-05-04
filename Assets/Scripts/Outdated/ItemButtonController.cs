using UnityEngine;
using UnityEngine.UI;

public class ItemButtonController : MonoBehaviour
{
    [Header("生成物体预制体")]
    public GameObject modelPrefab;

    [Header("图标对象（用于显示/隐藏）")]
    public GameObject iconObject;

    private bool hasSpawned = false;

    // 点击按钮直接生成物体
    public void OnClick()
    {
        if (hasSpawned || modelPrefab == null) return;

        RoomSpawner.Instance.Spawn(modelPrefab);
        SetSpawned(true);
    }

    // 被拖拽脚本调用：设置生成状态
    public void SetSpawned(bool state)
    {
        hasSpawned = state;
        if (iconObject != null)
            iconObject.SetActive(!state);
    }

    // 拖拽失败或手动重置调用
    public void ResetSlot()
    {
        hasSpawned = false;
        if (iconObject != null)
            iconObject.SetActive(true);
    }

    public bool HasSpawned() => hasSpawned;
}
