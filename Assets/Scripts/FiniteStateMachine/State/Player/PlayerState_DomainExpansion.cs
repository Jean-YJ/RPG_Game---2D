using UnityEngine;

public class PlayerState_DomainExpansion : PlayerState
{
    private Vector2 originalPosition;
    private float originalGravity;
    private float finalRiseDistance;

    private bool isLevitating = false;
    private bool createDomain = false;
    public PlayerState_DomainExpansion(StateMachine stateMachine, string animBoolSymbol, Player player) : base(stateMachine, animBoolSymbol, player)
    {
    }

    public override void Enter()
    {
        base.Enter();

        this.originalPosition = this.player.transform.position;
        this.originalGravity = this.rb.gravityScale;
        this.finalRiseDistance = GetAvaliableDistance();

        this.player.SetVelocity(0, this.player.riseSpeed);
        this.player.entity_Health.SetCanTakeDamage(false);

    }

    public override void Update()
    {
        base.Update();

        //上升距离达到最大后，进入静止状态
        //注意：由于上升期间仍受到重力影响，且只在Enter中施加了向上的速度，
        // 所以如果riseMaxDistance和riseSpeed数据设置不当，可能导致无法达到目标高度，导致角色卡在上升状态
        if (Vector2.Distance(this.originalPosition, this.player.transform.position) >= this.finalRiseDistance && this.isLevitating == false)
            Levitate();

        if (isLevitating)
        {
            this.skillManager.domainExpansion.DoSpellCasting();

            if (this.stateTimer < 0)
            {
                this.rb.gravityScale = this.originalGravity;
                this.isLevitating = false;

                this.stateMachine.ChangeState(this.player.fallState);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();

        this.createDomain = false;
        this.player.entity_Health.SetCanTakeDamage(true);
    }

    private void Levitate()
    {
        this.isLevitating = true;
        this.rb.linearVelocity = Vector2.zero;
        this.rb.gravityScale = 0;

        // 获取持续时间。暂时设置为5s
        this.stateTimer = this.skillManager.domainExpansion.GetDomainDuration();

        if (!this.createDomain)
        {
            this.createDomain = true;
            //通过skillManager创建领域
            this.skillManager.domainExpansion.CreateDomain();
        }
    }
    // 上升过程中可能会碰到顶部的建筑
    private float GetAvaliableDistance()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.player.transform.position, Vector2.up, this.player.riseMaxDistance, this.player.whatIsGround);

        return hit.collider != null ? hit.distance - 1 : this.player.riseMaxDistance - 0.5f;
    }
}
