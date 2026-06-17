using UnityEngine;

[CreateAssetMenu(menuName = "RPG SetUp/Item Data/Item effect/Refund all skills", fileName = "Item effect data - Refund all skills")]
public class ItemRefundData_SO : ItemEffectData_SO
{
    public override void ExcuteEffect()
    {
        base.ExcuteEffect();

        UI_CanvasRoot canvasRoot = FindFirstObjectByType<UI_CanvasRoot>();
        canvasRoot.skillTree.RefundAllSkills();
    }
}
