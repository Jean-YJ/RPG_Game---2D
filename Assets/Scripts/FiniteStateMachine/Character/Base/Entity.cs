using System;
using System.Collections;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public StateMachine stateMachine;
    public Animator animator { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public Entity_SFX sfx { get; private set; }


    [Header("Flip Detail")]
    private bool isFacingRight = true;
    public int faceDir { get; private set; } = 1;  //面向右为1，左为-1
    public Action OnFlip;

    [Header("Collision Detail")]
    [SerializeField] private float detectGroundDistance;
    [SerializeField] private float detectWallDistance;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform primaryWallCheck;
    [SerializeField] private Transform secondaryWallCheck;
    public bool isTouchingGround;
    public bool isTouchingWall;
    [SerializeField] public LayerMask whatIsGround;

    private bool isKnocked = false;
    private Coroutine knockBackCoroutine;
    private Coroutine chillCoroutine;


    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    protected virtual void Awake()
    {
        //animator、Rigidbody2D获取一定要在idleState和moveState初始化之前
        this.animator = this.GetComponentInChildren<Animator>();
        this.rb = this.GetComponent<Rigidbody2D>();
        this.sfx = this.GetComponent<Entity_SFX>();

        this.stateMachine = new StateMachine();


    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        this.stateMachine.UpdateActiveState();
        this.HandleCollisionDetect();
    }

    public void SetVelocity(float xVelocity, float yVelocity)
    {
        //如果处于受击中，停止在当前状态下的速度设置，避免受击速度和当前状态下的速度相互干扰
        if (this.isKnocked)
            return;

        this.rb.linearVelocity = new Vector2(xVelocity, yVelocity);
        HandleFlip(xVelocity);
    }

    public void HandleFlip(float xVelocity)
    {
        // Debug.Log("Player Handle:" + this.rb.linearVelocity.x);

        if (xVelocity > 0 && !this.isFacingRight)
            Flip();
        else if (xVelocity < 0 && this.isFacingRight)
            Flip();

        //由this.rb.linearVelocity.x 判定改为this.movementInput.x 判定的原因：
        //1、即使通过SetVelocity手动设置了速度，物理系统仍会造成些许的误差（浮点噪音），
        //使this.rb.linearVelocity出现微弱的偏差 导致判定出现异常
        //2、地图使用的是瓦片地图 + 组合碰撞器，由于组合碰撞器默认存在微小的offset distance，这些间隙
        //会影响碰撞判定，从而导致速度出现微弱的偏差。组合碰撞器默认存在微小的offset distance设为0，
        // 并使用0摩擦材质，可解决this.rb.linearVelocity偏差的问题

        // if (this.movementInput.x > 0 && !this.isFacingRight)
        //     Flip();
        // else if (this.movementInput.x < 0 && this.isFacingRight)
        //     Flip();
    }
    public void Flip()
    {
        // Debug.Log("Flip");
        this.transform.Rotate(new Vector3(0, 180, 0));
        this.isFacingRight = !this.isFacingRight;
        this.faceDir = this.faceDir * -1;

        this.OnFlip?.Invoke();
    }
    private void HandleCollisionDetect()
    {
        this.isTouchingGround = Physics2D.Raycast(this.groundCheck.position, Vector2.down, this.detectGroundDistance, this.whatIsGround);

        if (this.secondaryWallCheck != null)
            this.isTouchingWall = Physics2D.Raycast(this.primaryWallCheck.position, Vector2.right * this.faceDir, this.detectWallDistance, this.whatIsGround)
                            && Physics2D.Raycast(this.secondaryWallCheck.position, Vector2.right * this.faceDir, this.detectWallDistance, this.whatIsGround);
        else
            this.isTouchingWall = Physics2D.Raycast(this.primaryWallCheck.position, Vector2.right * this.faceDir, this.detectWallDistance, this.whatIsGround);
    }

    public void CurrentStateAnimationTrigger()
    {
        this.stateMachine.CurrentState.AnimationTrigger();
    }

    public void ReceiveKnockBack(Vector2 knockBackPower, float duration)
    {
        if (this.knockBackCoroutine != null)
            StopCoroutine(this.knockBackCoroutine);

        this.knockBackCoroutine = StartCoroutine(KnockBackCor(knockBackPower, duration));
    }
    private IEnumerator KnockBackCor(Vector2 knockBackPower, float duration)
    {
        this.isKnocked = true;
        this.rb.linearVelocity = knockBackPower;

        yield return new WaitForSeconds(duration);

        this.rb.linearVelocity = Vector2.zero;
        this.isKnocked = false;
    }

    public virtual void EntityDeath()
    {

    }

    //存在bug，overrideEff是false的话，如果非首次造成减速，减速会不生效
    public virtual void EntitySlowDown(float duration, float slowMultiplier, bool overrideEff = false)
    {
        if (this.chillCoroutine != null)
        {
            if (overrideEff)
                StopCoroutine(this.chillCoroutine);
            else
                return;
        }

        this.chillCoroutine = StartCoroutine(SlowDownCor(duration, slowMultiplier));
    }
    protected virtual IEnumerator SlowDownCor(float duration, float slowMultiplier)
    {
        yield return null;
    }
    public virtual void StopSlowDown()
    {
        StopCoroutine(this.chillCoroutine);
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawLine(this.groundCheck.position, this.groundCheck.position + new Vector3(0, -this.detectGroundDistance, 0));
        Gizmos.DrawLine(this.primaryWallCheck.position, this.primaryWallCheck.position + Vector3.right * this.faceDir * this.detectWallDistance);
        if (this.secondaryWallCheck != null)
            Gizmos.DrawLine(this.secondaryWallCheck.position, this.secondaryWallCheck.position + Vector3.right * this.faceDir * this.detectWallDistance);
    }
}
