using UnityEngine;

public class PlayerState_Jump : PlayerState_Air
{
    public PlayerState_Jump(StateMachine stateMachine, string animBoolSymbol, Player player) : base(stateMachine, animBoolSymbol, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
        //进入跳跃状态时，要施加向上的速度
        // Debug.Log("Jump Enter:"+this.player.rb.linearVelocity.x);
        this.player.SetVelocity(this.player.rb.linearVelocity.x, this.player.jumpForce);
    }

    public override void Update()
    {
        base.Update();
        //检测是否开始下落
        if (this.player.rb.linearVelocity.y < 0 && this.stateMachine.CurrentState != this.player.jumpAttackState)
            this.stateMachine.ChangeState(this.player.fallState);
    }
}
