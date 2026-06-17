using UnityEngine;

public class SkillObject_Base : MonoBehaviour
{
    [SerializeField] private GameObject onHitVfx;
    protected bool targetGotHit;
    protected Transform lastTraget;
    [Space]
    [SerializeField] protected LayerMask whatIsEnemy;
    [SerializeField] protected Transform targetCheck;
    [SerializeField] protected float checkRadius;

    protected E_ElementType currentUsedElement;

    protected Entity_Stats playerStats;
    protected DamageScaleData damageScaleData;

    protected Rigidbody2D rb;
    protected Animator animator;

    protected virtual void Awake()
    {
        this.rb = GetComponent<Rigidbody2D>();
        this.animator = GetComponentInChildren<Animator>();
    }

    protected Collider2D[] GetEnemiesAround(Transform t, float radius)
    {
        return Physics2D.OverlapCircleAll(t.position, radius, this.whatIsEnemy);
    }

    protected Transform GetClosestTarget()
    {
        Transform target = null;
        float closestDistance = Mathf.Infinity;
        foreach (var item in GetEnemiesAround(targetCheck, 10))
        {
            float distance = Vector2.Distance(item.transform.position, this.targetCheck.position);
            if (distance < closestDistance)
            {
                target = item.transform;
                closestDistance = distance;
            }
        }
        return target;
    }

    /// <summary>
    /// 范围内敌人受到伤害
    /// </summary>
    /// <param name="t">中心点</param>
    /// <param name="radius">半径</param>
    protected void DamageEnemiesInRadius(Transform t, float radius)
    {
        foreach (var target in GetEnemiesAround(t, radius))
        {
            IDamageable ida = target.GetComponent<IDamageable>();
            if (ida == null)
                continue;

            ElementEffectData elementEffect = new ElementEffectData(this.playerStats, this.damageScaleData);
            Entity_StatusHandler es = target.GetComponent<Entity_StatusHandler>();

            AttackData attackData = this.playerStats.GetAttackData(this.damageScaleData);
            // float physicalDamage = this.playerStats.GetPhysicalDamage(out bool isCrit, this.damageScaleData.phyiscal);
            // float elementDamage = this.playerStats.GetElementalDamage(out E_ElementType type, this.damageScaleData.elemental);
            float physicalDamage = attackData.phyiscalDamage;
            float elementDamage = attackData.elementalDamage;
            E_ElementType type = attackData.elementType;

            this.targetGotHit = ida.TakeDamage(physicalDamage, elementDamage, type, this.transform);

            if (type != E_ElementType.None)
                es?.ApplyStatusEffect(type, elementEffect);

            if (this.targetGotHit)
            {
                this.lastTraget = target.transform;
                Instantiate(this.onHitVfx, target.transform.position, Quaternion.identity);
            }

            this.currentUsedElement = type;
        }
    }

    protected virtual void OnDrawGizmos()
    {
        if (this.targetCheck == null)
            this.targetCheck = this.transform;

        Gizmos.DrawWireSphere(this.targetCheck.position, this.checkRadius);
    }
}
