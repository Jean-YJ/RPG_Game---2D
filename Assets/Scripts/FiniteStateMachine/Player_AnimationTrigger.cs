using UnityEngine;

public class Player_AnimationTrigger : Entity_AnimationTrigger
{
    private Player player;

    protected override void Awake()
    {
        base.Awake();

        this.player = GetComponentInParent<Player>();
    }

    private void TossSword() => this.player?.skillManager.swordToss.TossSword();
}
