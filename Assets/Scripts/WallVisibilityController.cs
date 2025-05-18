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

    // 每面墙独立的协程引用（防止互相打断）
    private Dictionary<GameObject, Coroutine> activeCoroutines = new();

    public void ShowMainView()  // ✅ 显示 0/1，压缩 2/3
    {
        AnimateWallToY(wall0, visibleScaleY);
        AnimateWallToY(wall1, visibleScaleY);
        AnimateWallToY(wall2, hiddenScaleY);
        AnimateWallToY(wall3, hiddenScaleY);
    }

    public void ShowSideView()  // ✅ 显示 0/3，压缩 1/2
    {
        AnimateWallToY(wall0, visibleScaleY);
        AnimateWallToY(wall1, hiddenScaleY);
        AnimateWallToY(wall2, hiddenScaleY);
        AnimateWallToY(wall3, visibleScaleY);
    }

    private void AnimateWallToY(GameObject wall, float targetY)
    {
        if (wall == null) return;

        // 停掉这面墙上已有协程
        if (activeCoroutines.TryGetValue(wall, out Coroutine existing))
            StopCoroutine(existing);

        Coroutine routine = StartCoroutine(AnimateScaleCoroutine(wall.transform, targetY));
        activeCoroutines[wall] = routine;
    }

    // private IEnumerator AnimateScaleCoroutine(Transform wallTransform, float targetY)
    // {
    //     Vector3 originalScale = wallTransform.localScale;
    //     Vector3 originalPos = wallTransform.localPosition;
    //     float startY = originalScale.y;
    //     float timeElapsed = 0f;

    //     // ✅ 用 Collider 判断 pivot，并计算偏移量（以便“贴地”）
    //     bool isCenterPivot = false;
    //     float pivotOffsetY = 0f;

    //     Collider col = wallTransform.GetComponent<Collider>();
    //     if (col != null)
    //     {
    //         Bounds bounds = col.bounds;
    //         Vector3 localCenter = wallTransform.InverseTransformPoint(bounds.center);
    //         isCenterPivot = Mathf.Abs(localCenter.y) < 1e-3f;

    //         // ✅ 用世界空间的半高作为贴地偏移量
    //         pivotOffsetY = 6f;

    //         Debug.Log($"[PIVOT DEBUG ✅] wall = {wallTransform.name}, isCenterPivot = {isCenterPivot}, extents.y = {pivotOffsetY:F4}");
    //     }
    //     else
    //     {
    //         Debug.LogWarning($"[PIVOT DEBUG ⚠️] wall = {wallTransform.name} has no collider!");
    //     }

    //     bool toHidden = Mathf.Approximately(targetY, hiddenScaleY);
    //     bool needsCompensation = isCenterPivot;

    //     Vector3 fromPos = originalPos;
    //     Vector3 toPos = originalPos;

    //     if (needsCompensation)
    //     {
    //         if (toHidden)
    //         {
    //             // ✅ 中心 pivot → 隐藏：将中心向下移半高
    //             toPos = originalPos + new Vector3(0, -pivotOffsetY, 0);
    //         }
    //         else
    //         {
    //             // ✅ 中心 pivot → 显示：从贴地状态抬上来
    //             fromPos = originalPos + new Vector3(0, +pivotOffsetY, 0);
    //         }
    //     }

    //     while (timeElapsed < transitionDuration)
    //     {
    //         float t = timeElapsed / transitionDuration;
    //         float newY = Mathf.Lerp(startY, targetY, t);
    //         Vector3 newPos = Vector3.Lerp(fromPos, toPos, t);

    //         wallTransform.localScale = new Vector3(originalScale.x, newY, originalScale.z);
    //         wallTransform.localPosition = newPos;

    //         timeElapsed += Time.deltaTime;
    //         yield return null;
    //     }

    //     wallTransform.localScale = new Vector3(originalScale.x, targetY, originalScale.z);
    //     wallTransform.localPosition = toPos;
    // }
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
