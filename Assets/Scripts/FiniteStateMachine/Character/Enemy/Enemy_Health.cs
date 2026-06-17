using UnityEngine;

public class Enemy_Health : Entity_Health
{
    private Enemy enemy => this.GetComponent<Enemy>();
    public override bool TakeDamage(float damage, float elementalDamage, E_ElementType elementType, Transform dealer)
    {
        if (!this.canTakeDamage)
            return false;
        bool isGetDamage = base.TakeDamage(damage, elementalDamage, elementType, dealer);
        if (!isGetDamage)
            return false;

        // Enemy子类中再次进行isDead判定是因为, 如果敌人若在遭受了本次攻击后已经死亡，就不需要再进入战斗状态了
        // Entity_Health父类中的isDead判定只是为了避免对已经死亡的敌人继续造成伤害和播放受击特效等.
        if (this.isDead)
            return false;

        if (dealer.GetComponent<Player>() != null)
            this.enemy.TryEnterBattleState(dealer);

        return true;
    }
}
