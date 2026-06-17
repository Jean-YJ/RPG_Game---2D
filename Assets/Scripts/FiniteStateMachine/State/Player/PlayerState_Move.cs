using UnityEngine;

public class PlayerState_Move : PlayerState_Ground
{
    public PlayerState_Move(StateMachine stateMachine, string stateName, Player player) : base(stateMachine, stateName, player)
    {

    }

    public override void Update()
    {
        base.Update();
        if (this.player.movementInput.x == 0 || this.player.isTouchingWall)
            this.stateMachine.ChangeState(this.player.idleState);

        this.player.SetVelocity(this.player.movementInput.x * this.player.moveSpeed,
                                this.player.rb.linearVelocity.y);
    }
}
