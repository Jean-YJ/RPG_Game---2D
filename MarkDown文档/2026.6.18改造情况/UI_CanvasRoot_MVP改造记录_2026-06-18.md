# UI_CanvasRoot MVP 改造记录 2026-06-18

## 一、本轮修改范围

本轮继续按照 UI MVP 改造计划推进，目标是拆分 `UI_CanvasRoot` 的职责。

新增文件：

- `Assets/Scripts/UI/UI_CanvasRootPresenter.cs`
- `Assets/Scripts/UI/UI_CanvasRootPresenter.cs.meta`

修改文件：

- `Assets/Scripts/UI/UI_CanvasRoot.cs`

本轮不修改 UI 外观，不移动 prefab，不修改场景，不重命名外部正在调用的 public 方法。

## 二、问题位置、成因、后果

### 1. UI_CanvasRoot 同时负责输入、显示和流程决策

问题位置：

```csharp
// Assets/Scripts/UI/UI_CanvasRoot.cs
public void ToggleSkillTreeUI()
{
    this.skillTree.transform.SetAsLastSibling();
    SetAllToolTipAtLastSibling();
    this.fadeScreen.transform.SetAsLastSibling();

    this.skillTreeEnabled = !this.skillTreeEnabled;
    this.skillTree.gameObject.SetActive(this.skillTreeEnabled);
    this.skillToolTip.ShowToolTip(false);

    StopInputPlayerControlIfNeeded();
}
```

问题成因：

- `UI_CanvasRoot` 既保存面板开关状态，又负责具体显示隐藏，还负责玩家输入启停。
- 输入回调、UI 层级处理、Tooltip 关闭、面板流程判断都集中在一个 MonoBehaviour 中。

可能后果：

- 后续新增 UI 面板时，`UI_CanvasRoot` 会不断膨胀。
- 面板显示规则、输入规则和具体 GameObject 操作混在一起，维护成本高。
- 修改某个面板开关逻辑时容易误伤其他面板。

详细修改方案：

- 新增 `UI_CanvasRootPresenter`。
- `UI_CanvasRoot` 实现 `IUIView`。
- `skillTreeEnabled`、`inventoryEnabled` 移入 Presenter。
- `ToggleSkillTreeUI()`、`ToggleInventoryUI()`、`ShowStorageUI()`、`ShowMerchantUI()`、`ShowSettingUI()`、`SwitchToInGameUI()`、`ShowDeathPanel()` 保持原 public 方法名，但内部只转发给 Presenter。
- `UI_CanvasRoot` 保留显示方法，例如 `ShowSkillTree()`、`ShowInventory()`、`ShowStorage()`、`ShowMerchant()`、`SwitchTo()`、`CloseAllToolTip()`。

为何这样修改：

- 外部脚本仍然可以继续调用原来的 `UI_CanvasRoot` 方法，降低改造风险。
- Presenter 负责“什么时候打开/关闭、是否禁用玩家输入”。
- View 负责“怎么显示、怎么调整层级、怎么关闭 Tooltip”。
- 不需要改 prefab，也不需要额外挂 Presenter 组件。

## 三、修改前后代码对比

### 1. 技能树面板开关

修改前：

```csharp
public void ToggleSkillTreeUI()
{
    this.skillTree.transform.SetAsLastSibling();
    SetAllToolTipAtLastSibling();
    this.fadeScreen.transform.SetAsLastSibling();

    this.skillTreeEnabled = !this.skillTreeEnabled;
    this.skillTree.gameObject.SetActive(this.skillTreeEnabled);
    this.skillToolTip.ShowToolTip(false);

    StopInputPlayerControlIfNeeded();
}
```

修改后：

```csharp
public void ToggleSkillTreeUI()
{
    EnsurePresenter();
    this.presenter.ToggleSkillTreeUI();
}
```

Presenter 中：

```csharp
public void ToggleSkillTreeUI()
{
    if (!HasView)
        return;

    skillTreeEnabled = !skillTreeEnabled;
    View.ShowSkillTree(skillTreeEnabled);
    View.RefreshPlayerInputByOpenPanels();
}
```

View 显示方法：

```csharp
public void ShowSkillTree(bool status)
{
    BringPanelToFront(this.skillTree.transform);
    this.skillTree.gameObject.SetActive(status);
    this.skillToolTip.ShowToolTip(false);
}
```

### 2. 设置键输入处理

修改前：

```csharp
private void OnSettingUIPerformed(InputAction.CallbackContext ctx)
{
    foreach (var element in this.uiElements)
    {
        if (element.gameObject.activeSelf)
        {
            SwitchToInGameUI();
            return;
        }
    }

    ShowSettingUI();
    Time.timeScale = 1;
}
```

修改后：

```csharp
private void OnSettingUIPerformed(InputAction.CallbackContext ctx)
{
    EnsurePresenter();
    this.presenter.HandleSettingInput();
}
```

Presenter 中：

```csharp
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
```

### 3. 仓库面板显示

修改前：

```csharp
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
```

修改后：

```csharp
public void ShowStorageUI(bool status)
{
    EnsurePresenter();
    this.presenter.ShowStorageUI(status);
}
```

Presenter 中：

```csharp
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
```

## 四、本轮建立和强化的规则

`UI_CanvasRoot` 作为 View：

- 持有 UI 子面板引用。
- 绑定和解绑输入事件。
- 接收输入回调并转发给 Presenter。
- 执行具体显示隐藏、层级调整、Tooltip 关闭。

`UI_CanvasRootPresenter` 作为 Presenter：

- 保存技能树、背包等开关状态。
- 决定打开或关闭哪个面板。
- 决定是否恢复/禁止玩家输入。
- 决定设置键是关闭 UI 还是打开设置面板。
- 不直接挂在 prefab 上，不负责具体 UI 外观。

## 五、风险说明

- 保留了 `ToggleSkillTreeUI`、`ToggleInventoryUI`、`ShowStorageUI`、`ShowMerchantUI`、`ShowSettingUI`、`SwitchToInGameUI`、`ShowDeathPanel` 等原 public 方法名。
- 外部脚本如 `Object_Blacksmith`、`Object_Merchant`、`Player_Health` 仍可按原方式调用 `UI_CanvasRoot`。
- Presenter 是普通 C# 对象，不需要改 Canvas prefab。
- 本轮没有改 `uiElements` 的序列化字段和子面板字段，避免破坏 Inspector 引用。

## 六、验证方式

建议在 Unity Editor 中验证：

1. 打开 `MainMenu.unity` 和主游戏场景，确认无编译错误。
2. 进入游戏后按技能树快捷键，确认技能树可打开/关闭，打开时玩家输入被禁用。
3. 按背包快捷键，确认背包可打开/关闭，Tooltip 不残留。
4. 按设置键，确认有 UI 打开时会回到游戏 UI；无 UI 打开时会打开设置面板。
5. 与铁匠交互，确认仓库 UI 打开/关闭正常，关闭时打造面板和 Tooltip 同步关闭。
6. 与商人交互，确认商人 UI 打开/关闭正常。
7. 触发死亡面板，确认死亡面板显示后玩家输入被禁用。
