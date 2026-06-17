using UnityEngine;

/// <summary>
/// 状态基类
/// </summary>
public abstract class PlayerState : EntityState
{
    protected PlayerInputSet input;
    protected Player player;
    protected Player_SkillManager skillManager;

    public PlayerState(StateMachine stateMachine, string animBoolSymbol, Player player) : base(stateMachine, animBoolSymbol)
    {
        this.player = player;
        this.animator = this.player.animator;
        this.rb = this.player.rb;
        this.input = this.player.p_Input;
        this.es = this.player.playerStats;
        this.skillManager = this.player.skillManager;
    }

    public override void Update()
    {
        base.Update();

        //检测是否按下了冲刺键
        if (this.player.p_Input.Player.Dash.WasPerformedThisFrame() && CanDash())
        {
            this.skillManager.dash.SetSkillOnCoolDown();
            this.stateMachine.ChangeState(this.player.dashState);
        }

        //检测是否按下了领域展开按键
        if (this.player.p_Input.Player.UltimateSpell.WasPerformedThisFrame() && this.skillManager.domainExpansion.CanUseSkill())
        {
            //Instant类型的会直接创建
            if (this.skillManager.domainExpansion.InstantDomain())
            {
                this.skillManager.domainExpansion.CreateDomain();
            }
            //非Instant类型的，会进入domainExpansionState，后续处理在domainExpansionState的逻辑中进行
            else
            {
                this.stateMachine.ChangeState(this.player.domainExpansionState);
            }

            this.skillManager.domainExpansion.SetSkillOnCoolDown();
        }
    }
    private bool CanDash()
    {
        //冷却
        if (!this.skillManager.dash.CanUseSkill())
            return false;

        if (this.player.isTouchingWall)
            return false;
        if (this.stateMachine.CurrentState == this.player.dashState || this.stateMachine.CurrentState == this.player.domainExpansionState)
            return false;
        return true;
    }

    public override void UpdateAnimationParameters()
    {
        base.UpdateAnimationParameters();

        this.animator.SetFloat("yVelocity", this.player.rb.linearVelocity.y);
    }
}
