using System;
using UnityEngine;

public class Entity_Combat : MonoBehaviour
{
    private Entity_VFX ev;
    private Entity_SFX sfx;
    // [SerializeField] private float damage = 10.0f;
    private Entity_Stats stats;
    public DamageScaleData basicDamageScale; // 本对象的基础攻击系数
    public event Action<float> onDoingPhysicalDamage;

    [Header("Target Detection")]
    [SerializeField] private Transform targetCheck;
    [SerializeField] private float targetCheckRadius;
    [SerializeField] private LayerMask whatIsTarget;

    // [Header("Effect Detail")]
    // [SerializeField] private float defaultDuration = 2.0f;
    // [SerializeField] private float chillSlowMultiplier = 0.5f;
    // [SerializeField] private float electrifyChargeBuildUp = 0.4f; // 每次攻击增加的电荷量
    // [Space]
    // [SerializeField] private float fireFactor = 0.8f; // 火焰伤害的倍率，可以通过调整这个值来平衡火焰伤害的强度
    // [SerializeField] private float lightningFactor = 1.5f; // 雷电伤害的倍率，可以通过调整这个值来平衡雷电伤害的强度


    void Awake()
    {
        this.ev = this.GetComponent<Entity_VFX>();
        this.sfx = this.GetComponent<Entity_SFX>();
        this.stats = this.GetComponent<Entity_Stats>();
    }

    public void PerformAttack()
    {
        bool targetDamaged = false;
        foreach (var item in this.GetDetectedColiders())
        {
            IDamageable ida = item.GetComponent<IDamageable>();
            if (ida == null)
                continue;

            AttackData attackData = this.stats.GetAttackData(this.basicDamageScale);
            // float damageValue = this.stats.GetPhysicalDamage(out bool isCrit);
            float phyiscalDamage = attackData.phyiscalDamage;
            // 获取元素伤害数值
            // float elementalDamageValue = this.stats.GetElementalDamage(out E_ElementType elementType);
            float elementalDamage = attackData.elementalDamage;
            E_ElementType elementType = attackData.elementType;

            targetDamaged = ida.TakeDamage(phyiscalDamage, elementalDamage,
                                                elementType, this.transform);

            ElementEffectData elementEffect = new ElementEffectData(this.stats, this.basicDamageScale);
            Entity_StatusHandler esh = item.GetComponent<Entity_StatusHandler>();

            if (elementType != E_ElementType.None)
                esh?.ApplyStatusEffect(elementType, elementEffect);

            if (targetDamaged)
            {
                this.onDoingPhysicalDamage?.Invoke(phyiscalDamage);
                // 一定要先更新VFX的颜色(已封装到CreateOnHitVfx中)，再创建VFX，
                // 因为CreateOnHitVfx会根据当前的hitVfxColor来设置生成的VFX的颜色
                this.ev.CreateOnHitVfx(item.transform, attackData.isCrit, elementType);

                this.sfx?.PlayAttackHit();
            }
        }

        if (!targetDamaged)
            this.sfx?.PlayAttackMiss();

    }
    protected virtual Collider2D[] GetDetectedColiders()
    {
        return Physics2D.OverlapCircleAll(this.targetCheck.position, this.targetCheckRadius, this.whatIsTarget);
    }


    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(this.targetCheck.position, this.targetCheckRadius);
    }
}
