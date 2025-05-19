using UnityEngine;

public class FengShuiLogic : MonoBehaviour
{
    public string objectType = "Bed"; // "Mirror", "TrashBin", ecc.
    public Transform headTransform;   // opzionale, usato solo per il letto

    public int EvaluateFengShuiScore()
    {
        int score = 0;

        switch (objectType)
        {
            case "Bed":
                score += EvaluateBedRules();
                break;

            case "Mirror":
                score += EvaluateMirrorRules();
                break;

            case "TrashBin":
                score -= 5;
                break;

            default:
                Debug.LogWarning($"[FengShui] No rules for type: {objectType}");
                break;
        }

        return score;
    }

    private int EvaluateBedRules()
    {
        int score = 0;

        // ✅ TESTIERA CONTRO IL MURO
        if (headTransform != null)
        {

            if (Physics.Raycast(headTransform.position, -transform.forward, out RaycastHit hit, 2f))
            {
                float wallDistance = hit.distance;
                Debug.Log($"[FengShui] DISTANZA testiera → muro: {wallDistance:F2} unità");

                if (hit.collider.CompareTag("Wall") && wallDistance <= 0.5f)
                {
                    score += 10;
                }
                else
                {
                    score -= 5;
                }
            }
            else
            {
                Debug.Log("[FengShui] ❌ Nessun muro dietro la testiera");
                Debug.Log($"[FengShui] ❌ Nessun colpo. Testiera: {headTransform.position}, direzione: {-transform.forward}");
                score -= 5;
            }
        }
        else
        {
            Debug.LogWarning("[FengShui] ⚠️ headTransform non assegnato nel prefab!");
        }

        // ✅ DISTANZA DALLA PORTA
        float distance = Vector3.Distance(transform.position, RoomManager.Instance.GetDoorPosition());
        Debug.Log($"[FengShui] DISTANZA letto → porta: {distance:F2} unità");

        if (distance < 2.0f)
        {
            score -= 5;
        }
        else
        {
            score += 3;
        }

        return score;
    }


  
    private int EvaluateMirrorRules()
    {
        int score = 0;

        var beds = FindObjectsOfType<FengShuiLogic>();
        foreach (var obj in beds)
        {
            if (obj.objectType == "Bed")
            {
                float dist = Vector3.Distance(transform.position, obj.transform.position);
                if (dist < 2.0f) score -= 10;
                else score += 5;
            }
        }

        return score;
    }
}
