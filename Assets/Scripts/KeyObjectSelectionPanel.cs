using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class KeyObjectSelectionPanel : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform contentParent;

    private CanvasGroup canvasGroup;
    private VerticalLayoutGroup layoutGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        // 强制配置 LayoutGroup（只需一次）
        layoutGroup = contentParent.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
            layoutGroup = contentParent.gameObject.AddComponent<VerticalLayoutGroup>();

        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.spacing = 20f;
        layoutGroup.padding = new RectOffset(40, 40, 20, 20); // 左右40，上下60
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;

        // 关闭时隐藏
        HidePanel();
    }

    public void Show(List<string> keyNames)
    {
        ClearButtons();

        foreach (var name in keyNames)
        {
            GameObject btn = Instantiate(buttonPrefab, contentParent);

            // 设置文本
            var label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = name.Replace("_Prefab", "");

            // 强制按钮尺寸
            var rect = btn.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(500f, 300f);

            // 强制设置 LayoutElement
            var layout = btn.GetComponent<LayoutElement>();
            if (layout == null) layout = btn.AddComponent<LayoutElement>();
            layout.preferredWidth = 500f;
            layout.preferredHeight = 300f;
            layout.flexibleWidth = 0;
            layout.flexibleHeight = 0;

            // 添加点击逻辑
            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log($"[KEY SELECT ✅] 玩家选择：{name}");
                PlayerPrefs.SetString("next_key_object", name);
                HidePanel();
                RoomManager.Instance.LoadNextRoom();
            });
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void HidePanel()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void ClearButtons()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);
    }
}
