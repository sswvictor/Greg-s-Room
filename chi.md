# Feng Shui Chi Scoring System Implementation Guide

This document outlines the implementation plan for the Chi Scoring System based on feng shui principles. The system will evaluate furniture placement based on traditional feng shui rules while being directionally aware (considering object orientation).

## Table of Contents

1. [System Overview](#system-overview)
2. [Implementation Blocks](#implementation-blocks)
3. [Testing Approach](#testing-approach)
4. [Best Practices](#best-practices)
5. [Integration Points](#integration-points)

## System Overview

The Chi Scoring System evaluates furniture placement according to feng shui principles, considering:

- The type of room (bedroom, living room, bathroom)
- Object position within the room (using a 3x3 grid: N, NE, E, SE, S, SW, W, NW, Center)
- Object orientation ("head" and "tail" positioning)
- Proximity to other objects
- Alignment with walls and entrances

Each piece of furniture will have specific rules determining good/neutral/bad placement zones that change based on its rotation.

## Implementation Blocks

### Block 1: Core Data Structures (MVP)

Create the basic data structures to represent feng shui rules:

```csharp
// Position type within a room
public enum PositionType
{
    North, NorthEast, East, SouthEast, South, 
    SouthWest, West, NorthWest, Center,
    NearWall, NearDoor, NearWindow
}

// Define zone quality types
public enum ZoneQuality 
{
    Excellent, // Green zones - optimal placement
    Acceptable, // White zones - neutral placement
    Poor // Red zones - bad placement
}

// Placement rule for a specific position
[System.Serializable]
public class PositionRule
{
    public PositionType positionType;
    public ZoneQuality zoneQuality;
    public int scoreModifier; // Chi points adjustment
    public string feedbackMessage; // Feedback to display
    
    public PositionRule(PositionType type, ZoneQuality quality, int score, string msg)
    {
        positionType = type;
        zoneQuality = quality;
        scoreModifier = score;
        feedbackMessage = msg;
    }
}

// Rule set for a specific object orientation
[System.Serializable]
public class DirectionalRule
{
    public Vector3 mainDirection; // Forward vector of furniture
    public List<PositionRule> positionRules;
}

// Complete rule set for an object type in a specific room
[System.Serializable]
public class FengShuiRuleSet
{
    public string objectType; // e.g., "Bed_Prefab"
    public string roomType; // e.g., "Bedroom"
    public List<DirectionalRule> directionalRules;
}
```

### Block 2: Feng Shui Rule Manager (MVP)

Create a ScriptableObject to store feng shui rules that can be edited in the inspector:

```csharp
[CreateAssetMenu(fileName = "FengShuiRules", menuName = "Greg's Room/Feng Shui Rules")]
public class FengShuiRulesData : ScriptableObject
{
    public List<FengShuiRuleSet> ruleSets = new List<FengShuiRuleSet>();
    
    // Helper method to get rules
    public FengShuiRuleSet GetRuleSet(string objectType, string roomType)
    {
        return ruleSets.Find(r => r.objectType == objectType && r.roomType == roomType);
    }
}
```

### Block 3: Position Mapping System (MVP)

Implement a system to map world positions to feng shui grid positions:

```csharp
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
        // Implementation depends on your room structure
        // Example: Check distance to any wall collider
        return false; // Placeholder
    }
    
    private static bool IsNearDoor(Vector3 pos, GameObject room)
    {
        // Find door objects and check distance
        return false; // Placeholder
    }
    
    private static bool IsNearWindow(Vector3 pos, GameObject room)
    {
        // Find window objects and check distance
        return false; // Placeholder
    }
}
```

### Block 4: Enhanced CHIScoreManager (MVP)

Update the CHIScoreManager to incorporate feng shui rules:

```csharp
public class CHIScoreManager : MonoBehaviour
{
    public static CHIScoreManager Instance;
    
    // Reference to rules data asset
    public FengShuiRulesData rulesData;
    
    // Current room type
    private string currentRoomType = "";
    
    // Base scores for simple compatibility
    private Dictionary<string, int> baseScores = new Dictionary<string, int>();
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        // Initialize base scores for backward compatibility
        InitializeBaseScores();
    }
    
    // Set the current room type when room changes
    public void SetCurrentRoomType(string roomType)
    {
        currentRoomType = roomType;
        Debug.Log($"[CHI] Room type set to: {roomType}");
    }
    
    // Calculate total CHI score
    public int CalculateTotalCHI()
    {
        int totalScore = 0;
        
        Transform parent = GameObject.Find("ItemSpawnRoot")?.transform;
        if (parent == null)
        {
            Debug.LogWarning("[CHI] ItemSpawnRoot not found");
            return 0;
        }
        
        foreach (Transform child in parent)
        {
            ItemAutoDestroy item = child.GetComponent<ItemAutoDestroy>();
            if (item != null && item.isValidPlacement)
            {
                // Get the base score
                int baseScore = GetBaseScore(child.gameObject);
                
                // Get feng shui modifier based on position and orientation
                int fengShuiModifier = EvaluateFengShuiPlacement(child.gameObject);
                
                // Calculate total item score
                int itemScore = baseScore + fengShuiModifier;
                
                Debug.Log($"[CHI] {child.name}: Base={baseScore}, FengShui={fengShuiModifier}, Total={itemScore}");
                totalScore += itemScore;
            }
        }
        
        Debug.Log($"[CHI] Total room score: {totalScore}");
        return totalScore;
    }
    
    // Evaluate feng shui placement of an object
    private int EvaluateFengShuiPlacement(GameObject obj)
    {
        if (string.IsNullOrEmpty(currentRoomType) || rulesData == null)
            return 0;
            
        string objectType = obj.name.Replace("(Clone)", "");
        
        // Get rule set for this object in current room
        FengShuiRuleSet ruleSet = rulesData.GetRuleSet(objectType, currentRoomType);
        if (ruleSet == null) return 0;
        
        // Get current position type
        Bounds roomBounds = GameObject.FindGameObjectWithTag("Room")?.GetComponent<Collider>()?.bounds ?? new Bounds();
        PositionType posType = PositionMapper.GetPositionType(obj.transform.position, roomBounds);
        
        // Find best matching directional rule based on object's forward vector
        DirectionalRule bestRule = GetBestDirectionalRule(ruleSet, obj.transform.forward);
        if (bestRule == null) return 0;
        
        // Find applicable position rule
        PositionRule posRule = bestRule.positionRules.Find(r => r.positionType == posType);
        if (posRule != null)
        {
            // Show feedback message
            if (FeedbackTextManager.Instance != null)
            {
                Color msgColor = posRule.zoneQuality == ZoneQuality.Excellent ? Color.green : 
                                 posRule.zoneQuality == ZoneQuality.Acceptable ? Color.white : 
                                 Color.red;
                                 
                FeedbackTextManager.Instance.ShowMessage(posRule.feedbackMessage, msgColor);
            }
            
            return posRule.scoreModifier;
        }
        
        return 0; // No specific rule found
    }
    
    // Find the directional rule that best matches the object's orientation
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
    
    // Initialize base scores for simple compatibility
    private void InitializeBaseScores()
    {
        baseScores.Clear();
        baseScores.Add("Plant", 5);
        baseScores.Add("Bed_Prefab", 3);
        baseScores.Add("Basketball_Prefab", 10);
        baseScores.Add("TrashBin", -3);
        baseScores.Add("Toilet", -5);
        baseScores.Add("Couch_Prefab", 4);
        // Add other objects as needed
    }
    
    // Get base score for backward compatibility
    private int GetBaseScore(GameObject go)
    {
        if (baseScores.TryGetValue(go.name.Replace("(Clone)", ""), out int score))
        {
            return score;
        }
        Debug.LogWarning($"[CHI] Unrecognized object {go.name}");
        return 0;
    }
}
```

### Block 5: Visual Feedback System (MVP)

Create a system to visualize placement zones during dragging:

```csharp
public class FengShuiVisualizer : MonoBehaviour
{
    // References
    public FengShuiRulesData rulesData;
    
    // Highlight prefabs
    public GameObject greenZonePrefab;
    public GameObject whiteZonePrefab;
    public GameObject redZonePrefab;
    
    // Pool of zone highlights
    private List<GameObject> highlightPool = new List<GameObject>();
    
    // Show zones for the current object being dragged
    public void ShowZonesForObject(GameObject obj, string roomType)
    {
        ClearAllHighlights();
        
        if (rulesData == null) return;
        
        string objectType = obj.name.Replace("(Clone)", "");
        
        // Get rule set
        FengShuiRuleSet ruleSet = rulesData.GetRuleSet(objectType, roomType);
        if (ruleSet == null) return;
        
        // Get best directional rule based on object's orientation
        DirectionalRule bestRule = GetBestDirectionalRule(ruleSet, obj.transform.forward);
        if (bestRule == null) return;
        
        // Get room bounds
        Bounds roomBounds = GameObject.FindGameObjectWithTag("Room")?.GetComponent<Collider>()?.bounds ?? new Bounds();
        if (roomBounds.size == Vector3.zero) return;
        
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
        // Same implementation as in CHIScoreManager
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
}
```

### Block 6: Integration with ItemAutoDestroy (MVP)

Update the `ItemAutoDestroy` to interact with the feng shui system:

```csharp
// Add to ItemAutoDestroy.cs
private FengShuiVisualizer fengShuiVisualizer;

void Start()
{
    // Existing code...
    fengShuiVisualizer = FindObjectOfType<FengShuiVisualizer>();
}

void Update()
{
    if (!isDragging) return;

    // Existing mouse position and snapping code...
    
    // Show feng shui zones when dragging
    if (fengShuiVisualizer != null)
    {
        string roomType = RoomManager.Instance?.GetCurrentRoomType() ?? "";
        fengShuiVisualizer.ShowZonesForObject(gameObject, roomType);
    }
    
    // Existing rotation handling...
}

void OnMouseUp()
{
    // Existing code for placement...
    
    // Hide feng shui visualizations after placement
    if (fengShuiVisualizer != null)
    {
        fengShuiVisualizer.ClearAllHighlights();
    }
    
    // Rest of existing code...
}
```

### Block 7: Update RoomManager (MVP)

Update RoomManager to support room type identification:

```csharp
// Add to RoomData class
public string roomType = ""; // e.g. "Bedroom", "LivingRoom", etc.

// Add to RoomManager class
public string GetCurrentRoomType()
{
    if (currentIndex >= 0 && currentIndex < rooms.Count)
    {
        return rooms[currentIndex].roomType;
    }
    return "";
}

// Update in SwitchRoomCoroutine
if (CHIScoreManager.Instance != null)
{
    CHIScoreManager.Instance.SetCurrentRoomType(room.roomType);
}
```

### Block 8: Editor Tooling (Optional)

Create an editor tool to help visualize and edit feng shui rules:

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class FengShuiRuleEditor : EditorWindow
{
    private FengShuiRulesData targetRules;
    private Vector2 scrollPosition;

    [MenuItem("Greg's Room/Feng Shui Rule Editor")]
    public static void ShowWindow()
    {
        GetWindow<FengShuiRuleEditor>("Feng Shui Rules");
    }

    private void OnGUI()
    {
        GUILayout.Label("Feng Shui Rules Editor", EditorStyles.boldLabel);
        
        targetRules = (FengShuiRulesData)EditorGUILayout.ObjectField(
            "Rules Data", targetRules, typeof(FengShuiRulesData), false);
            
        if (targetRules == null) return;
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // Display and edit rule sets
        for (int i = 0; i < targetRules.ruleSets.Count; i++)
        {
            FengShuiRuleSet ruleSet = targetRules.ruleSets[i];
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Display rule set info
            EditorGUILayout.LabelField($"Rule Set: {ruleSet.objectType} in {ruleSet.roomType}", EditorStyles.boldLabel);
            
            // Allow editing rule set properties
            ruleSet.objectType = EditorGUILayout.TextField("Object Type", ruleSet.objectType);
            ruleSet.roomType = EditorGUILayout.TextField("Room Type", ruleSet.roomType);
            
            // Display directional rules
            for (int j = 0; j < ruleSet.directionalRules.Count; j++)
            {
                DirectionalRule dirRule = ruleSet.directionalRules[j];
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"Direction {j+1}", EditorStyles.boldLabel);
                
                // Edit direction
                dirRule.mainDirection = EditorGUILayout.Vector3Field("Direction", dirRule.mainDirection);
                
                // Edit position rules
                EditorGUILayout.LabelField("Position Rules:", EditorStyles.boldLabel);
                
                for (int k = 0; k < dirRule.positionRules.Count; k++)
                {
                    PositionRule posRule = dirRule.positionRules[k];
                    
                    EditorGUILayout.BeginHorizontal();
                    
                    posRule.positionType = (PositionType)EditorGUILayout.EnumPopup(posRule.positionType);
                    posRule.zoneQuality = (ZoneQuality)EditorGUILayout.EnumPopup(posRule.zoneQuality);
                    posRule.scoreModifier = EditorGUILayout.IntField(posRule.scoreModifier);
                    
                    EditorGUILayout.EndHorizontal();
                    
                    posRule.feedbackMessage = EditorGUILayout.TextField("Message", posRule.feedbackMessage);
                    
                    if (GUILayout.Button("Remove Rule"))
                    {
                        dirRule.positionRules.RemoveAt(k);
                        k--;
                    }
                }
                
                if (GUILayout.Button("Add Position Rule"))
                {
                    dirRule.positionRules.Add(new PositionRule(
                        PositionType.Center,
                        ZoneQuality.Acceptable,
                        0,
                        "New position rule"
                    ));
                }
                
                EditorGUILayout.EndVertical();
                
                if (GUILayout.Button("Remove Direction"))
                {
                    ruleSet.directionalRules.RemoveAt(j);
                    j--;
                }
            }
            
            if (GUILayout.Button("Add Direction"))
            {
                DirectionalRule newDir = new DirectionalRule();
                newDir.mainDirection = Vector3.forward;
                newDir.positionRules = new List<PositionRule>();
                ruleSet.directionalRules.Add(newDir);
            }
            
            EditorGUILayout.EndVertical();
            
            if (GUILayout.Button("Remove Rule Set"))
            {
                targetRules.ruleSets.RemoveAt(i);
                i--;
            }
        }
        
        if (GUILayout.Button("Add Rule Set"))
        {
            FengShuiRuleSet newRuleSet = new FengShuiRuleSet();
            newRuleSet.objectType = "New_Object";
            newRuleSet.roomType = "New_Room";
            newRuleSet.directionalRules = new List<DirectionalRule>();
            targetRules.ruleSets.Add(newRuleSet);
        }
        
        EditorGUILayout.EndScrollView();
        
        if (GUI.changed)
        {
            EditorUtility.SetDirty(targetRules);
        }
    }
}
#endif
```

## Testing Approach

### Test Block 1: Rule Creation

1. Create a simple test rule set for a bed in a bedroom
2. Define rules for different orientations (North, East, South, West)
3. Test that the rules can be loaded and accessed at runtime
4. Verify that the rules editor can modify and save rules

### Test Block 2: Position Mapping

1. Test position mapping with various room sizes
2. Verify that the 3x3 grid correctly maps world positions to position types
3. Test edge cases (objects on grid boundaries)
4. Verify that special positions (near wall, door, window) are detected

### Test Block 3: Visual Feedback

1. Test visualization of zones during dragging
2. Verify that the correct colors appear based on the rules
3. Test that zone visualization updates when object is rotated
4. Ensure visualizations disappear after placement

### Test Block 4: Score Calculation

1. Test that base scores are still applied correctly
2. Verify that feng shui modifiers are applied based on placement
3. Test the total CHI calculation with multiple objects
4. Ensure feedback messages appear correctly

### Test Block 5: Room Type Integration

1. Test room transitions with different room types
2. Verify that room type is correctly passed to the CHI system
3. Test that different rooms can have different rule sets for the same object

## Best Practices

1. **Data-Driven Design**: Keep feng shui rules in ScriptableObjects to make them easy to edit without changing code.

2. **Modular Implementation**: 
   - Implement one block at a time
   - Test thoroughly before moving to the next block
   - Start with MVP features before adding optional ones

3. **Avoid Performance Issues**:
   - Pool visualization objects instead of creating/destroying them frequently
   - Cache calculations and references where possible
   - Be cautious with raycasts in Update()

4. **Backwards Compatibility**:
   - Maintain the existing BaseScore system as a fallback
   - Don't replace existing functionality - extend it
   - Make sure the system gracefully handles missing rule sets

5. **Debug Visibility**:
   - Add debug visualization options
   - Log key calculations with clear prefixes (`[FENG SHUI]`)
   - Create editor tools for easier configuration

6. **Keep Design Team Involved**:
   - Make the rule editor designer-friendly
   - Allow quick iteration on rule sets
   - Provide clear visual feedback

## Integration Points

1. **Room Manager Integration**:
   - Update RoomData to include roomType
   - Update RoomManager to set room type in CHIScoreManager
   - Make sure room objects are properly tagged

2. **ItemAutoDestroy Integration**:
   - Show feng shui zones during dragging
   - Update validation to consider feng shui rules
   - Update feedback messaging

3. **Visualization Integration**:
   - Create placeholder prefabs for zone visualization
   - Create a FengShuiVisualizer component to manage zone display
   - Add visualizer to scene

4. **Editor Integration**:
   - Create rule editor window
   - Add debug gizmos in scene view
   - Add runtime toggles for visualization

5. **Global Score Integration**:
   - Ensure CHI score is updated properly
   - Update feedback messages with feng shui advice
   - Keep CHI bar visual updates working 