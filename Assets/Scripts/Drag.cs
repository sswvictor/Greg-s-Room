using UnityEngine;

public class SmartDraggable3D : MonoBehaviour
{
    private Vector3 offset;
    private Plane dragPlane;
    private Vector3 initialPosition;

    public Collider itemBoxCollider;     // 拖回物品栏时判定区域
    public Collider validDropArea;       // 地板投放区域（可选）

    private Camera mainCamera;
    private bool isDragging = false;

    void Start()
    {
        mainCamera = Camera.main;
        initialPosition = transform.position;
    }

    void OnMouseDown()
    {
        dragPlane = new Plane(Vector3.up, transform.position);
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float enter))
        {
            offset = transform.position - ray.GetPoint(enter);
        }
        isDragging = true;
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float enter))
        {
            transform.position = ray.GetPoint(enter) + offset;
        }
    }

    void OnMouseUp()
    {
        isDragging = false;

        // 判断是否在 itemBox 区域内
        if (itemBoxCollider.bounds.Contains(transform.position))
        {
            // 回归原位
            transform.position = initialPosition;
        }
        else
        {
            // 保持当前位置（有效拖放）
        }
    }
}
