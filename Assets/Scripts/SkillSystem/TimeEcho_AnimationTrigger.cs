using UnityEngine;

public class TimeEcho_AnimationTrigger : MonoBehaviour
{
    private SkillObject_TimeEcho timeEcho;
    void Awake()
    {
        this.timeEcho = this.GetComponentInParent<SkillObject_TimeEcho>();
    }

    private void AttackTrigger()
    {
        this.timeEcho.PerformAttack();
    }

    private void TryTerminate(int currentAttackIndex)
    {
        if (currentAttackIndex == timeEcho.maxAttackAmount)
            this.timeEcho.HandleDeath();
    }
}
