using UnityEngine;

public class PlayerState_CounterAttack : PlayerState
{
    private Player_Combat pc;
    private bool counteredSomebody;
    public PlayerState_CounterAttack(StateMachine stateMachine, string animBoolSymbol, Player player) : base(stateMachine, animBoolSymbol, player)
    {
        this.pc = this.player.GetComponent<Player_Combat>();
    }

    public override void Enter()
    {
        base.Enter();

        this.counteredSomebody = this.pc.CounterAttackPerformed();
        this.animator.SetBool("counterAttackPerformed", counteredSomebody);
        this.stateTimer = this.pc.GetCounterRecoverDuration();
    }
    public override void Update()
    {
        base.Update();

        this.player.SetVelocity(0, this.rb.linearVelocity.y);

        //退出反击状态有两种情况：
        //1、没有弹反到目标，且弹反的时间结束。不会进入Performed，直接退出状态。
        //2、弹反到目标。进入Performed，由Performed动画结束的时间来触发stateTrigger，借此退出状态
        if (this.stateTrigger)
            this.stateMachine.ChangeState(this.player.idleState);

        //!this.counteredSomebody条件是为了避免 stateTimer结束了但是Performed动画还未结束 导致的中断
        if (this.stateTimer <= 0 && !this.counteredSomebody)
            this.stateMachine.ChangeState(this.player.idleState);
    }
}
