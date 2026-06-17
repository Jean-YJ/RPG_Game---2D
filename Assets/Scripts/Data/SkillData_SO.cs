using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill Data - ", menuName = "RPG SetUp/Skill Data")]
public class SkillData_SO : ScriptableObject
{
    public bool isUnlockByDefault;
    public string displayName;
    public Sprite icon;
    [TextArea]
    public string description;
    public int cost;
    public SkillType skillType;
    [Header("Upgrade & Upgrade")]
    public UpgradeData upgradeData;
}

[Serializable]
public class UpgradeData
{
    public float coolDown;
    public SkillUpgradeType skillUpgradeType;
    public DamageScaleData damageScaleData;
}
