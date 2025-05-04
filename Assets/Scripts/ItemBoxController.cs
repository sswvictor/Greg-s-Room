using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemBoxController : MonoBehaviour
{
    public GameObject buttonPrefab;     // 挂按钮Prefab（上面有ItemSlotController）
    public Transform contentParent;     // 放按钮的容器
    public int slotCount = 5;           // 固定空位
    public float buttonHeight = 100f;   // 每个按钮的高度（用于 ShowItems）
    public float spacing = 10f;         // 每个按钮的间距
    public float startYPosition = 0f;   // 第一个按钮起始Y位置

    private List<GameObject> buttonInstances = new List<GameObject>();

    // ✅ 用于旧结构，动态设置图标和模型
    public void ShowItems(List<Sprite> icons, List<GameObject> prefabs)
    {
        ClearItems();

        for (int i = 0; i < slotCount; i++)
        {
            var buttonGO = Instantiate(buttonPrefab, contentParent, false);
            buttonInstances.Add(buttonGO);

            var rect = buttonGO.GetComponent<RectTransform>();

            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(300f, 300f);

            float yPos = startYPosition - i * (buttonHeight + spacing);
            rect.anchoredPosition = new Vector2(0f, yPos);

            var button = buttonGO.GetComponent<ItemSlotController>();
            var image = buttonGO.GetComponent<Image>();

            if (i < icons.Count && i < prefabs.Count)
            {
                image.sprite = icons[i];
                button.modelPrefab = prefabs[i];
                image.enabled = true;
            }
            else
            {
                image.sprite = null;
                button.modelPrefab = null;
                image.enabled = false;
            }
        }
    }

    // ✅ 用于新结构：直接传入按钮 prefab 列表，每个按钮内部配置完整
    public void ShowButtons(List<GameObject> buttonPrefabs)
    {
        ClearItems();

        for (int i = 0; i < buttonPrefabs.Count; i++)
        {
            var prefab = buttonPrefabs[i];
            if (prefab == null) continue;

            var buttonGO = Instantiate(prefab, contentParent, false);
            buttonInstances.Add(buttonGO);

            var rect = buttonGO.GetComponent<RectTransform>();

            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(300f, 300f);  // ✅ 修复按钮压扁：设定固定尺寸

            float yPos = startYPosition - i * (buttonHeight + spacing);
            rect.anchoredPosition = new Vector2(0f, yPos);
        }
    }

    public void ClearItems()
    {
        foreach (var go in buttonInstances)
        {
            Destroy(go);
        }
        buttonInstances.Clear();
    }
}
