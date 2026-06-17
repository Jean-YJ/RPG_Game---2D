using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Entity_Health : MonoBehaviour, IDamageable
{
    private Entity_VFX ev;
    private Entity entity;
    private Entity_Stats es;
    private Entity_DropManager dropManager;
    private Slider healthBar;
    private bool isHealthBarVisible = true;
    public event Action onTakeDamage;
    public event Action onUpdateHealth;
    [SerializeField] protected float currentHealth;
    // [SerializeField] protected float maxHP = 100;
    protected bool canTakeDamage = true;
    public bool isDead { get; protected set; } = false;
    public float lastDamageTaken { get; private set; }
    [Header("Health Regen Detail")]
    [SerializeField] private float healthRegenInterval = 1.0f;
    [SerializeField] private bool canRegenerateHealth = true;

    [Header("KnockBack Detail")]
    [SerializeField] private Vector2 knockBackPower = new Vector2(1.5f, 2.5f);
    [SerializeField] private float knockBackDuration = 0.2f;
    [SerializeField] private Vector2 heavyKnockBackPower = new Vector2(7f, 7f);
    [SerializeField] private float heavyKnockBackDuration = 0.5f;
    //界定一次攻击消耗目标多少生命值才算是heavyDamage的标准
    //以0.3f、目标的maxHp100为例，一次攻击打掉了30以上血量就算作heavyDamage
    [SerializeField] private float heavyDamageThreshold = 0.3f;

    protected virtual void Awake()
    {
        this.entity = this.GetComponent<Entity>();
        this.ev = this.GetComponent<Entity_VFX>();
        this.es = this.GetComponent<Entity_Stats>();
        this.dropManager = this.GetComponent<Entity_DropManager>();

        this.healthBar = this.GetComponentInChildren<Slider>();

        SetUpHealth();
    }

    private void SetUpHealth()
    {
        if (this.es == null)
            return;

        this.currentHealth = this.es.GetMaxHP();
        this.onUpdateHealth += UpdateHealthBar;
        UpdateHealthBar();
        InvokeRepeating(nameof(RegenerateHealth), 0, this.healthRegenInterval);
    }
    public virtual bool TakeDamage(float damage, float elementalDamage, E_ElementType elementType, Transform dealer)
    {
        // 如果自身已经死亡，就不再受到伤害
        if (this.isDead)
            return false;
        if (!this.canTakeDamage)
            return false;
        // 触发闪避判定
        if (AttackEvaded())
        {
            Debug.Log($"{this.gameObject.name} evaded the attack!");
            return false;
        }

        //获取攻击者的状态值
        Entity_Stats attackerStats = dealer.GetComponent<Entity_Stats>();
        // 获取攻击者的护甲穿透数值
        float attackerArmorReduction = attackerStats != null ? attackerStats.GetArmorReduction() : 0;

        // 计算自身的护甲减伤和元素抗性减伤
        float mitigation = this.es != null ? this.es.GetArmorMitigation(attackerArmorReduction) : 0;
        float mitigatedPhysicalDamage = damage * (1 - mitigation);

        float elementalResistance = this.es != null ? this.es.GetElementalResistance(elementType) : 0;
        float resistedElementalDamage = elementalDamage * (1 - elementalResistance);

        //得到穿透、护甲、抗性 综合计算过后的最终伤害值
        float finalTakenDamage = mitigatedPhysicalDamage + resistedElementalDamage;

        Vector2 knockBack = this.CalaculateKnockBack(dealer, finalTakenDamage);
        float duration = this.CalaculateKnockBackDuration(finalTakenDamage);
        this.entity?.ReceiveKnockBack(knockBack, duration);

        ReduceHealth(finalTakenDamage);

        this.lastDamageTaken = finalTakenDamage;
        this.onTakeDamage?.Invoke();
        Debug.Log($"Final Damage: {finalTakenDamage}.\n Element Danage: {resistedElementalDamage} ElementType: {elementType}. \n  Physical Damage: {mitigatedPhysicalDamage}");
        return true;
    }
    private bool AttackEvaded()
    {
        if (this.es == null)
            return false;

        return UnityEngine.Random.Range(0, 100) < this.es.GetEvasion();
    }


    private Vector2 CalaculateKnockBack(Transform damageDealer, float damage)
    {
        //本物体是否在攻击者的右边？向右退：向左退
        int knockBackDir = this.transform.position.x > damageDealer.position.x ? 1 : -1;
        Vector2 knockBack = IsHeavyDamage(damage) ? this.heavyKnockBackPower : this.knockBackPower;
        knockBack.x = knockBack.x * knockBackDir;
        return knockBack;
    }
    private bool IsHeavyDamage(float damage)
    {

        if (this.es == null)
            return false;
        return damage / this.es.GetMaxHP() >= this.heavyDamageThreshold;
    }
    private float CalaculateKnockBackDuration(float damage)
    {
        return IsHeavyDamage(damage) ? this.heavyKnockBackDuration : this.knockBackDuration;
    }

    public void RegenerateHealth()
    {
        if (!this.canRegenerateHealth)
            return;

        float healAmount = this.es.resourceGroup.healthRegen.GetValue();

        IncreaseHealth(healAmount);
    }
    public void IncreaseHealth(float healAmount)
    {
        if (this.isDead)
            return;

        float newHealth = this.currentHealth + healAmount;
        float maxHP = this.es.GetMaxHP();
        this.currentHealth = Mathf.Min(newHealth, maxHP);

        this.onUpdateHealth?.Invoke();
    }
    public void ReduceHealth(float damage)
    {
        this.ev?.PlayOnDamagedVFX();
        this.currentHealth -= damage;
        this.onUpdateHealth?.Invoke();
        if (this.currentHealth <= 0)
        {
            Dead();
        }
    }
    // 更新血条显示
    private void UpdateHealthBar()
    {
        if (this.healthBar == null || !this.healthBar.transform.parent.gameObject.activeSelf)
            return;

        this.healthBar.value = this.currentHealth / this.es.GetMaxHP();
    }

    public void EnableHealthBar(bool enable) => this.healthBar?.transform.parent.gameObject.SetActive(enable);

    public float GetCurrentHealthPercentage()
    {
        return this.currentHealth / this.es.GetMaxHP();
    }
    public float GetCurrentHealth()
    {
        return this.currentHealth;
    }

    public void SetCurrentHealthByPercentage(float p)
    {
        this.currentHealth = this.es.GetMaxHP() * Mathf.Clamp01(p);
        this.onUpdateHealth?.Invoke();
    }

    protected virtual void Dead()
    {
        this.isDead = true;
        this.entity?.EntityDeath();
        this.dropManager?.DropItems();
    }

    public bool SetCanTakeDamage(bool value) => this.canTakeDamage = value;
}
