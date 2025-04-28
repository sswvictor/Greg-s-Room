using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragToSpawn : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject prefabToSpawn;
    public ItemSlotController slotController;

    private GameObject draggingInstance;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (slotController.HasSpawned()) return;

        // ✅ 动态查找当前房间的 ItemSpawnRoot
        Transform spawnParent = GameObject.Find("ItemSpawnRoot")?.transform;
        if (spawnParent == null)
        {
            Debug.LogWarning("[UIDragToSpawn] ItemSpawnRoot not found. Abort spawn.");
            return;
        }

        // ✅ 动态查找当前房间的 FloorGrid 上的 Collider（用于碰撞检测、投影等）
        Collider roomCollider = GameObject.Find("Floor")?.GetComponent<Collider>();
        if (roomCollider == null)
        {
            Debug.LogWarning("[UIDragToSpawn] FloorGrid collider not found.");
            return;
        }

        // ✅ 实例化物体并设置为 Room 中的子物体
        draggingInstance = Instantiate(prefabToSpawn, Vector3.zero, Quaternion.identity, spawnParent);
        draggingInstance.name = prefabToSpawn.name;

        // ✅ 初始化销毁器（传入 slot 控制器和房间的地面 collider）
        var tracker = draggingInstance.GetComponent<ItemAutoDestroy>();
        if (tracker != null)
        {
            tracker.Init(slotController, roomCollider);
        }

        slotController.RegisterInstance(draggingInstance);
        slotController.HideIcon();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 拖拽位置逻辑由 draggingInstance 自己负责
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggingInstance != null)
        {
            var tracker = draggingInstance.GetComponent<ItemAutoDestroy>();
            if (tracker != null)
            {
                tracker.CheckPositionImmediately(); // 检查落点是否合法
                tracker.StopDragging();             // 停止更新位置
            }
        }
    }
}
