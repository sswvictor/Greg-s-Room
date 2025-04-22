using UnityEngine;
using TMPro;
using System.Collections;

public class FeedbackTextManager : MonoBehaviour
{
    public static FeedbackTextManager Instance;
    public TextMeshProUGUI feedbackText;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowMessage(string message, Color color)
    {
        StopAllCoroutines(); // Ferma eventuali messaggi precedenti

        feedbackText.text = message;
        feedbackText.color = color;
        feedbackText.gameObject.SetActive(true);
        StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        feedbackText.gameObject.SetActive(false);
    }
}
