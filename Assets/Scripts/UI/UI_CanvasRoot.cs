using UnityEngine;

public class UI_CanvasRoot : MonoBehaviour
{
    private static UI_CanvasRoot instance;
    public static UI_CanvasRoot Instance;
    private UI_CanvasRoot() { }

    private PlayerInputSet p_InputSet;
    public bool alternativeInput;

    [SerializeField] private GameObject[] uiElements; //不包含InGame和和各种ToolTip
    public UI_SkillToolTip skillToolTip;
    public UI_SkillTree skillTree;
    private bool skillTreeEnabled;
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
    private bool inventoryEnabled;

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

        this.skillTreeEnabled = this.skillTree.gameObject.activeSelf;
        this.inventoryEnabled = this.inventory.gameObject.activeSelf;
    }

    void Start()
    {
        this.skillTree.UnLockDefaultSkills();
    }

    public void SetUpInputUIControl(PlayerInputSet inputSet)
    {
        this.p_InputSet = inputSet;

        this.p_InputSet.UI.SkillTreeUI.performed += ctx => this.ToggleSkillTreeUI();
        this.p_InputSet.UI.InventoryUI.performed += ctx => this.ToggleInventoryUI();

        this.p_InputSet.UI.Alternative.performed += ctx => this.alternativeInput = true;
        this.p_InputSet.UI.Alternative.canceled += ctx => this.alternativeInput = false;

        //按下SettingUI按键
        this.p_InputSet.UI.SettingUI.performed += ctx =>
        {
            foreach (var element in this.uiElements)
            {
                if (element.gameObject.activeSelf)
                {
                    //如果有界面显示中，就返回InGameUI
                    SwitchToInGameUI();
                    return;
                }
            }
            // 进入设置界面
            ShowSettingUI();
            Time.timeScale = 1;
        };
    }

    //设置p_InputSet.Player相关输入的启用和禁止
    private void EnableInputPlayerControl(bool enable)
    {
        if (enable)
            this.p_InputSet.Player.Enable();
        else
            this.p_InputSet.Player.Disable();
    }

    private void StopInputPlayerControlIfNeeded()
    {
        foreach (var element in this.uiElements)
        {
            if (element.gameObject.activeSelf)
            {
                EnableInputPlayerControl(false);
                return;
            }
        }
        EnableInputPlayerControl(true);
    }


    public void ToggleSkillTreeUI()
    {
        this.skillTree.transform.SetAsLastSibling();
        SetAllToolTipAtLastSibling();
        this.fadeScreen.transform.SetAsLastSibling();

        this.skillTreeEnabled = !this.skillTreeEnabled;
        this.skillTree.gameObject.SetActive(this.skillTreeEnabled);
        this.skillToolTip.ShowToolTip(false);

        //打开SkillTreeUI界面，禁止Player输入
        StopInputPlayerControlIfNeeded();
    }

    public void ToggleInventoryUI()
    {
        this.inventory.transform.SetAsLastSibling();
        SetAllToolTipAtLastSibling();
        this.fadeScreen.transform.SetAsLastSibling();

        this.inventoryEnabled = !this.inventoryEnabled;
        this.inventory.gameObject.SetActive(this.inventoryEnabled);
        this.itemToolTip.ShowToolTip(false);
        this.statToolTip.ShowToolTip(false);

        //打开InventoryUI界面，禁止Player输入
        StopInputPlayerControlIfNeeded();
    }

    public void ShowStorageUI(bool status)
    {
        this.storage.transform.SetAsLastSibling();
        SetAllToolTipAtLastSibling();
        this.fadeScreen.transform.SetAsLastSibling();

        this.storage.gameObject.SetActive(status);
        EnableInputPlayerControl(!status);

        if (!status)
        {
            this.craft.gameObject.SetActive(status);
            CloseAllToolTip();
        }
    }

    public void ShowMerchantUI(bool status)
    {
        this.merchant.transform.SetAsLastSibling();
        SetAllToolTipAtLastSibling();
        this.fadeScreen.transform.SetAsLastSibling();

        this.merchant.gameObject.SetActive(status);
        EnableInputPlayerControl(!status);

        if (!status)
            CloseAllToolTip();
    }
    public void ShowSettingUI()
    {

        CloseAllToolTip();
        EnableInputPlayerControl(false);
        SwitchTo(this.uiSetting.gameObject);
    }

    public void SwitchToInGameUI()
    {
        CloseAllToolTip();
        EnableInputPlayerControl(true);
        SwitchTo(this.inGameUI.gameObject);

        this.inventoryEnabled = false;
        this.skillTreeEnabled = false;
    }

    public void ShowDeathPanel()
    {
        SwitchTo(this.deathPanel.gameObject);

        this.p_InputSet.Disable(); //手柄的话此处要调整
    }
    public void SwitchTo(GameObject target)
    {
        foreach (var element in this.uiElements)
            element.gameObject.SetActive(false);

        target.SetActive(true);
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
}
