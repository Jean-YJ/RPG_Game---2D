using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Skill_DomainExpansion : Skill_Base
{
    [SerializeField] private GameObject domainOPrefab;

    [Header("Slowing Down Upgrade")]
    [SerializeField] private float slownDownPercent = 0.9f;
    [SerializeField] private float slowDownDomainDuration = 5;

    // [Header("Spell Casting Upgrade")]
    // [SerializeField] private int spellToCast = 10;
    // [SerializeField] private float spellCastingDomainSlowDown = 1;
    // [SerializeField] private float spellCastingDomainDuration = 5;

    [Header("Shard Cast Upgrade")]
    [SerializeField] private int shardToCast = 10;
    [SerializeField] private float shardCastDomainSlow = 1;
    [SerializeField] private float shardDomainDuration = 8;

    [Header("TimeEcho Cast Upgrade")]
    [SerializeField] private int echoToCast = 10;
    [SerializeField] private float echoCastDomainSlow = 1;
    [SerializeField] private float echoDomainDuration = 8;
    [SerializeField] private float healthToRestoreWithEcho = 0.05f;
    [Space]
    private float spellCastTimer;
    private float spellPerSecond;


    [Header("Domain Detail")]
    public float maxSize = 10;
    public float expandSpeed = 2;


    private List<Enemy> trappedTargets = new List<Enemy>();
    private Transform currentTarget;

    public float GetDomainDuration()
    {
        if (this.skillUpgradeType == SkillUpgradeType.Domain_SlowingDown)
            return this.slowDownDomainDuration;
        else if (this.skillUpgradeType == SkillUpgradeType.Domain_EchoSpam)
            return this.echoDomainDuration;
        else if (this.skillUpgradeType == SkillUpgradeType.Domain_ShardSpam)
            return this.shardDomainDuration;

        return 0;
    }

    public float GetSlowDownPercentage()
    {
        if (this.skillUpgradeType == SkillUpgradeType.Domain_SlowingDown)
            return this.slownDownPercent;
        else if (this.skillUpgradeType == SkillUpgradeType.Domain_EchoSpam)
            return this.echoCastDomainSlow;
        else if (this.skillUpgradeType == SkillUpgradeType.Domain_ShardSpam)
            return this.shardCastDomainSlow;

        return 0;
    }

    private int GetSpellsToCast()
    {
        if (this.skillUpgradeType == SkillUpgradeType.Domain_EchoSpam)
            return this.echoToCast;
        else if (this.skillUpgradeType == SkillUpgradeType.Domain_ShardSpam)
            return this.shardToCast;

        return 0;
    }



    /// <summary>
    /// 激活了Domain_EchoSpam或Domain_ShardSpam不会立刻释放领域
    /// </summary>
    /// <returns></returns>
    public bool InstantDomain()
    {
        return this.skillUpgradeType != SkillUpgradeType.Domain_EchoSpam &&
                this.skillUpgradeType != SkillUpgradeType.Domain_ShardSpam;
    }

    public void CreateDomain()
    {
        // Debug.Log("领域展开！");
        this.spellPerSecond = GetSpellsToCast() / GetDomainDuration();

        GameObject domainObj = Instantiate(this.domainOPrefab, this.transform.position, Quaternion.identity);
        domainObj.GetComponent<SkillObject_DomainExpansion>().SetUpDomainExpansion(this);
    }

    public void AddTarget(Enemy targetToAdd)
    {
        this.trappedTargets.Add(targetToAdd);
    }
    public void RemoveTarget(Enemy targetToRemove)
    {
        this.trappedTargets.Remove(targetToRemove);
    }

    public void ClearTargets()
    {
        foreach (var enemy in this.trappedTargets)
            enemy.StopSlowDown();

        this.trappedTargets = new List<Enemy>();
    }

    private Transform GetRandomTargetInDomain()
    {
        //domain内的敌人可能死亡或失效
        this.trappedTargets.RemoveAll(item => item.transform == null || item.enemyHealth.isDead == true);

        if (this.trappedTargets.Count == 0)
            return null;

        int randomIndex = Random.Range(0, this.trappedTargets.Count);
        Transform target = this.trappedTargets[randomIndex].transform;

        return target;
    }

    public void DoSpellCasting()
    {
        spellCastTimer -= Time.deltaTime;

        if (this.currentTarget == null)
            this.currentTarget = GetRandomTargetInDomain();

        if (this.currentTarget != null && this.spellCastTimer < 0)
        {
            CastSpell(this.currentTarget);
            this.spellCastTimer = 1 / this.spellPerSecond;
            this.currentTarget = null;
        }
    }

    private void CastSpell(Transform target)
    {
        if (this.skillUpgradeType == SkillUpgradeType.Domain_EchoSpam)
        {
            Vector3 offset = Random.value < 0.5f ? new Vector2(1, 0) : new Vector2(-1, 0);
            this.skillManager.timeEcho.CreateTimeEcho(target.transform.position + offset);
        }

        if (this.skillUpgradeType == SkillUpgradeType.Domain_ShardSpam)
        {
            this.skillManager.shard.CreateRawShard(target, true);
        }
    }
}
