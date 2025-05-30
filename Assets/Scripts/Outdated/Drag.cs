using UnityEngine;

public class SmartDraggable3D : MonoBehaviour
{
    private Vector3 offset;
    private Plane dragPlane;
    private Vector3 initialPosition;

    public Collider itemBoxCollider;      
    public Collider validDropArea;     

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

        if (Input.GetMouseButtonDown(1))
        {
            transform.Rotate(0f, 90f, 0f, Space.Self);
        }
    }


    void OnMouseUp()
    {
        isDragging = false;

        if (itemBoxCollider.bounds.Contains(transform.position))
        {
            transform.position = initialPosition;
        }
    }

}
