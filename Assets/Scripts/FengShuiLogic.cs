using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class FengShuiLogic : MonoBehaviour
{
    public string objectType = "Bed"; // "Couch", "bookshelf", etc.


    // we use this only for the bed in order to check the direction of the head.
    public Transform headTransform;
    

    private Transform FindRoomMarker(string markerName){
        Transform roomRoot = transform;

        // Risali di uno o due livelli se necessario per raggiungere il Room_College
        while (roomRoot != null && !roomRoot.name.Contains("Room"))
        {
            roomRoot = roomRoot.parent;
        }

        if (roomRoot == null)
        {
            Debug.LogWarning($"[FengShui] Room root not found for {name}");
            return null;
        }

        Transform marker = roomRoot.Find(markerName);
        if (marker == null)
        {
            Debug.LogWarning($"[FengShui] Marker '{markerName}' not found under room '{roomRoot.name}'");
        }

        return marker;
    }


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
    
    private bool IsFacingMarker(Vector3 origin, Vector3 direction, string markerName, float segmentHalfWidth = 0.75f, float maxDistance = 1.5f, float maxAngle = 35f)
    {
        Transform marker = FindRoomMarker(markerName);
        if (marker == null) return false;

        Vector3 flatOrigin = new Vector3(origin.x, 0f, origin.z);
        Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z).normalized;
        Vector3 targetPoint = flatOrigin + flatDirection * maxDistance;

        Vector3 markerCenter = new Vector3(marker.position.x, 0f, marker.position.z);
        float markerZ = markerCenter.z;
        float clampedX = Mathf.Clamp(targetPoint.x, markerCenter.x - segmentHalfWidth, markerCenter.x + segmentHalfWidth);
        Vector3 closestPoint = new Vector3(clampedX, 0f, markerZ);

        float distance = Vector3.Distance(targetPoint, closestPoint);
        float angle = Vector3.Angle(flatDirection, (closestPoint - flatOrigin).normalized);

        Debug.DrawLine(flatOrigin + Vector3.up, closestPoint + Vector3.up, Color.magenta, 5f);
        Debug.Log($"[DEBUG] Marker '{markerName}' check → Distance: {distance:F2}, Angle: {angle:F1}");

        return distance <= maxDistance && angle <= maxAngle;
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
        // Trova la root della stanza salendo nei parent
        if (IsFacingMarker(origin, direction, "DoorMarker")){
            Debug.Log("[FengShui] ❌ Bed faces the door marker → -20");
            score -= 20;
        }
        else{
            Debug.Log("[FengShui] ✅ No door detected in front (marker).");
        }

        if (Physics.Raycast(origin, direction, out RaycastHit hit, 2f))
        {
            if (hit.collider.CompareTag("Wall") && hit.distance <= 0.5f)
            {
                score += 35;
                Debug.Log("[FengShui] ✅ Testiera contro il muro → +35");
            }
            else
            {
                score += 10;
                Debug.Log("[FengShui] ❌ Qualcosa dietro al letto ma non è un muro → -10");
            }
        }
        else
        {
            //Debug.Log($"[FengShui] ❌ No wall hitted behind the bed.");
            //Debug.Log($"          ↳ Testiera: {origin}, Direzione: {direction}");
            score -= 10;
        }

        return score;
    }

    private int EvaluateCouchRules(){
        int score = 0;

        if (headTransform == null)
            return score;

        Vector3 center = RoomManager.Instance.GetRoomCenter();
        Vector3 toCenter = (center - headTransform.position).normalized;
        Vector3 facing = -headTransform.forward;

        Debug.DrawRay(headTransform.position, facing * 2f, Color.yellow, 5f);

        float alignment = Vector3.Dot(facing, toCenter);

        if (alignment > 0.5f)
        {
            FeedbackTextManager.Instance?.ShowMessage("Nice placement facing the room!", Color.green);
            score += 8;
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

        // ✅ Controllo marker: porta davanti
        if (IsFacingMarker(headTransform.position, facing, "DoorMarker"))
        {
            Debug.Log("[FengShui] ❌ Couch faces the door → -10");
            FeedbackTextManager.Instance?.ShowMessage("Don't sit facing the door, bro.", Color.red);
            score -= 10;
        }

        // ✅ Controllo marker: porta dietro
        if (IsFacingMarker(headTransform.position, -facing, "DoorMarker"))
        {
            Debug.Log("[FengShui] ❌ Door behind the couch → -6");
            score -= 6;
        }

        // ✅ Controllo marker: finestra dietro
        if (IsFacingMarker(headTransform.position, -facing, "WindowMarker"))
        {
            Debug.Log("[FengShui] ❌ Window behind the couch → -6");
            FeedbackTextManager.Instance?.ShowMessage("A Window behind your back? Never.", Color.red);
            score -= 6;
        }

        // ✅ Raycast per il supporto dietro (muro o altro)
        Vector3 behind = headTransform.position - facing;
        Debug.DrawRay(behind, -facing * 4f, Color.cyan, 4f);

        if (Physics.Raycast(behind, -facing, out RaycastHit hitBack, 4f))
        {
            if (hitBack.collider.CompareTag("Wall"))
            {
                Debug.Log("[FengShui] ✅ There is a wall behind the couch → +8");
                score += 8;
            }
            else
            {
                Debug.Log("[FengShui] ✅ There is some support behind the couch → +4");
                score += 4;
            }
        }
        else
        {
            Debug.Log("[FengShui] ❌ Nothing detected behind the couch → -5");
            FeedbackTextManager.Instance?.ShowMessage("You need support on the back.", Color.red);
            score -= 5;
        }

        return score;
    }



    private int EvaluateBookshelfRules(){
        int score = 0;

        Vector3 origin = headTransform.position;
        Vector3 forward = headTransform.forward;
        Vector3 back = -headTransform.forward;

        Debug.DrawRay(origin, back * 4f, Color.magenta, 5f); // retro
        Debug.DrawRay(origin, forward * 2f, Color.yellow, 5f);  // fronte

        // ✅ Controllo se dietro c'è un muro (con collider)
        if (Physics.Raycast(origin, forward, out RaycastHit hitBack, 4f))
        {
            Debug.Log($"[Bookshelf BACK] Hit {hitBack.collider.name}, Tag: {hitBack.collider.tag}");

            if (hitBack.collider.CompareTag("Wall"))
            {
                score += 10;
                Debug.Log("[FengShui] ✅ Bookshelf has a wall behind → +10");
            }
            else
            {
                score -= 4;
                Debug.Log("[FengShui] ❌ Bookshelf is against something else → -4");
            }
        }
        else
        {
            score -= 4;
            Debug.Log("[FengShui] ❌ Nothing behind the bookshelf → -4");
        }

        // ✅ Controllo con marker se c'è una finestra o porta DAVANTI (senza collider)
        bool frontIsWindow = IsFacingMarker(origin, back, "WindowMarker");
        if (frontIsWindow)
        {
            score -= 6;
            Debug.Log("[FengShui] ❌ Bookshelf is in front of a window → -6");
        }
        bool frontIsDoor = IsFacingMarker(origin, back, "DoorMarker");
        if (frontIsDoor)
        {
            score -= 6;
            Debug.Log("[FengShui] ❌ Bookshelf is in front of a door → -6");
        }

        bool backIsWindow = IsFacingMarker(origin, forward, "WindowMarker");
        if (frontIsWindow)
        {
            score -= 6;
            Debug.Log("[FengShui] ❌ Bookshelf the back is a window → -6");
        }
        bool backIsDoor = IsFacingMarker(origin, forward, "DoorMarker");
        if (frontIsDoor)
        {
            score -= 6;
            Debug.Log("[FengShui] ❌ Bookshelf back is a door → -6");
        }
        

        

        return score;
    }

    private int EvaluateNightstandRules(){
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

        // ✅ Controllo se c'è un letto accanto (via raycast)
        if (Physics.Raycast(origin, left, out RaycastHit hitLeft, 2f))
        {
            if (hitLeft.collider.name.Contains("Bed"))
            {
                Debug.Log("[FengShui] ✅ Nightstand next to bed (left) → +10");
                score += 10;
            }
        }

        if (Physics.Raycast(origin, right, out RaycastHit hitRight, 2f))
        {
            if (hitRight.collider.name.Contains("Bed"))
            {
                Debug.Log("[FengShui] ✅ Nightstand next to bed (right) → +10");
                score += 10;
            }
        }

        // ✅ Porta dietro (marker)
        if (IsFacingMarker(origin, back, "DoorMarker"))
        {
            Debug.Log("[FengShui] ❌ Nightstand backed by a door → -5");
            score -= 5;
        }

        // ✅ Porta davanti (marker)
        if (IsFacingMarker(origin, forward, "DoorMarker"))
        {
            Debug.Log("[FengShui] ❌ Nightstand facing a door → -5");
            score -= 5;
        }

        // ✅ Muro dietro (collider)
        if (Physics.Raycast(origin, back, out RaycastHit hitBack, 2f))
        {
            if (hitBack.collider.CompareTag("Wall"))
            {
                Debug.Log("[FengShui] ✅ Nightstand backed by a wall → +5");
                score += 5;
            }
        }

        return score;
    }


    private int EvaluateTableRules(){
        int score = 0;

        Vector3 origin = headTransform.position;
        Vector3 forward = headTransform.forward;
        Vector3 back = -forward;
        Vector3 right = headTransform.right;
        Vector3 left = -right;

        Debug.DrawRay(origin, forward * 2f, Color.green, 5f);
        Debug.DrawRay(origin, back * 2f, Color.red, 5f);

        // ✅ Porta davanti
        if (IsFacingMarker(origin, forward, "DoorMarker"))
        {
            Debug.Log("[FengShui] ❌ Table faces door → -5");
            score -= 5;
        }

        // ✅ Finestra davanti
        if (IsFacingMarker(origin, forward, "WindowMarker"))
        {
            Debug.Log("[FengShui] ✅ Table faces window → +4");
            score += 4;
        }

        // ✅ Porta dietro
        if (IsFacingMarker(origin, back, "DoorMarker"))
        {
            Debug.Log("[FengShui] ❌ Door behind table → -5");
            score -= 5;
        }

        // ✅ Finestra dietro
        if (IsFacingMarker(origin, back, "WindowMarker"))
        {
            Debug.Log("[FengShui] ❌ Window behind table → -5");
            score -= 5;
        }

        // ✅ Muro dietro (collider)
        if (Physics.Raycast(origin, back, out RaycastHit hitBack, 6f))
        {
            if (hitBack.collider.CompareTag("Wall"))
            {
                Debug.Log("[FengShui] ✅ Table has wall behind → +3");
                score += 3;
            }
        }

        // ✅ Muro davanti (opzionale)
        if (Physics.Raycast(origin, forward, out RaycastHit hitFront, 4f))
        {
            if (hitFront.collider.CompareTag("Wall"))
            {
                Debug.Log("[FengShui] ✅ Table faces wall → +3");
                score += 3;
            }
        }

        // ✅ Letto vicino (collider)
        if (Physics.Raycast(origin, left, out RaycastHit hitLeft, 7f))
        {
            if (hitLeft.collider.name.Contains("Bed"))
            {
                Debug.Log("[FengShui] ❌ Bed too close on left → -5");
                score -= 5;
            }
        }

        if (Physics.Raycast(origin, right, out RaycastHit hitRight, 7f))
        {
            if (hitRight.collider.name.Contains("Bed"))
            {
                Debug.Log("[FengShui] ❌ Bed too close on right → -5");
                score -= 5;
            }
        }

        return score;
    }



   private int EvaluateChairRules(){
        int score = 0;

        if (headTransform == null)
        {
            Debug.LogWarning("[FengShui] Chair is missing headTransform");
            return score;
        }

        Vector3 origin = headTransform.position;
        Vector3 forward = headTransform.forward;
        Vector3 back = -forward;
        Vector3 right = headTransform.right;
        Vector3 left = -right;

        if (IsFacingMarker(origin, forward, "DoorMarker"))
        {
            Debug.Log("[FengShui] ❌ Chair faces door → -5");
            score -= 5;
        }

        if (IsFacingMarker(origin, back, "DoorMarker"))
        {
            Debug.Log("[FengShui] ❌ Chair back faces door → -5");
            score -= 5;
        }

        if (IsFacingMarker(origin, right, "DoorMarker"))
        {
            Debug.Log("[FengShui] ❌ Chair right faces door → -5");
            score -= 5;
        }

        if (IsFacingMarker(origin, left, "DoorMarker"))
        {
            Debug.Log("[FengShui] ❌ Chair left faces door → -5");
            score -= 5;
        }

        return score;
    }


}
