using UnityEngine;

public class PlayerState_WallSlide : PlayerState
{
    public PlayerState_WallSlide(StateMachine stateMachine, string animBoolSymbol, Player player) : base(stateMachine, animBoolSymbol, player)
    {
    }

    public override void Update()
    {
        base.Update();

        HandleSlide();

        //检测是否滑落到地面
        if (this.player.isTouchingGround)
        {
            this.stateMachine.ChangeState(this.player.idleState);
            //贴墙滑行后如果没有水平方向上的输入，落地后的朝向应与原来相反
            if(this.player.movementInput.x != this.player.faceDir)
                this.player.Flip();
        }
        //检测是否离开墙面了
        if (!this.player.isTouchingWall)
        {
            this.stateMachine.ChangeState(this.player.fallState);
        }
        //检测是否按了跳跃键
        if(this.player.p_Input.Player.Jump.WasPerformedThisFrame())
            this.stateMachine.ChangeState(this.player.wallJumpState);
    }

    private void HandleSlide()
    {
        if (this.player.movementInput.y < 0)
            this.player.SetVelocity(this.player.movementInput.x, this.player.rb.linearVelocity.y);
        else
            this.player.SetVelocity(this.player.movementInput.x, this.player.rb.linearVelocity.y * this.player.wallSlideMultipler);
    }

}
