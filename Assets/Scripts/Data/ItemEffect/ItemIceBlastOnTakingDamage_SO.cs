using UnityEngine;

[CreateAssetMenu(menuName = "RPG SetUp/Item Data/Item effect/Ice Blast", fileName = "Item effect data - Ice Blast on Taking Damage")]
public class ItemIceBlastOnTakingDamage_SO : ItemEffectData_SO
{

    // 触发冰霜爆炸的血线百分比，0.2表示当玩家血量低于20%时触发
    [SerializeField] private float triggerPercentage = 0.2f;
    [SerializeField] private float coolDown = 40.0f; // 冰霜爆炸的冷却时间，单位为秒
    [SerializeField] private LayerMask whatIsEnemy; // 用于检测哪些对象是敌人，确保冰霜爆炸只对敌人造成伤害
    [SerializeField] private float damageAmount = 20f; // 冰霜爆炸造成的伤害值
    [SerializeField] private ElementEffectData elementEffectData;

    // 上次触发冰霜爆炸的时间，初始值设为负无穷大以确保第一次可以触发
    private float lastTriggerTime = -Mathf.Infinity;
    [Header("VFX Details")]
    [SerializeField] private GameObject iceBlastVFXPrefab;
    [SerializeField] private GameObject onHitVfxPrefab;

    public override void ExcuteEffect()
    {
        // base.ExcuteEffect();
        // Debug.Log("ExcuteEffect Ice Blast");
        // 在这里实现冰霜爆炸的效果
        bool isOnCoolDown = Time.time < lastTriggerTime + coolDown;
        bool isReachTriggerThreshold = this.player.entity_Health.GetCurrentHealthPercentage() <= triggerPercentage;
        // Debug.Log($"isOnCoolDown: {isOnCoolDown};  isReachTriggerThreshold: {isReachTriggerThreshold}");
        if (!isOnCoolDown && isReachTriggerThreshold)
        {
            this.player.vfx.CreateVfxBy(this.iceBlastVFXPrefab, this.player.transform);
            this.lastTriggerTime = Time.time; // 更新上次触发时间
            DamageEnemyWithIceBlast();
        }
    }

    private void DamageEnemyWithIceBlast()
    {
        // Debug.Log("DamageEnemyWithIceBlast");
        // 这里实现冰霜爆炸对敌人造成伤害的逻辑
        // 使用Physics.OverlapSphere来检测范围内的敌人，并对它们造成伤害
        // 2.5f是冰霜爆炸的范围半径
        Collider2D[] colliders = Physics2D.OverlapCircleAll(this.player.transform.position, 2.5f, this.whatIsEnemy);
        foreach (Collider2D collider in colliders)
        {
            IDamageable damageable = collider.GetComponent<IDamageable>();
            if (damageable == null) continue;

            bool isTargetGetHit = damageable.TakeDamage(0, this.damageAmount, E_ElementType.Ice, this.player.transform);
            if (isTargetGetHit)
            {
                Entity_StatusHandler esh = collider.GetComponent<Entity_StatusHandler>();
                esh?.ApplyStatusEffect(E_ElementType.Ice, this.elementEffectData);

                this.player.vfx.CreateVfxBy(this.onHitVfxPrefab, collider.transform);
            }
        }
    }

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);

        this.lastTriggerTime = -Mathf.Infinity;
        player.entity_Health.onTakeDamage += ExcuteEffect;
    }

    public override void Unsubscribe()
    {
        // base.Unsubscribe();
        this.player.entity_Health.onTakeDamage -= ExcuteEffect;
        this.player = null;
    }
}
