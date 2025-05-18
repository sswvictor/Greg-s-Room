using System.Collections.Generic;
using UnityEngine;

public class FengShuiVisualizer : MonoBehaviour
{
    // References
    public FengShuiRulesData rulesData;
    
    // Highlight prefabs (assign in inspector)
    public GameObject greenZonePrefab;
    public GameObject whiteZonePrefab;
    public GameObject redZonePrefab;
    
    // Pool of zone highlights
    private List<GameObject> highlightPool = new List<GameObject>();
    
    // Internal state
    private string lastObjectType = "";
    private string lastRoomType = "";
    private Vector3 lastDirection = Vector3.zero;
    
    private void Start()
    {
        // Set up default highlight prefabs if not assigned
        if (greenZonePrefab == null)
        {
            greenZonePrefab = CreateDefaultHighlight(Color.green);
        }
        
        if (whiteZonePrefab == null)
        {
            whiteZonePrefab = CreateDefaultHighlight(Color.white);
        }
        
        if (redZonePrefab == null)
        {
            redZonePrefab = CreateDefaultHighlight(Color.red);
        }
    }
    
    // Creates a default quad highlight with the given color
    private GameObject CreateDefaultHighlight(Color color)
    {
        GameObject highlight = new GameObject($"Default{color.ToString()}Highlight");
        highlight.AddComponent<MeshFilter>().mesh = CreateQuadMesh();
        
        Material material = new Material(Shader.Find("Standard"));
        material.color = new Color(color.r, color.g, color.b, 0.5f); // Semi-transparent
        
        highlight.AddComponent<MeshRenderer>().material = material;
        highlight.SetActive(false);
        
        return highlight;
    }
    
    // Create a simple quad mesh for highlights
    private Mesh CreateQuadMesh()
    {
        Mesh mesh = new Mesh();
        
        // Define the vertices (flat quad)
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-0.5f, 0, -0.5f),
            new Vector3(0.5f, 0, -0.5f),
            new Vector3(-0.5f, 0, 0.5f),
            new Vector3(0.5f, 0, 0.5f)
        };
        
        // Define the triangles
        int[] triangles = new int[6]
        {
            0, 2, 1,
            2, 3, 1
        };
        
        // Define the UVs
        Vector2[] uvs = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        
        // Assign to mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    // Show zones for the current object being dragged
    public void ShowZonesForObject(GameObject obj, string roomType)
    {
        if (rulesData == null) return;
        
        string objectType = obj.name.Replace("(Clone)", "");
        Vector3 objectForward = obj.transform.forward;
        
        // Check if we need to update visualization (to avoid constant recreation)
        if (objectType == lastObjectType && roomType == lastRoomType &&
            Vector3.Dot(objectForward.normalized, lastDirection.normalized) > 0.99f)
        {
            return; // No significant change, skip update
        }
        
        // Update state
        lastObjectType = objectType;
        lastRoomType = roomType;
        lastDirection = objectForward;
        
        // Clear previous highlights
        ClearAllHighlights();
        
        // Get rule set
        FengShuiRuleSet ruleSet = rulesData.GetRuleSet(objectType, roomType);
        if (ruleSet == null)
        {
            Debug.LogWarning($"[FENG SHUI] No rule set found for {objectType} in {roomType}");
            return;
        }
        
        // Get best directional rule based on object's orientation
        DirectionalRule bestRule = GetBestDirectionalRule(ruleSet, objectForward);
        if (bestRule == null)
        {
            Debug.LogWarning($"[FENG SHUI] No directional rules found for {objectType}");
            return;
        }
        
        // Get room bounds
        GameObject roomObj = GameObject.FindGameObjectWithTag("Room");
        if (roomObj == null)
        {
            Debug.LogWarning("[FENG SHUI] No room object found with 'Room' tag");
            return;
        }
        
        Collider roomCollider = roomObj.GetComponent<Collider>();
        if (roomCollider == null)
        {
            Debug.LogWarning("[FENG SHUI] Room has no collider");
            return;
        }
        
        Bounds roomBounds = roomCollider.bounds;
        
        // Create a 3x3 grid and show zone highlights
        CreateZoneHighlights(bestRule, roomBounds);
    }
    
    // Create grid of highlights
    private void CreateZoneHighlights(DirectionalRule rule, Bounds roomBounds)
    {
        float cellWidth = roomBounds.size.x / 3;
        float cellDepth = roomBounds.size.z / 3;
        float yOffset = 0.05f; // Slightly above floor
        
        // Iterate through 3x3 grid
        for (int x = 0; x < 3; x++)
        {
            for (int z = 0; z < 3; z++)
            {
                // Calculate position
                Vector3 cellCenter = new Vector3(
                    roomBounds.min.x + cellWidth * (x + 0.5f),
                    roomBounds.min.y + yOffset,
                    roomBounds.min.z + cellDepth * (z + 0.5f)
                );
                
                // Determine position type from grid coordinates
                PositionType posType = GetPositionTypeFromGrid(x, z);
                
                // Find matching rule
                PositionRule posRule = rule.positionRules.Find(r => r.positionType == posType);
                if (posRule != null)
                {
                    // Create appropriate highlight
                    CreateHighlight(posRule.zoneQuality, cellCenter, new Vector3(cellWidth, 0.01f, cellDepth));
                }
            }
        }
    }
    
    // Create a single highlight object
    private void CreateHighlight(ZoneQuality quality, Vector3 position, Vector3 size)
    {
        GameObject prefab = null;
        
        switch (quality)
        {
            case ZoneQuality.Excellent:
                prefab = greenZonePrefab;
                break;
            case ZoneQuality.Acceptable:
                prefab = whiteZonePrefab;
                break;
            case ZoneQuality.Poor:
                prefab = redZonePrefab;
                break;
        }
        
        if (prefab != null)
        {
            GameObject highlight = Instantiate(prefab, position, Quaternion.Euler(90, 0, 0));
            highlight.transform.localScale = size;
            highlight.SetActive(true);
            highlightPool.Add(highlight);
        }
    }
    
    // Clear all highlights
    public void ClearAllHighlights()
    {
        foreach (GameObject highlight in highlightPool)
        {
            Destroy(highlight);
        }
        highlightPool.Clear();
    }
    
    // Find best directional rule
    private DirectionalRule GetBestDirectionalRule(FengShuiRuleSet ruleSet, Vector3 objectForward)
    {
        if (ruleSet.directionalRules.Count == 0) return null;
        
        DirectionalRule bestRule = null;
        float bestDot = -1f;
        
        foreach (DirectionalRule rule in ruleSet.directionalRules)
        {
            float dot = Vector3.Dot(objectForward.normalized, rule.mainDirection.normalized);
            if (dot > bestDot)
            {
                bestDot = dot;
                bestRule = rule;
            }
        }
        
        return bestRule;
    }
    
    // Map grid coordinates to position type
    private PositionType GetPositionTypeFromGrid(int x, int z)
    {
        if (x == 0)
        {
            if (z == 0) return PositionType.SouthWest;
            else if (z == 1) return PositionType.West;
            else return PositionType.NorthWest;
        }
        else if (x == 1)
        {
            if (z == 0) return PositionType.South;
            else if (z == 1) return PositionType.Center;
            else return PositionType.North;
        }
        else
        {
            if (z == 0) return PositionType.SouthEast;
            else if (z == 1) return PositionType.East;
            else return PositionType.NorthEast;
        }
    }
    
    private void OnDestroy()
    {
        ClearAllHighlights();
    }
} 