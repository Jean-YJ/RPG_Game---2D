using UnityEngine;

public class PlayerState_WallJump : PlayerState
{
    public PlayerState_WallJump(StateMachine stateMachine, string animBoolSymbol, Player player) : base(stateMachine, animBoolSymbol, player)
    {
    }
    public override void Enter()
    {
        base.Enter();
        //施加一个沿wallJumpForce向量方向上的速度
        this.player.SetVelocity(this.player.wallJumpForce.x * -this.player.faceDir,
                                this.player.wallJumpForce.y);
        // this.player.Flip();
    }

    public override void Update()
    {
        base.Update();
        //检测是否开始下落
        if (this.player.rb.linearVelocity.y < 0)
            this.stateMachine.ChangeState(this.player.fallState);
    }
}
