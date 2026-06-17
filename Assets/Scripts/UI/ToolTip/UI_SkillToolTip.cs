using System.Collections;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UI_SkillToolTip : UI_ToolTip
{
    private UI_CanvasRoot canvasroot;
    private UI_SkillTree SkillTree;

    [SerializeField] private TextMeshProUGUI txt_SkillName;
    [SerializeField] private TextMeshProUGUI txt_SkillDes;
    [SerializeField] private TextMeshProUGUI txt_SkillCooldown;
    [SerializeField] private TextMeshProUGUI txt_SkillReq;

    [Space]
    [SerializeField] private string metConditionHex;
    [SerializeField] private string notMetConditionHex;
    [SerializeField] private string importantInfoHex;
    [SerializeField] private Color exampleColor;
    [SerializeField] private string lockedSkillText = "You've taken a diffrent path - this skill is now locked.";
    private Coroutine textBlinkEffCor;

    protected override void Awake()
    {
        base.Awake();
        this.canvasroot = this.GetComponentInParent<UI_CanvasRoot>();
        this.SkillTree = this.canvasroot.GetComponentInChildren<UI_SkillTree>(true);
    }

    public void ShowToolTip(UI_TreeNode node, SkillData_SO skillData, bool show, RectTransform target = null)
    {
        base.ShowToolTip(show, target);

        if (!show)
            return;

        this.txt_SkillName.text = skillData.displayName;
        this.txt_SkillDes.text = skillData.description;
        this.txt_SkillCooldown.text = $"Cooldown: {skillData.upgradeData.coolDown} seconds.";

        if (node == null)
        {
            this.txt_SkillReq.text = "";
            return;
        }

        string skillLockedText = GetColoredText(this.importantInfoHex, this.lockedSkillText);
        string requirements = node.isLockedByConflict ? skillLockedText :
                        GetRequirementText(node.skillData.cost, node.unlockNeededNodes, node.conflictNodes);

        this.txt_SkillReq.text = requirements;
    }

    public void ShowLockedEff()
    {
        StopLockedEff();
        this.textBlinkEffCor = StartCoroutine(TextBlinkEffCoroutine(this.txt_SkillReq, 0.15f, 4));
    }

    public void StopLockedEff()
    {
        if (this.textBlinkEffCor != null)
            StopCoroutine(this.textBlinkEffCor);
    }

    private IEnumerator TextBlinkEffCoroutine(TextMeshProUGUI text, float blinkInterval, int blinkCount)
    {
        for (int i = 0; i < blinkCount; i++)
        {
            text.text = GetColoredText(this.notMetConditionHex, this.lockedSkillText);
            yield return new WaitForSeconds(blinkInterval);

            text.text = GetColoredText(this.importantInfoHex, this.lockedSkillText);
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private string GetRequirementText(int cost, UI_TreeNode[] unlockNeededNodes, UI_TreeNode[] conflictedNodes)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Requirements: ");

        string costColor = this.SkillTree.EnoughSkillPoint(cost) ? this.metConditionHex : this.notMetConditionHex;
        // string costText = $"<color={costColor}>- {cost} skill points</color>";
        string costText = GetColoredText(costColor, $"- {cost} skill points");
        sb.AppendLine(costText);

        foreach (var node in unlockNeededNodes)
        {
            if (node == null) continue;
            string nodeColor = node.isUnlocked ? this.metConditionHex : this.notMetConditionHex;
            // string nodeText = $"<color={nodeColor}>- {node.skillData.skillName}</color>";
            string nodeText = GetColoredText(nodeColor, $"- {node.skillData.displayName}");
            sb.AppendLine(nodeText);

        }

        if (conflictedNodes.Length <= 0)
            return sb.ToString();

        sb.AppendLine(); // 空行
        // sb.AppendLine($"<color={this.importantInfoHex}>{"Locks Out: "}</color>");
        sb.AppendLine(GetColoredText(this.importantInfoHex, "Locks Out: "));

        foreach (var node in conflictedNodes)
        {
            if (node == null) continue;
            // string nodeText = $"<color={this.importantInfoHex}>- {node.skillData.skillName}</color>";
            string nodeText = GetColoredText(this.importantInfoHex, $"- {node.skillData.displayName}");
            sb.AppendLine(nodeText);
        }

        return sb.ToString();
    }
}
