using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SummaryUIController : MonoBehaviour
{
    public GameObject entryPrefab;         // 每一行展示用的 prefab（应包含 Text + Image）
    public Transform contentParent;        // 滚动列表容器
    public TextMeshProUGUI totalText;      // CHI 总分文本

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
    }
}
