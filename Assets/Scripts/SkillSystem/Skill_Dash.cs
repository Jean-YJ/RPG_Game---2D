using UnityEngine;

public class Skill_Dash : Skill_Base
{
    //具体Dash技能能力的实现逻辑

    public void OnStartEff()
    {
        if (IsUpgradeUnLocked(SkillUpgradeType.Dash_CloneOnStart) || IsUpgradeUnLocked(SkillUpgradeType.Dash_CloneOnStartAndArrival))
            CreateClone();

        if (IsUpgradeUnLocked(SkillUpgradeType.Dash_ShardOnShart) || IsUpgradeUnLocked(SkillUpgradeType.Dash_ShardOnStartAndArrival))
            CreateShard();

    }
    public void OnArrivalEff()
    {
        if (IsUpgradeUnLocked(SkillUpgradeType.Dash_CloneOnStartAndArrival))
            CreateClone();

        if (IsUpgradeUnLocked(SkillUpgradeType.Dash_ShardOnStartAndArrival))
            CreateShard();
    }

    private void CreateClone()
    {
        // Debug.Log("Create TIME ECHO!");
        skillManager.timeEcho.CreateTimeEcho();
    }

    private void CreateShard()
    {
        // Debug.Log("Create TIME Shard!");
        this.skillManager.shard.CreateRawShard();
    }
}
