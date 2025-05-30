using UnityEngine;
using UnityEngine.UI;

public class CutsceneDirector : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Image layerImage;
        public Vector2 parallaxOffset = new Vector2(200f, 50f); 

        public Vector2 startPosition;
        public bool preserveAspect = true;
        public float zPosition = 0f;
        public Vector2 scale = Vector2.one;
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
                var rectTransform = layer.layerImage.rectTransform;
                layer.layerImage.preserveAspect = layer.preserveAspect;
                
                rectTransform.localScale = layer.scale;
                Vector3 pos = rectTransform.localPosition;
                pos.z = layer.zPosition;
                rectTransform.localPosition = pos;
            }
        }
    }

    public void UpdateParallax(float normalizedPosition)
    {
        foreach (var layer in parallaxLayers)
        {
            if (layer.layerImage != null)
            {
                Vector2 offset = layer.parallaxOffset * normalizedPosition;
                Vector2 targetPosition = layer.startPosition + offset;

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
