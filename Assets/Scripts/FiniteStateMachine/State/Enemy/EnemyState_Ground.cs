using UnityEngine;

public class EnemyState_Ground : EnemyState
{
    public EnemyState_Ground(StateMachine stateMachine, string animBoolSymbol, Enemy enemy) : base(stateMachine, animBoolSymbol, enemy)
    {
    }

    public override void Update()
    {
        base.Update();

        if(this.enemy.PlayerDetected() == true)
            this.stateMachine.ChangeState(this.enemy.battleState);
    }
}
