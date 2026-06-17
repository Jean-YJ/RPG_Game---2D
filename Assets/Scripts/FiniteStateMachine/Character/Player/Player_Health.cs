using UnityEngine;

public class Player_Health : Entity_Health
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
            Dead();
    }

    protected override void Dead()
    {
        base.Dead();

        // GameManager.Instance.SetLastPlayerPosition(this.transform.position);
        // GameManager.Instance.ReStartScene();
        Player.Instance.canvasRoot.ShowDeathPanel();
    }
}
