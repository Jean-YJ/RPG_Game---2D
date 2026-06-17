using Unity.VisualScripting;
using UnityEngine;

public class Skill_Base : MonoBehaviour
{
    public Player player { get; private set; }
    public Player_SkillManager skillManager { get; private set; }
    public DamageScaleData damageScaleData { get; private set; }
    [Header("General Details")]
    [SerializeField] protected SkillType skillType; //技能类型
    [SerializeField] protected SkillUpgradeType skillUpgradeType; //强化类型
    [SerializeField] protected float coolDown;
    private float lastTimeSkillUsed;

    protected virtual void Awake()
    {
        this.player = this.GetComponentInParent<Player>();
        this.skillManager = this.GetComponentInParent<Player_SkillManager>();

        // 确保程序运行后技能可以直接使用
        this.lastTimeSkillUsed = this.lastTimeSkillUsed - coolDown;

        this.damageScaleData = new DamageScaleData();
    }
    public virtual bool CanUseSkill()
    {
        if (this.skillUpgradeType == SkillUpgradeType.None)
            return false;
        if (OnCoolDown())
            return false;

        return true;
    }
    public virtual void Try2UseSkill() { }
    protected bool OnCoolDown() => Time.time < this.coolDown + this.lastTimeSkillUsed;
    public void SetSkillOnCoolDown()
    {
        this.lastTimeSkillUsed = Time.time;
        this.player.canvasRoot.inGameUI.GetSkillSlot(this.skillType).StartCoolDown(this.coolDown);
    }
    public void ReduceCoolDownBy(float coolDownReuction)
    {
        this.lastTimeSkillUsed += coolDownReuction;
    }
    public void ResetCoolDown()
    {
        this.lastTimeSkillUsed = Time.time - this.coolDown;
        this.player.canvasRoot.inGameUI.GetSkillSlot(this.skillType).ResetCoolDown();
    }

    //将强化后的数值赋值给技能
    public void SetSkillUpgrade(SkillData_SO skillData)
    {
        UpgradeData upgrade = skillData.upgradeData;

        this.skillUpgradeType = upgrade.skillUpgradeType;
        this.coolDown = upgrade.coolDown;
        this.damageScaleData = upgrade.damageScaleData;

        this.player.canvasRoot.inGameUI.GetSkillSlot(this.skillType).SetSkillSlot(skillData);

        ResetCoolDown();
    }

    protected bool IsUpgradeUnLocked(SkillUpgradeType type) => this.skillUpgradeType == type;

    public SkillType GetSkillType() => this.skillType;
    public SkillUpgradeType GetSkillUpgradeType() => this.skillUpgradeType;
}
