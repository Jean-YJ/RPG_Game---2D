using System.Linq;
using TMPro;
using UnityEngine;

public class UI_SkillTree : MonoBehaviour, ISaveable
{
    [SerializeField] private int skillPoint;
    [SerializeField] private TextMeshProUGUI txt_PointNumber;
    [SerializeField] private UI_TreeConnectHandler[] rootNodes; // 树中的最初根节点集合
    public Player_SkillManager skillManager { get; private set; }
    private UI_TreeNode[] allTreeNodes;


    void Start()
    {
        this.UpdateAllConnection();
        UpdateSkillPointUI();
    }

    public bool EnoughSkillPoint(int cost) => this.skillPoint >= cost;
    public void ReduceSkillPoint(int count)
    {
        this.skillPoint -= count;
        UpdateSkillPointUI();
    }
    public void AddSkillPoint(int count)
    {
        this.skillPoint += count;
        UpdateSkillPointUI();
    }

    [ContextMenu("Update All Connections")]
    public void UpdateAllConnection()
    {
        foreach (var node in this.rootNodes)
        {
            node.UpdateAllConnection();
        }
    }

    [ContextMenu("Reset Skill Tree")]
    public void RefundAllSkills()
    {
        UI_TreeNode[] nodes = this.GetComponentsInChildren<UI_TreeNode>();

        foreach (var node in nodes)
        {
            node.Refund();
        }
    }

    public void UnLockDefaultSkills()
    {
        if (this.skillManager == null)
            this.skillManager = FindAnyObjectByType<Player_SkillManager>();
            // FindFirstObjectByType<Player_SkillManager>();

        this.allTreeNodes = this.GetComponentsInChildren<UI_TreeNode>(true);

        foreach (var node in this.allTreeNodes)
        {
            node.UnLockDefaultSkills();
        }
    }

    private void UpdateSkillPointUI()
    {
        this.txt_PointNumber.text = this.skillPoint.ToString();
    }

    public void LoadData(GameData data)
    {
        // throw new System.NotImplementedException();
        this.skillPoint = data.skillPoint;
        foreach (var node in this.allTreeNodes)
        {
            string skillName = node.skillData.displayName;
            //存储的数据中有记录且 记录的isUnlock为true
            if (data.skillTreeUI.TryGetValue(skillName, out bool isUnlock) && isUnlock)
                node.UnLockBySaveData();
        }

        foreach (var skill in this.skillManager.allSkills)
        {
            if (data.skillUpgrades.TryGetValue(skill.GetSkillType(), out SkillUpgradeType skillUpgradeType))
            {
                var upgradeNode = this.allTreeNodes.FirstOrDefault(node =>
                                        node.skillData.upgradeData.skillUpgradeType == skillUpgradeType);

                if (upgradeNode != null)
                    skill.SetSkillUpgrade(upgradeNode.skillData);
            }

        }

    }

    public void SaveData(ref GameData data)
    {
        // throw new System.NotImplementedException();
        data.skillTreeUI.Clear();
        data.skillUpgrades.Clear();

        data.skillPoint = this.skillPoint;

        foreach (var node in this.allTreeNodes)
        {
            string skillName = node.skillData.displayName;
            data.skillTreeUI[skillName] = node.isUnlocked;
        }

        foreach (var skill in this.skillManager.allSkills)
        {
            SkillType skillType = skill.GetSkillType();
            SkillUpgradeType skillUpgradeType = skill.GetSkillUpgradeType();

            data.skillUpgrades[skillType] = skillUpgradeType;
        }
    }
}
