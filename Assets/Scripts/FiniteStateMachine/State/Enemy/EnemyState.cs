using UnityEngine;
using UnityEngine.Rendering.Universal;

public abstract class EnemyState : EntityState
{
    protected Enemy enemy;
    public EnemyState(StateMachine stateMachine, string animBoolSymbol, Enemy enemy) : base(stateMachine, animBoolSymbol)
    {
        this.enemy = enemy;
        this.rb = this.enemy.rb;
        this.animator = this.enemy.animator;
        this.es = this.enemy.es;
    }


    public override void UpdateAnimationParameters()
    {
        base.UpdateAnimationParameters();

        float battleAnimSpeedMultiplier = this.enemy.battleMoveSpeed / this.enemy.moveSpeed;
        this.animator.SetFloat("moveAnimSpeedMultiplier", this.enemy.moveAnimSpeedMultiplier);
        this.animator.SetFloat("battleAnimSpeedMultiplier", battleAnimSpeedMultiplier);
        this.animator.SetFloat("xVelocity", this.rb.linearVelocity.x);
    }
}
