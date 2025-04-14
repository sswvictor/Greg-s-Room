using UnityEngine;

public class ItemAutoDestroy : MonoBehaviour
{
    private ItemSlotController originSlot;
    private Collider roomCollider;
    private bool isDragging = true;

    public void Init(ItemSlotController slot, Collider room)
    {
        originSlot = slot;
        roomCollider = room;
    }

    void Update()
    {
        if (!isDragging) return;

        if (Input.GetMouseButton(0))
        {
            transform.position = CameraMapper.MappedMousePosition;  // ✅ 使用映射坐标
        }
    }

    void OnMouseDown()
    {
        isDragging = true;
    }

    void OnMouseUp()
    {
        isDragging = false;
        CheckPositionImmediately();
    }

    public void CheckPositionImmediately()
    {
        if (!roomCollider.bounds.Contains(transform.position))
        {
            originSlot.ClearInstance();
            originSlot.ShowIcon();
            Destroy(gameObject);
        }
    }
}
