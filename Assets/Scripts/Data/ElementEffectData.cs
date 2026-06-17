using System;
using UnityEngine;

/// <summary>
/// 附加元素状态的数据
/// </summary>
[Serializable]
public class ElementEffectData
{
    public float chillDuration;
    public float chillSlowMulitplier;

    public float burnDuration;
    public float burnDamage;

    public float shockDuration;
    public float shockDamage;
    public float shockCharge;

    public ElementEffectData(Entity_Stats stats, DamageScaleData scaleData)
    {
        this.chillDuration = scaleData.chillDuration;
        this.chillSlowMulitplier = scaleData.chillSlowMulitplier;

        this.burnDuration = scaleData.burnDuratin;
        this.burnDamage = stats.offenseGroup.fireDamage.GetValue() * scaleData.burnDamageScale;

        this.shockDuration = scaleData.shockDuration;
        this.shockCharge = scaleData.shockCharge;
        this.shockDamage = stats.offenseGroup.lightningDamage.GetValue() * scaleData.shockDamageScale;
    }
}
