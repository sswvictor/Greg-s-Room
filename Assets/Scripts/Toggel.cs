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
            return;
        }

        spriteA = Resources.Load<Sprite>(imageAPath);
        spriteB = Resources.Load<Sprite>(imageBPath);

        if (spriteA == null || spriteB == null)
        {
            return;
        }

        img.sprite = spriteA;

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
