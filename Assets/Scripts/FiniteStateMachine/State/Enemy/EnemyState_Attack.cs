using UnityEngine;

public class EnemyState_Attack : EnemyState
{
    public EnemyState_Attack(StateMachine stateMachine, string animBoolSymbol, Enemy enemy) : base(stateMachine, animBoolSymbol, enemy)
    {
    }

    public override void Enter()
    {
        base.Enter();

        SyncAttackSpeed();
    }
    public override void Update()
    {
        base.Update();

        if (this.stateTrigger)
            this.stateMachine.ChangeState(this.enemy.battleState);
    }
}
