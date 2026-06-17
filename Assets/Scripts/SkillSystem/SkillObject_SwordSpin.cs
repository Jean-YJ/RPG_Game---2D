using UnityEngine;

public class SkillObject_SwordSpin : SkillObject_SwordRegular
{
    private float maxSpinDistance;
    private int attackAmountPersecond;
    private float attackTimer;

    public override void SetUpSword(Skill_SwordToss swordManager, Vector2 direction)
    {
        base.SetUpSword(swordManager, direction);

        this.animator.SetTrigger("swordSpin");

        this.maxSpinDistance = swordManager.maxSpinDistance;
        this.attackAmountPersecond = swordManager.attackAmountPersecond;

        Invoke(nameof(GetSwordBackToPlayer), swordManager.maxSpinDuration);
    }

    protected override void Update()
    {
        // base.Update();
        HandleAttack();
        HandleStop();
        HandleComeback();
    }

    // 1秒内攻击attackAmountPersecond次
    private void HandleAttack()
    {
        this.attackTimer -= Time.deltaTime;

        if (this.attackTimer <= 0)
        {
            DamageEnemiesInRadius(this.transform, 1);
            this.attackTimer = 1.0f / this.attackAmountPersecond;
        }
    }

    // 旋转的剑脱手后，若路径上没有Enemy，达到一定距离后会停止移动
    private void HandleStop()
    {
        float distance = Vector2.Distance(this.transform.position, this.player.transform.position);

        if (distance >= this.maxSpinDistance && this.rb.simulated == true)
            this.rb.simulated = false;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // base.OnTriggerEnter2D(other);
        this.rb.simulated = false;
    }
}
