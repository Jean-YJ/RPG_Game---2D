using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_StatSlot : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rect;
    private UI_CanvasRoot canvasRoot;
    private Entity_Stats playerStats;

    [SerializeField] private StatType statType;
    [SerializeField] private TextMeshProUGUI statNameText;
    [SerializeField] private TextMeshProUGUI statValueText;
    private void Awake()
    {
        this.rect = GetComponent<RectTransform>();
        this.canvasRoot = GetComponentInParent<UI_CanvasRoot>();
        this.playerStats = FindFirstObjectByType<Player_Stats>();

        // Subscribe to stat change events so this slot updates when modifiers/base change
        // 这里的逻辑是，在UI_StatSlot界面被创建时，就去监听Player_Stats中对应属性类型的Stat对象的OnStatChanged事件，
        // 当属性值发生变化时，Stat对象会触发OnStatChanged事件，
        // UI_StatSlot界面监听到这个事件后会调用UpdateStatValue方法来更新显示的属性数值
        Stat statToWatch = this.playerStats.GetStatByType(this.statType);
        if (statToWatch != null)
        {
            statToWatch.OnStatChanged += UpdateStatValue;
            UpdateStatValue();
        }
    }
    void OnValidate()
    {
        this.gameObject.name = $"txt_Stat - {GetStatsTextByType(statType)}";
        statNameText.text = GetStatsTextByType(statType);
    }

    public void UpdateStatValue()
    {
        Stat statToUpdate = this.playerStats.GetStatByType(this.statType);

        if (statToUpdate == null && this.statType != StatType.ElementalDamage)
        {

            return;
        }


        float value = 0;
        switch (this.statType)
        {
            //Major Group
            case StatType.Strength:
                value = this.playerStats.majorGroup.strength.GetValue();
                break;
            case StatType.Agility:
                value = this.playerStats.majorGroup.agility.GetValue();
                break;
            case StatType.Intelligence:
                value = this.playerStats.majorGroup.intelligence.GetValue();
                break;
            case StatType.Vitality:
                value = this.playerStats.majorGroup.vitality.GetValue();
                break;

            //Offense Group
            case StatType.AttackSpeed:
                // 数据是小数，所以乘以100转换成百分比显示
                value = this.playerStats.offenseGroup.attackSpeed.GetValue() * 100;
                break;
            case StatType.Damage:
                // 直接调用Entity_Stats中的GetDamage方法，获取伤害值(基础+属性加成)
                value = this.playerStats.GetDamage();
                break;
            case StatType.CritChance:
                // 已经包含了敏捷提供的暴击率加成
                value = this.playerStats.GetCritChance();
                break;
            case StatType.CritPower:
                value = this.playerStats.GetCritPower() * 100;
                break;
            case StatType.ArmorReduction:
                //乘以100转换成百分比显示
                value = this.playerStats.GetArmorReduction() * 100;
                break;


            //Defense Group
            case StatType.MaxHealth:
                value = this.playerStats.GetMaxHP();
                break;
            case StatType.HealthRegen:
                value = this.playerStats.resourceGroup.healthRegen.GetValue();
                break;
            case StatType.Armor:
                value = this.playerStats.GetArmor();
                break;
            case StatType.Evasion:
                value = this.playerStats.GetEvasion();
                break;

            // Elemental Damage Group
            case StatType.FireDamage:
                value = this.playerStats.offenseGroup.fireDamage.GetValue();
                break;
            case StatType.IceDamage:
                value = this.playerStats.offenseGroup.iceDamage.GetValue();
                break;
            case StatType.LightningDamage:
                value = this.playerStats.offenseGroup.lightningDamage.GetValue();
                break;
            case StatType.ElementalDamage:
                value = this.playerStats.GetElementalDamage(out E_ElementType elementType);
                break;

            // Elemental Resistance Group
            case StatType.IceResistance:
                value = this.playerStats.GetElementalResistance(E_ElementType.Ice) * 100;
                break;
            case StatType.FireResistance:
                value = this.playerStats.GetElementalResistance(E_ElementType.Fire) * 100;
                break;
            case StatType.LightningResistance:
                value = this.playerStats.GetElementalResistance(E_ElementType.Lightning) * 100;
                break;
        }

        this.statValueText.text = IsPercentageStat(this.statType) ? value + "%" : value.ToString();
    }
    private string GetStatsTextByType(StatType type)
    {
        switch (type)
        {
            case StatType.Strength:
                return "Strength";
            case StatType.Agility:
                return "Agility";
            case StatType.Intelligence:
                return "Intelligence";
            case StatType.Vitality:
                return "Vitality";
            case StatType.MaxHealth:
                return "Max Health";
            case StatType.HealthRegen:
                return "Health Regenerateion";
            case StatType.AttackSpeed:
                return "Attack Speed";
            case StatType.Damage:
                return "Damage";
            case StatType.CritChance:
                return "Critical Chance";
            case StatType.CritPower:
                return "Critical Power";
            case StatType.ArmorReduction:
                return "Armor Reduction";
            case StatType.FireDamage:
                return "Fire Damage";
            case StatType.IceDamage:
                return "Ice Damage";
            case StatType.LightningDamage:
                return "Lightning Damage";
            case StatType.ElementalDamage:
                return "Elemental Damage";
            case StatType.Armor:
                return "Armor";
            case StatType.Evasion:
                return "Evasion";
            case StatType.IceResistance:
                return "Ice Resistance";
            case StatType.FireResistance:
                return "Fire Resistance";
            case StatType.LightningResistance:
                return "Lightning Resistance";
            default:
                return "Unknown Stat";
        }
    }

    private bool IsPercentageStat(StatType type)
    {
        switch (type)
        {
            case StatType.CritChance:
            case StatType.CritPower:
            case StatType.ArmorReduction:
            case StatType.IceResistance:
            case StatType.FireResistance:
            case StatType.LightningResistance:
            case StatType.AttackSpeed:
            case StatType.Evasion:
                return true;
            default:
                return false;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // throw new System.NotImplementedException();
        this.canvasRoot.statToolTip.ShowToolTip(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // throw new System.NotImplementedException();
        this.canvasRoot.statToolTip.ShowToolTip(true, this.statType, this.rect);
    }

    private void OnDestroy()
    {
        // Unsubscribe from stat change events to prevent memory leaks
        Stat statToWatch = this.playerStats != null ? this.playerStats.GetStatByType(this.statType) : null;
        if (statToWatch != null)
            statToWatch.OnStatChanged -= UpdateStatValue;
    }
}
