using UnityEngine;

public class PlayerState_Dash : PlayerState
{
    private float originGravityScale;
    private int dashDir;
    public PlayerState_Dash(StateMachine stateMachine, string animBoolSymbol, Player player) : base(stateMachine, animBoolSymbol, player)
    {
    }
    public override void Enter()
    {
        base.Enter();

        this.skillManager.dash.OnStartEff();
        this.player.vfx.PlayImageEchoEff(this.player.dashDuration);

        this.stateTimer = this.player.dashDuration;
        //记录冲刺前的重力系数
        this.originGravityScale = this.player.rb.gravityScale;
        this.player.rb.gravityScale = 0;

        this.dashDir = this.player.movementInput.x != 0 ? (int)this.player.movementInput.x : this.player.faceDir;

        this.player.entity_Health.SetCanTakeDamage(false);

    }
    public override void Update()
    {
        base.Update();

        ExitDashIfNeeded();

        //设置一个较大的速度，让角色在dashDuration期间产生大位移，达到冲刺的效果
        //冲刺时垂直方向应不下落，将垂直速度清0。但角色仍会受到重力的影响，产生微弱的垂直速度。
        //所以要在Enter将重力系数设为0；在Exit恢复重力系数
        this.player.SetVelocity(this.player.dashSpeed * this.dashDir, 0);

        //检测冲刺状态持续时间是否归0
        if (this.stateTimer < 0)
        {
            if (this.player.isTouchingGround)
                this.stateMachine.ChangeState(this.player.idleState);
            else
                this.stateMachine.ChangeState(this.player.fallState);
        }
    }
    public override void Exit()
    {
        base.Exit();
        //dash状态结束后，如果不对速度进行控制，
        // this.player.SetVelocity(this.player.dashSpeed * this.player.faceDir, 0)设置的速度会影响其他状态下的速度
        this.player.SetVelocity(0, 0);
        //恢复重力系数
        this.player.rb.gravityScale = originGravityScale;

        this.skillManager.dash.OnArrivalEff();
        this.player.entity_Health.SetCanTakeDamage(true);
    }

    private void ExitDashIfNeeded()
    {
        if (this.player.isTouchingWall)
        {
            if (this.player.isTouchingGround)
                this.stateMachine.ChangeState(this.player.idleState);
            else
                this.stateMachine.ChangeState(this.player.wallSlideState);
        }
    }
}
