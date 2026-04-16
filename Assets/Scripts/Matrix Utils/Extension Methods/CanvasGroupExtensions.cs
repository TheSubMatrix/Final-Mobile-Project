using System.Collections;
using UnityEngine;
public static class CanvasGroupExtensions
{
    public static IEnumerator FadeToOpacity(this CanvasGroup groupToFade, float desiredAlpha, float duration)
    {
        float elapsed = 0;
        float startAlpha = groupToFade.alpha;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            groupToFade.alpha = Mathf.Lerp(startAlpha, desiredAlpha, elapsed / duration);
            yield return null;
        }
    }
}
