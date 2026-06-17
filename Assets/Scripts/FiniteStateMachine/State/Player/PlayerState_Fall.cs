using UnityEngine;

public class PlayerState_Fall : PlayerState_Air
{
    public PlayerState_Fall(StateMachine stateMachine, string animBoolSymbol, Player player) : base(stateMachine, animBoolSymbol, player)
    {
    }

    public override void Update()
    {
        base.Update();
        //检测是否落地
        if(this.player.isTouchingGround)
            this.stateMachine.ChangeState(this.player.idleState);

        //检测是否贴墙
        if(this.player.isTouchingWall)
        {
            this.stateMachine.ChangeState(this.player.wallSlideState);
            
        }
    }
}
