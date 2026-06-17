using System.Collections;
using UnityEngine;

public class Entity_StatusHandler : MonoBehaviour
{
    private E_ElementType currentEffect = E_ElementType.None;
    private Entity entity;
    private Entity_VFX ev;
    private Entity_Stats es;
    private Entity_Health eh;
    [Header("Shock Effect Detail")]
    [SerializeField] private GameObject shockEffectPrefab;
    [SerializeField] private float currentCharge = 0f;
    [SerializeField] private float maximumCharge = 1f; // 满电阈
    private Coroutine shockEffectCoroutine;


    void Awake()
    {
        this.entity = this.GetComponent<Entity>();
        this.ev = this.GetComponent<Entity_VFX>();
        this.es = this.GetComponent<Entity_Stats>();
        this.eh = this.GetComponent<Entity_Health>();
    }

    /// <summary>
    /// 检查该实体是否可以应用新的元素状态
    /// </summary>
    /// <param name="elementType">要应用的元素状态类型</param>
    /// <returns></returns>
    public bool CanBeApplied(E_ElementType elementType)
    {
        // 雷电状态有所不同，需要在当前状态为雷电时仍然应用新的雷电状态，以刷新状态持续时间并累积进度条
        if (elementType == E_ElementType.Lightning && elementType == E_ElementType.Lightning)
        {
            return true;
        }

        // 如果当前没有元素状态，或者当前元素状态与要应用的元素状态不同，则可以应用新的元素状态
        return this.currentEffect == E_ElementType.None || this.currentEffect != elementType;
    }

    public void ApplyStatusEffect(E_ElementType elementType, ElementEffectData effectData)
    {
        if (elementType == E_ElementType.Fire)
            ApplyBurnStatus(effectData.burnDuration, effectData.burnDamage);
        if (elementType == E_ElementType.Ice)
            ApplyChillStatus(effectData.chillDuration, effectData.chillSlowMulitplier);
        if (elementType == E_ElementType.Lightning)
            ApplyShockStatus(effectData.shockDuration, effectData.shockDamage, effectData.shockCharge);
    }


    /// <summary>
    /// 应用冰冻状态效果，降低移动速度并播放状态效果视觉效果
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="slowMultiplier"></param>
    public void ApplyChillStatus(float duration, float slowMultiplier)
    {
        float resistance = this.es.GetElementalResistance(E_ElementType.Ice);
        float actualDuration = duration * (1 - resistance); // 根据抗性调整状态持续时间

        // 播放状态效果视觉效果
        StartCoroutine(ChillStatusCor(actualDuration, slowMultiplier));
    }

    // 切换实体的当前状态为寒冷并 播放状态效果视觉效果
    private IEnumerator ChillStatusCor(float duration, float slowMultiplier)
    {
        // 应用冰冻状态效果
        this.entity.EntitySlowDown(duration, slowMultiplier);
        this.currentEffect = E_ElementType.Ice;
        this.ev.PlayStatusEffectVFX(duration, E_ElementType.Ice);

        yield return new WaitForSeconds(duration);

        // 状态效果结束，重置当前状态
        // 视觉效果的恢复由 this.ev.PlayStatusEffectVFX内部的协程控制，这里不需要再次调用视觉效果的恢复
        this.currentEffect = E_ElementType.None;
    }

    public void ApplyBurnStatus(float duration, float totalDamage)
    {
        float resistance = this.es.GetElementalResistance(E_ElementType.Fire);
        float actualDamage = totalDamage * (1 - resistance); // 根据抗性调整总伤害

        StartCoroutine(BurnStatusCor(duration, actualDamage));
    }
    private IEnumerator BurnStatusCor(float duration, float totalDamage)
    {
        // 切换状态为燃烧并播放状态效果视觉效果
        this.currentEffect = E_ElementType.Fire;
        this.ev.PlayStatusEffectVFX(duration, E_ElementType.Fire);

        int ticksPerSecond = 2; // 每秒造成2次伤害
        int totalTicks = Mathf.RoundToInt(duration * ticksPerSecond); // 计算总的伤害次数
        float damagePerTick = totalDamage / totalTicks; // 计算每次伤害
        float tickInterval = 1f / ticksPerSecond; // 计算每次伤害的时间间隔

        for (int i = 0; i < totalTicks; i++)
        {
            this.eh.ReduceHealth(damagePerTick); // 造成伤害

            yield return new WaitForSeconds(tickInterval);
        }

        // 状态效果结束，重置当前状态
        this.currentEffect = E_ElementType.None;
    }

    public void ApplyShockStatus(float duration, float damage, float chargeAmount)
    {
        // this.currentEffect = E_ElementType.Lightning;
        float resistance = this.es.GetElementalResistance(E_ElementType.Lightning);
        float actualCharge = chargeAmount * (1 - resistance); // 根据抗性调整充能量
        this.currentCharge += actualCharge;

        // 判断累加的充能是否达到触发雷电打击的阈值
        if (this.currentCharge >= this.maximumCharge)
        {
            // 触发过载效果，造成一次强力的雷电伤害. 该伤害不受抗性的影响
            DoLightningStrike(damage);

            // 清空状态效果
            StopShockEffect();

            return;
        }

        // 没有达到阈值，重启状态协程刷新状态持续时间
        if (this.shockEffectCoroutine != null)
            StopCoroutine(this.shockEffectCoroutine);
        this.shockEffectCoroutine = StartCoroutine(ShockEffectCor(duration));
    }

    /// <summary>
    /// 在这里实现雷电打击的具体效果
    /// </summary>
    /// <param name="damage">雷电打击的伤害值</param>
    private void DoLightningStrike(float damage)
    {
        Debug.Log($"Lightning Strike: {damage}");
        // 播放特效
        Instantiate(this.shockEffectPrefab, this.transform.position, Quaternion.identity);

        // 调用Entity_Health组件的方法来造成伤害
        this.eh.ReduceHealth(damage);
    }

    private void StopShockEffect()
    {
        this.currentEffect = E_ElementType.None;
        this.currentCharge = 0f;
        this.ev.StopAllVfx();
    }
    private IEnumerator ShockEffectCor(float duration)
    {
        this.currentEffect = E_ElementType.Lightning;
        this.ev.PlayStatusEffectVFX(duration, E_ElementType.Lightning);

        yield return new WaitForSeconds(duration);
        // 状态效果结束，重置当前状态
        StopShockEffect();
    }

    public void StopAllEffect()
    {
        StopAllCoroutines();
        currentEffect = E_ElementType.None;
        this.ev.StopAllVfx();
    }
}
