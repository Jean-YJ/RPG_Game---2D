using UnityEngine;

public class PlayerState_Air : PlayerState
{
    public PlayerState_Air(StateMachine stateMachine, string animBoolSymbol, Player player) : base(stateMachine, animBoolSymbol, player)
    {
    }

    public override void Update()
    {
        base.Update();

        if (this.player.movementInput.x != 0)
            this.player.SetVelocity(this.player.movementInput.x * (this.player.moveSpeed * this.player.airMoveMultipler),
                                this.player.rb.linearVelocity.y);

        //检测是否在空中按下了攻击
        if (this.player.p_Input.Player.BasicAttack.WasPerformedThisFrame())
            this.stateMachine.ChangeState(this.player.jumpAttackState);
    }
}
