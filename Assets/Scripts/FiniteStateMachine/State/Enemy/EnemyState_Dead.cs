using UnityEngine;

public class EnemyState_Dead : EnemyState
{
    private Collider2D collider;
    public EnemyState_Dead(StateMachine stateMachine, string animBoolSymbol, Enemy enemy) : base(stateMachine, animBoolSymbol, enemy)
    {
        this.collider = this.enemy.GetComponent<Collider2D>();
    }

    public override void Enter()
    {
        this.animator.enabled = false;
        this.rb.gravityScale = 12;
        this.enemy.rb.linearVelocity = new Vector2(this.rb.linearVelocity.x, 15.0f);
        this.collider.enabled = false;

        this.stateMachine.SwitchOffStateMachine();
    }
}
