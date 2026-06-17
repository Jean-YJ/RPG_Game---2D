using System.Collections;
using UnityEngine;

public class VFX_AutoController : MonoBehaviour
{
    private SpriteRenderer sr;
    [Header("Auto Destory")]
    [SerializeField] private bool autoDestory = true;
    [SerializeField] private float destoryDelay = 10.0f;
    [Header("Random Offset")]
    [SerializeField] private bool randomOffset = true;
    [SerializeField] private float xMinOffset = -0.3f;
    [SerializeField] private float xMaxOffset = 0.3f;
    [Space]
    [SerializeField] private float yMinOffset = -0.3f;
    [SerializeField] private float yMaxOffset = -0.3f;
    [Header("Random Rotation")]
    [SerializeField] private bool randomRotation = true;
    [SerializeField] private float minRotation = -10.0f;
    [SerializeField] private float maxRotation = 10.0f;

    [Header("Fade Effect")]
    [SerializeField] private bool canFade = false;
    [SerializeField] private float fadeSpeed = 1.0f;

    void Awake()
    {
        this.sr = this.GetComponentInChildren<SpriteRenderer>();
    }

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        if (this.canFade)
            StartCoroutine(FadeEffCor());

        ApplyRandomOffset();
        ApplyRandomRotation();

        if (this.autoDestory)
        {
            Destroy(this.gameObject, this.destoryDelay);
        }
    }

    private void ApplyRandomOffset()
    {
        if (!this.randomOffset)
            return;

        float xOffset = Random.Range(this.xMinOffset, this.xMaxOffset);
        float yOffset = Random.Range(this.yMinOffset, this.yMaxOffset);

        this.transform.position = this.transform.position + new Vector3(xOffset, yOffset);
    }
    private void ApplyRandomRotation()
    {
        if (!this.randomRotation)
            return;

        float zRotation = Random.Range(this.minRotation, this.maxRotation);
        this.transform.Rotate(0, 0, zRotation);
    }

    // 每帧让sr的透明度提高直到消失
    private IEnumerator FadeEffCor()
    {
        Color targetColor = Color.white;

        while (targetColor.a > 0)
        {
            targetColor.a -= (this.fadeSpeed * Time.deltaTime);
            this.sr.color = targetColor;

            yield return null;
        }

        this.sr.color = targetColor;
    }

}
