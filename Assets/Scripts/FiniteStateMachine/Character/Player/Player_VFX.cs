using System.Collections;
using UnityEngine;

public class Player_VFX : Entity_VFX
{
    [Header("Echo Fade Detail")]
    [Range(.01f, .2f)]
    [SerializeField] private float imageEchoInterval = 0.05f; //创建虚影的间隔
    [SerializeField] private GameObject imageEchoPrefab; //虚影预设体
    private Coroutine imageEchoCor;

    public void PlayImageEchoEff(float duration)
    {
        if (this.imageEchoCor != null)
            StopCoroutine(this.imageEchoCor);

        this.imageEchoCor = StartCoroutine(ImageEchoEffCor(duration));
    }

    //Player冲刺后，在冲刺中间歇性的创建虚影
    private IEnumerator ImageEchoEffCor(float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            CreateEchoImage();

            yield return new WaitForSeconds(this.imageEchoInterval);
            timer += this.imageEchoInterval;
        }
    }

    // 将Player当前的sr实例化 作为FadeEff的基础
    private void CreateEchoImage()
    {
        GameObject echoImg = Instantiate(this.imageEchoPrefab, this.transform.position, this.transform.rotation);
        echoImg.GetComponentInChildren<SpriteRenderer>().sprite = this.sr.sprite;
    }

    public void CreateVfxBy(GameObject vfxPrefab,Transform targetTransform)
    {
        Instantiate(vfxPrefab, targetTransform.position, Quaternion.identity);
    }
}
