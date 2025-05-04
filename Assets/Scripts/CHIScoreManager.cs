using System.Collections.Generic;
using UnityEngine;

public class CHIScoreManager : MonoBehaviour
{
    public static CHIScoreManager Instance;

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
        // 可继续添加更多物品...
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
                int score = GetBaseScore(child.gameObject);
                Debug.Log($"[CHI ✅] {child.name} contributes {score} points.");
                totalScore += score;
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
}
