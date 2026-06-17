using TMPro;
using UnityEngine;

public class Player_SkillManager : MonoBehaviour
{
    public Skill_Dash dash { get; private set; }
    public Skill_Shard shard { get; private set; }
    public Skill_SwordToss swordToss { get; private set; }
    public Skill_TimeEcho timeEcho { get; private set; }
    public Skill_DomainExpansion domainExpansion { get; private set; }


    public Skill_Base[] allSkills { get; private set; }

    void Awake()
    {
        this.dash = this.GetComponentInChildren<Skill_Dash>();
        this.shard = this.GetComponentInChildren<Skill_Shard>();
        this.swordToss = this.GetComponentInChildren<Skill_SwordToss>();
        this.timeEcho = this.GetComponentInChildren<Skill_TimeEcho>();
        this.domainExpansion = this.GetComponentInChildren<Skill_DomainExpansion>();
        this.allSkills = this.GetComponentsInChildren<Skill_Base>();
    }

    public Skill_Base GetSkillRefByType(SkillType type)
    {
        switch (type)
        {
            case SkillType.Dash:
                return this.dash;
            case SkillType.TimeShard:
                return this.shard;
            case SkillType.SwordThrow:
                return this.swordToss;
            case SkillType.TimeEcho:
                return this.timeEcho;
            case SkillType.DomainExpansion:
                return this.domainExpansion;
            default:
                Debug.Log($"Skill Type {type} is not implemented yet");
                return null;
        }
    }

    public void ReduceAllSkillCoolDownBy(float amount)
    {
        foreach (var skill in this.allSkills)
            skill.ReduceCoolDownBy(amount);
    }
}
