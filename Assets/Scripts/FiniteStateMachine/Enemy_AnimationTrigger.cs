using UnityEngine;

public class Enemy_AnimationTrigger : Entity_AnimationTrigger
{
    private Enemy enemy;
    private Enemy_VFX ev;
    protected override void Awake()
    {
        base.Awake();

        this.enemy = this.GetComponentInParent<Enemy>();
        this.ev = this.GetComponentInParent<Enemy_VFX>();
    }

    private void EnableCounterWindow()
    {
        this.ev.SetAttackAlert(true);
        this.enemy.SetCounterWindowStatus(true);
    }
    private void DisableCounterWindow()
    {
        this.ev.SetAttackAlert(false);
        this.enemy.SetCounterWindowStatus(false);
    }
}
