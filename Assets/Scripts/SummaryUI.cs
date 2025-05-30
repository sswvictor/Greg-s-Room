using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SummaryUIController : MonoBehaviour
{
    public GameObject entryPrefab;         
    public Transform contentParent;       
    public TextMeshProUGUI totalText;      
    public Image gregFinalImage;


    void Start()
    {
        totalText.text = $"Total CHI Score: {GameSummary.totalCHI}";

        for (int i = 0; i < GameSummary.roomIndices.Count; i++)
        {
            var go = Instantiate(entryPrefab, contentParent);
            var texts = go.GetComponentsInChildren<TextMeshProUGUI>();
            var img = go.GetComponentInChildren<Image>();

            if (texts.Length >= 2)
            {
                texts[0].text = $"Room {GameSummary.roomIndices[i]}";
                texts[1].text = GameSummary.roomTexts[i];
            }

            if (img != null)
                img.sprite = GameSummary.roomIcons[i];


        }

        if (gregFinalImage != null && !string.IsNullOrEmpty(GameSummary.finalGregSpritePath))
        {
            var sprite = Resources.Load<Sprite>(GameSummary.finalGregSpritePath);
            if (sprite != null)
            {
                gregFinalImage.sprite = sprite;
            }
            else
            {
                Debug.LogWarning($"[SummaryUI] Fail to find：{GameSummary.finalGregSpritePath}");
            }
        }
    }
}
