using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SummaryUIController : MonoBehaviour
{
    public GameObject entryPrefab;         // 每一行展示用的 prefab（应包含 Text + Image）
    public Transform contentParent;        // 滚动列表容器
    public TextMeshProUGUI totalText;      // CHI 总分文
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
                Debug.Log($"[SummaryUI ✅] 显示最终状态图：{GameSummary.finalGregSpritePath}");
            }
            else
            {
                Debug.LogWarning($"[SummaryUI ❌] 找不到 GREG 图片：{GameSummary.finalGregSpritePath}");
            }
        }
    }
}
