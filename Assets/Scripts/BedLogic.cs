using UnityEngine;
using TMPro;

public class BedLogic : MonoBehaviour
{
    private FloorGrid floorGrid;

    private void Start()
    {
        floorGrid = Object.FindFirstObjectByType<FloorGrid>();
    }

    public void ShowFeedback()
    {
        if (FeedbackTextManager.Instance == null)
        {
            Debug.LogWarning("FeedbackTextManager.Instance è NULL");
            return;
        }

        Vector3 position = transform.position;

        if (floorGrid != null && floorGrid.IsPositionInRedZone(position))
        {
            FeedbackTextManager.Instance.ShowMessage("What the hell", Color.red);
        }
        else
        {
            FeedbackTextManager.Instance.ShowMessage("Damn bro, you nailed it", Color.white);
        }
    }
}
