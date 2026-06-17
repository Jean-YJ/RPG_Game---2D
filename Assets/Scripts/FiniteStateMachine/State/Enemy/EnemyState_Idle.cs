using UnityEngine;

public class EnemyState_Idle : EnemyState_Ground
{
    public EnemyState_Idle(StateMachine stateMachine, string animBoolSymbol, Enemy enemy) : base(stateMachine, animBoolSymbol, enemy)
    {
    }
    public override void Enter()
    {
        base.Enter();
        this.stateTimer = this.enemy.idleTime;
    }
    public override void Update()
    {
        base.Update();


        //检测是否到达Idle时限
        if (this.stateTimer < 0)
            this.stateMachine.ChangeState(this.enemy.moveState);
    }
}
