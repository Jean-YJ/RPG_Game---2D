using UnityEngine;

public class PlayerState_Dead : PlayerState
{
    public PlayerState_Dead(StateMachine stateMachine, string animBoolSymbol, Player player) : base(stateMachine, animBoolSymbol, player)
    {
    }

    public override void Enter()
    {
        base.Enter();

        this.input.Disable();
        this.rb.simulated = false;
    }
}
