using UnityEngine;
using UnityEngine.UI;

public class PauseImageToggler : MonoBehaviour
{
    public string imageAPath = "PauseImages/PauseImage_A";
    public string imageBPath = "PauseImages/PauseImage_B";

    private Image img;
    private Sprite spriteA;
    private Sprite spriteB;
    private bool showingA = true;

    void Start()
    {
        img = GetComponent<Image>();
        if (img == null)
        {
            Debug.LogError("[PauseImageToggler] ❌ 找不到 Image 组件！");
            return;
        }

        spriteA = Resources.Load<Sprite>(imageAPath);
        spriteB = Resources.Load<Sprite>(imageBPath);

        if (spriteA == null || spriteB == null)
        {
            Debug.LogError("[PauseImageToggler] ❌ 图片资源加载失败！");
            return;
        }

        img.sprite = spriteA;

        // 自动绑定点击事件
        var btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(ToggleImage);
    }

    void ToggleImage()
    {
        showingA = !showingA;
        img.sprite = showingA ? spriteA : spriteB;
    }
}
