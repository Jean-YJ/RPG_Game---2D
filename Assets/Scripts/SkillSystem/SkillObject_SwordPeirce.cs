using UnityEngine;

public class SkillObject_SwordPeirce : SkillObject_SwordRegular
{
    private int amountToPeirce;

    public override void SetUpSword(Skill_SwordToss swordManager, Vector2 direction)
    {
        base.SetUpSword(swordManager, direction);

        this.amountToPeirce = swordManager.amountToPeirce;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // base.OnTriggerEnter2D(other);
        bool hitGround = other.gameObject.layer == LayerMask.NameToLayer("Ground");

        // 穿透次数为0或撞击到墙壁，则停止
        if (this.amountToPeirce <= 0 || hitGround)
        {
            StopSword(other);
            DamageEnemiesInRadius(this.transform, 0.3f);
            return;
        }

        this.amountToPeirce--;
        DamageEnemiesInRadius(this.transform, 0.3f);
    }
}
