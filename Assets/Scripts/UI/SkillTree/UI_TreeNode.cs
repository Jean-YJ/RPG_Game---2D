using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UI_TreeNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private UI_CanvasRoot canvasRoot;
    private UI_SkillTree skillTree;
    private UI_TreeConnectHandler connectHandler;

    [Header("UnLock Detail")]
    // 技能树存在分支，选择了某个分支就无法再选择其他分支了
    public bool isLockedByConflict;
    //本机能树节点是否已解锁，已解锁的节点会显示正常颜色，未解锁的节点会显示灰色
    public bool isUnlocked;
    [SerializeField] private string lockedColorHex = "#9F9797";
    // 解锁本节点所需的全部前置节点
    public UI_TreeNode[] unlockNeededNodes;
    // 与本节点互斥的分支节点
    public UI_TreeNode[] conflictNodes;

    [Header("Skill Detail")]
    [SerializeField] private string skillName = "Skill Name";
    [SerializeField] private int skillCost = 1;
    [SerializeField] private Image skillIcon;
    public SkillData_SO skillData;
    private Color lastIconColor;

    void Start()
    {
        if (this.isUnlocked)
            UpdateNodeIconColor(Color.white);
        else
            UpdateNodeIconColor(GetColorByHex(this.lockedColorHex));
        UnLockDefaultSkills();
    }

    public void UnLockDefaultSkills()
    {
        SetNeededComponents();

        if (this.skillData.isUnlockByDefault)
            UnLockNode();
    }

    private void SetNeededComponents()
    {
        if (this.canvasRoot == null)
            this.canvasRoot = this.GetComponentInParent<UI_CanvasRoot>();
        if (this.skillTree == null)
            this.skillTree = this.GetComponentInParent<UI_SkillTree>(true);
        if (this.connectHandler == null)
            this.connectHandler = this.GetComponent<UI_TreeConnectHandler>();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        // Debug.Log("Pointer entered the tree node.");
        this.canvasRoot.skillToolTip.ShowToolTip(this, this.skillData, true, this.GetComponent<RectTransform>());

        // 未解锁 且 没有被冲突分支锁定,鼠标放上会高亮显示
        // 已解锁 或 未解锁但被被冲突分支锁定，鼠标放上无特殊显示
        if (this.isUnlocked || this.isLockedByConflict)
            return;

        ToggleHighLight(true);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        // Debug.Log("Pointer exited the tree node.");

        this.canvasRoot.skillToolTip.ShowToolTip(false);
        this.canvasRoot.skillToolTip.StopLockedEff();

        // 未解锁 且 没有被冲突分支锁定,鼠标离开会从高亮回复
        if (this.isUnlocked || this.isLockedByConflict)
            return;

        ToggleHighLight(false);
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        // Debug.Log("Pointer clicked the tree node.");
        if (this.CanBeUnlocked())
        {
            this.UnLockNode();
        }
        else if (this.isLockedByConflict)
        {
            this.canvasRoot.skillToolTip.ShowLockedEff();
        }
    }

    private void ToggleHighLight(bool isHighLight)
    {
        Color highLightIconColor = Color.white * 0.8f;
        highLightIconColor.a = 1.0f; // 不透明，避免看到链接线的子节点占位图

        Color colorToApply = isHighLight ? highLightIconColor : this.lastIconColor;

        UpdateNodeIconColor(colorToApply);
    }

    private bool CanBeUnlocked()
    {
        // 已解锁的和被放弃的技能节点不能再被解锁
        if (this.isUnlocked || this.isLockedByConflict)
        {
            return false;
        }

        // 角色技能点是否足够
        if (!this.skillTree.EnoughSkillPoint(this.skillCost))
            return false;

        //前置节点存在未解锁
        foreach (var node in this.unlockNeededNodes)
        {
            if (!node.isUnlocked)
                return false;
        }
        //解锁了互斥节点
        foreach (var node in this.conflictNodes)
        {
            if (node.isUnlocked)
                return false;
        }

        return true;
    }

    private Color GetColorByHex(string hex)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(hex, out color))
        {
            return color;
        }
        else
        {
            Debug.LogWarning("Invalid hex color string: " + hex);
            return Color.white; // 默认颜色
        }
    }

    private void UpdateNodeIconColor(Color color)
    {
        if (this.skillIcon == null)
            return;

        this.lastIconColor = this.skillIcon.color;
        this.skillIcon.color = color;
    }

    private void UnLockNode()
    {
        if (this.isUnlocked)
            return;

        this.isUnlocked = true;
        this.UpdateNodeIconColor(Color.white); // 解锁后恢复原色
        LockConflictNodes(); // 锁住分支冲突的节点

        //扣除技能点
        if (this.skillData.isUnlockByDefault)
            this.skillTree.ReduceSkillPoint(0);
        else
            this.skillTree.ReduceSkillPoint(this.skillCost);
        //更新链接线的显示
        this.connectHandler.UpdateUnlockConnectLineImage(this.isUnlocked);

        // 通过技能管理器进行能力的解锁
        this.skillTree.skillManager.GetSkillRefByType(this.skillData.skillType).
                                    SetSkillUpgrade(this.skillData);
    }

    public void UnLockBySaveData()
    {
        this.isUnlocked = true;
        this.UpdateNodeIconColor(Color.white); // 解锁后恢复原色
        LockConflictNodes(); // 锁住分支冲突的节
        this.connectHandler.UpdateUnlockConnectLineImage(this.isUnlocked);//更新链接线的显示
    }

    private void LockConflictNodes()
    {
        foreach (var node in this.conflictNodes)
        {
            node.isLockedByConflict = true;
            node.LockAllChildNodeForConflict();
        }
    }

    public void LockAllChildNodeForConflict()
    {
        this.isLockedByConflict = true;
        foreach (var node in this.connectHandler.GetChildNodes())
            node.LockAllChildNodeForConflict();
    }

    // 回退技能
    public void Refund()
    {
        // 默认解锁的技能节点不能被回退，未解锁的技能节点也不需要被回退(且回退了会造成技能点凭空增多的异常)
        if (this.skillData.isUnlockByDefault || !this.isUnlocked)
        {
            return;
        }

        this.isUnlocked = false;
        this.isLockedByConflict = false;
        this.UpdateNodeIconColor(this.GetColorByHex(this.lockedColorHex));

        this.skillTree.AddSkillPoint(this.skillCost);

        this.connectHandler.UpdateUnlockConnectLineImage(false);
    }

    // 这个方法可以在编辑器中被调用(Inspector窗口中变量发生改变时自动调用)，用于实时更新节点的数据
    void OnValidate()
    {
        if (this.skillData == null)
            return;

        this.skillName = this.skillData.displayName;
        this.skillIcon.sprite = this.skillData.icon;
        this.skillCost = this.skillData.cost;
        this.gameObject.name = "UI_TreeNode - " + this.skillData.displayName;
    }

    void OnDisable()
    {
        if (this.isLockedByConflict)
            UpdateNodeIconColor(GetColorByHex(this.lockedColorHex));
        if (this.isUnlocked)
            UpdateNodeIconColor(Color.white);
    }
}
