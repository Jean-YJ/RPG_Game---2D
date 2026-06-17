using System.Collections;
using UnityEngine;

public class Enemy : Entity
{
    public EnemyState_Idle idleState { get; protected set; }
    public EnemyState_Move moveState { get; protected set; }
    public EnemyState_Attack attackState { get; protected set; }
    public EnemyState_Battle battleState { get; protected set; }
    public EnemyState_Dead deadState { get; protected set; }
    public EnemyState_Stunned stunnedState { get; protected set; }

    public Enemy_Health enemyHealth { get; protected set; }
    public Entity_Stats es { get; private set; }


    [Header("Move Detail")]
    public float idleTime = 2.0f;
    public float moveSpeed = 1.4f;
    [Range(0, 2)]
    public float moveAnimSpeedMultiplier = 1.0f;
    public float slowDownMultiplier { get; private set; } = 1.0f;
    [Header("Player Detection")]
    [SerializeField] private float detectDistance;
    [SerializeField] private Transform detectCheck;
    [SerializeField] private LayerMask whatIsPlayer;

    [Header("Battle Detail")]
    public float battleMoveSpeed = 3.0f;
    public float attackDistance = 2.0f;
    public float battleDuration = 3.0f;
    public float minRetreatDistance = 1.0f;
    public Vector2 retreatVelocity;
    public Transform damageDealer;
    public float horizontalTolerance = 0.1f;
    public float verticalTolerance = 0.75f;


    [Header("Stun Detail")]
    public float stunnedDuration = 1.0f;
    public Vector2 stunnedVelocity;
    public bool canBeStunned;



    protected override void Awake()
    {
        base.Awake();
        this.enemyHealth = GetComponent<Enemy_Health>();
        this.es = this.GetComponent<Entity_Stats>();
    }

    void OnEnable()
    {
        Player.OnPlayerDead += this.HandlePlayerDeath;
    }

    public RaycastHit2D PlayerDetected()
    {
        //同时检测Ground和Player
        RaycastHit2D hit = Physics2D.Raycast(this.detectCheck.position, Vector2.right * this.faceDir,
                                    this.detectDistance, this.whatIsPlayer | this.whatIsGround);
        //如果没有碰撞对象或碰撞对象的层级不是Player，返回default RaycastHit2D
        //default意味着collider为null
        if (hit.collider == null || hit.collider.gameObject.layer != LayerMask.NameToLayer("Player"))
            return default;
        //碰撞对象的层级是Player，则返回对应的RaycastHit2D
        return hit;
    }

    public void TryEnterBattleState(Transform dealer)
    {
        if (this.stateMachine.CurrentState == this.battleState || this.stateMachine.CurrentState == this.attackState)
            return;

        this.damageDealer = dealer;
        this.stateMachine.ChangeState(this.battleState);
    }
    public Transform GetDamageDealerReference()
    {
        if (this.damageDealer == null)
            this.damageDealer = this.PlayerDetected().transform;
        return this.damageDealer;
    }

    public override void EntityDeath()
    {
        base.EntityDeath();
        this.stateMachine.ChangeState(this.deadState);
    }

    private void HandlePlayerDeath()
    {
        this.stateMachine.ChangeState(this.idleState);
    }
    public void SetCounterWindowStatus(bool enable)
    {
        this.canBeStunned = enable;
    }

    public float GetMoveSpeed()
    {
        return this.moveSpeed * this.slowDownMultiplier;
    }
    public float GetBattleMoveSpeed()
    {
        return this.battleMoveSpeed * this.slowDownMultiplier;
    }
    protected override IEnumerator SlowDownCor(float duration, float slowMultiplier)
    {
        this.slowDownMultiplier = 1 - slowMultiplier;
        this.animator.speed *= this.slowDownMultiplier;

        yield return new WaitForSeconds(duration);
        StopSlowDown();

    }
    public override void StopSlowDown()
    {
        this.slowDownMultiplier = 1;
        this.animator.speed = 1;
        base.StopSlowDown();
    }

    void OnDisable()
    {
        Player.OnPlayerDead -= this.HandlePlayerDeath;
    }
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(this.detectCheck.position, this.detectCheck.position + Vector3.right * this.faceDir * this.detectDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(this.detectCheck.position, this.detectCheck.position + Vector3.right * this.faceDir * this.attackDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(this.detectCheck.position, this.detectCheck.position + Vector3.right * this.faceDir * this.minRetreatDistance);

    }
}
