using UnityEngine;
using UnityEngine.UI;

public class ItemButtonController : MonoBehaviour
{
    [Header("生成物体预制体")]
    public GameObject modelPrefab;

    [Header("图标对象（用于显示/隐藏）")]
    public GameObject iconObject;

    private bool hasSpawned = false;

    public void OnClick()
    {
        if (hasSpawned || modelPrefab == null) return;

        RoomSpawner.Instance.Spawn(modelPrefab);
        SetSpawned(true);
    }

    public void SetSpawned(bool state)
    {
        hasSpawned = state;
        if (iconObject != null)
            iconObject.SetActive(!state);
    }

    public void ResetSlot()
    {
        hasSpawned = false;
        if (iconObject != null)
            iconObject.SetActive(true);
    }

    public bool HasSpawned() => hasSpawned;
}
