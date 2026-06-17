using System;
using Unity.VisualScripting;
using UnityEngine;

public class SkillObject_SwordRegular : SkillObject_Base
{
    protected Skill_SwordToss swordManager;

    protected Transform player;
    protected bool shouldComeback = false;
    protected float comebackSpeed = 20.0f;
    protected float maxAllowedDistance = 25.0f; //脱手嵌入目标后，与Player的最大距离，超过则自动回收

    protected virtual void Update()
    {
        this.transform.right = rb.linearVelocity;
        HandleComeback();
    }

    public virtual void SetUpSword(Skill_SwordToss swordManager, Vector2 direction)
    {
        this.swordManager = swordManager;
        this.player = this.swordManager.transform.root;

        // this.rb = this.GetComponent<Rigidbody2D>();
        this.rb.linearVelocity = direction;

        this.playerStats = this.swordManager.player.playerStats;
        this.damageScaleData = this.swordManager.damageScaleData;
    }


    /// <summary>
    /// Sent when another object enters a trigger collider attached to this
    /// object (2D physics only).
    /// </summary>
    /// <param name="other">The other Collider2D involved in this collision.</param>
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        StopSword(other);
        DamageEnemiesInRadius(this.transform, 1); // 撞击后让一定范围内的敌人受到伤害
    }

    protected void StopSword(Collider2D collision)
    {
        this.rb.simulated = false;
        this.transform.parent = collision.transform; // 让剑留在碰撞物体上
    }

    public void GetSwordBackToPlayer() => this.shouldComeback = true;
    protected void HandleComeback()
    {
        float distance = Vector2.Distance(this.transform.position, this.player.position);
        if (distance >= this.maxAllowedDistance)
            GetSwordBackToPlayer();

        if (!this.shouldComeback)
            return;

        this.transform.position = Vector2.MoveTowards(this.transform.position, this.player.position, this.comebackSpeed * Time.deltaTime);

        if (distance < 0.5f)
            Destroy(this.gameObject);
    }
}
