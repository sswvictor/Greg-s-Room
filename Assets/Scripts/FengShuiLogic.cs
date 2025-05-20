using UnityEngine;

public class FengShuiLogic : MonoBehaviour
{
    public string objectType = "Bed"; // "Mirror", "TrashBin", ecc.

    // we use this only for the bed in order to check the direction of the head.
    public Transform headTransform;   

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

        if (headTransform == null)
        {
            Debug.LogError("[FengShui] ❌ headTransform not assigned!");
            return score;
        }

        Vector3 origin = headTransform.position;
        Vector3 direction = headTransform.right;
        
        

        // debug ray for checking the direction of the bed
        Debug.DrawRay(origin, direction * 2f, Color.red, 5f);

        // check if the bed to the door
        if (Physics.Raycast(origin, direction, out RaycastHit doorHit, 2f))
        {
            if (doorHit.collider.CompareTag("Door") && doorHit.distance <= 0.5f)
            {
                Debug.Log($"[FengShui] ❌ Letto davanti alla porta (colpita {doorHit.collider.name})");
                score -= 10;
            }
        }

        if (Physics.Raycast(origin, direction, out RaycastHit hit, 2f))
        {
            //Debug.Log($"[FengShui] ✅ HIT: {hit.collider.name}, distance: {hit.distance:F2}, point: {hit.point}");
            score += (hit.collider.CompareTag("Wall") && hit.distance <= 0.5f) ? 10 : -5;
        }
        else
        {
            //Debug.Log($"[FengShui] ❌ No wall hitted behind the bed.");
            //Debug.Log($"          ↳ Testiera: {origin}, Direzione: {direction}");
            score -= 5;
        }

        Ray ray = new Ray(headTransform.position, headTransform.right);
        RaycastHit[] hits = Physics.RaycastAll(ray, 2f);

        foreach (RaycastHit x in hits)
        {
            Debug.Log($"[RaycastAll] Hit {hit.collider.name}");

            if (hit.collider.CompareTag("Door"))
            {
                Debug.Log("[FengShui] ❌ Colpita la porta → malus feng-shui");
                score -= 10;
                break; // assegna malus solo una volta
            }

            if (hit.collider.CompareTag("Wall"))
            {
                score += 10; // o altro punteggio per muro
            }
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
