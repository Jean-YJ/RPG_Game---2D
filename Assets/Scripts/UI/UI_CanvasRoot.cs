using UnityEngine;
using UnityEngine.InputSystem;

public class UI_CanvasRoot : MonoBehaviour, IUIView
{
    private static UI_CanvasRoot instance;
    public static UI_CanvasRoot Instance => instance;
    private UI_CanvasRoot() { }

    private PlayerInputSet p_InputSet;
    private UI_CanvasRootPresenter presenter;
    public bool alternativeInput;

    [SerializeField] private GameObject[] uiElements;
    public UI_SkillToolTip skillToolTip;
    public UI_SkillTree skillTree;
    public UI_ItemToolTip itemToolTip;
    public UI_StatToolTip statToolTip;
    public UI_Inventory inventory;
    public UI_Storage storage;
    public UI_Craft craft;
    public UI_Merchant merchant;
    public UI_InGame inGameUI;
    public UI_Setting uiSetting;
    public UI_DeathPanel deathPanel;
    public UI_FadeScreen fadeScreen;

    public bool IsSkillTreeVisible => this.skillTree != null && this.skillTree.gameObject.activeSelf;
    public bool IsInventoryVisible => this.inventory != null && this.inventory.gameObject.activeSelf;

    void Awake()
    {
        instance = this;

        this.skillToolTip = this.GetComponentInChildren<UI_SkillToolTip>();
        this.itemToolTip = this.GetComponentInChildren<UI_ItemToolTip>();
        this.statToolTip = this.GetComponentInChildren<UI_StatToolTip>();

        this.skillTree = this.GetComponentInChildren<UI_SkillTree>(true);
        this.inventory = this.GetComponentInChildren<UI_Inventory>(true);
        this.storage = this.GetComponentInChildren<UI_Storage>(true);
        this.craft = this.GetComponentInChildren<UI_Craft>(true);
        this.merchant = this.GetComponentInChildren<UI_Merchant>(true);
        this.inGameUI = this.GetComponentInChildren<UI_InGame>(true);
        this.uiSetting = this.GetComponentInChildren<UI_Setting>(true);
        this.deathPanel = this.GetComponentInChildren<UI_DeathPanel>(true);
        this.fadeScreen = this.GetComponentInChildren<UI_FadeScreen>(true);

        EnsurePresenter();
    }

    void Start()
    {
        this.skillTree.UnLockDefaultSkills();
    }

    public void SetUpInputUIControl(PlayerInputSet inputSet)
    {
        UnbindInputUIControl();
        this.p_InputSet = inputSet;

        if (this.p_InputSet == null)
            return;

        BindInputUIControl();
    }

    private void BindInputUIControl()
    {
        this.p_InputSet.UI.SkillTreeUI.performed += OnSkillTreeUIPerformed;
        this.p_InputSet.UI.InventoryUI.performed += OnInventoryUIPerformed;
        this.p_InputSet.UI.Alternative.performed += OnAlternativePerformed;
        this.p_InputSet.UI.Alternative.canceled += OnAlternativeCanceled;
        this.p_InputSet.UI.SettingUI.performed += OnSettingUIPerformed;
    }

    private void UnbindInputUIControl()
    {
        if (this.p_InputSet == null)
            return;

        this.p_InputSet.UI.SkillTreeUI.performed -= OnSkillTreeUIPerformed;
        this.p_InputSet.UI.InventoryUI.performed -= OnInventoryUIPerformed;
        this.p_InputSet.UI.Alternative.performed -= OnAlternativePerformed;
        this.p_InputSet.UI.Alternative.canceled -= OnAlternativeCanceled;
        this.p_InputSet.UI.SettingUI.performed -= OnSettingUIPerformed;
    }

    private void OnDestroy()
    {
        UnbindInputUIControl();
        this.presenter?.Detach();
    }

    private void OnSkillTreeUIPerformed(InputAction.CallbackContext ctx) => ToggleSkillTreeUI();
    private void OnInventoryUIPerformed(InputAction.CallbackContext ctx) => ToggleInventoryUI();
    private void OnAlternativePerformed(InputAction.CallbackContext ctx) => this.alternativeInput = true;
    private void OnAlternativeCanceled(InputAction.CallbackContext ctx) => this.alternativeInput = false;

    private void OnSettingUIPerformed(InputAction.CallbackContext ctx)
    {
        EnsurePresenter();
        this.presenter.HandleSettingInput();
    }

    /// <summary>
    /// 设置玩家输入是否启用。
    /// View 持有输入对象，Presenter 只决定何时启用或禁用。
    /// </summary>
    public void SetPlayerInputEnabled(bool enable)
    {
        if (this.p_InputSet == null)
            return;

        if (enable)
            this.p_InputSet.Player.Enable();
        else
            this.p_InputSet.Player.Disable();
    }

    /// <summary>
    /// 根据根 UI 面板是否打开，刷新玩家输入状态。
    /// 只要存在一个根 UI 面板处于显示状态，就禁止玩家移动/战斗输入。
    /// </summary>
    public void RefreshPlayerInputByOpenPanels()
    {
        SetPlayerInputEnabled(!HasAnyRootUIOpen());
    }

    /// <summary>
    /// 判断是否有任意根 UI 面板处于打开状态。
    /// 该判断给 Presenter 使用，用于决定是否恢复玩家输入。
    /// </summary>
    public bool HasAnyRootUIOpen()
    {
        foreach (var element in this.uiElements)
        {
            if (element != null && element.gameObject.activeSelf)
                return true;
        }

        return false;
    }

    public void ToggleSkillTreeUI()
    {
        EnsurePresenter();
        this.presenter.ToggleSkillTreeUI();
    }

    public void ToggleInventoryUI()
    {
        EnsurePresenter();
        this.presenter.ToggleInventoryUI();
    }

    public void ShowStorageUI(bool status)
    {
        EnsurePresenter();
        this.presenter.ShowStorageUI(status);
    }

    public void ShowMerchantUI(bool status)
    {
        EnsurePresenter();
        this.presenter.ShowMerchantUI(status);
    }

    public void ShowSettingUI()
    {
        EnsurePresenter();
        this.presenter.ShowSettingUI();
    }

    public void SwitchToInGameUI()
    {
        EnsurePresenter();
        this.presenter.SwitchToInGameUI();
    }

    public void ShowDeathPanel()
    {
        EnsurePresenter();
        this.presenter.ShowDeathPanel();
    }

    /// <summary>
    /// View 显示职责：切换技能树面板显示，并处理层级和 Tooltip。
    /// </summary>
    public void ShowSkillTree(bool status)
    {
        BringPanelToFront(this.skillTree.transform);
        this.skillTree.gameObject.SetActive(status);
        this.skillToolTip.ShowToolTip(false);
    }

    /// <summary>
    /// View 显示职责：切换背包面板显示，并关闭相关 Tooltip。
    /// </summary>
    public void ShowInventory(bool status)
    {
        BringPanelToFront(this.inventory.transform);
        this.inventory.gameObject.SetActive(status);
        this.itemToolTip.ShowToolTip(false);
        this.statToolTip.ShowToolTip(false);
    }

    /// <summary>
    /// View 显示职责：切换仓库面板显示。
    /// </summary>
    public void ShowStorage(bool status)
    {
        BringPanelToFront(this.storage.transform);
        this.storage.gameObject.SetActive(status);
    }

    /// <summary>
    /// View 显示职责：切换商人面板显示。
    /// </summary>
    public void ShowMerchant(bool status)
    {
        BringPanelToFront(this.merchant.transform);
        this.merchant.gameObject.SetActive(status);
    }

    /// <summary>
    /// View 显示职责：切换打造面板显示。
    /// </summary>
    public void ShowCraft(bool status)
    {
        this.craft.gameObject.SetActive(status);
    }

    /// <summary>
    /// 禁用当前输入集合。
    /// 死亡面板打开后需要阻止玩家继续操作。
    /// </summary>
    public void DisableAllInput()
    {
        this.p_InputSet?.Disable();
    }

    public void SwitchTo(GameObject target)
    {
        foreach (var element in this.uiElements)
            element.gameObject.SetActive(false);

        target.SetActive(true);
    }

    /// <summary>
    /// 将目标面板和 Tooltip、FadeScreen 放到更高层级，避免被其他 UI 遮挡。
    /// </summary>
    public void BringPanelToFront(Transform target)
    {
        target.SetAsLastSibling();
        SetAllToolTipAtLastSibling();
        this.fadeScreen.transform.SetAsLastSibling();
    }

    public void SetAllToolTipAtLastSibling()
    {
        this.skillToolTip.transform.SetAsLastSibling();
        this.itemToolTip.transform.SetAsLastSibling();
        this.statToolTip.transform.SetAsLastSibling();
    }

    public void CloseAllToolTip()
    {
        this.skillToolTip.ShowToolTip(false);
        this.itemToolTip.ShowToolTip(false);
        this.statToolTip.ShowToolTip(false);
    }

    /// <summary>
    /// 确保 View 已绑定 Presenter。
    /// 使用普通 C# 对象承载流程逻辑，避免本轮需要修改 Canvas.prefab。
    /// </summary>
    private void EnsurePresenter()
    {
        if (this.presenter != null)
            return;

        this.presenter = new UI_CanvasRootPresenter();
        this.presenter.Attach(this);
    }
}
