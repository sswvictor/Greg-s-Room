using System.Collections.Generic;
using UnityEngine;

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
    public Vector3 mainDirection = Vector3.forward; // Forward vector of furniture
    public List<PositionRule> positionRules = new List<PositionRule>();
}

// Complete rule set for an object type in a specific room
[System.Serializable]
public class FengShuiRuleSet
{
    public string objectType; // e.g., "Bed_Prefab"
    public string roomType; // e.g., "Bedroom"
    public List<DirectionalRule> directionalRules = new List<DirectionalRule>();
}

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