using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class FengShuiLogic : MonoBehaviour
{
    public string objectType = "Bed"; // "Couch", "bookshelf", etc.


    // we use this only for the bed in order to check the direction of the head.
    public Transform headTransform;


    private Transform FindRoomMarker(string markerName)
    {
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

            case "TVStand":
                score += EvaluateTVStandRules();
                break;

            case "TV":
                score += EvaluateTVRules();
                break;

            case "Kallax":
                score += EvaluateKallaxRules();
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
            return score;
        }

        Vector3 origin = headTransform.position;
        Vector3 direction = headTransform.right;

        // debug ray for checking the direction of the bed
        Debug.DrawRay(origin, direction * 2f, Color.cyan, 5f);

        // check if the bed to the door
        // Trova la root della stanza salendo nei parent
        if (IsFacingMarker(origin, direction, "DoorMarker"))
        {
            score -= 20;
        }
        else{
        }

        if (Physics.Raycast(origin, direction, out RaycastHit hit, 2f))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                score += 35;
            }
            else
            {
                score += 10;
            }
        }
        else
        {
            score -= 20;
        }

        return score;
    }

    private int EvaluateCouchRules()
    {
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

        if (IsFacingMarker(headTransform.position, facing, "DoorMarker"))
        {
            FeedbackTextManager.Instance?.ShowMessage("Don't sit facing the door, bro.", Color.red);
            score -= 10;
        }

        if (IsFacingMarker(headTransform.position, -facing, "DoorMarker"))
        {
            score -= 6;
        }

        if (IsFacingMarker(headTransform.position, -facing, "WindowMarker"))
        {
            FeedbackTextManager.Instance?.ShowMessage("A Window behind your back? Never.", Color.red);
            score -= 6;
        }

        Vector3 behind = headTransform.position - facing;
        Debug.DrawRay(behind, -facing * 4f, Color.cyan, 4f);

        if ((Physics.Raycast(behind, -facing, out RaycastHit hitBack, 4f)) && ((IsFacingMarker(headTransform.position, facing, "DoorMarker")) == false))
        {
            if (hitBack.collider.CompareTag("Wall"))
            {
                score += 8;
            }
            else
            {
                score += 4;
            }
        }
        else
        {
            FeedbackTextManager.Instance?.ShowMessage("You need support on the back.", Color.red);
            score -= 5;
        }

        return score;
    }



    private int EvaluateBookshelfRules()
    {
        int score = 0;

        Vector3 origin = headTransform.position;
        Vector3 forward = headTransform.forward;
        Vector3 back = -headTransform.forward;

        Debug.DrawRay(origin, back * 4f, Color.magenta, 5f); // retro
        Debug.DrawRay(origin, forward * 2f, Color.yellow, 5f);  // fronte

        if (Physics.Raycast(origin, forward, out RaycastHit hitBack, 4f))
        {

            if (hitBack.collider.CompareTag("Wall"))
            {
                score += 10;
            }
            else
            {
                score -= 4;
            }
        }
        else
        {
            score -= 4;
        }

        // ✅ Controllo con marker se c'è una finestra o porta DAVANTI (senza collider)
        bool frontIsWindow = IsFacingMarker(origin, back, "WindowMarker");
        if (frontIsWindow)
        {
            score -= 6;
        }
        bool frontIsDoor = IsFacingMarker(origin, back, "DoorMarker");
        if (frontIsDoor)
        {
            score -= 6;
        }

        bool backIsWindow = IsFacingMarker(origin, forward, "WindowMarker");
        if (frontIsWindow)
        {
            score -= 6;
        }
        bool backIsDoor = IsFacingMarker(origin, forward, "DoorMarker");
        if (frontIsDoor)
        {
            score -= 6;
        }




        return score;
    }

    private int EvaluateNightstandRules()
    {
        int score = 0;

        if (headTransform == null)
        {
            return score;
        }

        Vector3 origin = headTransform.position;
        Vector3 forward = -headTransform.forward;
        Vector3 back = -forward;
        Vector3 right = headTransform.right;
        Vector3 left = -headTransform.right;

        if (Physics.Raycast(origin, left, out RaycastHit hitLeft, 2f))
        {
            if (hitLeft.collider.name.Contains("Bed"))
            {
                score += 10;
            }
        }

        if (Physics.Raycast(origin, right, out RaycastHit hitRight, 2f))
        {
            if (hitRight.collider.name.Contains("Bed"))
            {
                score += 10;
            }
        }

        if (IsFacingMarker(origin, back, "DoorMarker"))
        {
            score -= 5;
        }

        if (IsFacingMarker(origin, forward, "DoorMarker"))
        {
            score -= 5;
        }

        if (Physics.Raycast(origin, back, out RaycastHit hitBack, 2f))
        {
            if (hitBack.collider.CompareTag("Wall"))
            {
                score += 5;
            }
        }

        return score;
    }


    private int EvaluateTableRules()
    {
        int score = 0;

        Vector3 origin = headTransform.position;
        Vector3 forward = headTransform.forward;
        Vector3 back = -forward;
        Vector3 right = headTransform.right;
        Vector3 left = -right;

        Debug.DrawRay(origin, forward * 2f, Color.green, 5f);
        Debug.DrawRay(origin, back * 2f, Color.red, 5f);

        if (IsFacingMarker(origin, forward, "DoorMarker"))
        {
            score -= 5;
        }

        if (IsFacingMarker(origin, forward, "WindowMarker"))
        {
            score += 4;
        }

        if (IsFacingMarker(origin, back, "DoorMarker"))
        {
            score -= 5;
        }

        if (IsFacingMarker(origin, back, "WindowMarker"))
        {
            score -= 5;
        }

        if (Physics.Raycast(origin, back, out RaycastHit hitBack, 6f))
        {
            if (hitBack.collider.CompareTag("Wall"))
            {
                score += 3;
            }
        }

        // ✅ Letto vicino (collider)
        if (Physics.Raycast(origin, left, out RaycastHit hitLeft, 7f))
        {
            if (hitLeft.collider.name.Contains("Bed"))
            {
                score -= 5;
            }
        }

        if (Physics.Raycast(origin, right, out RaycastHit hitRight, 7f))
        {
            if (hitRight.collider.name.Contains("Bed"))
            {
                score -= 5;
            }
        }

        return score;
    }



    private int EvaluateChairRules()
    {
        int score = 0;

        if (headTransform == null)
        {
            return score;
        }

        Vector3 origin = headTransform.position;
        Vector3 forward = headTransform.forward;
        Vector3 back = -forward;
        Vector3 right = headTransform.right;
        Vector3 left = -right;

        if (IsFacingMarker(origin, forward, "DoorMarker"))
        {
            score -= 5;
        }

        if (IsFacingMarker(origin, back, "DoorMarker"))
        {
            score -= 5;
        }

        if (IsFacingMarker(origin, right, "DoorMarker"))
        {
            score -= 5;
        }

        if (IsFacingMarker(origin, left, "DoorMarker"))
        {
            score -= 5;
        }

        return score;
    }

    private int EvaluateTVStandRules()
    {
        int score = 0;

        if (headTransform == null) return score;

        Vector3 origin = headTransform.position;
        Vector3 forward = headTransform.forward;
        Vector3 back = -forward;

        // There is a wall behind the TV Stand
        if (Physics.Raycast(origin, back, out RaycastHit hitBack, 2f))
        {
            if (hitBack.collider.CompareTag("Wall"))
            {
                score += 5;
            }
        }

        // in front of the object, there is something
        if (Physics.Raycast(origin, forward, out RaycastHit hitFront, 2f))
        {
            score -= 5;
        }

        // The object is somewhere near the door 
        Vector3[] directions = { forward, back, headTransform.right, -headTransform.right };
        foreach (var dir in directions)
        {
            if (IsFacingMarker(origin, dir, "DoorMarker"))
            {
                score -= 10;
                break;
            }
        }

        return score;
    }

    private int EvaluateTVRules()
    {
        int score = 0;

        if (headTransform == null) return score;

        Vector3 origin = headTransform.position;
        Vector3 forward = headTransform.forward;
        Vector3 back = -forward;

        // No window in front of the TV
        if (IsFacingMarker(origin, forward, "WindowMarker"))
        {
            score -= 6;
        }

        // No window behind the TV
        if (IsFacingMarker(origin, back, "WindowMarker"))
        {
            score -= 6;
        }

        // Too close to the door 
        Vector3[] directions = { forward, back, headTransform.right, -headTransform.right };
        foreach (var dir in directions)
        {
            if (IsFacingMarker(origin, dir, "DoorMarker"))
            {
                score -= 6;
                break;
            }
        }

        Debug.DrawRay(origin, forward * 10f, Color.green, 15f);
        // The couch in front of the TV
        if (Physics.Raycast(origin, forward, out RaycastHit hit, 10f))
        {
            if (hit.collider.name.Contains("Couch"))
            {
                score += 6;
            }
        }
        else
        {
            score -= 3;
        }
        if (hit.collider != null)
        {
            Debug.Log($"Hit object: {hit.collider.name}, full path: {hit.collider.transform.root.name}");
        }


        return score;
    }

    private int EvaluateKallaxRules(){
        int score = 0;

        if (headTransform == null) return score;

        Vector3 origin = headTransform.position;
        Vector3 forward = headTransform.forward;
        Vector3 back = -forward;

        Debug.DrawRay(origin, forward * 2f, Color.cyan, 2f);
        Debug.DrawRay(origin, back * 2f, Color.red, 2f);

        // There is a wall behind the Kallax
        if (Physics.Raycast(origin, back, out RaycastHit hitBack, 2f))
        {
            if (hitBack.collider.CompareTag("Wall"))
            {
                score -= 6;
            }
        }

        // If there are objects in front or behind the Kallax, give points
        if (Physics.Raycast(origin, forward, out RaycastHit hitF, 3f))
        {
            if (hitF.collider.name.Contains("Bed") || hitF.collider.name.Contains("Couch") || hitF.collider.name.Contains("Table"))
            {
                score += 5;
            }
        }

        if (Physics.Raycast(origin, back, out RaycastHit hitB, 3f))
        {
            if (hitB.collider.name.Contains("Bed") || hitB.collider.name.Contains("Couch") || hitB.collider.name.Contains("Table"))
            {
                score += 5;
            }
        }

        // Cannot face the door
        Vector3[] directions = { forward, back, headTransform.right, -headTransform.right };
        foreach (var dir in directions)
        {
            if (IsFacingMarker(origin, dir, "DoorMarker"))
            {
                score -= 8;
                break;
            }
        }

        return score;
    }
    

}

