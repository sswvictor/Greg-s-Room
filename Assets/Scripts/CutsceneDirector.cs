using UnityEngine;
using UnityEngine.UI;
using System;

public class CutsceneDirector : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Image layerImage;
        [Tooltip("Controls how much this layer moves relative to others")]
        public float parallaxMultiplier = 1f;
        [Tooltip("Starting position in Canvas space")]
        public Vector2 startAnchoredPos;
        [Tooltip("Ending position in Canvas space")]
        public Vector2 endAnchoredPos;
        public bool preserveAspect = true;
        public Vector2 scale = Vector2.one;
        [Tooltip("Higher numbers draw on top")]
        public int sortingOrder;
    }

    public ParallaxLayer[] parallaxLayers;
    public float transitionDuration = 1f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public event Action OnCutsceneComplete;

    private void Start()
    {
        InitializeLayers();
        // Auto-play on start
        PlayParallax();
    }

    private void InitializeLayers()
    {
        foreach (var layer in parallaxLayers)
        {
            if (layer.layerImage != null)
            {
                // Get the parent Canvas (should be set up in scene)
                var canvas = layer.layerImage.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = layer.sortingOrder;
                }
                else
                {
                    Debug.LogError($"Layer {layer.layerImage.name} must be child of a Canvas!");
                    continue;
                }

                // Setup image
                var rectTransform = layer.layerImage.rectTransform;
                rectTransform.localScale = layer.scale;
                layer.layerImage.preserveAspect = layer.preserveAspect;

                // Store current position if start position not set
                if (layer.startAnchoredPos == Vector2.zero)
                    layer.startAnchoredPos = rectTransform.anchoredPosition;

                rectTransform.anchoredPosition = layer.startAnchoredPos;
            }
        }
    }

    public void UpdateParallax(float progress)
    {
        float curveValue = transitionCurve.Evaluate(progress);

        foreach (var layer in parallaxLayers)
        {
            if (layer.layerImage != null)
            {
                Vector2 movement = layer.endAnchoredPos - layer.startAnchoredPos;
                Vector2 scaledMovement = movement * layer.parallaxMultiplier;
                Vector2 targetPos = layer.startAnchoredPos + (scaledMovement * curveValue);

                layer.layerImage.rectTransform.anchoredPosition = targetPos;
            }
        }
    }

    // **This is the public, parameterless method you can now hook to your Button**
    public void PlayParallax()
    {
        TransitionToRoom(1f);
        Debug.Log("Parallax Play Test");
    }

    public void TransitionToRoom(float targetProgress = 1f)
    {
        StartCoroutine(TransitionCoroutine(targetProgress));
    }

    private System.Collections.IEnumerator TransitionCoroutine(float targetProgress)
    {
        float startTime = Time.time;
        float progress = 0f;

        while (Time.time - startTime < transitionDuration)
        {
            progress = (Time.time - startTime) / transitionDuration;
            UpdateParallax(progress);
            yield return null;
        }

        UpdateParallax(targetProgress);
        OnCutsceneComplete?.Invoke();
        
        // Notify SceneController to unload cutscene
        SceneController.Instance.UnloadCutscene();
    }
}