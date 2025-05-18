using System.Collections.Generic;
using UnityEngine;

public class PositionMapper : MonoBehaviour
{
    // Map a world position to a feng shui position type
    public static PositionType GetPositionType(Vector3 worldPos, Bounds roomBounds)
    {
        // Normalize position within room bounds (0-1 range)
        float normalizedX = Mathf.InverseLerp(roomBounds.min.x, roomBounds.max.x, worldPos.x);
        float normalizedZ = Mathf.InverseLerp(roomBounds.min.z, roomBounds.max.z, worldPos.z);
        
        // Use 3x3 grid to determine position
        if (normalizedX < 0.33f)
        {
            if (normalizedZ < 0.33f) return PositionType.SouthWest;
            else if (normalizedZ < 0.66f) return PositionType.West;
            else return PositionType.NorthWest;
        }
        else if (normalizedX < 0.66f)
        {
            if (normalizedZ < 0.33f) return PositionType.South;
            else if (normalizedZ < 0.66f) return PositionType.Center;
            else return PositionType.North;
        }
        else
        {
            if (normalizedZ < 0.33f) return PositionType.SouthEast;
            else if (normalizedZ < 0.66f) return PositionType.East;
            else return PositionType.NorthEast;
        }
    }
    
    // Check for special positions like near walls, doors, etc.
    public static List<PositionType> GetSpecialPositionTypes(Vector3 worldPos, GameObject room)
    {
        List<PositionType> specialPositions = new List<PositionType>();
        
        // Check proximity to walls
        if (IsNearWall(worldPos, room))
            specialPositions.Add(PositionType.NearWall);
            
        // Check proximity to doors
        if (IsNearDoor(worldPos, room))
            specialPositions.Add(PositionType.NearDoor);
            
        // Check proximity to windows
        if (IsNearWindow(worldPos, room))
            specialPositions.Add(PositionType.NearWindow);
            
        return specialPositions;
    }
    
    // Helper methods to determine special positions
    private static bool IsNearWall(Vector3 pos, GameObject room) 
    {
        // Simple implementation - check if position is near the room's edges
        Bounds bounds = room.GetComponent<Collider>().bounds;
        
        float edgeThreshold = 0.5f; // Distance to consider "near wall"
        
        // Check if point is near any of the 4 walls
        bool nearXMin = Mathf.Abs(pos.x - bounds.min.x) < edgeThreshold;
        bool nearXMax = Mathf.Abs(pos.x - bounds.max.x) < edgeThreshold;
        bool nearZMin = Mathf.Abs(pos.z - bounds.min.z) < edgeThreshold;
        bool nearZMax = Mathf.Abs(pos.z - bounds.max.z) < edgeThreshold;
        
        return nearXMin || nearXMax || nearZMin || nearZMax;
    }
    
    private static bool IsNearDoor(Vector3 pos, GameObject room)
    {
        // This would require knowledge of door positions
        // For MVP, we'll use a placeholder that can be improved later
        
        // Find door object if it exists
        Transform doorTransform = room.transform.Find("Door");
        if (doorTransform != null)
        {
            float doorThreshold = 1.0f; // Distance to consider "near door"
            return Vector3.Distance(pos, doorTransform.position) < doorThreshold;
        }
        
        return false;
    }
    
    private static bool IsNearWindow(Vector3 pos, GameObject room)
    {
        // This would require knowledge of window positions
        // For MVP, we'll use a placeholder that can be improved later
        
        // Find window objects if they exist
        Transform[] windows = room.transform.FindChildrenWithTag("Window");
        if (windows != null && windows.Length > 0)
        {
            float windowThreshold = 1.0f; // Distance to consider "near window"
            
            foreach (Transform window in windows)
            {
                if (Vector3.Distance(pos, window.position) < windowThreshold)
                    return true;
            }
        }
        
        return false;
    }
    
    // Helper extension for finding children with a specific tag
    private static Transform[] FindChildrenWithTag(this Transform parent, string tag)
    {
        List<Transform> children = new List<Transform>();
        
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
                children.Add(child);
        }
        
        return children.ToArray();
    }
} 