using System.Collections.Generic;
using UnityEngine;

public class SkillObject_SwordBounce : SkillObject_SwordRegular
{
    [SerializeField] private float bounceSpeed = 15.0f;
    private int bounceCount;
    private Transform nextTarget;
    private Collider2D[] enemyTargets;
    private List<Transform> selectedBefore = new List<Transform>();

    public override void SetUpSword(Skill_SwordToss swordManager, Vector2 direction)
    {
        base.SetUpSword(swordManager, direction);

        this.animator.SetTrigger("swordSpin");
        this.bounceSpeed = swordManager.bounceSpeed;
        this.bounceCount = swordManager.bounceCount;
    }

    protected override void Update()
    {
        // base.Update();
        HandleBounce();
        HandleComeback();
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // base.OnTriggerEnter2D(other);
        if (this.enemyTargets == null)
        {
            //获取范围内的所有敌人作为目标集
            this.enemyTargets = this.GetEnemiesAround(this.transform, 10.0f);
            // 只有第一次使用刚体模拟运动，后续的弹射通过逻辑控制
            this.rb.simulated = false;
        }

        DamageEnemiesInRadius(this.transform, 1);
        if (this.enemyTargets.Length <= 1 || bounceCount == 0)
        {
            GetSwordBackToPlayer();
        }
        else
        {
            this.nextTarget = GetNextTarget();
        }
    }

    private List<Transform> GetAliveTargets()
    {
        List<Transform> aliveTargets = new List<Transform>();

        //enemy可能在获取到enemyTargets集合中后死亡或被删除，在此处进行一次筛选
        foreach (var target in this.enemyTargets)
        {
            if (target != null)
                aliveTargets.Add(target.transform);
        }
        return aliveTargets;
    }

    private List<Transform> GetValidTargets()
    {
        List<Transform> aliveTargets = GetAliveTargets();
        List<Transform> validTargets = new List<Transform>();

        foreach (var target in aliveTargets)
        {
            // selectedBefore中已被弹射过的目标，则不优先作为目标
            if (target != null && !this.selectedBefore.Contains(target))
                validTargets.Add(target);
        }

        if (validTargets.Count > 0)
            return validTargets;
        //validTargets的数量为0，说明所有的目标已被弹射过了。
        // 将selectedBefore清空，所有的目标可作为下一次弹射的目标了
        else
        {
            selectedBefore.Clear();
            return aliveTargets;
        }
    }

    private Transform GetNextTarget()
    {
        List<Transform> validTargets = GetValidTargets();

        int randomIndex = Random.Range(0, validTargets.Count);
        Transform nextTarget = validTargets[randomIndex];
        this.selectedBefore.Add(nextTarget);

        return nextTarget;
    }

    private void BounceToNextTarget()
    {
        this.nextTarget = GetNextTarget();
        this.bounceCount--;
    }

    private void HandleBounce()
    {
        if (this.nextTarget == null)
            return;

        this.transform.position = Vector2.MoveTowards(this.transform.position, this.nextTarget.position, this.bounceSpeed * Time.deltaTime);
        if (Vector2.Distance(this.transform.position, this.nextTarget.position) < 0.75f)
        {
            DamageEnemiesInRadius(this.transform, 1.0f);
            BounceToNextTarget();

            if (this.bounceCount == 0 || this.nextTarget == null)
            {
                nextTarget = null; //this.bounceCount = 0 回收时将目标置空
                GetSwordBackToPlayer();
            }
        }
    }
}
