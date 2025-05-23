using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class FengShuiLogic : MonoBehaviour
{
    public string objectType = "Bed"; // "Couch", "bookshelf", etc.


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

            case "Couch":
                score += EvaluateCouchRules();
                break;

            case "Bookshelf":
                score += EvaluateBookshelfRules();
                break;

            case "Nightstand":
                score += EvaluateNightstandRules();
                break;
            
            case "Table":
                score += EvaluateTableRules();
                break;

            case "Chair":
                score += EvaluateChairRules();
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

        return score;
    }

    private int EvaluateCouchRules()
    {
        int score = 0;

        if (headTransform == null)
        {
            //Debug.LogError("[FengShui] ❌ headTransform non assegnato per il couch!");
            return score;
        }

        Vector3 center = RoomManager.Instance.GetRoomCenter();
        Vector3 toCenter = (center - headTransform.position).normalized;
        Vector3 facing = -headTransform.forward;

        Debug.DrawRay(headTransform.position, facing * 2f, Color.yellow, 5f);

        float alignment = Vector3.Dot(facing, toCenter);
        //Debug.Log($"[FengShui] Couch alignment via HEAD: {alignment:F2}");

        if (alignment > 0.5f)
        {
            FeedbackTextManager.Instance?.ShowMessage("Nice placement facing the room!", Color.green);
            score += 5;
        }
        else if (alignment < -0.6f)
        {
            FeedbackTextManager.Instance?.ShowMessage("Don't ignore the room, bro.", Color.red);
            score -= 5;
        }
        else
        {
            score += 2;
        }

        if (Physics.Raycast(headTransform.position, facing, out RaycastHit hitFront, 2f))
        {
            Debug.Log($"[Raycast FRONT] Hit: {hitFront.collider.name}, Tag: {hitFront.collider.tag}");

            if (hitFront.collider.CompareTag("Door"))
            {
                Debug.Log("[FengShui] ❌ The couch is positioned in front of a door → -10");
                FeedbackTextManager.Instance?.ShowMessage("Don't sit facing the door, bro.", Color.red);
                score -= 10;
            }
        }
        else
        {
            Debug.Log("[Raycast FRONT] no object hit in front of the couch.");
        }


        Vector3 behind = headTransform.position - facing;
        Debug.DrawRay(behind, -facing * 5f, Color.cyan, 4f);

        if (Physics.Raycast(behind, -facing, out RaycastHit hitBack, 4f))
        {
            Debug.Log($"[Raycast BACK] Hit: {hitBack.collider.name}, Tag: {hitBack.collider.tag}");

            if (hitBack.collider.CompareTag("Window"))
            {
                Debug.Log("[FengShui] ❌ There is a window behind the Couch→ -5");
                FeedbackTextManager.Instance?.ShowMessage("A Window behind your back? Never.", Color.red);
                score -= 5;
            }
            else if (hitBack.collider.CompareTag("Door"))
            {
                Debug.Log("[FengShui] Door behind the couch → -5");
                score -= 5;
            }
            else
            {
                Debug.Log("[FengShui] ✅ There is a support behind the couch → +5");
                score += 5;
            }
        }
        else
        {
            Debug.Log("[Raycast BACK] No object hit behind the couch.");
            FeedbackTextManager.Instance?.ShowMessage("You need support on the back.", Color.red);
            score -= 5;
        }

        return score;
    }


    private int EvaluateBookshelfRules()
    {
        int score = 0;

        Debug.DrawRay(headTransform.position, -headTransform.forward * 4f, Color.magenta, 5f); // retro
        Debug.DrawRay(headTransform.position, headTransform.forward * 2f, Color.yellow, 5f);  // fronte


        if (Physics.Raycast(headTransform.position, headTransform.forward, out RaycastHit hitBack, 4f))
        {
            Debug.Log($"[Bookshelf BACK] Hit {hitBack.collider.name}, Tag: {hitBack.collider.tag}");
            if (hitBack.collider.CompareTag("Wall"))
            {
                score += 5;
                Debug.Log("[FengShui] ✅ Bookshelf have a wall behind → +5");
            }
            else if (hitBack.collider.CompareTag("Window") || hitBack.collider.CompareTag("Door"))
            {
                score -= 5;
                Debug.Log("[FengShui] ❌ Bookshelf is against a windor or a Door → -5");
            }
            else
            {
                score -= 3;
                Debug.Log("[FengShui] ❌ Bookshelf is against something else → -3");
            }
        }
        else
        {
            score -= 3;
            Debug.Log("[FengShui] ❌ Nothing behind the bookshelf → -3");
        }

        if (Physics.Raycast(headTransform.position, -headTransform.forward, out RaycastHit hitFront, 2f))
        {
            Debug.Log($"[Bookshelf FRONT] Hit {hitFront.collider.name}, Tag: {hitFront.collider.tag}");

            if (hitFront.collider.CompareTag("Window") || hitFront.collider.CompareTag("Door") || hitFront.collider.CompareTag("Wall"))
            {
                score -= 5;
                Debug.Log("[FengShui] ❌ Bookshelf is in front of a window or a door → -5");
            }
        }
        return score;
    }

    private int EvaluateNightstandRules()
    {
        int score = 0;

        if (headTransform == null)
        {
            Debug.LogWarning("[FengShui] ⚠️ No headTransform assigned for nightstand.");
            return score;
        }

        Vector3 origin = headTransform.position;
        Vector3 forward = -headTransform.forward;
        Vector3 back = -forward;
        Vector3 right = headTransform.right;
        Vector3 left = -headTransform.right;

        // Check if there's a bed to the left or right (X axis)
        if (Physics.Raycast(origin, left, out RaycastHit hitLeft, 2f))
        {
            Debug.Log($"[Raycast LEFT] Hit: {hitLeft.collider.name}");
            if (hitLeft.collider.name.Contains("Bed"))
            {
                Debug.Log("[FengShui] ✅ Nightstand next to bed (left) → +10 points");
                score += 10;
            }
        }

        if (Physics.Raycast(origin, right, out RaycastHit hitRight, 2f))
        {
            Debug.Log($"[Raycast RIGHT] Hit: {hitRight.collider.name}");
            if (hitRight.collider.name.Contains("Bed"))
            {
                Debug.Log("[FengShui] ✅ Nightstand next to bed (right) → +10 points");
                score += 10;
            }
        }

        // Check BACK (-Z)
        if (Physics.Raycast(origin, back, out RaycastHit hitBack, 2f))
        {
            Debug.Log($"[Raycast BACK] Hit: {hitBack.collider.name}, Tag: {hitBack.collider.tag}");

            if (hitBack.collider.CompareTag("Wall"))
            {
                Debug.Log("[FengShui] ✅ Nightstand is backed by a wall → +5 points");
                score += 5;
            }
            else if (hitBack.collider.CompareTag("Door"))
            {
                Debug.Log("[FengShui] ❌ Nightstand is backed by a door → -5 points");
                score -= 5;
            }
        }

        // Check FRONT (+Z)
        if (Physics.Raycast(origin, forward, out RaycastHit hitFront, 2f))
        {
            Debug.Log($"[Raycast FRONT] Hit: {hitFront.collider.name}, Tag: {hitFront.collider.tag}");

            if (hitFront.collider.CompareTag("Door"))
            {
                Debug.Log("[FengShui] ❌ Nightstand is facing a door → -5 points");
                score -= 5;
            }
        }

        return score;
    }

    private int EvaluateTableRules()
    {
        int score = 0;

        Vector3 origin = headTransform.position;
        Vector3 forward = headTransform.forward;
        Vector3 back = -headTransform.forward;
        Vector3 right = headTransform.right;
        Vector3 left = -headTransform.right;

        Debug.DrawRay(origin, forward * 2f, Color.green, 5f);     // FRONT
        Debug.DrawRay(origin, -forward * 2f, Color.red, 5f);       // BACK
        Debug.DrawRay(origin, right * 2f, Color.blue, 5f);         // RIGHT
        Debug.DrawRay(origin, -right * 2f, Color.cyan, 5f);      // LEFT

        if (Physics.Raycast(origin, forward, out RaycastHit hitFront, 4f)){
            Debug.Log($"[Table] Forward hit: {hitFront.collider.name}");

            if (hitFront.collider.CompareTag("Wall"))
            {
                Debug.Log("[FengShui] Table faces wall → OK");
                score += 2;
            }
            else if (hitFront.collider.CompareTag("Door"))
            {
                Debug.Log("[FengShui]  Table faces door → -5");
                score -= 5;
            }
            else if (hitFront.collider.CompareTag("Window"))
            {
                Debug.Log("[FengShui]  Table faces window → +3 (positive energy)");
                score += 3;
            }
        }

        // BACKWARD check
        if (Physics.Raycast(origin, back, out RaycastHit hitBack, 6f))
        {
            Debug.Log($"[Table] Back hit: {hitBack.collider.name}");

            if (hitBack.collider.CompareTag("Wall"))
            {
                Debug.Log("[FengShui]  Table has wall behind → OK");
                score += 2;
            }
            else if (hitBack.collider.CompareTag("Door"))
            {
                Debug.Log("[FengShui]  Table has door behind → -5");
                score -= 5;
            }
            else if (hitBack.collider.CompareTag("Window"))
            {
                Debug.Log("[FengShui]  Window behind table → -3");
                score -= 3;
            }
        }

        // SIDE (LEFT)
        if (Physics.Raycast(origin, left, out RaycastHit hitLeft, 6f))
        {
            if (hitLeft.collider.name.Contains("Bed"))
            {
                Debug.Log("[FengShui]  Bed too close on the left side → -4");
                score -= 4;
            }
        }

        // SIDE (RIGHT)
        if (Physics.Raycast(origin, right, out RaycastHit hitRight, 6f))
        {
            if (hitRight.collider.name.Contains("Bed"))
            {
                Debug.Log("[FengShui]  Bed too close on the right side → -4");
                score -= 4;
            }
        }

        return score;
    
    }


    private int EvaluateChairRules()
    {
        int score = 0;

        if (headTransform == null){
            Debug.LogWarning("[FengShui] Chair is missing headTransform");
            return score;
        }

        Vector3 origin = headTransform.position;

        // Forward (Z+)
        if (Physics.Raycast(origin, headTransform.forward, out RaycastHit hitFwd, 2f)){
            Debug.Log($"[ChairRaycast] → Forward hit {hitFwd.collider.name}");
            if (hitFwd.collider.CompareTag("Door")){
                Debug.Log("[FengShui] ❌ Chair faces door → -5");
                score -= 5;
            }
        }

        // Backward (Z-)
        if (Physics.Raycast(origin, -headTransform.forward, out RaycastHit hitBack, 2f)){
            Debug.Log($"[ChairRaycast] ← Backward hit {hitBack.collider.name}");
            if (hitBack.collider.CompareTag("Door")){
                Debug.Log("[FengShui] ❌ Chair back faces door → -5");
                score -= 5;
            }
        }

        // Right (X+)
        if (Physics.Raycast(origin, headTransform.right, out RaycastHit hitRight, 2f)){
            Debug.Log($"[ChairRaycast] → Right hit {hitRight.collider.name}");
            if (hitRight.collider.CompareTag("Door")){
                Debug.Log("[FengShui] ❌ Chair right faces door → -5");
                score -= 5;
            }
        }

        // Left (X-)
        if (Physics.Raycast(origin, -headTransform.right, out RaycastHit hitLeft, 2f)){
            Debug.Log($"[ChairRaycast] ← Left hit {hitLeft.collider.name}");
            if (hitLeft.collider.CompareTag("Door")){
                Debug.Log("[FengShui] ❌ Chair left faces door → -5");
                score -= 5;
            }
        }

    return score;
    }

}
