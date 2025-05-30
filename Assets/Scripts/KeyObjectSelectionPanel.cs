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

        layoutGroup = contentParent.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
            layoutGroup = contentParent.gameObject.AddComponent<VerticalLayoutGroup>();

        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.spacing = 20f;
        layoutGroup.padding = new RectOffset(40, 40, 20, 20); 
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;

        HidePanel();
    }

    public void Show(List<string> keyNames)
    {
        ClearButtons();

        foreach (var name in keyNames)
        {
            GameObject btn = Instantiate(buttonPrefab, contentParent);

            string displayName = name.Replace("_Prefab", "");

            var label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = displayName;


            var image = btn.transform.Find("Image")?.GetComponent<Image>();
            if (image != null)
            {
                string path = $"Life Choice Objects/{displayName}";
                var sprite = Resources.Load<Sprite>(path);
                if (sprite != null)
                {
                    image.sprite = sprite;
                    image.enabled = true;
                }
                else
                {
                    image.enabled = false;
                }
            }

            var rect = btn.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(500f, 300f);

            var layout = btn.GetComponent<LayoutElement>();
            if (layout == null) layout = btn.AddComponent<LayoutElement>();
            layout.preferredWidth = 500f;
            layout.preferredHeight = 300f;
            layout.flexibleWidth = 0;
            layout.flexibleHeight = 0;

            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
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
