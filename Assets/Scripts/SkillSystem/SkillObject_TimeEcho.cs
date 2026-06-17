using UnityEngine;

public class SkillObject_TimeEcho : SkillObject_Base
{
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private GameObject onDeathVfx;
    public int maxAttackAmount { get; private set; }
    private Skill_TimeEcho timeEchoManager;

    private TrailRenderer wispTrail;
    [SerializeField] private float wispMoveSpeed = 15.0f;
    private Transform playerTransform;
    private bool shouldMoveToPlayer;

    private Entity_Health playerHealth;
    private Player_SkillManager skillManager;
    private Entity_StatusHandler statusHander;
    private SkillObject_Health echoHealth;

    public void SetUpEcho(Skill_TimeEcho timeEchoManager)
    {
        this.timeEchoManager = timeEchoManager;
        this.maxAttackAmount = this.timeEchoManager.GetMaxAttackAmount();
        this.playerStats = this.timeEchoManager.player.playerStats;
        this.damageScaleData = this.timeEchoManager.damageScaleData;
        this.playerHealth = this.timeEchoManager.player.entity_Health;
        this.statusHander = this.timeEchoManager.player.status;
        this.skillManager = this.timeEchoManager.player.skillManager;

        this.echoHealth = GetComponent<SkillObject_Health>();
        this.wispTrail = this.GetComponentInChildren<TrailRenderer>();
        this.wispTrail.gameObject.SetActive(false);
        this.playerTransform = this.timeEchoManager.transform.root;

        FlipToTarget();

        this.animator.SetBool("canAttack", this.maxAttackAmount > 0);
        Invoke(nameof(HandleDeath), this.timeEchoManager.GetTimeEchoDuration());
    }

    void Update()
    {
        if (this.shouldMoveToPlayer)
            HandleWispMove();
        else
        {
            animator.SetFloat("yVelocity", this.rb.linearVelocity.y);
            StopHorizontalMovement();

        }
    }

    private void StopHorizontalMovement()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position, Vector2.down, 1.5f, this.whatIsGround);
        if (hit.collider != null)
            this.rb.linearVelocity = new Vector2(0, this.rb.linearVelocity.y);
    }

    public void HandleDeath()
    {
        Instantiate(this.onDeathVfx, this.transform.position, Quaternion.identity);

        if (this.timeEchoManager.ShouldBeWisp())
            TurnIntoWisp();
        else
            Destroy(this.gameObject);
    }

    private void TurnIntoWisp()
    {
        this.animator.gameObject.SetActive(false);
        this.wispTrail.gameObject.SetActive(true);
        this.shouldMoveToPlayer = true;
        this.rb.simulated = false;
    }

    public void PerformAttack()
    {
        DamageEnemiesInRadius(this.targetCheck, 1);

        if (!this.targetGotHit)
            return;

        bool canDuplicate = Random.value < this.timeEchoManager.GetDuplicateChance();

        if (canDuplicate)
        {
            //敌人在残影右侧，新复制出来的残影应该在敌人的右侧；反之亦然
            float offset = this.transform.position.x < this.lastTraget.position.x ? 1 : -1;

            this.timeEchoManager.CreateTimeEcho(this.lastTraget.position + new Vector3(offset, 0));
        }
    }

    private void FlipToTarget()
    {
        Transform target = GetClosestTarget();

        //残影的默认朝向时右侧的，当其在敌人左侧时让其翻身
        if (target != null && target.position.x < this.transform.position.x)
            this.transform.Rotate(0, 180, 0);
    }

    private void HandleWispMove()
    {
        this.transform.position = Vector2.MoveTowards(this.transform.position, this.playerTransform.position, this.wispMoveSpeed * Time.deltaTime);

        if (Vector2.Distance(this.transform.position, this.playerTransform.position) < 0.25f)
        {
            HandleTouchPlayer();
            Destroy(this.gameObject);
        }
    }

    private void HandleTouchPlayer()
    {
        float healAmount = this.echoHealth.lastDamageTaken * this.timeEchoManager.GetPercentOfDamageHealed();
        this.playerHealth.IncreaseHealth(healAmount);

        float coolReduction = this.timeEchoManager.GetCoolDownReduceInSecond();
        this.skillManager.ReduceAllSkillCoolDownBy(coolReduction);

        if (this.timeEchoManager.CanRemoveNegativeEffects())
            this.statusHander.StopAllCoroutines();
    }
}
