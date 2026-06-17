using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Skill_Shard : Skill_Base
{
    [SerializeField] private GameObject shardPrefab;
    [SerializeField] private float detonateTime = 2;
    private SkillObject_Shard currentShard;
    private Entity_Health eh;

    [Header("SkillUpgrade Shard_MoveToEnemy Detail")]
    [SerializeField] private float shardSpeed = 7.0f;

    [Header("SkillUpgrade Shard_Multicast Detail")]
    [SerializeField] private int maxCharges = 3;
    [SerializeField] private int currentCharge = 3;
    [SerializeField] private bool isCharging = false;
    [Header("SkillUpgrade Shard_Teleport Detail")]
    [SerializeField] private float shardExitDuration = 10.0f;
    [Header("SkillUpgrade Shard_TeleportHpRewind Detail")]
    [SerializeField] private float savedHealthPercentage;

    protected override void Awake()
    {
        base.Awake();

        this.currentCharge = this.maxCharges;
        this.eh = this.GetComponentInParent<Entity_Health>();
    }

    public override void Try2UseSkill()
    {
        // base.Try2UseSkill();
        if (!CanUseSkill())
            return;
        if (IsUpgradeUnLocked(SkillUpgradeType.Shard))
            HandleShardRegular();
        if (IsUpgradeUnLocked(SkillUpgradeType.Shard_MoveToEnemy))
            HandleShardMoving();
        if (IsUpgradeUnLocked(SkillUpgradeType.Shard_Multicast))
            HandleShardMulticast();
        if (IsUpgradeUnLocked(SkillUpgradeType.Shard_Teleport))
            HandleShardTeleport();
        if (IsUpgradeUnLocked(SkillUpgradeType.Shard_TeleportHpRewind))
            HandleShardTeleportAndHpRewind();

    }

    private void HandleShardRegular()
    {
        CreateShard();

        SetSkillOnCoolDown();
    }

    private void HandleShardMoving()
    {
        CreateShard();
        this.currentShard.MoveToClosestTarget(this.shardSpeed);

        SetSkillOnCoolDown();
    }

    private void HandleShardMulticast()
    {
        if (this.currentCharge <= 0)
            return;

        CreateShard();
        this.currentShard.MoveToClosestTarget(this.shardSpeed);
        this.currentCharge--;

        if (!this.isCharging)
            StartCoroutine(ShardReChargeCor());

    }
    private IEnumerator ShardReChargeCor()
    {
        this.isCharging = true;
        while (this.currentCharge < this.maxCharges)
        {
            yield return new WaitForSeconds(this.coolDown);
            this.currentCharge++;
        }
        this.isCharging = false;
    }

    private void HandleShardTeleport()
    {
        if (this.currentShard == null)
            CreateShard();
        else
        {
            SwapPlayerAndShard();
            SetSkillOnCoolDown();
        }
    }
    private void SwapPlayerAndShard()
    {
        Vector2 playerPosition = this.player.transform.position;
        Vector2 shardPosition = this.currentShard.transform.position;

        this.currentShard.Explode();
        this.currentShard.transform.position = playerPosition;

        this.player.TeleportPlayerTo(shardPosition);
    }

    private void HandleShardTeleportAndHpRewind()
    {
        if (this.currentShard == null)
        {
            CreateShard();
            this.savedHealthPercentage = this.eh.GetCurrentHealthPercentage();
        }
        else
        {
            SwapPlayerAndShard();
            this.eh.SetCurrentHealthByPercentage(this.savedHealthPercentage);
            SetSkillOnCoolDown();
        }
    }

    public void CreateShard()
    {
        GameObject shardObj = Instantiate(this.shardPrefab, this.transform.position, Quaternion.identity);
        this.currentShard = shardObj.GetComponent<SkillObject_Shard>();
        this.currentShard.SetUpShard(this);

        if (IsUpgradeUnLocked(SkillUpgradeType.Shard_Teleport) || IsUpgradeUnLocked(SkillUpgradeType.Shard_TeleportHpRewind))
            currentShard.OnExplode += ForceCoolDown;
    }

    public void CreateRawShard(Transform target = null, bool domainForceMove = false)
    {
        bool canMove = domainForceMove != false ? domainForceMove :
            IsUpgradeUnLocked(SkillUpgradeType.Shard_MoveToEnemy) || IsUpgradeUnLocked(SkillUpgradeType.Shard_Multicast);

        GameObject shardObj = Instantiate(this.shardPrefab, this.transform.position, Quaternion.identity);
        SkillObject_Shard obj_shard = shardObj.GetComponent<SkillObject_Shard>();

        obj_shard.SetUpShard(this, this.detonateTime, canMove, this.shardSpeed, target);
    }

    public float GetDetonateTime()
    {
        if (this.skillUpgradeType == SkillUpgradeType.Shard_Teleport || this.skillUpgradeType == SkillUpgradeType.Shard_TeleportHpRewind)
            return this.shardExitDuration;

        return this.detonateTime;
    }

    private void ForceCoolDown()
    {
        if (!OnCoolDown())
        {
            SetSkillOnCoolDown();
            currentShard.OnExplode -= ForceCoolDown;
        }
    }
}
