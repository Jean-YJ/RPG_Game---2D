using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Stat
{
    [SerializeField] private float baseValue;

    // 该属性的增益/减益列表，例如装备、buff等对该属性的影响
    [SerializeField] private List<StatModifier> modifiers = new List<StatModifier>();
    private float finalValue;

    // 该属性的最终值是否需要重新计算，默认为true(首次获取属性状态)，
    // 当增益/减益发生变化时设置为true，在GetValue方法中进行判断，
    // 如果为true则重新计算最终值并将该标志重置为false
    private bool needToBeReCalculated = true;

    // 当属性值发生变化时触发的事件，UI_PlayerStat界面需要监听该事件来更新显示的属性数值
    public event Action OnStatChanged;

    public float GetValue()
    {
        if (this.needToBeReCalculated)
        {
            this.finalValue = GetFinalValue();
            this.needToBeReCalculated = false;
        }
        return this.finalValue;
    }
    // 后续有关该属性的值的变化都可在本类中进行计算，
    // 如基本生命值为100，佩戴饰品或buff可提升生命值，相关的计算在本类中进行
    public void AddModifier(float value, string source)
    {
        StatModifier modifier = new StatModifier(value, source);
        this.modifiers.Add(modifier);

        this.needToBeReCalculated = true;
        // 当属性值发生变化时触发事件，通知UI界面更新显示的属性数值
        OnStatChanged?.Invoke();
    }
    public void RemoveModifier(string source)
    {
        this.modifiers.RemoveAll(modifer => modifer.source == source);

        this.needToBeReCalculated = true;
        // 当属性值发生变化时触发事件，通知UI界面更新显示的属性数值
        OnStatChanged?.Invoke();
    }

    private float GetFinalValue()
    {
        float finalValue = baseValue;
        foreach (var modifier in modifiers)
        {
            finalValue += modifier.value;
        }
        return finalValue;
    }

    public void SetBaseValue(float value)
    {
        this.baseValue = value;
        this.needToBeReCalculated = true;
        // 当属性值发生变化时触发事件，通知UI界面更新显示的属性数值
        OnStatChanged?.Invoke();
    }

}
[Serializable]
public class StatModifier
{
    public float value;
    public string source; // 该增益/减益的来源，例如装备、buff等

    public StatModifier(float value, string source)
    {
        this.value = value;
        this.source = source;
    }
}

public enum StatType
{
    MaxHealth,
    HealthRegen,
    Strength,
    Agility,
    Intelligence,
    Vitality,
    AttackSpeed,
    Damage,
    CritChance,
    CritPower,
    ArmorReduction,
    FireDamage,
    IceDamage,
    LightningDamage,
    Armor,
    Evasion,
    IceResistance,
    FireResistance,
    LightningResistance,
    ElementalDamage
}