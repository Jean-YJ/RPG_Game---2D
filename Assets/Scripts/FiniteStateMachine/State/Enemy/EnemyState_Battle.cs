using System.ComponentModel;
using UnityEngine;

public class EnemyState_Battle : EnemyState
{
    private Transform player; // 当前被锁定的玩家目标或残影目标
    private Transform lastTarget; // 上一次锁定的目标，用于目标切换
    private float lastTimeWasInBattle;
    public EnemyState_Battle(StateMachine stateMachine, string animBoolSymbol, Enemy enemy) : base(stateMachine, animBoolSymbol, enemy)
    {
    }

    public override void Enter()
    {
        base.Enter();
        this.UpdateBattleTimer();
        if (this.player == null)
            this.player = this.enemy.GetDamageDealerReference();

        // 距离Player过近时，Enemy会后退以保持距离
        if (this.ShouldRetreat())
        {
            int directionToPlayer = this.DirectionToPlayer();

            // 后跳方向应与Player所在方向相反，因此乘以-1
            this.rb.linearVelocity = new Vector2((this.enemy.retreatVelocity.x * this.enemy.slowDownMultiplier) * -directionToPlayer, this.enemy.retreatVelocity.y);
            // 后退时Enemy的朝向应与Player所在方向相同，因此传入directionToPlayer
            if (directionToPlayer != 0)
                this.enemy.HandleFlip(directionToPlayer);
        }

    }
    public override void Update()
    {
        base.Update();

        // 如果Player离开了Battle状态的检测范围，Enemy会继续保持Battle状态一段时间，之后Enemy会切换回Idle状态
        // 如果在这段时间内Player重新进入了检测范围，Enemy会继续保持Battle状态
        if (this.enemy.PlayerDetected() == true)
        {
            this.UpdateBattleTimer();
            UpdateTargetIfNeed();
        }
        if (this.BattleTimeIsOver())
        {
            this.stateMachine.ChangeState(this.enemy.idleState);
            return;
        }

        //检测Player是否进入了攻击范围
        if (this.WithinAttackRange() && this.enemy.PlayerDetected())
        {
            this.stateMachine.ChangeState(this.enemy.attackState);
            return;
        }

        // 根据Player的位置调整Enemy的移动方向和速度
        this.HandleVelocityIfNeed();

    }

    // private float DistanceToPlayer()
    // {
    //     if (this.player == null)
    //         return float.MaxValue;

    //     return Mathf.Abs(this.enemy.transform.position.x - this.player.position.x);
    // }

    /// <summary>
    /// 根据Player的位置调整Enemy的移动方向和速度
    /// 如果Enemy碰到了墙壁，Enemy会停止移动并调整面向方向以朝向Player
    /// 如果Player在Enemy的水平容差范围内，Enemy会停止水平移动以避免频繁调整面向方向导致的视觉抖动
    /// 否则，Enemy会以battleMoveSpeed朝向Player移动
    /// </summary>
    private void HandleVelocityIfNeed()
    {
        int dir = this.DirectionToPlayer();

        //如果Enemy碰到了墙壁，Enemy会停止移动
        if (this.enemy.isTouchingWall)
        {
            // 当目标Player在Enemy的另一侧时，Enemy需要调整面向方向以朝向Player
            if (dir != this.enemy.faceDir)
            {
                this.enemy.HandleFlip(dir);
            }
            //停止移动
            this.enemy.SetVelocity(0, this.rb.linearVelocity.y);
            return;
        }

        // 如果Player在Enemy的水平距离在容差范围内，Enemy会停止水平移动以避免频繁调整面向方向导致的视觉抖动
        if (dir == 0 && this.AbsoluteVerticalDistanceToPlayer() > this.enemy.verticalTolerance)
        {
            this.enemy.SetVelocity(0, this.rb.linearVelocity.y);
            return;
        }
        // 正常移动
        this.enemy.SetVelocity(this.enemy.GetBattleMoveSpeed() * dir, this.rb.linearVelocity.y);
    }

    private void UpdateTargetIfNeed()
    {
        if (this.enemy.PlayerDetected() == false)
            return;

        Transform newTatget = this.enemy.PlayerDetected().transform;
        //检测到的新目标与当前记录的锁定目标不同，则切换目标
        if (newTatget != this.lastTarget)
        {
            this.lastTarget = newTatget;
            this.player = newTatget;
        }
    }

    // 以下是一些辅助方法，用于计算Enemy与Player之间的距离、方向以及Battle状态的持续时间等

    // HorizontalDistanceToPlayer返回正值表示Player在Enemy右侧，Enemy的朝向为右，
    // 返回负值表示Player在Enemy左侧，Enemy的朝向为左
    private float HorizontalDistanceToPlayer()
    {
        if (this.player == null)
            return float.MaxValue;

        return this.player.position.x - this.enemy.transform.position.x;
    }

    // AbsoluteHorizontalDistanceToPlayer返回Enemy与Player之间的水平距离的绝对值
    private float AbsoluteHorizontalDistanceToPlayer()
    {
        float deltaX = this.HorizontalDistanceToPlayer();

        if (deltaX == float.MaxValue)
            return float.MaxValue;

        return Mathf.Abs(deltaX);
    }

    // AbsoluteVerticalDistanceToPlayer返回Enemy与Player之间的垂直距离的绝对值
    private float AbsoluteVerticalDistanceToPlayer()
    {
        if (this.player == null)
            return float.MaxValue;

        return Mathf.Abs(this.player.position.y - this.enemy.transform.position.y);
    }

    // WithinAttackRange方法用于检测Player是否进入了攻击范围，
    // 攻击范围由Enemy的attackDistance和verticalTolerance共同决定
    private bool WithinAttackRange()
    {
        // 水平距离小于attackDistance且垂直距离小于verticalTolerance时，认为Player进入了攻击范围
        return this.enemy.attackDistance >= this.AbsoluteHorizontalDistanceToPlayer()
                && this.enemy.verticalTolerance >= this.AbsoluteVerticalDistanceToPlayer();
    }

    // DirectionToPlayer方法用于判断Player相对于Enemy的方向，
    // 返回1表示Player在Enemy右侧，返回-1表示Player在Enemy左侧，返回0表示Player在Enemy的水平容差范围内或特殊情况
    private int DirectionToPlayer()
    {
        if (this.player == null)
            return 0;

        //HorizontalDistanceToPlayer返回正值表示Player在Enemy右侧，Enemy的朝向为右，
        //返回负值表示Player在Enemy左侧，Enemy的朝向为左
        float deltaX = this.HorizontalDistanceToPlayer();
        if (deltaX == float.MaxValue)
            return 0;

        // 如果绝对值小于水平容差则返回0，表示Player在Enemy很近的位置，Enemy不需要调整面向方向，
        // 避免频繁调整面向方向导致的视觉抖动
        if (Mathf.Abs(deltaX) <= this.enemy.horizontalTolerance)
            return 0;

        return deltaX > 0 ? 1 : -1;
    }

    private void UpdateBattleTimer() => this.lastTimeWasInBattle = Time.time;
    private bool BattleTimeIsOver() => Time.time > this.lastTimeWasInBattle + this.enemy.battleDuration;
    private bool ShouldRetreat() => this.AbsoluteHorizontalDistanceToPlayer() < this.enemy.minRetreatDistance;
}
