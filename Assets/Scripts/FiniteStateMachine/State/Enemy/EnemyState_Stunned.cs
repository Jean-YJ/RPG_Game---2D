using UnityEngine;

public class EnemyState_Stunned : EnemyState
{
    private Enemy_VFX ev;
    public EnemyState_Stunned(StateMachine stateMachine, string animBoolSymbol, Enemy enemy) : base(stateMachine, animBoolSymbol, enemy)
    {
        this.ev = this.enemy.GetComponent<Enemy_VFX>();
    }
    public override void Enter()
    {
        base.Enter();

        this.ev.SetAttackAlert(false);
        this.enemy.SetCounterWindowStatus(false);
        this.stateTimer = this.enemy.stunnedDuration;
        this.rb.linearVelocity = new Vector2(this.enemy.stunnedVelocity.x * -this.enemy.faceDir, this.enemy.stunnedVelocity.y);
    }
    public override void Update()
    {
        base.Update();
        if (this.stateTimer <= 0)
            this.stateMachine.ChangeState(this.enemy.idleState);
    }
}
