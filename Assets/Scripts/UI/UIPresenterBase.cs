/// <summary>
/// UI Presenter 的轻量基类。
/// Presenter 持有 View 引用并处理业务逻辑，View 负责显示和输入转发。
/// </summary>
public abstract class UIPresenterBase<TView> where TView : class, IUIView
{
    protected TView View { get; private set; }

    // 是否已经绑定 UI View，子类在执行业务逻辑前应先判断该状态。
    protected bool HasView => View != null;

    /// <summary>
    /// 绑定 UI View。
    /// </summary>
    /// <param name="view">实现 IUIView 的 UI View 对象。</param>
    public virtual void Attach(TView view)
    {
        View = view;
    }

    /// <summary>
    /// 解除 UI View 引用，避免 View 销毁后 Presenter 继续持有旧对象。
    /// </summary>
    public virtual void Detach()
    {
        View = null;
    }
}
