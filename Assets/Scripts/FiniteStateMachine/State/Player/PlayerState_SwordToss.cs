
using UnityEngine;

public class PlayerState_SwordToss : PlayerState
{
    private Camera mainCamera;
    public PlayerState_SwordToss(StateMachine stateMachine, string animBoolSymbol, Player player) : base(stateMachine, animBoolSymbol, player)
    {
    }

    public override void Enter()
    {
        base.Enter();

        this.player.skillManager.swordToss.SetDotEnable(true);

        if (this.mainCamera != Camera.main)
            this.mainCamera = Camera.main;
    }

    public override void Update()
    {
        base.Update();

        // 投掷瞄准期间不应该移动
        this.player.SetVelocity(0, this.rb.linearVelocity.y);
        Vector2 directToMouse = DirectionToMouse();
        // 根据鼠标和玩家的位置来判断是否需要转向
        this.player.HandleFlip(directToMouse.x);

        // 显示轨迹预测
        this.player.skillManager.swordToss.PredictTrajectoryPoint(directToMouse);

        // 松开投掷键，取消状态
        // 进行投掷攻击后（stateTrigger已经为true了），如果仍按着投掷瞄准，此时不应该保持在该状态了，应该退出
        if (this.player.p_Input.Player.RangeAttack.WasReleasedThisFrame() || this.stateTrigger)
            this.stateMachine.ChangeState(this.player.idleState);

        // 投掷瞄准期间按下攻击键，进行投掷攻击
        if (this.player.p_Input.Player.BasicAttack.WasPerformedThisFrame())
        {
            this.animator.SetBool("swordTossPerformed", true);

            this.player.skillManager.swordToss.ConfirmTrajectoryDir(directToMouse);
            this.player.skillManager.swordToss.SetDotEnable(false);

            // 创建swrod实体
        }

    }

    public override void Exit()
    {
        base.Exit();

        this.player.skillManager.swordToss.SetDotEnable(false);
        // 如果是通过投掷攻击退出状态的话 swordTossPerformed会为true
        // 需要将其重置为false，否则下次再次进入投掷瞄准会直接进行攻击
        this.animator.SetBool("swordTossPerformed", false);
    }

    private Vector2 DirectionToMouse()
    {
        Vector2 playerPos = this.player.transform.position;
        Vector2 worldMousePos = this.mainCamera.ScreenToWorldPoint(this.player.mousePosition);

        Vector2 dir = worldMousePos - playerPos;
        return dir.normalized;
    }
}
