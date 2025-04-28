using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemBoxController : MonoBehaviour
{
    public GameObject buttonPrefab;     // 挂按钮Prefab（上面有ItemButtonController）
    public Transform contentParent;     // 放按钮的容器
    public int slotCount = 5;            // 固定5个空位
    public float buttonHeight = 100f;    // 每个按钮的高度
    public float spacing = 10f;          // 每个按钮的间距
    public float startYPosition = 0f;    // ✅ 新增：第一个按钮起始Y位置

    private List<GameObject> buttonInstances = new List<GameObject>();

    public void ShowItems(List<Sprite> icons, List<GameObject> prefabs)
    {
        ClearItems();

        for (int i = 0; i < slotCount; i++)
        {
            var buttonGO = Instantiate(buttonPrefab, contentParent, false);
            buttonInstances.Add(buttonGO);

            var rect = buttonGO.GetComponent<RectTransform>();

            // 强制归零属性
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            // ✅ 用startYPosition作为第一个按钮起点
            float yPos = startYPosition - i * (buttonHeight + spacing);
            rect.anchoredPosition = new Vector2(0f, yPos);

            var button = buttonGO.GetComponent<ItemButtonController>();
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

    public void ClearItems()
    {
        foreach (var go in buttonInstances)
        {
            Destroy(go);
        }
        buttonInstances.Clear();
    }
}
