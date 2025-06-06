using UnityEngine;
using UnityEngine.UI;

public class ChiFillBar : MonoBehaviour
{
    [Header("Assign the Image with Fill type set to Horizontal.")]
    [SerializeField] private Image chiFillImage;

    public void UpdateBar(float current, float max)
    {
        if (chiFillImage == null)
        {
            return;
        }

        chiFillImage.fillAmount = Mathf.Clamp01(current / max);
    }
}
