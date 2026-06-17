using UnityEngine;

public class EnemyState_Move : EnemyState_Ground
{
    public EnemyState_Move(StateMachine stateMachine, string animBoolSymbol, Enemy enemy) : base(stateMachine, animBoolSymbol, enemy)
    {
    }
    public override void Enter()
    {
        base.Enter();
        //检测是否走到平台边缘 或 撞墙
        if (!this.enemy.isTouchingGround || this.enemy.isTouchingWall)
            this.enemy.Flip();
    }
    public override void Update()
    {
        base.Update();

        this.enemy.SetVelocity(this.enemy.GetMoveSpeed() * this.enemy.faceDir, this.rb.linearVelocity.y);

        //检测是否走到平台边缘 或 撞墙
        if (!this.enemy.isTouchingGround || this.enemy.isTouchingWall)
        {
            this.stateMachine.ChangeState(this.enemy.idleState);
        }
    }
}
