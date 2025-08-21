using System.Collections;
using DG.Tweening;
using UnityEngine;

public class ScreenFadeManager : MonoBehaviour
{
    public float startAlpha = 1f;
    public float endAlpha = 0f;
    public float waitTime = 0f;
    public float fadeDuration = 1f;

    private void Start()
    {
        GetComponent<CanvasGroup>().alpha = startAlpha;
        StartCoroutine(FadeRoutineFNC());
    }

    IEnumerator FadeRoutineFNC()
    {
        yield return new WaitForSeconds(waitTime);
        GetComponent<CanvasGroup>().DOFade(endAlpha, fadeDuration);
    }
}
