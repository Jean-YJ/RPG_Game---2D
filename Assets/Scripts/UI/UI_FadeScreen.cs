using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_FadeScreen : MonoBehaviour
{
    private Image fadeImage;
    public Coroutine fadeEffCor;

    void Awake()
    {
        this.fadeImage = this.GetComponent<Image>();
        this.fadeImage.color = new Color(0, 0, 0, 1);
    }

    public void FadeIn(float duration = 1) //纯黑 -> 内容
    {
        this.fadeImage.color = new Color(0, 0, 0, 1);
        FadeEffect(0, duration);
    }

    public void FadeOut(float duration = 1) //内容 -> 纯黑
    {
        this.fadeImage.color = new Color(0, 0, 0, 0);
        FadeEffect(1, duration);
    }

    private void FadeEffect(float targetAlpha, float duration)
    {
        if (this.fadeEffCor != null)
            StopCoroutine(this.fadeEffCor);

        this.fadeEffCor = StartCoroutine(FadeEffectCoroutine(targetAlpha, duration));
    }

    private IEnumerator FadeEffectCoroutine(float targetAlpha, float duration)
    {
        float initialAlpha = this.fadeImage.color.a;
        float timer = 0f;
        Color fadeImageColor = this.fadeImage.color;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            fadeImageColor.a = Mathf.Lerp(initialAlpha, targetAlpha, timer / duration);
            this.fadeImage.color = fadeImageColor;
            yield return null;
        }
    }
}
