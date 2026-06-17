using UnityEngine;

public class Skill_TimeEcho : Skill_Base
{
    [SerializeField] private GameObject timeEchoPrefab;
    [SerializeField] private float timeEchoDuration;

    [Header("Attack Upgrade")]
    [SerializeField] private int maxAttackAmount = 3;
    [SerializeField] private float duplicateChance = 0.3f;

    [Header("Heal Wisp Upgrade")]
    [SerializeField] private float damagePercentHealed = 0.5f;
    [SerializeField] private float coolDownReduceInSeconds = 1.0f;




    public override void Try2UseSkill()
    {
        // base.Try2UseSkill();
        if (!CanUseSkill())
            return;

        CreateTimeEcho();
        SetSkillOnCoolDown();
    }

    public void CreateTimeEcho(Vector2? targetPos = null)
    {
        Vector2 pos = targetPos ?? this.transform.position;
        GameObject timeEchoObj = Instantiate(this.timeEchoPrefab, pos, Quaternion.identity);
        timeEchoObj.GetComponent<SkillObject_TimeEcho>().SetUpEcho(this);
    }

    public float GetTimeEchoDuration()
    {
        return this.timeEchoDuration;
    }

    public int GetMaxAttackAmount()
    {
        if (this.skillUpgradeType == SkillUpgradeType.TimeEcho_SingleAttack || this.skillUpgradeType == SkillUpgradeType.TimeEcho_ChanceToDuplicate)
            return 1;
        if (this.skillUpgradeType == SkillUpgradeType.TimeEcho_MultiAttack)
            return this.maxAttackAmount;

        return 0;
    }
    public float GetDuplicateChance()
    {
        if (this.skillUpgradeType != SkillUpgradeType.TimeEcho_ChanceToDuplicate)
            return 0;

        return this.duplicateChance;
    }

    public bool ShouldBeWisp()
    {
        return this.skillUpgradeType == SkillUpgradeType.TimeEcho_HealWisp ||
                this.skillUpgradeType == SkillUpgradeType.TimeEcho_CleanseWisp ||
                this.skillUpgradeType == SkillUpgradeType.TimeEcho_CooldownWisp;
    }

    public float GetPercentOfDamageHealed()
    {
        if (!ShouldBeWisp())
            return 0;

        return this.damagePercentHealed;
    }

    public float GetCoolDownReduceInSecond()
    {
        if (this.skillUpgradeType != SkillUpgradeType.TimeEcho_CooldownWisp)
            return 0;

        return this.coolDownReduceInSeconds;
    }

    public bool CanRemoveNegativeEffects()
    {
        return this.skillUpgradeType == SkillUpgradeType.TimeEcho_CleanseWisp;
    }
}
