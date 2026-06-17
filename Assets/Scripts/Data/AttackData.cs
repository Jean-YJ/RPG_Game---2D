using UnityEngine;

/// <summary>
/// 伤害数据，根据Stat、DamageScale和ElementEffect计算伤害
/// </summary>
[System.Serializable]
public class AttackData
{
    public float phyiscalDamage;
    public float elementalDamage;
    public bool isCrit;
    public E_ElementType elementType;
    public ElementEffectData effectData;

    public AttackData(Entity_Stats stats, DamageScaleData scaleData)
    {
        this.phyiscalDamage = stats.GetPhysicalDamage(out this.isCrit, scaleData.phyiscal);
        this.elementalDamage = stats.GetElementalDamage(out this.elementType, scaleData.elemental);

        this.effectData = new ElementEffectData(stats, scaleData);
    }
}
