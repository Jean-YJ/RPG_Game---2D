using UnityEngine;

public class PlayerState_JumpAttack : PlayerState
{
    private bool isTouchingGround;
    public PlayerState_JumpAttack(StateMachine stateMachine, string animBoolSymbol, Player player) : base(stateMachine, animBoolSymbol, player)
    {
    }
    public override void Enter()
    {
        base.Enter();
        this.isTouchingGround = false;

        //注意：跳跃攻击时为了加快垂直速度，我们会将this.player.jumpAttackVelocity的Y值设为负值，这会导致一个bug
        //Bug:在空中Fall阶段进行跳跃攻击是正常的；在Jump阶段进行跳跃攻击会出现攻击阶段和动画丢失
        //原因：Jump到Fall的条件时this.player.rb.linearVelocity.y小于0，由于在JumpAttack状态进入时,
        //会设置this.player.jumpAttackVelocity的Y值设为负值，且Jump的Exit和JumpAttack的Enter是在同一帧中执行的。
        // 也就是说此时该帧中Jump的Update还在检测中，我们在JumpAttack的Enter将垂直速度设为小于0的值，触发了
        // Jump -> Fall 的条件，所以会在一帧中发生以下的变化：Jump -> JumpAttack -> Fall
        //解决方法：修改Jump -> Fall 的条件，确保this.player.rb.linearVelocity.y小于0且当前状态不是JumpAttack
        this.player.SetVelocity(this.player.faceDir * this.player.jumpAttackVelocity.x, this.player.jumpAttackVelocity.y);
    }
    public override void Update()
    {
        base.Update();
        if (this.player.isTouchingGround && !this.isTouchingGround)
        {
            this.isTouchingGround = true;
            this.animator.SetTrigger("jumpAttackTrigger");
            this.player.SetVelocity(0, this.player.rb.linearVelocity.y);
        }

        if (this.stateTrigger && this.player.isTouchingGround)
            this.stateMachine.ChangeState(this.player.idleState);

    }
}
