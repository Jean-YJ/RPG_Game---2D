using UnityEngine;
using UnityEngine.Rendering;

public enum E_ElementType
{
    None, Fire, Ice, Lightning
}
public class Entity_Stats : MonoBehaviour
{
    // public Stat maxHP;
    public StatSetUpData_SO defaultStatSetUp;
    // public Stat vitality;
    public Stat_ResourceGroup resourceGroup;
    public Stat_MajorGroup majorGroup;
    public Stat_OffenseGroup offenseGroup;
    public Stat_DefenseGroup defenseGroup;

    public AttackData GetAttackData(DamageScaleData scaleData)
    {
        return new AttackData(this, scaleData);
    }

    public float GetMaxHP()
    {
        float baseHP = this.resourceGroup.maxHealth.GetValue();
        float bonusHP = this.majorGroup.vitality.GetValue() * 5; // 每点体力增加5点生命值
        float finalHP = baseHP + bonusHP;
        return finalHP;
    }

    public float GetEvasion()
    {
        float baseEvasion = this.defenseGroup.evasion.GetValue();
        float bonusEvasion = this.majorGroup.agility.GetValue() * 0.5f; // 每点敏捷增加0.5%的闪避率
        float evasionCap = 85f; // 闪避率上限为85%
        float finalEvasion = Mathf.Clamp(baseEvasion + bonusEvasion, 0, evasionCap); // 将闪避率限制在0到85%之间
        return finalEvasion;
    }

    public float GetPhysicalDamage(out bool isCrit, float scaleFactor = 1)
    {
        float damage = GetDamage();

        // 计算暴击情况
        // 暴击率 = 基础暴击率 + 敏捷提供的暴击率
        float critChance = GetCritChance(); // 已经包含了敏捷提供的暴击率加成

        // 暴击倍率 = 基础暴击倍率 + 力量提供的暴击倍率
        float critPower = GetCritPower();

        // 判断是否暴击
        isCrit = Random.Range(0, 100) < critChance;
        float finalDamage = isCrit ? damage * critPower : damage;

        return finalDamage * scaleFactor;
    }

    // 计算伤害时，基础伤害来自攻击力，额外伤害来自力量
    public float GetDamage()
    {
        float baseDamage = this.offenseGroup.damage.GetValue();
        float bonusDamage = this.majorGroup.strength.GetValue(); // 每点力量增加1点伤害

        float finalDamage = baseDamage + bonusDamage;

        return finalDamage;
    }

    // 计算暴击率时，基础暴击率来自属性点，敏捷提供额外的暴击率加成
    public float GetCritChance()
    {
        float baseCritChance = this.offenseGroup.critChance.GetValue();
        float bonsCritChance = this.majorGroup.strength.GetValue() * 0.3f; // 每点力量增加0.3%的暴击率
        float finalCritChance = baseCritChance + bonsCritChance;

        return finalCritChance;
    }

    // 计算暴击倍率时，基础暴击倍率来自属性点，力量提供额外的暴击倍率加成
    public float GetCritPower()
    {
        float baseCritPower = this.offenseGroup.critPower.GetValue();
        float bonusCritPower = this.majorGroup.strength.GetValue() * 0.5f; // 每点力量增加0.5的暴击倍率
        float finalCritPower = (baseCritPower + bonusCritPower) / 100; // 将暴击倍率转换为小数，例如50%变为0.5

        return finalCritPower;
    }

    public float GetArmorMitigation(float armorReduction)
    {
        float armor = GetArmor();

        float effectiveMultiplier = Mathf.Clamp((1 - armorReduction), 0, 1); // 将穿甲效果限制在0到1之间，确保不会出现负数或超过100%的穿甲效果
        float effectiveArmor = armor * effectiveMultiplier; // 计算穿甲后的有效护甲    

        // 计算伤害减免百分比，假设每100点护甲提供50%的伤害减免,护甲越高收益越低
        // 计算公式为：免伤比例 = 护甲值 / (护甲值 + 100) 
        // 例如：100点护甲提供50%减免，200点护甲提供66.7%减免，300点护甲提供75%减免，以此类推
        // 并且减免效果有一个上限，例如80%
        float mitigation = effectiveArmor / (effectiveArmor + 100);
        float mitigationCap = 0.8f; // 减免上限为80%
        float finalMitigation = Mathf.Clamp(mitigation, 0, mitigationCap); // 将减免限制在0到80%之间
        return finalMitigation;
    }
    // 计算护甲时，基础护甲来自属性点，活力提供额外的护甲穿透加成
    public float GetArmor()
    {
        float baseArmor = this.defenseGroup.armor.GetValue();
        float bonusArmor = this.majorGroup.vitality.GetValue(); // 每点体力增加1点护甲
        float finalArmor = baseArmor + bonusArmor;
        return finalArmor;
    }

    public float GetArmorReduction()
    {
        float armorReduction = this.offenseGroup.armorReduction.GetValue() / 100; // 将护甲穿透百分比转换为小数，例如20%变为0.2

        return armorReduction;
    }

    public float GetElementalDamage(out E_ElementType elementType, float scaleFactor = 1)
    {
        //获取元素伤害数值
        float fireDamege = this.offenseGroup.fireDamage.GetValue();
        float iceDamage = this.offenseGroup.iceDamage.GetValue();
        float lightningDamage = this.offenseGroup.lightningDamage.GetValue();

        //确定主要元素类型和伤害值，且单次攻击的元素类型由伤害最高的元素决定
        float highestElementalDamage = Mathf.Max(fireDamege, iceDamage, lightningDamage);
        //假如所有的元素伤害都为0，则该攻击没有元素伤害，元素类型为None
        if (highestElementalDamage <= 0)
        {
            elementType = E_ElementType.None;
            return 0;
        }
        if (highestElementalDamage == fireDamege)
            elementType = E_ElementType.Fire;
        else if (highestElementalDamage == iceDamage)
            elementType = E_ElementType.Ice;
        else
            elementType = E_ElementType.Lightning;

        //次要元素伤害提供50%的效果，智力提供额外的元素伤害加成
        float bonusElementalDamage = this.majorGroup.intelligence.GetValue(); // 每点智力1点元素伤害

        // 非最高元素伤害提供50%的效果
        float weakerFireDamage = elementType == E_ElementType.Fire ? 0 : fireDamege * 0.5f;
        float weakerIceDamage = elementType == E_ElementType.Ice ? 0 : iceDamage * 0.5f;
        float weakerLightningDamage = elementType == E_ElementType.Lightning ? 0 : lightningDamage * 0.5f;
        float weakerElementalDamage = weakerFireDamage + weakerIceDamage + weakerLightningDamage;

        float finalElementalDamage = highestElementalDamage + bonusElementalDamage + weakerElementalDamage;
        return finalElementalDamage * scaleFactor;
    }

    public float GetElementalResistance(E_ElementType elementType)
    {
        float resistance = 0;
        switch (elementType)
        {
            case E_ElementType.Fire:
                resistance = this.defenseGroup.fireRes.GetValue();
                break;
            case E_ElementType.Ice:
                resistance = this.defenseGroup.iceRes.GetValue();
                break;
            case E_ElementType.Lightning:
                resistance = this.defenseGroup.lightningRes.GetValue();
                break;
            default:
                resistance = 0;
                break;
        }
        float bonusResistance = this.majorGroup.intelligence.GetValue() * 0.5f; // 每点智力增加0.5点元素抗性
        float resistanceCap = 75f; // 元素抗性上限为75%
        float finalResistance = Mathf.Clamp(resistance + bonusResistance, 0, resistanceCap) / 100;// 将元素抗性限制在0到75%之间
        return finalResistance;
    }

    public Stat GetStatByType(StatType statType)
    {
        switch (statType)
        {
            case StatType.MaxHealth:
                return this.resourceGroup.maxHealth;
            case StatType.HealthRegen:
                return this.resourceGroup.healthRegen;
            case StatType.Strength:
                return this.majorGroup.strength;
            case StatType.Vitality:
                return this.majorGroup.vitality;
            case StatType.Agility:
                return this.majorGroup.agility;
            case StatType.Intelligence:
                return this.majorGroup.intelligence;
            case StatType.AttackSpeed:
                return this.offenseGroup.attackSpeed;
            case StatType.Damage:
                return this.offenseGroup.damage;
            case StatType.CritPower:
                return this.offenseGroup.critPower;
            case StatType.CritChance:
                return this.offenseGroup.critChance;
            case StatType.ArmorReduction:
                return this.offenseGroup.armorReduction;
            case StatType.FireDamage:
                return this.offenseGroup.fireDamage;
            case StatType.IceDamage:
                return this.offenseGroup.iceDamage;
            case StatType.LightningDamage:
                return this.offenseGroup.lightningDamage;
            case StatType.Armor:
                return this.defenseGroup.armor;
            case StatType.Evasion:
                return this.defenseGroup.evasion;
            case StatType.FireResistance:
                return this.defenseGroup.fireRes;
            case StatType.IceResistance:
                return this.defenseGroup.iceRes;
            case StatType.LightningResistance:
                return this.defenseGroup.lightningRes;
            case StatType.ElementalDamage:
                return null;
            default:
                Debug.LogError($"Invalid stat type: {statType}");
                return null;
        }
    }

    [ContextMenu("Apply Default Stat SetUp")]
    private void ApplyDefaultStatSetUp()
    {
        if (defaultStatSetUp == null)
        {
            Debug.LogError("Default Stat SetUp is not assigned!");
            return;
        }

        // 应用默认属性设置
        this.resourceGroup.maxHealth.SetBaseValue(defaultStatSetUp.maxHealth);
        this.resourceGroup.healthRegen.SetBaseValue(defaultStatSetUp.healthRegen);

        this.majorGroup.strength.SetBaseValue(defaultStatSetUp.strength);
        this.majorGroup.agility.SetBaseValue(defaultStatSetUp.agility);
        this.majorGroup.intelligence.SetBaseValue(defaultStatSetUp.intelligence);
        this.majorGroup.vitality.SetBaseValue(defaultStatSetUp.vitality);

        this.offenseGroup.attackSpeed.SetBaseValue(defaultStatSetUp.attackSpeed);
        this.offenseGroup.damage.SetBaseValue(defaultStatSetUp.damage);
        this.offenseGroup.critChance.SetBaseValue(defaultStatSetUp.critChance);
        this.offenseGroup.critPower.SetBaseValue(defaultStatSetUp.critPower);
        this.offenseGroup.armorReduction.SetBaseValue(defaultStatSetUp.armorReduction);
        this.offenseGroup.fireDamage.SetBaseValue(defaultStatSetUp.fireDamage);
        this.offenseGroup.iceDamage.SetBaseValue(defaultStatSetUp.iceDamage);
        this.offenseGroup.lightningDamage.SetBaseValue(defaultStatSetUp.lightningDamage);

        this.defenseGroup.armor.SetBaseValue(defaultStatSetUp.armor);
        this.defenseGroup.evasion.SetBaseValue(defaultStatSetUp.evasion);
        this.defenseGroup.fireRes.SetBaseValue(defaultStatSetUp.fireResistance);
        this.defenseGroup.iceRes.SetBaseValue(defaultStatSetUp.iceResistance);
        this.defenseGroup.lightningRes.SetBaseValue(defaultStatSetUp.lightningResistance);
    }
}
