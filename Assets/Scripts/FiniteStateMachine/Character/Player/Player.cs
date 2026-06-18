using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Player : Entity
{
    private static Player instance;
    public static Player Instance => instance;
    private Player() { }

    public PlayerInputSet p_Input { get; private set; }
    public UI_CanvasRoot canvasRoot;
    public Player_SkillManager skillManager { get; private set; }
    public Player_VFX vfx { get; private set; }
    public Entity_StatusHandler status { get; private set; }
    public Entity_Health entity_Health { get; private set; }
    public Player_Stats playerStats { get; private set; }
    public Player_Combat playerCombat { get; private set; }
    public Inventory_Player playerInventory { get; private set; }

    #region State Variables
    public PlayerState_Idle idleState { get; private set; }
    public PlayerState_Move moveState { get; private set; }
    public PlayerState_Jump jumpState { get; private set; }
    public PlayerState_Fall fallState { get; private set; }
    public PlayerState_WallSlide wallSlideState { get; private set; }
    public PlayerState_WallJump wallJumpState { get; private set; }
    public PlayerState_Dash dashState { get; private set; }
    public PlayerState_BasicAttack basicAttackState { get; private set; }
    public PlayerState_JumpAttack jumpAttackState { get; private set; }
    public PlayerState_Dead deadState { get; private set; }
    public PlayerState_CounterAttack counterAttackState { get; private set; }
    public PlayerState_SwordToss swordTossState { get; private set; }
    public PlayerState_DomainExpansion domainExpansionState { get; private set; }
    #endregion

    [Header("Move Detail")]
    // public Vector2 movementInput;
    public float moveSpeed;
    public float airMoveMultipler = 0.7f;
    public float wallSlideMultipler = 0.4f;
    public float jumpForce = 5.0f;
    public Vector2 wallJumpForce;
    public float dashSpeed = 20.0f;
    public float dashDuration = 0.25f;
    public Vector2 movementInput { get; private set; }
    public Vector2 mousePosition { get; private set; }

    [Header("Attack Detail")]
    //设计在进入攻击时会有一小段的位移
    //给位移状态施加的速度
    public Vector2[] attackVelocity;
    public Vector2 jumpAttackVelocity;
    //位移的时限
    public float attackDuration = 0.1f;
    //重置攻击的时间间隔
    public float comboResetTime = 1.0f;
    private Coroutine queueAttackCor;

    public static UnityAction OnPlayerDead;

    [Header("Ultimate Abliity Detail")]
    public float riseSpeed = 25.0f;
    public float riseMaxDistance = 5.0f;

    void OnEnable()
    {
        if (this.p_Input == null)
            return;

        //绑定前先进行一次解绑，避免重复
        UnbindInputActions();
        BindInputActions();
        this.p_Input.Enable();

        // this.p_Input.Player.Movement.started     输入事件刚开始，例如：开始按下按键
        // this.p_Input.Player.Movement.performed   输入事件进行中，例如：完全按下按键、按键持续处于按下状态
        // this.p_Input.Player.Movement.canceled    输入事件结束，例如：松开按键
        // this.p_Input.Player.Movement.performed += (callbackcontext) =>
        // {
        //     // Debug.Log(callbackcontext.ReadValue<Vector2>());
        //     this.movementInput = callbackcontext.ReadValue<Vector2>();
        // };
        // this.p_Input.Player.Movement.canceled += (callbackcontext) =>
        // {
        //     this.movementInput = Vector2.zero;
        // };
        // this.p_Input.Player.ToggleSkillTreeUI.performed += (callbackcontext) =>
        // {
        //     this.canvasRoot.ToggleSkillTreeUI();
        // };
        // this.p_Input.Player.ToggleInventoryUI.performed += (callbackcontext) =>
        // {
        //     this.canvasRoot.ToggleInventoryUI();
        // };
    }

    private void BindInputActions()
    {
        // this.p_Input.Player.Movement.started     输入事件刚开始，例如：开始按下按键
        // this.p_Input.Player.Movement.performed   输入事件进行中，例如：完全按下按键、按键持续处于按下状态
        // this.p_Input.Player.Movement.canceled    输入事件结束，例如：松开按键
        this.p_Input.Player.Movement.performed += OnMovementPerformed;
        this.p_Input.Player.Movement.canceled += OnMovementCanceled;
        this.p_Input.Player.Spell.performed += OnSpellPerformed;
        this.p_Input.Player.Mouse.performed += OnMousePerformed;
        this.p_Input.Player.Interact.performed += OnInteractPerformed;
        this.p_Input.Player.QuickItem1.performed += OnQuickItem1Performed;
        this.p_Input.Player.QuickItem2.performed += OnQuickItem2Performed;
    }

    private void UnbindInputActions()
    {
        if (this.p_Input == null)
            return;

        this.p_Input.Player.Movement.performed -= OnMovementPerformed;
        this.p_Input.Player.Movement.canceled -= OnMovementCanceled;
        this.p_Input.Player.Spell.performed -= OnSpellPerformed;
        this.p_Input.Player.Mouse.performed -= OnMousePerformed;
        this.p_Input.Player.Interact.performed -= OnInteractPerformed;
        this.p_Input.Player.QuickItem1.performed -= OnQuickItem1Performed;
        this.p_Input.Player.QuickItem2.performed -= OnQuickItem2Performed;
    }

    private void OnMovementPerformed(InputAction.CallbackContext callbackcontext) => this.movementInput = callbackcontext.ReadValue<Vector2>();
    private void OnMovementCanceled(InputAction.CallbackContext callbackcontext) => this.movementInput = Vector2.zero;
    private void OnSpellPerformed(InputAction.CallbackContext callbackcontext)
    {
        this.skillManager.shard.Try2UseSkill();
        this.skillManager.timeEcho.Try2UseSkill();
    }
    private void OnMousePerformed(InputAction.CallbackContext callbackcontext) => this.mousePosition = callbackcontext.ReadValue<Vector2>();
    private void OnInteractPerformed(InputAction.CallbackContext callbackcontext) => TryInteract();
    private void OnQuickItem1Performed(InputAction.CallbackContext callbackcontext) => this.playerInventory.TryToUseQuickItem(1);
    private void OnQuickItem2Performed(InputAction.CallbackContext callbackcontext) => this.playerInventory.TryToUseQuickItem(2);

    protected override void Awake()
    {
        base.Awake();
        instance = this;

        this.canvasRoot = FindAnyObjectByType<UI_CanvasRoot>();
        this.p_Input = new PlayerInputSet();
        this.canvasRoot.SetUpInputUIControl(this.p_Input);
        this.skillManager = this.GetComponent<Player_SkillManager>();
        this.vfx = this.GetComponent<Player_VFX>();
        this.status = this.GetComponent<Entity_StatusHandler>();
        this.entity_Health = this.GetComponent<Entity_Health>();
        this.playerCombat = this.GetComponent<Player_Combat>();
        this.playerInventory = this.GetComponent<Inventory_Player>();
        this.playerStats = this.GetComponent<Player_Stats>();

        this.idleState = new PlayerState_Idle(this.stateMachine, "idle", this);
        this.moveState = new PlayerState_Move(this.stateMachine, "move", this);
        this.jumpState = new PlayerState_Jump(this.stateMachine, "jumpFall", this);
        this.fallState = new PlayerState_Fall(this.stateMachine, "jumpFall", this);
        this.wallSlideState = new PlayerState_WallSlide(this.stateMachine, "wallSlide", this);
        this.wallJumpState = new PlayerState_WallJump(this.stateMachine, "jumpFall", this);
        this.dashState = new PlayerState_Dash(this.stateMachine, "dash", this);
        this.basicAttackState = new PlayerState_BasicAttack(this.stateMachine, "basicAttack", this);
        this.jumpAttackState = new PlayerState_JumpAttack(this.stateMachine, "jumpAttack", this);
        this.deadState = new PlayerState_Dead(this.stateMachine, "dead", this);
        this.counterAttackState = new PlayerState_CounterAttack(this.stateMachine, "counterAttack", this);
        this.swordTossState = new PlayerState_SwordToss(this.stateMachine, "swordToss", this);
        this.domainExpansionState = new PlayerState_DomainExpansion(this.stateMachine, "jumpFall", this);
    }
    protected override void Start()
    {
        base.Start();

        stateMachine.Initialize(this.idleState);
    }

    public void EnterAttackStateWithDealy()
    {
        if (this.queueAttackCor != null)
            StopCoroutine(this.queueAttackCor);
        this.queueAttackCor = StartCoroutine(EnterAttackStateWithDealyCor());
    }
    private IEnumerator EnterAttackStateWithDealyCor()
    {
        yield return new WaitForEndOfFrame();
        this.stateMachine.ChangeState(this.basicAttackState);
    }

    public override void EntityDeath()
    {
        base.EntityDeath();
        this.stateMachine.ChangeState(this.deadState);
        OnPlayerDead?.Invoke();
    }

    protected override IEnumerator SlowDownCor(float duration, float slowMultiplier)
    {
        // return base.SlowDownCor(duration, slowMultiplier);
        float originalMoveSpeed = this.moveSpeed;
        float originalAnimSpeed = this.animator.speed;
        float originalJumpForce = this.jumpForce;
        Vector2 originalWallJumpForce = this.wallJumpForce;
        Vector2 originJumpAttackVelocity = this.jumpAttackVelocity;
        Vector2[] originalAttackVelocity = new Vector2[this.attackVelocity.Length];
        Array.Copy(this.attackVelocity, originalAttackVelocity, this.attackVelocity.Length);

        float speedMultiplier = 1 - slowMultiplier;

        this.moveSpeed *= speedMultiplier;
        this.animator.speed *= speedMultiplier;
        this.jumpForce *= speedMultiplier;
        this.wallJumpForce *= speedMultiplier;
        this.jumpAttackVelocity *= speedMultiplier;
        for (int i = 0; i < this.attackVelocity.Length; i++)
        {
            this.attackVelocity[i] *= speedMultiplier;
        }

        yield return new WaitForSeconds(duration);

        this.moveSpeed = originalMoveSpeed;
        this.animator.speed = originalAnimSpeed;
        this.jumpForce = originalJumpForce;
        this.wallJumpForce = originalWallJumpForce;
        this.jumpAttackVelocity = originJumpAttackVelocity;
        for (int i = 0; i < this.attackVelocity.Length; i++)
        {
            this.attackVelocity[i] = originalAttackVelocity[i];
        }
    }

    public void TeleportPlayerTo(Vector2 position) => this.transform.position = position;

    public void TryInteract()
    {
        Transform closest = null;
        float closestDistance = Mathf.Infinity;

        //获取范围内所有可IInteractable的对象
        Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, 1.5f);

        foreach (var colider in colliders)
        {
            IInteractable interactable = colider.GetComponent<IInteractable>();
            if (interactable == null) continue;

            //得到距离最近的对象
            float distance = Vector2.Distance(this.transform.position, colider.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = colider.transform;
            }
        }

        if (closest == null) return;

        closest.GetComponent<IInteractable>()?.Interact();

    }

    void OnDisable()
    {
        UnbindInputActions();
        if (this.p_Input == null)
            return;

        this.p_Input.Disable();
    }
}
