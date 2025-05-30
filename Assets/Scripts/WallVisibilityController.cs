using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WallVisibilityController : MonoBehaviour
{
    public GameObject wall0;
    public GameObject wall1;
    public GameObject wall2;
    public GameObject wall3;

    [Header("可见与隐藏时的Y缩放值")]
    public float visibleScaleY = 1.0f;
    public float hiddenScaleY = 0.1f;

    [Header("动画时长（秒）")]
    public float transitionDuration = 0.25f;

    private Dictionary<GameObject, Coroutine> activeCoroutines = new();

    public void ShowMainView()  
    {
        AnimateWallToY(wall0, visibleScaleY);
        AnimateWallToY(wall1, visibleScaleY);
        AnimateWallToY(wall2, hiddenScaleY);
        AnimateWallToY(wall3, hiddenScaleY);
    }

    public void ShowSideView()  
    {
        AnimateWallToY(wall0, visibleScaleY);
        AnimateWallToY(wall1, hiddenScaleY);
        AnimateWallToY(wall2, hiddenScaleY);
        AnimateWallToY(wall3, visibleScaleY);
    }

    private void AnimateWallToY(GameObject wall, float targetY)
    {
        if (wall == null) return;

        if (activeCoroutines.TryGetValue(wall, out Coroutine existing))
            StopCoroutine(existing);

        Coroutine routine = StartCoroutine(AnimateScaleCoroutine(wall.transform, targetY));
        activeCoroutines[wall] = routine;
      
    }

    
    private IEnumerator AnimateScaleCoroutine(Transform wallTransform, float targetY)
    {
        Vector3 originalScale = wallTransform.localScale;
        float startY = originalScale.y;
        float timeElapsed = 0f;

        while (timeElapsed < transitionDuration)
        {
            float t = timeElapsed / transitionDuration;
            float newY = Mathf.Lerp(startY, targetY, t);
            wallTransform.localScale = new Vector3(originalScale.x, newY, originalScale.z);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        wallTransform.localScale = new Vector3(originalScale.x, targetY, originalScale.z);
    }


}
