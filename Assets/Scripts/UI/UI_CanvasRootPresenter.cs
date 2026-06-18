using UnityEngine;

/// <summary>
/// CanvasRoot 的 Presenter。
/// 负责判断 UI 面板的打开/关闭流程，View 只负责真正显示、隐藏和输入转发。
/// </summary>
public class UI_CanvasRootPresenter : UIPresenterBase<UI_CanvasRoot>
{
    private bool skillTreeEnabled;
    private bool inventoryEnabled;

    public override void Attach(UI_CanvasRoot view)
    {
        base.Attach(view);

        // 初始化时读取场景中面板的实际激活状态，避免 Presenter 状态和 prefab 初始状态不一致。
        skillTreeEnabled = view.IsSkillTreeVisible;
        inventoryEnabled = view.IsInventoryVisible;
    }

    /// <summary>
    /// 切换技能树面板。
    /// 这里处理开关状态，具体显示、层级和 Tooltip 关闭由 View 执行。
    /// </summary>
    public void ToggleSkillTreeUI()
    {
        if (!HasView)
            return;

        skillTreeEnabled = !skillTreeEnabled;
        View.ShowSkillTree(skillTreeEnabled);
        View.RefreshPlayerInputByOpenPanels();
    }

    /// <summary>
    /// 切换背包面板。
    /// 背包关闭时同步关闭相关 Tooltip，避免残留悬浮信息。
    /// </summary>
    public void ToggleInventoryUI()
    {
        if (!HasView)
            return;

        inventoryEnabled = !inventoryEnabled;
        View.ShowInventory(inventoryEnabled);
        View.RefreshPlayerInputByOpenPanels();
    }

    /// <summary>
    /// 设置界面快捷键的处理流程。
    /// 如果当前已有全屏 UI 打开，则回到游戏 UI；否则打开设置面板。
    /// </summary>
    public void HandleSettingInput()
    {
        if (!HasView)
            return;

        if (View.HasAnyRootUIOpen())
        {
            SwitchToInGameUI();
            return;
        }

        ShowSettingUI();
        Time.timeScale = 1;
    }

    /// <summary>
    /// 显示或关闭仓库 UI。
    /// 关闭仓库时顺手关闭打造面板和 Tooltip，保持原有行为。
    /// </summary>
    public void ShowStorageUI(bool status)
    {
        if (!HasView)
            return;

        View.ShowStorage(status);
        View.SetPlayerInputEnabled(!status);

        if (!status)
        {
            View.ShowCraft(false);
            View.CloseAllToolTip();
        }
    }

    /// <summary>
    /// 显示或关闭商人 UI。
    /// 打开商人时禁止玩家移动，关闭时恢复玩家输入。
    /// </summary>
    public void ShowMerchantUI(bool status)
    {
        if (!HasView)
            return;

        View.ShowMerchant(status);
        View.SetPlayerInputEnabled(!status);

        if (!status)
            View.CloseAllToolTip();
    }

    /// <summary>
    /// 打开设置面板。
    /// 业务流程由 Presenter 决定，具体切换到哪个 GameObject 由 View 执行。
    /// </summary>
    public void ShowSettingUI()
    {
        if (!HasView)
            return;

        View.CloseAllToolTip();
        View.SetPlayerInputEnabled(false);
        View.SwitchTo(View.uiSetting.gameObject);
    }

    /// <summary>
    /// 切回游戏内主 UI。
    /// 同步重置背包和技能树状态，避免下次打开时状态错乱。
    /// </summary>
    public void SwitchToInGameUI()
    {
        if (!HasView)
            return;

        View.CloseAllToolTip();
        View.SetPlayerInputEnabled(true);
        View.SwitchTo(View.inGameUI.gameObject);

        inventoryEnabled = false;
        skillTreeEnabled = false;
    }

    /// <summary>
    /// 显示死亡面板并禁用整套输入。
    /// </summary>
    public void ShowDeathPanel()
    {
        if (!HasView)
            return;

        View.SwitchTo(View.deathPanel.gameObject);
        View.DisableAllInput();
    }
}
