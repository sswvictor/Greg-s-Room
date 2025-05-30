using System.Collections.Generic;
using UnityEngine;

public class CHIScoreManager : MonoBehaviour
{
    public static CHIScoreManager Instance;

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

        baseScores.Add("Bed_Prefab", 20);
        baseScores.Add("Basketball_Prefab", 5);
        baseScores.Add("Bookshelf_Prefab", 10);
        baseScores.Add("Couch_Prefab", 15);  
        baseScores.Add("Nightstand_Prefab", 10);
        baseScores.Add("Frame_Prefab", 5);            
                  
        baseScores.Add("TrashBin", -3);
        baseScores.Add("Toilet", -5);
    }

    public int CalculateTotalCHI()
    {
        int totalScore = 0;

        Transform parent = GameObject.Find("ItemSpawnRoot")?.transform;
        if (parent == null)
        {
            return 0;
        }

        foreach (Transform child in parent)
        {
            var item = child.GetComponent<ItemAutoDestroy>();
            if (item != null && item.isValidPlacement)
            {
                var feng = child.GetComponent<FengShuiLogic>();
                if (feng != null)
                {
                    int score = feng.EvaluateFengShuiScore();
                    totalScore += score;
                }
            }
        }

        return totalScore;
    }

    private int GetBaseScore(GameObject go)
    {
        if (baseScores.TryGetValue(go.name.Replace("(Clone)", ""), out int score))
        {
            return score;
        }
        return 0; 
    }
}
