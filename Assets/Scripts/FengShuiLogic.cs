using System;
using UnityEngine;

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

        return score;
    }

    private int EvaluateCouchRules(){
        int score = 0;

        if (headTransform == null){
            //Debug.LogError("[FengShui] ❌ headTransform non assegnato per il couch!");
            return score;
        }

        Vector3 center = RoomManager.Instance.GetRoomCenter();
        Vector3 toCenter = (center - headTransform.position).normalized;
        Vector3 facing = -headTransform.forward;

        Debug.DrawRay(headTransform.position, facing * 2f, Color.yellow, 5f);

        float alignment = Vector3.Dot(facing, toCenter);
        //Debug.Log($"[FengShui] Couch alignment via HEAD: {alignment:F2}");

        if (alignment > 0.5f){
           FeedbackTextManager.Instance?.ShowMessage("Nice placement facing the room!", Color.green);
            score += 5;
        }
        else if (alignment < -0.6f){
            FeedbackTextManager.Instance?.ShowMessage("Don't ignore the room, bro.", Color.red);
            score -= 5;
        }
        else{
            score += 2;
        }

        if (Physics.Raycast(headTransform.position, facing, out RaycastHit hitFront, 2f)){
            Debug.Log($"[Raycast FRONT] Hit: {hitFront.collider.name}, Tag: {hitFront.collider.tag}");

            if (hitFront.collider.CompareTag("Door")){
                Debug.Log("[FengShui] ❌ The couch is positioned in front of a door → -10");
                FeedbackTextManager.Instance?.ShowMessage("Don't sit facing the door, bro.", Color.red);
                score -= 10;
            }   
        }
        else{
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
}
