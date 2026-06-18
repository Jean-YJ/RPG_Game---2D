using UnityEngine;

/// <summary>
/// 主菜单面板的 Presenter。
/// 负责处理主菜单的业务流程，View 只负责显示和把按钮点击转发到这里。
/// </summary>
public class UI_MainMenuPresenter : UIPresenterBase<UI_MainMenu>
{
    private const string MainMenuBGMGroup = "playList_MainMenu";
    private const string ButtonClickSFX = "btn_click";

    /// <summary>
    /// 主菜单进入时的初始化流程。
    /// 这里统一安排“加载设置显示、淡入表现、播放 BGM”，避免 View 直接依赖多个 Manager。
    /// </summary>
    public void Initialize()
    {
        if (!HasView)
            return;

        View.LoadSettingPanelAudio();
        View.PlayFadeIn();
        AudioManager.Instance.StartBGM(MainMenuBGMGroup);
    }

    /// <summary>
    /// 响应“开始游戏”按钮。
    /// 播放按钮音效和继续游戏属于业务流程，放在 Presenter 中统一处理。
    /// </summary>
    public void PlayGame()
    {
        AudioManager.Instance.PlayGlobalSFX(ButtonClickSFX);
        GameManager.Instance.ContinuePlay();
    }

    /// <summary>
    /// 响应“退出游戏”按钮。
    /// View 不直接调用 Application，方便后续在这里加入二次确认或平台差异处理。
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
}
