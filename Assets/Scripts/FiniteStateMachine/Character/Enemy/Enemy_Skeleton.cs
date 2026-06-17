using UnityEngine;

public class Enemy_Skeleton : Enemy, ICounterable
{
    public bool CanBeCountered { get => this.canBeStunned; }

    protected override void Awake()
    {
        base.Awake();
        this.idleState = new EnemyState_Idle(this.stateMachine, "idle", this);
        this.moveState = new EnemyState_Move(this.stateMachine, "move", this);
        this.attackState = new EnemyState_Attack(this.stateMachine, "attack", this);
        this.battleState = new EnemyState_Battle(this.stateMachine, "battle", this);
        this.deadState = new EnemyState_Dead(this.stateMachine, "idle", this);
        this.stunnedState = new EnemyState_Stunned(this.stateMachine, "stunned", this);
    }

    protected override void Start()
    {
        base.Start();
        this.stateMachine.Initialize(this.idleState);
    }
    protected override void Update()
    {
        base.Update();

    }
    public void HandleCounter()
    {
        if (!this.canBeStunned)
            return;
        this.stateMachine.ChangeState(this.stunnedState);
    }
}
