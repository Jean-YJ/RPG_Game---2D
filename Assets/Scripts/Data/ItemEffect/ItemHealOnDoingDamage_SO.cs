using UnityEngine;

[CreateAssetMenu(menuName = "RPG SetUp/Item Data/Item effect/Heal On Doing Damage", fileName = "Item effect data - Heal On Doing phy Damage")]
public class ItemHealOnDoingDamage : ItemEffectData_SO
{
    [SerializeField] private float percentageHealedOnAttack = 0.2f;

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);

        this.player.playerCombat.onDoingPhysicalDamage += HealingOnDoingDamage;
    }

    public override void Unsubscribe()
    {
        base.Unsubscribe();

        this.player.playerCombat.onDoingPhysicalDamage -= HealingOnDoingDamage;
        this.player = null;
    }

    private void HealingOnDoingDamage(float damage)
    {
        this.player.entity_Health.IncreaseHealth(damage * this.percentageHealedOnAttack);
    }
}
