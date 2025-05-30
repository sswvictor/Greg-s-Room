using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class FengShuiLogic : MonoBehaviour
{
    public string objectType = "Bed"; 

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
                break;
        }

        return score;
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




        if (Physics.Raycast(origin, direction, out RaycastHit doorHit, 2f))
        {
            if (doorHit.collider.CompareTag("Door") && doorHit.distance <= 0.5f)
            {
                score -= 20;
            }
        }

        if (Physics.Raycast(origin, direction, out RaycastHit hit, 2f))
        {
            if (hit.collider.CompareTag("Wall")){
                score += 35;
                FeedbackTextManager.Instance?.ShowMessage("Perfect placing bro", Color.green);
            }
            else{
                score += 10;
            }
        }
        else
        {
            score -= 10;
        }

        return score;
    }

    private int EvaluateCouchRules()
    {
        int score = 0;

        if (headTransform == null)
        {
            return score;
        }

        Vector3 center = RoomManager.Instance.GetRoomCenter();
        Vector3 toCenter = (center - headTransform.position).normalized;
        Vector3 facing = -headTransform.forward;


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

        if (Physics.Raycast(headTransform.position, facing, out RaycastHit hitFront, 2f))
        {

            if (hitFront.collider.CompareTag("Door"))
            {
                FeedbackTextManager.Instance?.ShowMessage("Don't sit facing the door, bro.", Color.red);
                score -= 10;
            }
        }


        Vector3 behind = headTransform.position - facing;

        if (Physics.Raycast(behind, -facing, out RaycastHit hitBack, 4f))
        {

            if (hitBack.collider.CompareTag("Window"))
            {
                FeedbackTextManager.Instance?.ShowMessage("A Window behind your back? Never.", Color.red);
                score -= 6;
            }
            else if (hitBack.collider.CompareTag("Door"))
            {
                score -= 6;
            }
            else
            {
                score += 8;
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



        if (Physics.Raycast(headTransform.position, headTransform.forward, out RaycastHit hitBack, 4f))
        {
            if (hitBack.collider.CompareTag("Wall"))
            {
                score += 10;
            }
            else if (hitBack.collider.CompareTag("Window") || hitBack.collider.CompareTag("Door"))
            {
                score -= 6;
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

        if (Physics.Raycast(headTransform.position, -headTransform.forward, out RaycastHit hitFront, 2f))
        {

            if (hitFront.collider.CompareTag("Window") || hitFront.collider.CompareTag("Door") || hitFront.collider.CompareTag("Wall"))
            {
                score -= 6;
            }
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

        if (Physics.Raycast(origin, back, out RaycastHit hitBack, 2f))
        {

            if (hitBack.collider.CompareTag("Wall"))
            {
                score += 5;
            }
            else if (hitBack.collider.CompareTag("Door"))
            {
                score -= 5;
            }
        }

        if (Physics.Raycast(origin, forward, out RaycastHit hitFront, 2f))
        {

            if (hitFront.collider.CompareTag("Door"))
            {
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
     

        if (Physics.Raycast(origin, forward, out RaycastHit hitFront, 4f)){

            if (hitFront.collider.CompareTag("Wall"))
            {
                score += 3;
            }
            else if (hitFront.collider.CompareTag("Door"))
            {
                score -= 5;
            }
            else if (hitFront.collider.CompareTag("Window"))
            {
                score += 4;
            }
        }

        if (Physics.Raycast(origin, back, out RaycastHit hitBack, 6f))
        {

            if (hitBack.collider.CompareTag("Wall"))
            {
                score += 3;
            }
            else if (hitBack.collider.CompareTag("Door"))
            {
                score -= 5;
            }
            else if (hitBack.collider.CompareTag("Window"))
            {
                score -= 5;
            }
        }

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

        if (headTransform == null){
            return score;
        }

        Vector3 origin = headTransform.position;

        if (Physics.Raycast(origin, headTransform.forward, out RaycastHit hitFwd, 2f)){
            if (hitFwd.collider.CompareTag("Door")){
                score -= 5;
            }
        }

        if (Physics.Raycast(origin, -headTransform.forward, out RaycastHit hitBack, 2f)){
            if (hitBack.collider.CompareTag("Door")){
                score -= 5;
            }
        }

        if (Physics.Raycast(origin, headTransform.right, out RaycastHit hitRight, 2f)){
            if (hitRight.collider.CompareTag("Door")){
                score -= 5;
            }
        }

        if (Physics.Raycast(origin, -headTransform.right, out RaycastHit hitLeft, 2f)){
            if (hitLeft.collider.CompareTag("Door")){
                score -= 5;
            }
        }

    return score;
    }

}
