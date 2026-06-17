using UnityEngine;

public class PlayerState_Idle : PlayerState_Ground
{
    public PlayerState_Idle(StateMachine stateMachine, string stateName, Player player) : base(stateMachine, stateName, player)
    {

    }

    public override void Enter()
    {
        base.Enter();
        this.player.SetVelocity(0, this.player.rb.linearVelocity.y);
    }
    public override void Update()
    {
        base.Update();
        if(this.player.movementInput.x == this.player.faceDir && this.player.isTouchingWall)
            return; 

        if (this.player.movementInput.x != 0)
            this.stateMachine.ChangeState(this.player.moveState);
    }
}
