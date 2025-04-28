using UnityEngine;

public class ItemButtonController : MonoBehaviour
{
    public GameObject modelPrefab;

    public void OnClick()
    {
        if (modelPrefab != null)
            RoomSpawner.Instance.Spawn(modelPrefab);
    }
}
