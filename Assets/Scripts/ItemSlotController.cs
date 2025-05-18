using UnityEngine;
using UnityEngine.UI;

public class ItemSlotController : MonoBehaviour
{
    [Header("要生成的物体 prefab")]
    public GameObject modelPrefab;

    [Header("图标对象（子物体）")]
    public GameObject iconObject;

    private GameObject spawnedInstance;



    public void RegisterInstance(GameObject obj)
    {
        spawnedInstance = obj;
    }

    public void ClearInstance()
    {
        spawnedInstance = null;
    }

    public void SetSpawned(bool state)
    {
        if (!state)
        {
            ClearInstance();
            ShowIcon();
        }
        else
        {
            HideIcon();
        }
    }

    public void HideIcon()
    {
        if (iconObject != null)
            iconObject.SetActive(false);
    }

    public void ShowIcon()
    {
        if (iconObject != null)
            iconObject.SetActive(true);
    }

    public bool HasSpawned()
    {
        return spawnedInstance != null;
    }

    public GameObject GetSpawnedInstance()
    {
        return spawnedInstance;
    }

    // ✅ 提取辅助函数（用于单击与拖拽共用）
    private static Collider FindTargetCollider(PlacementType type)
    {
        if (type == PlacementType.Floor)
        {
            GameObject floorGO = GameObject.Find("floor");
            return floorGO?.GetComponent<Collider>();
        }

        if (type == PlacementType.Wall)
        {
            string[] wallNames = { "wall0", "wall1", "wall2", "wall3" };
            Vector3 mouse = CameraMapper.MappedMousePositionXY;
            float minDelta = float.MaxValue;
            Collider closest = null;

            foreach (string name in wallNames)
            {
                GameObject wall = GameObject.Find(name);
                if (wall == null) continue;
                if (wall.transform.localScale.y < 0.2f) continue;

                Collider col = wall.GetComponent<Collider>();
                if (col == null || !col.enabled) continue;

                Vector3 forward = wall.transform.forward;
                float delta = Mathf.Abs(
                    Mathf.Abs(forward.z) > Mathf.Abs(forward.x)
                    ? wall.transform.position.z - mouse.z
                    : wall.transform.position.x - mouse.x
                );

                if (delta < minDelta)
                {
                    minDelta = delta;
                    closest = col;
                }
            }

            return closest;
        }

        return null;
    }
}
