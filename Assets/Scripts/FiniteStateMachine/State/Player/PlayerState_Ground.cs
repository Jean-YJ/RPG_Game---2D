using UnityEngine;

public class PlayerState_Ground : PlayerState
{
    public PlayerState_Ground(StateMachine stateMachine, string animBoolSymbol, Player player) : base(stateMachine, animBoolSymbol, player)
    {
    }

    public override void Update()
    {
        base.Update();

        //垂直方向速度小于0且下方没有接触地面时，切换到下落状态
        if (this.player.rb.linearVelocity.y < 0 && !this.player.isTouchingGround)
            this.stateMachine.ChangeState(this.player.fallState);

        //检测是否按了跳跃
        if (this.player.p_Input.Player.Jump.WasPressedThisFrame())
        {
            // Debug.Log("Jump");
            this.stateMachine.ChangeState(this.player.jumpState);
        }
        //检测是否按了攻击键
        if (this.player.p_Input.Player.BasicAttack.WasPressedThisFrame())
            this.stateMachine.ChangeState(this.player.basicAttackState);

        //检测是否按了格挡反击键
        if (this.player.p_Input.Player.CounterAttack.WasPressedThisFrame())
            this.stateMachine.ChangeState(this.player.counterAttackState);

        //检测是否按下了投掷瞄准
        if (this.player.p_Input.Player.RangeAttack.WasPerformedThisFrame() && 
                                this.player.skillManager.swordToss.CanUseSkill())
            this.stateMachine.ChangeState(this.player.swordTossState);
    }
}
