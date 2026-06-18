using UnityEngine;

public class UI_DeathPanel : MonoBehaviour, IUIView
{
    private UI_DeathPanelPresenter presenter;

    private void Awake()
    {
        EnsurePresenter();
    }

    /// <summary>
    /// 按钮事件入口：回到城镇。
    /// 方法名保持不变，避免 Canvas.prefab 中已有 Button 绑定丢失。
    /// </summary>
    public void GoToTown()
    {
        EnsurePresenter();
        this.presenter.GoToTown();
    }

    /// <summary>
    /// 按钮事件入口：回到最近检查点。
    /// View 只转发输入，具体流程由 Presenter 决定。
    /// </summary>
    public void GoToCheckPoint()
    {
        EnsurePresenter();
        this.presenter.GoToCheckPoint();
    }

    /// <summary>
    /// 按钮事件入口：返回主菜单。
    /// View 不直接写场景名，减少 UI 层和游戏流程的耦合。
    /// </summary>
    public void GoToMainMenu()
    {
        EnsurePresenter();
        this.presenter.GoToMainMenu();
    }

    /// <summary>
    /// 确保 View 已绑定 Presenter。
    /// Presenter 不挂在场景物体上，避免本轮需要改 prefab。
    /// </summary>
    private void EnsurePresenter()
    {
        if (this.presenter != null)
            return;

        this.presenter = new UI_DeathPanelPresenter();
        this.presenter.Attach(this);
    }

    private void OnDestroy()
    {
        this.presenter?.Detach();
    }
}
