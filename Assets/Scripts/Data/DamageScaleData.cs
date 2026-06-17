using UnityEngine;

/// <summary>
/// 伤害系数数据
/// </summary>
[System.Serializable]
public class DamageScaleData
{
    // 角色直接攻击的伤害系数
    [Header("Damage")]
    public float phyiscal = 1; 
    public float elemental = 1;

    // 各种元素附加状态的系数
    [Header("Chill")]
    public float chillDuration = 3;
    public float chillSlowMulitplier = .2f;

    [Header("Burn")]
    public float burnDuratin = 3;
    public float burnDamageScale = 1;


    [Header("Shock")]
    public float shockDuration = 3;
    public float shockDamageScale = 1;
    public float shockCharge = .4f;
}
