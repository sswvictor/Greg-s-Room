using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SummarySceneController : MonoBehaviour
{
    public Button continueButton;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI chiScoreText;
    public Image profileImage;

    private void Start()
    {
        // ✅ Popola la UI con i dati salvati
        if (SummaryData.Instance != null)
        {
            characterNameText.text = SummaryData.Instance.characterName;
            chiScoreText.text = "CHI Score: " + SummaryData.Instance.totalCHI;
            profileImage.sprite = SummaryData.Instance.characterSprite;
        }

        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(() =>
        {
            Debug.Log("➡️ Loading MainScene3D...");
            SceneManager.LoadScene("MainScene3D"); // ⚠️ assicurati che il nome della scena sia corretto!
        });
    }
}
