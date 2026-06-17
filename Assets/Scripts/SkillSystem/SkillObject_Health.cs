using UnityEngine;

public class SkillObject_Health : Entity_Health
{
    protected override void Dead()
    {
        // base.Dead();
        SkillObject_TimeEcho timeEcho = this.GetComponent<SkillObject_TimeEcho>();
        timeEcho.HandleDeath();
    }
}
