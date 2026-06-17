using System;
using UnityEngine;

public class SkillObject_Shard : SkillObject_Base
{
    [SerializeField] private GameObject vfxPrefab;

    private Transform target;
    private float speed;

    public event Action OnExplode;
    private Skill_Shard shard;

    public void Explode()
    {
        DamageEnemiesInRadius(this.targetCheck, this.checkRadius);
        GameObject vfx_Obj = Instantiate(this.vfxPrefab, this.transform.position, Quaternion.identity);
        vfx_Obj.GetComponentInChildren<SpriteRenderer>().color = this.shard.player.vfx.GetOnHitVfxColor(this.currentUsedElement);

        this.OnExplode?.Invoke();

        Destroy(this.gameObject);
    }

    /// <summary>
    /// 设置爆炸时间，根据skill的数据获取伤害来源和伤害系数
    /// Shard即使没有被触发，一段时间后也应该自行爆炸
    /// </summary>
    /// <param name="shard"></param>
    public void SetUpShard(Skill_Shard shard)
    {
        this.shard = shard;
        this.playerStats = this.shard.player.playerStats;
        this.damageScaleData = this.shard.damageScaleData;

        float detonationTime = this.shard.GetDetonateTime();
        Invoke(nameof(Explode), detonationTime);
    }
    public void SetUpShard(Skill_Shard shard, float detonationTime, bool canMove, float speed,Transform target = null)
    {
        this.shard = shard;
        this.playerStats = this.shard.player.playerStats;
        this.damageScaleData = this.shard.damageScaleData;

        Invoke(nameof(Explode), detonationTime);

        if (canMove)
            MoveToClosestTarget(speed,target);
    }

    public void MoveToClosestTarget(float speed, Transform newTarget = null)
    {
        this.target = newTarget == null ? GetClosestTarget() : newTarget;
        this.speed = speed;
    }

    void Update()
    {
        if (this.target == null)
            return;

        this.transform.position = Vector3.MoveTowards(this.transform.position, this.target.position, this.speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Enemy>() == null)
            return;

        Explode();
    }
}
