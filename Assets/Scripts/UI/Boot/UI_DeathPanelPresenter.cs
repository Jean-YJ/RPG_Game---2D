/// <summary>
/// 死亡面板的 Presenter。
/// 负责处理死亡后的场景流转，View 只负责接收按钮事件并转发。
/// </summary>
public class UI_DeathPanelPresenter : UIPresenterBase<UI_DeathPanel>
{
    private const string TownSceneName = "Level_0";
    private const string MainMenuSceneName = "MainMenu";

    /// <summary>
    /// 回到城镇复活点。
    /// 场景名称集中在 Presenter 中，避免 View 层散落魔法字符串。
    /// </summary>
    public void GoToTown()
    {
        GameManager.Instance.ChangeScene(TownSceneName, RespawnType.NonSpecific);
    }

    /// <summary>
    /// 回到最近检查点。
    /// 具体重开当前场景的逻辑仍由 GameManager 负责。
    /// </summary>
    public void GoToCheckPoint()
    {
        GameManager.Instance.ReStartScene();
    }

    /// <summary>
    /// 返回主菜单。
    /// View 不关心场景名，避免 UI 显示层直接管理游戏流程。
    /// </summary>
    public void GoToMainMenu()
    {
        GameManager.Instance.ChangeScene(MainMenuSceneName, RespawnType.NonSpecific);
    }
}
