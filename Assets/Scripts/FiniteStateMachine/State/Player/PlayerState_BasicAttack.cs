using UnityEngine;

public class PlayerState_BasicAttack : PlayerState
{
    private float attackVelocityTimer;
    private const int FirstComboIndex = 1;
    private int lastComboIndex = 3;
    private int currentComboIndex = 1;
    private float lastAttackedTime;
    private bool attackComboQueued;
    private int attackDir;
    public PlayerState_BasicAttack(StateMachine stateMachine, string animBoolSymbol, Player player) : base(stateMachine, animBoolSymbol, player)
    {
        if (this.lastComboIndex != this.player.attackVelocity.Length)
        {
            Debug.LogWarning("出现异常：最大连击数和攻击速度设置数组的长度不一致，已经以攻击速度设置数组为基准进行了统一");
            this.lastComboIndex = this.player.attackVelocity.Length;
        }
    }
    public override void Enter()
    {
        base.Enter();
        this.attackComboQueued = false;
        ResetComboIndexIfNeed();
        this.attackDir = this.player.movementInput.x != 0 ? (int)this.player.movementInput.x : this.player.faceDir;
        ApplyAttackVelocity();
        this.animator.SetInteger("basicAttackIndex", this.currentComboIndex);

        SyncAttackSpeed();
    }
    public override void Update()
    {
        base.Update();
        HandleAttackVelocity();
        //伤害检测
        //

        //检测在一次攻击中是否又再次按下了攻击键
        if (this.player.p_Input.Player.BasicAttack.WasPerformedThisFrame())
            QueueNextAttack();


        if (this.stateTrigger)
            HandleStateExit();

    }
    public override void Exit()
    {
        base.Exit();
        this.currentComboIndex++;
        //记录本次攻击完成的时间
        this.lastAttackedTime = Time.time;
    }
    private void HandleAttackVelocity()
    {
        this.attackVelocityTimer -= Time.deltaTime;
        //超过设置的攻击位移时限，则停止位移
        if (this.attackVelocityTimer < 0)
            this.player.SetVelocity(0, this.player.rb.linearVelocity.y);
    }
    private void ApplyAttackVelocity()
    {
        Vector2 attackVelocity = this.player.attackVelocity[this.currentComboIndex - 1];
        this.attackVelocityTimer = this.player.attackDuration;
        this.player.SetVelocity(this.attackDir * attackVelocity.x, attackVelocity.y);
    }
    private void ResetComboIndexIfNeed()
    {
        //如果超过了最大攻击间隔，重置连击
        if (Time.time > this.lastAttackedTime + this.player.comboResetTime)
            this.currentComboIndex = FirstComboIndex;
        //最后一段连击完毕，重置连击
        if (this.currentComboIndex > this.lastComboIndex)
            this.currentComboIndex = FirstComboIndex;
    }

    private void HandleStateExit()
    {
        //一次攻击中再次按下了攻击键,本次攻击状态退出后直接进入下一次的攻击状态，避免出现Idle状态闪动
        if (this.attackComboQueued)
        {
            //由于上一次状态的Exit和下一次状态的Enter是在同一帧中执行的。且连击的前后两次状态都是攻击状态
            //会导致动画状态机的参数一直为basicAttack = true，使得无法退出上一次攻击的状态动画
            //在这切换状态时手动将basicAttack置为false，并通过协程延迟一帧再切换状态，进入下一次的攻击动画
            // this.stateMachine.ChangeState(this.player.basicAttackState);
            this.animator.SetBool(this.animBoolSymbol, false);
            this.player.EnterAttackStateWithDealy();

        }
        else
            this.stateMachine.ChangeState(this.player.idleState);
    }
    private void QueueNextAttack()
    {
        if (this.currentComboIndex >= this.lastComboIndex)
            this.attackComboQueued = false;
        else
            this.attackComboQueued = true;
    }
}
