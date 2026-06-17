using UnityEngine;

[CreateAssetMenu(menuName = "RPG SetUp/Item Data/Item effect/Grant Skill effect", fileName = "Item effect data - Grant Skill Point")]
public class ItemGrantSkillData_SO : ItemEffectData_SO
{
    [SerializeField] private int pointToAdd;

    public override void ExcuteEffect()
    {
        // base.ExcuteEffect();
        UI_CanvasRoot canvasRoot = FindFirstObjectByType<UI_CanvasRoot>();

        if (canvasRoot != null)
        {
            canvasRoot.skillTree.AddSkillPoint(pointToAdd);
        }
    }
}
