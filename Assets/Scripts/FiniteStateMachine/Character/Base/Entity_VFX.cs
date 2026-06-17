using System.Collections;
using UnityEngine;

public class Entity_VFX : MonoBehaviour
{
    protected SpriteRenderer sr;
    private Entity entity;

    [Header("On Taking Damage Visual Effect")]
    [SerializeField] private Material onDamagedMaterial;
    [SerializeField] private Material originalMaterial;
    [SerializeField] private float duration = 0.15f;
    private Coroutine OnDamagedVFXCoroutine;

    [Header("On Doing Damage Visual Effect")]
    [SerializeField] private GameObject onHitVfxPrefab;
    [SerializeField] private GameObject onCritHitVfxPrefab;
    [SerializeField] private Color hitVfxColor = Color.white;

    [Header("Elemental Damage Visual Effect")]
    [SerializeField] private Color iceDamageColor = Color.cyan;
    [SerializeField] private Color burnDamageColor = Color.lightGoldenRodYellow;
    [SerializeField] private Color electrifyDamageColor = Color.gold;
    private Color originHitVfxColor;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    protected virtual void Awake()
    {
        this.sr = this.GetComponentInChildren<SpriteRenderer>();
        this.originalMaterial = this.sr.material;
        this.entity = this.GetComponent<Entity>();
        this.originHitVfxColor = this.hitVfxColor;
    }

    public void PlayOnDamagedVFX()
    {
        if (this.OnDamagedVFXCoroutine != null)
            StopCoroutine(this.OnDamagedVFXCoroutine);

        this.OnDamagedVFXCoroutine = StartCoroutine(OnDamagedVFXCor());
    }

    private IEnumerator OnDamagedVFXCor()
    {
        this.sr.material = this.onDamagedMaterial;
        yield return new WaitForSeconds(this.duration);
        this.sr.material = this.originalMaterial;
    }

    /// <summary>
    /// 创建打击特效
    /// </summary>
    /// <param name="target">目标实体</param>
    /// <param name="isCrit">本次攻击是否暴击</param>
    public void CreateOnHitVfx(Transform target, bool isCrit, E_ElementType elementType)
    {
        // 根据是否暴击选择不同的特效预制体
        GameObject hitVfxPrefab = isCrit ? this.onCritHitVfxPrefab : this.onHitVfxPrefab;
        GameObject hitVfx = Instantiate(hitVfxPrefab, target.position, Quaternion.identity);

        // 设置特效颜色，颜色由UpdateOnHitVfxColor方法根据元素类型更新
        // 所以要改变颜色时一定要先调用UpdateOnHitVfxColor方法更新颜色，再调用CreateOnHitVfx方法创建特效
        // 否则都会是默认的特效颜色，无法体现元素伤害的区别
        // 想要采用带元素颜色的 击打特效 启用下一行代码后即可。 现在采用原色的设计
        // hitVfx.GetComponentInChildren<SpriteRenderer>().color = GetOnHitVfxColor(elementType);

        // 由于暴击特效的形状并非水平对称，不进行翻转视觉效果会不自然，
        // 所以当攻击者朝向左边且是暴击时，将特效水平翻转
        if (this.entity.faceDir == -1 && isCrit)
            hitVfx.transform.Rotate(0, 180, 0);

    }
    /// <summary>
    /// 根据元素类型更新打击特效的颜色，例如冰元素伤害可以是青色，火元素伤害可以是红色等
    /// 这样就能通过打击特效的颜色来区分不同的元素伤害了
    /// 注意：要改变颜色时一定要先调用这个方法更新颜色，再调用CreateOnHitVfx方法创建特效，因为CreateOnHitVfx方法会根据当前的hitVfxColor来设置生成的特效的颜色
    /// </summary>
    /// <param name="elementType">攻击的元素类型</param>
    public Color GetOnHitVfxColor(E_ElementType elementType)
    {
        switch (elementType)
        {
            case E_ElementType.Ice:
                return this.iceDamageColor;
            case E_ElementType.Fire:
                return this.burnDamageColor;
            case E_ElementType.Lightning:
                return this.electrifyDamageColor;
            default:
                return this.originHitVfxColor;
        }
    }

    /// <summary>
    /// 播放特殊状态效果的视觉效果，例如改变颜色、播放粒子效果等
    /// </summary>
    /// <param name="duration">状态持续时间</param>
    /// <param name="elementType">状态效果的元素类型</param>
    public void PlayStatusEffectVFX(float duration, E_ElementType elementType)
    {
        switch (elementType)
        {
            case E_ElementType.Ice:
                StartCoroutine(PlayStatusEffectVFXCor(duration, this.iceDamageColor));
                break;
            case E_ElementType.Fire:
                StartCoroutine(PlayStatusEffectVFXCor(duration, this.burnDamageColor));
                break;
            case E_ElementType.Lightning:
                StartCoroutine(PlayStatusEffectVFXCor(duration, this.electrifyDamageColor));
                break;
            default:
                break;
        }

    }
    /// <summary>
    /// 实现状态效果的视觉效果，例如改变颜色、播放粒子效果等
    /// </summary>
    /// <param name="duration">状态持续时间</param>
    /// <param name="vfxColor">状态颜色效果</param>
    /// <returns></returns>
    private IEnumerator PlayStatusEffectVFXCor(float duration, Color vfxColor)
    {
        // 在这里实现状态效果的视觉效果，例如改变颜色、播放粒子效果等
        float tickInterval = 0.25f; // 每0.5秒更新一次视觉效果
        float timePassed = 0f; // 记录在该状态下已经过去的时间
        bool toggle = false; // 用于交替改变颜色

        Color lightColor = vfxColor * 1.5f; // 计算更亮的颜色
        Color darkColor = vfxColor * 0.5f; // 计算更暗的颜色

        while (timePassed < duration)
        {
            // 交替改变颜色
            this.sr.color = toggle ? lightColor : darkColor;
            toggle = !toggle;

            yield return new WaitForSeconds(tickInterval);

            timePassed += tickInterval;
        }

        this.sr.color = Color.white; // 恢复原始颜色
    }

    public void StopAllVfx()
    {
        StopAllCoroutines();
        this.sr.material = this.originalMaterial;
        this.sr.color = Color.white;
    }
}
