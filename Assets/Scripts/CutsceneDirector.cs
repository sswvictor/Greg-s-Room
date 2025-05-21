using UnityEngine;
using UnityEngine.UI;

public class CutsceneDirector : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Image layerImage;
        public float parallaxSpeed = 1f;
        public Vector2 startPosition;
        public bool preserveAspect = true;
    }

    public ParallaxLayer[] parallaxLayers;
    public float transitionDuration = 1f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private void Start()
    {
        InitializeLayers();
    }

    private void InitializeLayers()
    {
        foreach (var layer in parallaxLayers)
        {
            if (layer.layerImage != null)
            {
                layer.startPosition = layer.layerImage.rectTransform.anchoredPosition;
                layer.layerImage.preserveAspect = layer.preserveAspect;
            }
        }
    }

    public void UpdateParallax(float normalizedPosition)
    {
        foreach (var layer in parallaxLayers)
        {
            if (layer.layerImage != null)
            {
                Vector2 targetPosition = layer.startPosition + new Vector2(normalizedPosition * layer.parallaxSpeed, 0);
                layer.layerImage.rectTransform.anchoredPosition = targetPosition;
            }
        }
    }

    public void TransitionToRoom(float targetPosition)
    {
        StartCoroutine(TransitionCoroutine(targetPosition));
    }

    private System.Collections.IEnumerator TransitionCoroutine(float targetPosition)
    {
        float startTime = Time.time;
        float currentPosition = 0f;

        while (Time.time - startTime < transitionDuration)
        {
            float normalizedTime = (Time.time - startTime) / transitionDuration;
            float curveValue = transitionCurve.Evaluate(normalizedTime);
            currentPosition = Mathf.Lerp(0, targetPosition, curveValue);
            
            UpdateParallax(currentPosition);
            yield return null;
        }

        UpdateParallax(targetPosition);
    }
}
