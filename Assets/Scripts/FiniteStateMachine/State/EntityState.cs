using UnityEngine;

public abstract class EntityState
{
    protected StateMachine stateMachine;
    protected string animBoolSymbol;
    protected Animator animator;
    protected Rigidbody2D rb;
    protected Entity_Stats es;
    protected float stateTimer;
    protected bool stateTrigger;

    public EntityState(StateMachine stateMachine, string animBoolSymbol)
    {
        this.stateMachine = stateMachine;
        this.animBoolSymbol = animBoolSymbol;
    }

    /// <summary>
    /// 每次状态改变为另一个状态时，会调用该方法
    /// </summary>
    public virtual void Enter()
    {
        // Debug.Log("enter a state " + this.animBoolSymbol);
        this.stateTrigger = false;
        this.animator.SetBool(this.animBoolSymbol, true);
    }

    /// <summary>
    /// 本状态下的相关逻辑都会在该方法中运行
    /// </summary>
    public virtual void Update()
    {
        // Debug.Log("run the update logic of " + this.animBoolSymbol);
        this.stateTimer -= Time.deltaTime;
        this.UpdateAnimationParameters();

    }

    /// <summary>
    /// 退出某个状态并进入另一个新状态时会调用该方法
    /// </summary>
    public virtual void Exit()
    {
        // Debug.Log("exit the state of " + this.animBoolSymbol);
        this.animator.SetBool(this.animBoolSymbol, false);
    }

    public void AnimationTrigger()
    {
        this.stateTrigger = true;
    }
    public virtual void UpdateAnimationParameters()
    {

    }

    public void SyncAttackSpeed()
    {
        float attackSpeed = this.es.offenseGroup.attackSpeed.GetValue();
        this.animator.SetFloat("attackSpeedMultiplier", attackSpeed);
    }
}
