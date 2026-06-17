using UnityEngine;

[CreateAssetMenu(menuName = "RPG SetUp/Item Data/Item effect/Heal effect", fileName = "Item effect data - Heal")]
public class ItemHealData_SO : ItemEffectData_SO
{
    [SerializeField] private float healPercentage = 0.1f;

    public override void ExcuteEffect()
    {
        base.ExcuteEffect();

        Player player = FindAnyObjectByType<Player>();
        float healAmount = player.playerStats.GetMaxHP() * healPercentage;
        player.entity_Health.IncreaseHealth(healAmount);
    }
}
