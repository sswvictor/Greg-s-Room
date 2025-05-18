using System.Collections.Generic;
using UnityEngine;

public class CHIScoreManager : MonoBehaviour
{
    public static CHIScoreManager Instance;

    // Reference to feng shui rules data
    public FengShuiRulesData rulesData;
    
    // Current room type
    private string currentRoomType = "";

    // 基础得分表（Prefab名字 -> 分数）
    private Dictionary<string, int> baseScores = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 初始化基础得分（根据你的Prefab名字调整）
        baseScores.Add("Plant", 5);
        baseScores.Add("Bed_Prefab", 3);
        baseScores.Add("Basketball_Prefab", 10);
        baseScores.Add("TrashBin", -3);
        baseScores.Add("Toilet", -5);
        baseScores.Add("Couch_Prefab", 4);
        // 可继续添加更多物品...
    }
    
    // Set the current room type for feng shui calculations
    public void SetCurrentRoomType(string roomType)
    {
        currentRoomType = roomType;
        Debug.Log($"[CHI] Room type set to: {roomType}");
    }

    // 计算当前房间总CHI分
    public int CalculateTotalCHI()
    {
        int totalScore = 0;

        Transform parent = GameObject.Find("ItemSpawnRoot")?.transform;
        if (parent == null)
        {
            Debug.LogWarning("[CHI] ItemSpawnRoot not found. Skip CHI calculation.");
            return 0;
        }

        foreach (Transform child in parent){
            var item = child.GetComponent<ItemAutoDestroy>();
            if (item != null && item.isValidPlacement)
            {
                // Get the base score
                int baseScore = GetBaseScore(child.gameObject);
                
                // Get feng shui modifier based on position and orientation
                int fengShuiModifier = EvaluateFengShuiPlacement(child.gameObject);
                
                // Calculate total item score
                int itemScore = baseScore + fengShuiModifier;
                
                Debug.Log($"[CHI ✅] {child.name}: Base={baseScore}, FengShui={fengShuiModifier}, Total={itemScore}");
                totalScore += itemScore;
            }
            else
            {
                Debug.Log($"[CHI ❌] {child.name} skipped (invalid position or no component).");
            }
        }

        Debug.Log($"[CHI] Total CHI in this room = {totalScore}");
        return totalScore;
    }

    private int GetBaseScore(GameObject go)
    {
        if (baseScores.TryGetValue(go.name.Replace("(Clone)", ""), out int score))
        {
            return score;
        }
        Debug.LogWarning($"[CHI][⚠️] Unrecognized object {go.name}, contributes 0 points.");
        return 0; // 未定义物体默认0分
    }
    
    // Evaluate feng shui placement of an object
    private int EvaluateFengShuiPlacement(GameObject obj)
    {
        if (string.IsNullOrEmpty(currentRoomType) || rulesData == null)
        {
            return 0; // No feng shui rules to apply
        }
            
        string objectType = obj.name.Replace("(Clone)", "");
        
        // Get rule set for this object in current room
        FengShuiRuleSet ruleSet = rulesData.GetRuleSet(objectType, currentRoomType);
        if (ruleSet == null)
        {
            Debug.Log($"[FENG SHUI] No rule set for {objectType} in {currentRoomType}");
            return 0;
        }
        
        // Get current position type
        GameObject roomObj = GameObject.FindGameObjectWithTag("Room");
        if (roomObj == null)
        {
            Debug.LogWarning("[FENG SHUI] No room found with 'Room' tag");
            return 0;
        }
        
        Collider roomCollider = roomObj.GetComponent<Collider>();
        if (roomCollider == null)
        {
            Debug.LogWarning("[FENG SHUI] Room has no collider component");
            return 0;
        }
        
        PositionType posType = PositionMapper.GetPositionType(obj.transform.position, roomCollider.bounds);
        
        // Get special positions if applicable
        List<PositionType> specialTypes = PositionMapper.GetSpecialPositionTypes(obj.transform.position, roomObj);
        
        // Find best matching directional rule based on object's forward vector
        DirectionalRule bestRule = GetBestDirectionalRule(ruleSet, obj.transform.forward);
        if (bestRule == null)
        {
            Debug.Log($"[FENG SHUI] No directional rules for {objectType}");
            return 0;
        }
        
        // Find applicable position rule
        PositionRule posRule = bestRule.positionRules.Find(r => r.positionType == posType);
        if (posRule != null)
        {
            // Show feedback message if not already done by ItemAutoDestroy
            if (FeedbackTextManager.Instance != null)
            {
                Color msgColor = posRule.zoneQuality == ZoneQuality.Excellent ? Color.green : 
                                 posRule.zoneQuality == ZoneQuality.Acceptable ? Color.white : 
                                 Color.red;
                                 
                FeedbackTextManager.Instance.ShowMessage(posRule.feedbackMessage, msgColor);
            }
            
            return posRule.scoreModifier;
        }
        
        // Check special position rules
        foreach (PositionType specialType in specialTypes)
        {
            PositionRule specialRule = bestRule.positionRules.Find(r => r.positionType == specialType);
            if (specialRule != null)
            {
                // Show feedback for special position
                if (FeedbackTextManager.Instance != null)
                {
                    Color msgColor = specialRule.zoneQuality == ZoneQuality.Excellent ? Color.green : 
                                     specialRule.zoneQuality == ZoneQuality.Acceptable ? Color.white : 
                                     Color.red;
                                     
                    FeedbackTextManager.Instance.ShowMessage(specialRule.feedbackMessage, msgColor);
                }
                
                return specialRule.scoreModifier;
            }
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
}
