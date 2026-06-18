using UnityEngine;

public class UI_MainMenu : MonoBehaviour, IUIView
{
    private UI_MainMenuPresenter presenter;

    private void Awake()
    {
        EnsurePresenter();
    }

    void Start()
    {
        EnsurePresenter();
        this.presenter.Initialize();
    }

    /// <summary>
    /// 按钮事件入口：开始游戏。
    /// 方法名保持不变，避免 MainMenu.unity 中已有 Button 绑定丢失。
    /// </summary>
    public void PlayGame()
    {
        EnsurePresenter();
        this.presenter.PlayGame();
    }

    /// <summary>
    /// 按钮事件入口：退出游戏。
    /// 方法名保持不变，避免 MainMenu.unity 中已有 Button 绑定丢失。
    /// </summary>
    public void QuitGame()
    {
        EnsurePresenter();
        this.presenter.QuitGame();
    }

    /// <summary>
    /// View 层显示职责：让设置面板按存档数据刷新 Slider 显示。
    /// Presenter 负责决定何时调用，View 负责找到同一 Canvas 下的 UI 组件并执行显示刷新。
    /// </summary>
    public void LoadSettingPanelAudio()
    {
        UI_Setting setting = transform.root.GetComponentInChildren<UI_Setting>(true);
        setting?.LoadAudioSetting();
    }

    /// <summary>
    /// View 层显示职责：播放主菜单淡入效果。
    /// 空引用保护可以避免缺少 FadeScreen 时直接中断主菜单流程。
    /// </summary>
    public void PlayFadeIn()
    {
        UI_FadeScreen fadeScreen = transform.root.GetComponentInChildren<UI_FadeScreen>();
        fadeScreen?.FadeIn();
    }

    /// <summary>
    /// 确保 View 已绑定 Presenter。
    /// 使用代码创建 Presenter，暂时不要求在 prefab 上额外挂组件，降低试点改造成本。
    /// </summary>
    private void EnsurePresenter()
    {
        if (this.presenter != null)
            return;

        this.presenter = new UI_MainMenuPresenter();
        this.presenter.Attach(this);
    }

    private void OnDestroy()
    {
        this.presenter?.Detach();
    }
}
