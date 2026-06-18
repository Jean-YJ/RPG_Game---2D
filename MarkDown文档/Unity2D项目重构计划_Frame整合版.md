# Unity 2D 项目重构计划 - Frame 整合版

> 当前文档只做代码审查、架构分析和重构计划输出。  
> 未修改任何业务代码、框架代码、场景、Prefab 或 Git 提交。  
> 项目真实入口确认为 `Assets/Scenes/MainMenu.unity`，`SampleScene.unity` 仅作为测试场景。

## A. 项目当前结构概览

### A.1 当前主要目录

- `Assets/Scenes`
  - `MainMenu.unity`：真实入口场景。
  - `Level_0.unity`、`Level_1.unity`、`Level_2.unity`：正式游戏关卡。
  - `SampleScene.unity`：测试场景。
- `Assets/Scripts`
  - 根目录：`GameManager`、`LevelManager`、`AudioManager`、`AudioRangeController`。
  - `FiniteStateMachine`：玩家、敌人、状态机、动画触发、实体基础组件、VFX/SFX。
  - `UI`：主菜单、设置、死亡面板、HUD、背包、仓库、商人、锻造、技能树、Tooltip。
  - `InventoySystem`：玩家背包、仓库、商人库存、装备槽、物品运行时实例。
  - `SkillSystem`：技能基础类、技能管理器、技能对象。
  - `Stats`：角色属性、属性组、属性修改器。
  - `SaveSystem`：存档数据、文件读写、可序列化字典。
  - `Data`：ScriptableObject 数据结构。
  - `Object`：传送门、路点、宝箱、NPC、拾取物、交互物。
  - `Frame`：新导入的通用框架代码。
- `Assets/Data`
  - 道具、技能、属性、音频 ScriptableObject 数据。
- `Assets/Prefab`
  - Player、Enemy、Canvas、技能物、VFX、掉落物、Manager prefab。
- `Assets/Resources`
  - 当前看到 `UI/Canvas.prefab`、`UI/EventSystem.prefab`、`UI/UICamera.prefab`。
  - Frame 中 `UIMgr` 和 `PoolMgr` 都依赖 Resources 或 Addressables 的资源命名约定，需要后续统一资源配置。

### A.2 当前主要模块

- 游戏流程模块
  - `GameManager`：场景切换、存档触发、重生点选择。
  - `LevelManager`：按场景播放 BGM。
  - `SaveManager`：收集 `ISaveable` 并加载/保存 `GameData`。
- 角色与战斗模块
  - `Player`：输入、状态机、组件引用、交互、死亡事件。
  - `Entity`：状态机更新、碰撞检测、翻转、击退、减速。
  - `Entity_Health`：伤害计算、回血、死亡、血条事件。
  - `Entity_Stats`：属性计算。
  - `Player_SkillManager` / `Skill_Base`：技能引用、技能升级、冷却。
- UI 模块
  - `UI_CanvasRoot`：当前 UI 总入口，持有大部分面板引用并处理面板切换。
  - `UI_InGame`：血条、快捷栏、技能槽。
  - `UI_Inventory` / `UI_Storage` / `UI_Merchant` / `UI_Craft`：背包、仓库、商店、制作。
  - `UI_SkillTree` / `UI_TreeNode`：技能树显示、解锁逻辑、存档。
  - `UI_ToolTip` 系列：道具、技能、属性提示。
- 数据模块
  - `GameData`：存档数据。
  - `ItemData_SO`、`SkillData_SO`、`StatSetUpData_SO`、`AudioDataBase_SO`：配置数据。
  - `Inventory_Item`：运行时物品实例。
- 新导入 Frame 模块
  - `Singleton`：`BaseSingleton<T>`、`MonoSingleton<T>`、`AutoMonoSingleton<T>`。
  - `Mono`：`MonoMgr`，为非 Mono 类提供 Update、FixedUpdate、LateUpdate、Coroutine。
  - `EventCenter`：枚举型事件中心。
  - `Pool`：GameObject 和普通 C# 类对象池。
  - `ResourceLoad`：`Resources` 引用计数加载器。
  - `Addressables`：Addressables 引用计数加载器。
  - `UI`：`BasePanel`、`UIMgr`。
  - `Audio`：基于 Addressables 和 Pool 的 `AudioMgr`。
  - `Scene`：`SceneMgr`。
  - `Timer`：`TimerMgr` / `TimerItem`。
  - `UnityWebRequest`：网络/本地资源加载。
  - `Util`：`TextUtil`。

### A.3 当前核心类

- `GameManager`
  - 负责 `ChangeScene`、`ContinuePlay`、`ReStartScene`、重生点选择。
  - 直接依赖 `SaveManager.Instance`、`Player.Instance`、`UI_CanvasRoot.Instance`、`Object_Portal.Instance`。
- `SaveManager`
  - 在 `Start` 中 `FindObjectsByType<MonoBehaviour>().OfType<ISaveable>()` 收集存档对象。
  - 在每个场景中依赖本场景对象是否存在。
- `Player`
  - 初始化输入、状态机、技能管理器、背包、属性、UI。
  - 当前直接查找 `UI_CanvasRoot` 并把输入交给 UI。
- `UI_CanvasRoot`
  - 当前是 UI God Object。
  - 持有所有主要 UI 面板、Tooltip、FadeScreen。
  - 处理 UI 打开关闭、输入启停、面板层级。
- `Inventory_Player`
  - 负责背包、装备、消耗品使用、快捷栏、存档。
  - 直接依赖 `Player`、`Inventory_Storage`。
- `UI_InventoryItemSlot`
  - 同时作为 View 和 Controller，直接调用使用、装备、删除逻辑。
- `UI_SkillTree` / `UI_TreeNode`
  - 同时负责 View、解锁规则、扣点、冲突锁定、技能应用和存档。
- `Frame/UIMgr`
  - 负责统一创建 UI Camera、Canvas、EventSystem，并通过 Addressables 异步加载面板。
  - 要求面板资源名与面板类名一致，面板类继承 `BasePanel`。

### A.4 当前游戏流程

1. `MainMenu` 场景启动。
2. `UI_MainMenu.Start()` 播放主菜单 BGM。
3. 玩家点击继续游戏后调用 `GameManager.ContinuePlay()`。
4. `GameManager.ChangeScene()` 调用 `SaveManager.Instance.SaveGame()`，播放淡出，加载目标场景。
5. 新场景中的 `SaveManager.Start()` 收集 `ISaveable` 并 `LoadGame()`。
6. `GameManager` 等待自身 `LoadData()` 设置加载完成标志。
7. `GameManager` 根据重生类型查找路点、传送门或检查点，把 `Player` 传送到目标位置。
8. `LevelManager.Start()` 调用 `AudioManager.Instance.StartBGM()` 播放当前场景音乐。

### A.5 当前 UI 流程

1. `Player.Awake()` 创建 `PlayerInputSet`。
2. `Player.Awake()` 查找 `UI_CanvasRoot`，调用 `canvasRoot.SetUpInputUIControl(p_Input)`。
3. `UI_CanvasRoot` 订阅 UI 输入，控制技能树、背包、设置面板开关。
4. `UI_CanvasRoot` 根据打开面板禁用或恢复玩家输入。
5. `UI_InGame` 订阅玩家血量、背包、快捷栏事件。
6. 背包、商人、仓库、锻造、技能树面板直接访问业务对象并执行业务逻辑。

### A.6 当前业务代码与 Frame 的关系

- 当前业务代码基本没有接入新导入的 Frame。
- 项目已有一套旧的 `GameManager`、`AudioManager`、`SaveManager`、`UI_CanvasRoot`。
- Frame 中也提供了 `SceneMgr`、`AudioMgr`、`UIMgr`、`EventCenter`、`PoolMgr`、`ResMgr`、`TimerMgr` 等能力。
- 不能机械地全部替换，否则会同时改变 UI、资源、音频、场景和对象生命周期，风险过高。
- 更适合采用“先局部接入、再替换旧职责”的方式。

## B. 主要问题清单

### B.1 P0：必须优先修复，否则会影响功能稳定性

#### P0-1：真实入口与 Build Settings 可能不一致

- 涉及文件或类
  - `ProjectSettings/EditorBuildSettings.asset`
  - `Assets/Scenes/MainMenu.unity`
  - `Assets/Scenes/SampleScene.unity`
  - `GameManager`
  - `UI_DeathPanel`
  - `UI_Setting`
- 问题描述
  - 你已确认真实入口是 `MainMenu.unity`，`SampleScene` 只是测试场景。
  - 之前读取到的 Build Settings 中启用了 `SampleScene` 和 `Level_0/1/2`，代码里却多处加载 `"MainMenu"`。
- 产生原因
  - 开发过程中测试场景与正式入口场景并存，Build Settings 未同步整理。
- 可能后果
  - 打包后点击返回主菜单或继续游戏时加载失败。
  - 测试场景被错误作为首场景。
- 建议解决方案
  - 后续经你确认后，把 `MainMenu.unity` 设置为第一个启用场景。
  - `SampleScene.unity` 保留为测试场景，但不要作为正式构建入口。
- Unity Editor 验证方式
  - 打开 Build Settings，确认第一个启用场景为 `MainMenu`。
  - 打包或 Play Mode 中测试：主菜单 -> 继续游戏 -> 死亡返回主菜单 -> 设置返回主菜单。

#### P0-2：Frame 中 `TextUtil.cs` 疑似存在语法破损

- 涉及文件或类
  - `Assets/Scripts/Frame/Util/TextUtil.cs`
- 问题描述
  - 文件中出现明显乱码和引号破损，例如字符串替换、中文单位、时间单位相关代码可见不完整字符串。
  - 这类问题不只是注释乱码，可能是 C# 语法级错误。
- 产生原因
  - 可能是文件编码导入错误，或复制时中文标点/字符串被破坏。
- 可能后果
  - Unity 编译失败，整个项目无法进入 Play Mode。
- 建议解决方案
  - 第一轮实际修改时优先确认 Unity Console 编译错误。
  - 如果 `TextUtil` 当前未被业务使用，可以先临时从编译中隔离或修复字符串。
  - 长期建议改为 UTF-8，并把中文字符串常量整理清楚。
- Unity Editor 验证方式
  - 打开 Unity，观察 Console 是否有 `TextUtil.cs` 编译错误。
  - 修复后确认脚本重新编译通过。

#### P0-3：Runtime 代码直接引用 `UnityEditor`

- 涉及文件或类
  - `Assets/Scripts/Data/Item/ItemData_SO.cs`
  - `Assets/Scripts/Data/Item/ItemListData_SO.cs`
  - `Assets/Scripts/Object/Object_CheckPoint.cs`
- 问题描述
  - `ItemData_SO.cs` 和 `ItemListData_SO.cs` 顶部直接 `using UnityEditor;`。
  - `Object_CheckPoint.cs` 的 `UnityEditor.EditorUtility.SetDirty` 位于 `#if UNITY_EDITOR` 内，风险较小，但仍建议更规范。
- 产生原因
  - 编辑器辅助逻辑和运行时代码混在同一个脚本。
- 可能后果
  - Player Build 编译失败。
- 建议解决方案
  - 把 `using UnityEditor;` 放到 `#if UNITY_EDITOR` 内。
  - 或将自动收集、自动生成 ID 等编辑器逻辑拆到 `Editor` 文件夹。
- Unity Editor 验证方式
  - 执行一次目标平台 Build。
  - 确认 Player Build 不再出现 `UnityEditor` 命名空间错误。

#### P0-4：输入事件重复订阅风险

- 涉及文件或类
  - `Player.OnEnable()`
  - `UI_CanvasRoot.SetUpInputUIControl()`
- 问题描述
  - 当前使用大量 lambda 进行 `+=` 订阅。
  - `OnDisable()` 只调用 `p_Input.Disable()`，没有对事件进行 `-=`.
- 产生原因
  - 把输入绑定写在生命周期方法里，但没有设计成可退订的命名方法。
- 可能后果
  - Player 被重复启用后，同一次按键可能触发多次技能、交互或 UI 切换。
  - 事件持有对象引用，可能导致泄漏或隐藏行为。
- 建议解决方案
  - 输入订阅改为命名方法。
  - 在 `OnEnable` 订阅，在 `OnDisable` 退订。
  - 或只在 `Awake` 订阅一次，在 `OnDestroy` 统一退订。
- Unity Editor 验证方式
  - 反复切换场景、死亡重开、禁用/启用 Player。
  - 每个输入只触发一次，快捷键不会重复使用。

#### P0-5：快捷栏空引用风险

- 涉及文件或类
  - `Inventory_Player.TryToUseQuickItem()`
- 问题描述
  - `quickItems` 初始数组元素可能为 `null`，当前直接访问 `item.itemData`。
- 产生原因
  - 缺少空槽位保护。
- 可能后果
  - 玩家按快捷键时出现 `NullReferenceException`。
- 建议解决方案
  - 第一轮安全修复中增加 `item == null || item.itemData == null` 保护。
- Unity Editor 验证方式
  - 不设置快捷物品，直接按快捷键 1/2，不应报错。

### B.2 P1：强烈建议修复，影响架构、维护性或性能

#### P1-1：`UI_CanvasRoot` 是 UI God Object

- 涉及文件或类
  - `UI_CanvasRoot`
- 问题描述
  - 同时负责面板引用、面板开关、Tooltip、层级、输入启停、死亡 UI、设置 UI。
- 产生原因
  - 初学项目常见做法：先把 UI 入口集中在一个类中。
- 可能后果
  - 新增 UI 功能时类继续膨胀。
  - UI 与 Player 输入强绑定。
  - 后续接入 Frame `UIMgr` 困难。
- 建议解决方案
  - 短期保留 `UI_CanvasRoot` 作为 View Root。
  - 新增 `UIFlowController` 或 `GameUIController` 管理 UI 流程。
  - 后续再判断是否接入 Frame `UIMgr`。

#### P1-2：UI View 直接操作业务逻辑

- 涉及文件或类
  - `UI_InventoryItemSlot`
  - `UI_InventoryEquipSlot`
  - `UI_StorageItemSlot`
  - `UI_MerchantSlot`
  - `UI_CraftPreview`
  - `UI_SkillTree`
  - `UI_TreeNode`
- 问题描述
  - UI 组件直接调用背包、商店、锻造、技能解锁逻辑。
- 产生原因
  - View 和 Controller/Presenter 职责混合。
- 可能后果
  - UI prefab 改动容易破坏业务。
  - 业务逻辑难复用、难测试。
- 建议解决方案
  - 优先采用简化版 MVP。
  - View 只显示和转发输入。
  - Presenter/Controller 调用 Inventory、Skill、GameFlow 等业务对象。

#### P1-3：Frame `UIMgr` 不适合立即整体替换当前 UI

- 涉及文件或类
  - `Frame/UI/UIMgr.cs`
  - `Frame/UI/BasePanel.cs`
  - 当前 `UI_*` 面板
- 问题描述
  - `UIMgr` 要求面板继承 `BasePanel`，且面板资源名与类名一致。
  - `UIMgr` 使用 Addressables 异步加载面板。
  - 当前 UI 多为场景/Canvas prefab 内已有对象，且大量使用序列化字段引用。
- 产生原因
  - Frame 是通用 UI 框架，而当前项目 UI 已经有一套基于 `UI_CanvasRoot` 的实现。
- 可能后果
  - 强行迁移会涉及大量 prefab、Addressables 配置、面板生命周期和事件绑定。
- 建议解决方案
  - 第一阶段不要直接替换。
  - 先用 MVP 拆出 Presenter。
  - 等 UI 职责清晰后，选择一个低风险面板试点接入 `BasePanel/UIMgr`。

#### P1-4：事件订阅生命周期不统一

- 涉及文件或类
  - `UI_InGame`
  - `UI_Merchant`
  - `UI_Craft`
  - `UI_Storage`
  - `Player`
  - `UI_CanvasRoot`
- 问题描述
  - 部分面板订阅事件后缺少可靠退订。
  - 部分 SetUp 方法重复调用时可能重复订阅。
- 产生原因
  - 没有统一规定 View/Presenter 的 `Bind/Unbind` 生命周期。
- 可能后果
  - UI 重复刷新。
  - 对象销毁后仍被事件引用。
- 建议解决方案
  - 明确规则：`OnEnable/Bind` 订阅，`OnDisable/Unbind` 退订。
  - Presenter 负责业务事件订阅，View 只暴露 UI 事件。

#### P1-5：Frame `EventCenter.Clear(E_EventType)` 逻辑疑似写反

- 涉及文件或类
  - `Frame/EventCenter/EventCenter.cs`
- 问题描述
  - `Clear(E_EventType eventName)` 中逻辑为 `if(!eventDic.ContainsKey(eventName)) eventDic.Remove(eventName);`。
  - 只有不包含 key 时才 Remove，实际不会清除已存在事件。
- 产生原因
  - 条件判断写反。
- 可能后果
  - 事件监听无法按指定事件清空。
  - 如果接入 UI 或游戏事件，会增加泄漏风险。
- 建议解决方案
  - 接入事件中心前先修复。
  - 同时增加类型不匹配保护，避免有参/无参事件名称复用造成强转空引用。

#### P1-6：Frame `PoolMgr` 依赖 Resources 路径和 `PoolObj`

- 涉及文件或类
  - `Frame/Pool/PoolMgr.cs`
  - `Frame/Pool/PoolObj.cs`
- 问题描述
  - `GetGameObject(typeName)` 使用 `Resources.Load<GameObject>(typeName)`。
  - 要求可池化 prefab 挂 `PoolObj` 设置最大数量。
  - 当前项目技能、VFX、掉落物大多在 `Assets/Prefab` 下，不一定在 Resources 下。
- 产生原因
  - 框架对象池采用 Resources 加载模式。
- 可能后果
  - 直接接入会加载不到资源。
  - 如果 prefab 未挂 `PoolObj` 会报错。
- 建议解决方案
  - 不要一次性改所有 `Instantiate/Destroy`。
  - 先选择 VFX prefab 试点，复制或配置到 Resources 路径，挂 `PoolObj`。
  - 或扩展对象池支持从序列化 prefab 注册，而不是强依赖 Resources。

#### P1-7：Frame `AudioMgr` 与现有 `AudioManager` 职责重叠

- 涉及文件或类
  - `AudioManager`
  - `Frame/Audio/AudioMgr.cs`
- 问题描述
  - 当前项目已有基于 `AudioDataBase_SO` 的音频管理器。
  - Frame `AudioMgr` 基于 Addressables 和 Pool 播放 BGM/SFX。
- 产生原因
  - 旧业务音频系统与新框架音频系统并存。
- 可能后果
  - 两套音频管理器同时存在会造成音量、生命周期、资源加载策略混乱。
- 建议解决方案
  - 暂时保留现有 `AudioManager`。
  - 后续如果要迁移，先让 `AudioManager` 内部逐步使用 `AudioMgr` 的对象池/SFX 管理能力。
  - 不建议立刻把所有音频调用改成 `Frame.AudioMgr`。

#### P1-8：Frame `SceneMgr` 无法直接替代当前 `GameManager.ChangeScene`

- 涉及文件或类
  - `GameManager`
  - `Frame/Scene/SceneMgr.cs`
- 问题描述
  - 当前切场景不仅加载场景，还包含存档、Fade、等待 SaveManager 加载、重生点定位。
  - `SceneMgr` 只负责同步/异步加载和进度事件。
- 产生原因
  - Frame 是底层工具，业务流程还在 `GameManager`。
- 可能后果
  - 直接替换会丢失存档和重生流程。
- 建议解决方案
  - 将 `SceneMgr` 作为底层加载工具。
  - 中期新增 `SceneFlowController` 封装完整业务流程。
  - `GameManager` 逐步瘦身。

#### P1-9：Addressables 已导入，但当前项目资源体系未统一

- 涉及文件或类
  - `Frame/Addressables/AddressablesMgr.cs`
  - `Frame/UI/UIMgr.cs`
  - `Frame/Audio/AudioMgr.cs`
  - `Packages/manifest.json`
- 问题描述
  - `com.unity.addressables` 已存在于 manifest。
  - `UIMgr` 和 `AudioMgr` 已依赖 Addressables。
  - 当前主要资源仍通过 Inspector、Prefab、Resources、ScriptableObject 数据库使用。
- 产生原因
  - 新框架引入后，资源加载方式尚未迁移。
- 可能后果
  - 一部分资源走 Addressables，一部分走 Resources，一部分走 Inspector，管理复杂度上升。
- 建议解决方案
  - 短期不要为了框架而强迁移全部资源。
  - UI 面板、音频、可池化 VFX 可以作为逐步 Addressables 化对象。
  - 核心 Player、Enemy、场景引用保持现状。

### B.3 P2：可以后续优化，属于代码质量或扩展性问题

#### P2-1：命名和拼写问题较多

- 涉及文件或类
  - `InventoySystem`
  - `glodPoints`
  - `Aduio`
  - `Cosumable`
  - `Peirce`
  - `Respwan`
  - `destoryDelay`
- 可能后果
  - 降低可读性。
  - 初学阶段容易继续复制错误命名。
- 建议解决方案
  - 不要轻易重命名 serialized public/private 字段。
  - 先新增正确命名方法或局部变量。
  - 后续在确认 prefab 引用安全后再统一清理。

#### P2-2：注释和部分 Frame 文件存在编码乱码

- 涉及文件或类
  - 多个业务脚本和 Frame 脚本。
- 可能后果
  - 学习和维护困难。
  - 某些文件可能已不只是注释乱码，而是字符串破损。
- 建议解决方案
  - 先修会导致编译失败的文件。
  - 后续统一用 UTF-8。
  - 重要注释重写为简短中文。

#### P2-3：魔法字符串和魔法数字较多

- 涉及文件或类
  - 场景名：`"MainMenu"`、`"Level_0"`。
  - BGM group：`"playList_MainMenu"` 等。
  - PlayerPrefs key：`"SETTINGDATA"`。
  - Animator 参数：`"idle"`、`"move"`、`"isActive"` 等。
- 建议解决方案
  - 新增常量类或 ScriptableObject 配置。
  - 优先整理场景名、UI 面板名、音频 key。

#### P2-4：`Debug.Log` 在运行路径较多

- 涉及文件或类
  - `AudioManager`
  - `Entity_Health`
  - `Entity_StatusHandler`
  - `FileDataHandler`
  - `Inventory_Player`
- 可能后果
  - Development Build 或 Editor 中日志过多，影响性能和定位。
- 建议解决方案
  - 增加简单日志开关。
  - 高频日志改为 Debug 模式下输出。

## C. 推荐目标架构

### C.1 总体目标

目标不是引入复杂企业级架构，而是让当前项目在保留玩法的前提下逐步清晰：

- UI 只负责显示和输入转发。
- Presenter/Controller 处理 UI 事件和业务调用。
- Model/Data 保存状态。
- GamePlay 系统不直接持有具体 UI 面板。
- Frame 作为工具层被业务按需使用，不反过来支配所有业务。

### C.2 推荐分层

#### Core / Framework 层

- 位置建议
  - `Assets/Scripts/Frame`
- 包含
  - `BaseSingleton`
  - `MonoMgr`
  - `EventCenter`
  - `PoolMgr`
  - `ResMgr`
  - `AddressablesMgr`
  - `TimerMgr`
  - `SceneMgr`
  - `UIMgr`
  - `AudioMgr`
- 职责
  - 提供通用能力。
  - 不引用业务类，如 `Player`、`Inventory_Player`、`UI_Inventory`。
- 当前建议
  - 先修 Frame 内部明显问题，再局部接入。

#### GamePlay 层

- 位置建议
  - `Assets/Scripts/GamePlay`
  - 或保留现有 `FiniteStateMachine`、`SkillSystem`、`Object`，逐步调整命名。
- 包含
  - Player、Enemy、Combat、Skill、Interactable、Drop、Portal、Waypoint。
- 依赖允许
  - 可以依赖 Core、Data、Config。
- 禁止依赖
  - 不应直接调用具体 UI 面板。
  - 不应直接写 UI 文本、图标、冷却遮罩。

#### UI 层

- 位置建议
  - `Assets/Scripts/UI`
- 包含
  - View：现有 `UI_*` MonoBehaviour。
  - Presenter/Controller：后续新增，如 `InventoryPresenter`、`SkillTreeController`。
- 职责
  - View：显示、输入事件转发、基础动画。
  - Presenter：处理点击、调用业务、监听业务事件并刷新 View。
- 禁止依赖
  - View 不直接修改背包、技能、存档、场景。

#### Data / Model 层

- 包含
  - `Inventory_Item`
  - `GameData`
  - 技能树运行时状态
  - 快捷栏状态
  - 玩家当前血量/属性状态
- 职责
  - 表示运行时状态。
  - 不依赖 UI。

#### Config 层

- 包含
  - `ItemData_SO`
  - `ItemListData_SO`
  - `SkillData_SO`
  - `StatSetUpData_SO`
  - `AudioDataBase_SO`
- 职责
  - 编辑器配置数据。
  - 不写运行时状态，除非明确是缓存。

#### Resource 层

- 当前方案
  - Inspector 引用 + Resources + 少量 Addressables 并存。
- 推荐目标
  - 常驻核心对象仍用 Inspector/Prefab。
  - UI 面板和音频可逐步 Addressables。
  - 可池化 VFX/技能物统一由对象池获取。

#### Audio 层

- 当前方案
  - 业务使用 `AudioManager` + `AudioDataBase_SO`。
- 推荐目标
  - 短期保留 `AudioManager`。
  - 中期把 SFX 播放对象池化。
  - 长期再决定是否迁移到 Frame `AudioMgr`。

#### Scene / Flow 层

- 当前方案
  - `GameManager` 同时处理场景、存档、Fade、重生。
- 推荐目标
  - 新增 `SceneFlowController` 或 `GameFlowController`。
  - `SceneMgr` 只负责底层加载。
  - `GameManager` 逐步变成全局状态入口，而非所有流程的实现者。

### C.3 依赖规则

- 允许
  - UI Presenter -> GamePlay / Data / Core。
  - GamePlay -> Core / Data / Config。
  - SaveSystem -> Data / ISaveable。
  - Frame -> Unity API。
- 禁止
  - Frame -> 具体业务类。
  - GamePlay -> 具体 UI 面板。
  - ScriptableObject ItemEffect -> 直接查找 UI 并操作。
  - UI View -> 直接调用 `GameManager.ChangeScene`、`Inventory_Player.TryToEquipItem`、`Skill_Base.SetSkillUpgrade`。

## D. UI 重构方案

### D.1 总体建议：简化版 MVP 优先

当前 UI 代码不适合立刻全量迁移到 Frame `UIMgr/BasePanel`，原因是：

- 当前 UI 大量依赖场景或 Canvas prefab 中的序列化引用。
- `UIMgr` 要求面板 Addressables 化，并且面板名与类名一致。
- 直接迁移会同时改 prefab、资源配置、继承体系、生命周期。

推荐路线：

1. 先保留现有 UI prefab 和 View 类。
2. 新增 Presenter/Controller，把业务逻辑从 View 中抽出。
3. View 保持 MonoBehaviour，负责显示和输入转发。
4. Presenter 负责调用 Inventory、Skill、GameFlow、Audio。
5. 等一个面板拆清楚后，再决定是否改造成 `BasePanel`。

### D.2 `UI_CanvasRoot`

- 当前问题
  - 持有所有面板。
  - 负责面板开关、输入启停、Tooltip、层级和死亡流程。
- 建议模式
  - 简化版 MVC / UI Root + Controller。
- View 职责
  - 保存面板引用。
  - 提供 `Show/Hide` 基础接口。
  - 管理 Tooltip 层级。
- Controller 职责
  - `UIFlowController` 处理输入、互斥面板、暂停/恢复玩家输入。
  - Death、Setting、Inventory、SkillTree 的打开关闭由 Controller 决策。
- Model 数据来源
  - 当前不需要单独 Model，只需要 UI 状态，如当前打开面板。
- 通信方式
  - UI 输入 -> UIFlowController。
  - GamePlay 事件，如 PlayerDead -> UIFlowController -> DeathPanel。
- 是否需要事件中心
  - 可后续接入。
  - 先用 C# event 或直接 Controller 引用即可。

### D.3 `UI_InGame`

- 当前问题
  - 直接查找 `Player`。
  - 直接订阅血量、背包、快捷栏事件。
  - 技能冷却由 `Skill_Base` 直接调用 HUD。
- 建议模式
  - MVP。
- View 职责
  - `SetHealth(current, max, percent)`。
  - `SetQuickSlot(index, itemViewData)`。
  - `SetSkillSlot(skillType, icon, cooldown)`。
  - 转发快捷栏点击。
- Presenter 职责
  - 监听玩家血量变化。
  - 监听背包快捷栏变化。
  - 监听技能冷却事件。
- Model 数据来源
  - `Entity_Health`
  - `Player_Stats`
  - `Inventory_Player.quickItems`
  - `Skill_Base` 冷却状态
- 通信方式
  - 短期：Presenter 直接订阅现有 C# event。
  - 中期：技能系统触发 `SkillCooldownStarted` 事件，HUD 监听。
- 每步验证
  - 受伤、回血、装备改变最大生命、使用快捷物品、技能解锁、技能冷却显示。

### D.4 `UI_Inventory` 和 Slot

- 当前问题
  - `UI_InventoryItemSlot` 直接使用、装备、删除物品。
  - View 直接访问 `Inventory_Player`。
- 建议模式
  - MVP。
- View 职责
  - 显示物品图标、数量、装备槽状态、金币。
  - 转发点击事件：左键、右键、Ctrl/Alt 状态。
- Presenter 职责
  - 判断点击含义。
  - 调用 `Inventory_Player.TryToUseItem`、`TryToEquipItem`、`RemoveAllStack`。
  - 订阅背包更新并刷新 View。
- Model 数据来源
  - `Inventory_Player.itemList`
  - `Inventory_Player.equipmentSlots`
  - `Inventory_Player.gold`
- 通信方式
  - 背包事件仍可用现有 `onInventoryUpdateded`，后续可换成事件中心。
- 是否立即接入 Frame `UIMgr`
  - 不建议。先拆 Presenter。

### D.5 `UI_Storage`

- 当前问题
  - `SetStorage` 和 `OnEnable` 都会处理订阅，重复绑定风险较高。
  - View 直接访问仓库和玩家背包。
- 建议模式
  - MVP。
- View 职责
  - 显示玩家背包、仓库、材料库三组 Slot。
  - 转发 Slot 点击。
- Presenter 职责
  - 绑定当前 `Inventory_Storage`。
  - 处理背包到仓库、仓库到背包、材料移动。
  - 负责订阅和退订。
- Model 数据来源
  - `Inventory_Player`
  - `Inventory_Storage`
- 通信方式
  - Blacksmith 交互 -> UIFlowController 打开 Storage -> StoragePresenter.Bind(storage)。

### D.6 `UI_Merchant`

- 当前问题
  - `SetUpMerchant` 每次调用都订阅事件，未看到退订。
  - UI Slot 直接依赖 `Inventory_Merchant`。
- 建议模式
  - MVP。
- View 职责
  - 显示商人商品、玩家背包、装备、金币。
  - 转发购买/出售点击。
- Presenter 职责
  - 绑定商人库存和玩家背包。
  - 处理购买、出售、整组买卖。
  - 管理事件订阅。
- Model 数据来源
  - `Inventory_Merchant.itemList`
  - `Inventory_Player.itemList`
  - `Inventory_Player.gold`
- 通信方式
  - Merchant NPC 只发起打开商店请求，不直接操作 View。

### D.7 `UI_Craft` / `UI_CraftPreview`

- 当前问题
  - Preview 直接创建 `Inventory_Item`。
  - View 直接判断材料和执行制作。
- 建议模式
  - MVP。
- View 职责
  - 显示可制作列表、选中物品、材料需求、制作按钮状态。
  - 转发选择物品和点击制作。
- Presenter 职责
  - 根据选中 ItemData 生成预览数据。
  - 调用 `Inventory_Storage.CanCraftItem` 和 `CraftItem`。
  - 刷新材料需求和背包。
- Model 数据来源
  - `ItemData_SO.craftRecipe`
  - `Inventory_Storage`
  - `Inventory_Player`
- 通信方式
  - Storage 与 Craft 可以共享一个 Blacksmith UI Controller。

### D.8 `UI_SkillTree` / `UI_TreeNode`

- 当前问题
  - 节点同时负责显示、点击、解锁判断、扣点、冲突锁定、技能应用。
  - `UI_SkillTree` 同时实现存档。
- 建议模式
  - 简化版 MVC，优先 Controller。
- View 职责
  - 节点显示：锁定、解锁、冲突、可高亮。
  - 转发点击、Hover。
  - 显示技能点。
- Controller 职责
  - 判断是否可解锁。
  - 扣除技能点。
  - 锁定冲突节点。
  - 调用 `Player_SkillManager` 应用技能升级。
  - 处理退款。
- Model 数据来源
  - `SkillData_SO`
  - 技能点数量
  - 节点解锁状态
  - 技能升级状态
- 通信方式
  - 解锁成功后触发 `OnSkillUnlocked`。
  - 短期由 Controller 直接调 `Player_SkillManager`，中期改事件。

### D.9 Tooltip 系列

- 当前问题
  - 各 UI Slot 直接访问 `canvasRoot.itemToolTip`、`skillToolTip`、`statToolTip`。
- 建议模式
  - 保持 View，但新增 TooltipService。
- View 职责
  - 显示 tooltip。
- Controller/Service 职责
  - 对外提供 `ShowItem`、`ShowSkill`、`ShowStat`、`HideAll`。
- 是否需要事件中心
  - 不需要，直接服务引用更清晰。

### D.10 `UI_Setting`

- 当前问题
  - 直接查找 Player。
  - 直接调用 `SaveManager` 和 `GameManager`。
  - 使用 `PlayerPrefsDataMgr` 存设置，但设置、流程和 View 混在一起。
- 建议模式
  - 简化版 MVC。
- View 职责
  - Slider、Toggle、按钮显示和输入转发。
- Controller 职责
  - 保存设置。
  - 应用音量和血条显示。
  - 请求返回主菜单。
- Model 数据来源
  - `SettingData`
  - `PlayerPrefsDataMgr`
- 通信方式
  - 设置 Controller -> GameFlowController。

### D.11 `UI_DeathPanel`

- 当前问题
  - 直接调用 `GameManager.ChangeScene` 和 `ReStartScene`。
- 建议模式
  - 保持简单 View + Flow Controller。
- View 职责
  - 显示死亡按钮。
  - 转发“重开”“回主菜单”等点击事件。
- Controller 职责
  - 调用游戏流程。

## E. 框架代码接入方案

### E.1 Frame 总体评价

Frame 提供的能力较完整，但目前更像“教学型通用框架”，适合逐步使用，不适合一次性替换当前业务系统。

建议接入原则：

1. 先修 Frame 内部明显问题。
2. 先接入低耦合、高收益模块。
3. 不替换已稳定运行的业务流程。
4. 每次只接一个业务点，并提供 Editor 验证路径。

### E.2 `BaseSingleton<T>` / `MonoSingleton<T>` / `AutoMonoSingleton<T>`

- 适合使用场景
  - `EventCenter`、`PoolMgr`、`ResMgr`、`TimerMgr` 等纯工具管理器。
  - `MonoMgr` 这种需要自动挂载的通用 Mono 管理器。
- 当前业务接入建议
  - 不建议立即让 `GameManager`、`SaveManager`、`AudioManager` 全部继承 Frame 单例。
  - 这些类已有场景 prefab 和序列化字段，贸然改继承会影响引用和生命周期。
- 接入收益
  - 单例写法统一。
  - 减少重复实例代码。
- 接入风险
  - 当前 `BaseSingleton` 依赖私有无参构造函数和反射，对初学者调试不直观。
  - `AutoMonoSingleton` 动态创建对象，隐藏生命周期，不适合所有业务管理器。
- 是否建议立即接入
  - 工具层可以继续使用。
  - 业务 Manager 暂不立即替换。

### E.3 `MonoMgr`

- 适合使用场景
  - 非 Mono 类需要协程或 Update。
  - `TimerMgr`、`ResMgr` 已使用。
- 当前业务接入建议
  - 不建议把 Player、Enemy、Skill 的 Update 转移到 `MonoMgr`。
  - 可用于后续非 Mono Presenter 或 Flow Controller 需要协程时。
- 接入收益
  - 非 Mono 类可以统一开协程。
- 接入风险
  - 若忘记移除 UpdateAction，会产生泄漏或重复执行。
- 是否建议立即接入
  - 不主动接业务，作为框架依赖保留。

### E.4 `EventCenter`

- 适合使用场景
  - 跨模块、低频事件。
  - 例如 PlayerDead、InventoryChanged、SkillUnlocked、SceneLoadProgress。
- 当前业务接入建议
  - UI 架构重构中可以逐步用，但不要替代所有 C# event。
  - 先扩展 `E_EventType` 为项目真实事件。
- 接入收益
  - 降低 Player/Skill/Inventory 对具体 UI 的依赖。
- 接入风险
  - 当前 `Clear(E_EventType)` 疑似 bug，需先修。
  - 枚举事件中心类型安全较弱，有参/无参混用可能运行时报错。
  - 过度使用会让调用链不清晰。
- 是否建议立即接入
  - 不建议马上全局接入。
  - 建议先用于 `PlayerDead` 和 `SkillUnlocked` 这类跨 UI/Gameplay 事件试点。

### E.5 `PoolMgr`

- 适合使用场景
  - VFX。
  - 技能物：Shard、Sword、TimeEcho、Domain。
  - 掉落物。
  - 音效 AudioSource 对象。
  - 轨迹点。
- 当前业务接入建议
  - 第一批只接 VFX，例如 Hit、CritHit、Smoke、ShardExplode。
  - 第二批接高频技能物。
  - 第三批接掉落物。
- 接入收益
  - 减少 `Instantiate/Destroy`。
  - 降低 GC 和战斗卡顿。
- 接入风险
  - 依赖 Resources 路径。
  - Prefab 必须挂 `PoolObj`。
  - 复用对象必须 Reset 状态，否则残留速度、事件、材质、颜色、协程。
- 是否建议立即接入
  - UI 优先阶段不立即接。
  - 性能阶段强烈建议接。

### E.6 `ResMgr`

- 适合使用场景
  - Resources 下的 UI 根预设体、通用 prefab、配置资源。
- 当前业务接入建议
  - 当前项目核心资源多为 Inspector 引用，不需要强迁移。
  - 如果继续使用 Resources 下 UI 根对象，可由 `UIMgr` 内部或启动器统一加载。
- 接入收益
  - 引用计数和异步加载。
- 接入风险
  - Resources 本身不适合大规模资源管理。
  - 引用计数需要严格 Load/Unload 配对。
- 是否建议立即接入
  - 暂不作为优先项。

### E.7 `AddressablesMgr`

- 适合使用场景
  - UI 面板。
  - 音频。
  - 后续大型资源。
- 当前业务接入建议
  - 项目 manifest 已包含 `com.unity.addressables`。
  - 但当前面板并未确认已配置 Addressables key。
  - 如果要接 `UIMgr` 或 `AudioMgr`，必须先建立 Addressables 分组和 key 规范。
- 接入收益
  - 异步加载和资源释放更规范。
  - 后续支持 AssetBundle/远程资源更自然。
- 接入风险
  - 配置成本高。
  - 初学阶段容易因为 key 不一致导致加载失败。
- 是否建议立即接入
  - 不作为第一阶段。
  - 可在 UI MVP 稳定后，选择单个新面板试点。

### E.8 `UIMgr` / `BasePanel`

- 适合使用场景
  - 新增的独立弹窗。
  - 后续新增功能面板。
  - 已完成 MVP 拆分且资源可 Addressables 化的面板。
- 当前业务接入建议
  - 不要直接替换 `UI_CanvasRoot`。
  - 先把 `UI_CanvasRoot` 瘦身成 UI Root。
  - 新增面板可以尝试继承 `BasePanel`。
- 接入收益
  - 面板统一显示/隐藏。
  - UI 层级统一。
  - 动态加载面板。
- 接入风险
  - 需要 Canvas 预设体有 `Bottom/Middle/Top/System` 层级。
  - 需要 Resources 下 UI 根预设体和 Addressables 面板资源配置正确。
  - `BasePanel` 按控件名字自动收集，依赖命名规范。
- 是否建议立即接入
  - 当前 UI 架构重构阶段不建议立即整体接入。
  - 可以作为新增面板的试点框架。

### E.9 `AudioMgr`

- 适合使用场景
  - Addressables 音频资源。
  - 需要池化的 SFX AudioSource。
- 当前业务接入建议
  - 现有 `AudioManager` 已有 `AudioDataBase_SO`、BGM 随机播放、距离衰减、渐变等逻辑。
  - 不建议直接替换。
  - 后续可以把 `AudioManager.PlayGlobalSFX` 或局部 SFX 改为内部使用对象池。
- 接入收益
  - SFX AudioSource 可复用。
  - 与 Addressables 资源释放结合。
- 接入风险
  - 需要 `Sound` prefab 在 Resources 中，并挂 `PoolObj` 与 `AudioSource`。
  - 与现有音频数据库重复。
- 是否建议立即接入
  - 否。

### E.10 `SceneMgr`

- 适合使用场景
  - 纯场景加载。
  - 加载进度广播。
- 当前业务接入建议
  - 可作为 `GameManager.ChangeScene` 内部的底层工具。
  - 不能替代完整流程。
- 接入收益
  - 异步加载封装。
  - 进度可通过事件中心通知 UI。
- 接入风险
  - 当前 `GameManager` 还有存档、Fade、重生点、等待加载。
- 是否建议立即接入
  - 暂不作为 UI 优先阶段。

### E.11 `TimerMgr`

- 适合使用场景
  - Buff 计时。
  - 技能冷却。
  - UI 闪烁。
  - 延迟执行。
- 当前业务接入建议
  - 不要立刻替换全部协程。
  - 可先用于 UI 层的非关键计时，例如 Tooltip 闪烁或按钮反馈。
- 接入收益
  - 定时逻辑集中。
  - 普通类也可以创建计时器。
- 接入风险
  - 当前 `RemoveTimer` 把 TimerItem 放入池后又加入 `deleteList`，需要确认是否可能重复回收。
  - 计时精度为 0.1 秒，不适合精确动作判定。
- 是否建议立即接入
  - 低优先级。

### E.12 `PlayerPrefsDataMgr`

- 适合使用场景
  - 音量设置。
  - 是否显示血条。
  - UI 偏好设置。
- 当前业务接入建议
  - `UI_Setting` 已使用，可以继续保留。
  - 不建议用于主存档，主存档继续走 `SaveManager` 和 JSON。
- 接入收益
  - 简单偏好存储方便。
- 接入风险
  - 反射存储对字段名敏感。
  - 乱码注释影响学习理解。
- 是否建议立即接入
  - 保持现状。

### E.13 `UWQResMgr`

- 适合使用场景
  - 从本地 file/http 加载外部资源。
  - 后续热更新或下载 AssetBundle。
- 当前业务接入建议
  - 当前项目暂不需要。
- 是否建议立即接入
  - 否。

### E.14 `TextUtil`

- 适合使用场景
  - 字符串拆分、数字显示、时间格式化。
- 当前业务接入建议
  - 当前不建议接入。
  - 先确认文件是否能编译。
- 接入风险
  - 当前文件疑似语法破损。
- 是否建议立即接入
  - 否。

## F. 性能与内存优化计划

### F.1 当前可能的性能问题

#### F.1.1 频繁 `Instantiate/Destroy`

- 涉及位置
  - `Skill_Shard.CreateShard`
  - `Skill_Shard.CreateRawShard`
  - `Skill_SwordToss.TossSword`
  - `Skill_TimeEcho.CreateTimeEcho`
  - `Entity_VFX.CreateOnHitVfx`
  - `Entity_DropManager.DropItems`
  - `VFX_AutoController`
  - `Object_ItemPickup`
- 优先级
  - 高。
- 推荐做法
  - 用 `PoolMgr` 或扩展后的对象池。
  - 先从 VFX 开始，因为 VFX 生命周期短、状态相对简单。
- 验证方式
  - Profiler 查看 `GC Alloc`、`Instantiate`、`Destroy` spikes。
  - 长时间战斗释放技能，观察帧率和内存。

#### F.1.2 `renderer.material` / `SpriteRenderer.material` 材质实例化

- 涉及位置
  - `Entity_VFX`
- 优先级
  - 高。
- 推荐做法
  - 如果只是改颜色或闪白，优先考虑 `MaterialPropertyBlock`。
  - 如果确实需要独立材质实例，要明确释放或复用。
- 验证方式
  - Memory Profiler 查看 Material 实例数量。
  - 多次受击后 Material 数量不应持续增长。

#### F.1.3 `FindAnyObjectByType` / `FindFirstObjectByType` 使用较多

- 涉及位置
  - `Player`
  - `UI_InGame`
  - `UI_Inventory`
  - `UI_SkillTree`
  - `UI_StatSlot`
  - `ItemEffectData_SO` 子类
- 优先级
  - 中。
- 推荐做法
  - 初始化阶段可以暂时接受。
  - UI 重构后由 Presenter 显式注入依赖。
  - ScriptableObject 效果不应自己 Find UI 或 Player。
- 验证方式
  - Profiler Timeline。
  - 场景启动和打开 UI 时无明显查找尖峰。

#### F.1.4 UI 刷新可能过于粗粒度

- 涉及位置
  - `Inventory_Base.onInventoryUpdateded`
  - `UI_Inventory.UpdateUISlots`
  - `UI_Storage.UpdateUI`
  - `UI_Merchant.UpdateMerchantUI`
  - `UI_InGame.UpdateQuickSlotUI`
- 优先级
  - 中。
- 推荐做法
  - 第一阶段不做复杂增量刷新。
  - Presenter 层建立后，可区分金币变化、物品变化、装备变化、快捷栏变化。
- 验证方式
  - 打开背包进行连续使用/购买/移动物品，Profiler 查看 UI Rebuild。

#### F.1.5 LINQ 和临时 List 分配

- 涉及位置
  - `GameManager.GetPlayerRespawnPosition`
  - `UI_InGame.ShowQuickItemOptions`
  - `Inventory_Storage.AddMaterialToStash`
  - `SaveManager.FindISaveables`
- 优先级
  - 低到中。
- 推荐做法
  - 低频路径保留。
  - 高频 UI 展示可复用 List 或缓存过滤结果。
- 验证方式
  - Deep Profile 查看 GC Alloc。

#### F.1.6 协程、Timer、事件未释放风险

- 涉及位置
  - `Player`
  - `UI_InGame`
  - `UI_Merchant`
  - `UI_Craft`
  - `Skill_Shard`
  - `Entity_Health.InvokeRepeating`
- 优先级
  - 高。
- 推荐做法
  - 明确订阅/退订规则。
  - `InvokeRepeating` 对象销毁或禁用时确认是否需要 `CancelInvoke`。
  - 池化对象归还时停止协程、退订事件、重置状态。
- 验证方式
  - 反复切场景、开关 UI、释放技能，确认无重复回调和 MissingReference。

### F.2 优化优先级排序

1. 编译和打包风险。
2. 空引用和输入重复订阅。
3. UI 事件生命周期。
4. VFX 和技能物对象池。
5. 材质实例化。
6. UI 刷新粒度。
7. LINQ/字符串/日志优化。

### F.3 推荐验证清单

- 从 `MainMenu` 进入游戏。
- 从 Level_0 切 Level_1/2，再返回。
- 死亡、重开、返回主菜单。
- 打开/关闭背包、技能树、设置。
- 与商人交易。
- 与铁匠打开仓库和制作。
- 使用快捷栏空槽位。
- 解锁技能并释放技能。
- 高频释放 Shard、Sword、TimeEcho。
- 长时间战斗观察 Profiler。

## G. 分阶段重构路线图

### 第 0 阶段：只读基线确认

- 修改目标
  - 不修改代码。
  - 确认当前 Unity Console 编译状态。
  - 确认 `MainMenu.unity` 是正式入口，`SampleScene` 是测试场景。
  - 确认 Frame 导入后是否有编译错误，尤其是 `TextUtil.cs`。
- 涉及文件
  - 不修改文件。
- 修改风险
  - 无。
- 验证方式
  - 打开 Unity Editor，等待脚本编译。
  - 截图或记录 Console 错误。
- 是否适合初学者立即执行
  - 非常适合。

### 第 1 阶段：UI 架构重构前的安全修复

- 修改目标
  - 修复 P0 问题，不改变玩法。
  - 确保后续 UI 重构建立在可编译、可运行项目上。
- 涉及文件
  - `TextUtil.cs`
  - `ItemData_SO.cs`
  - `ItemListData_SO.cs`
  - `Inventory_Player.cs`
  - `Player.cs`
  - `UI_CanvasRoot.cs`
- 修改风险
  - 中。
  - 输入订阅修复需要仔细验证。
- 验证方式
  - Unity 编译通过。
  - 空快捷栏按键不报错。
  - UI 快捷键只触发一次。
  - Build 不报 `UnityEditor` 错误。
- 是否适合初学者立即执行
  - 适合，但建议一次只修一个问题。

### 第 2 阶段：UI Presenter 基础设施

- 修改目标
  - 新增少量 Presenter/Controller 基础类。
  - 不改 UI 外观，不移动 prefab。
  - 建立 View 只显示、Presenter 处理逻辑的规则。
- 建议新增脚本
  - `UIFlowController`
  - `IUIView` 或更具体的 View 接口，视复杂度决定。
  - `InventoryPresenter`
  - `InGameHudPresenter`
- 涉及文件
  - `UI_CanvasRoot`
  - `UI_InGame`
  - `UI_Inventory`
  - 相关 Slot 类
- 修改风险
  - 中。
- 验证方式
  - 打开/关闭背包和 HUD 正常。
  - 血条、快捷栏、金币、物品显示正常。
  - 无重复事件。
- 是否适合初学者立即执行
  - 适合，但要从一个面板试点。

### 第 3 阶段：背包 UI MVP 试点

- 修改目标
  - 先选 `UI_Inventory` 作为 MVP 试点。
  - Slot 不再直接调用背包业务，而是把点击交给 Presenter。
- 涉及文件
  - `UI_Inventory`
  - `UI_InventoryItemSlot`
  - `UI_InventoryEquipSlot`
  - `UI_ItemSlotGroup`
  - `UI_EquipSlotGroup`
  - 新增 `InventoryPresenter`
- 修改风险
  - 中。
  - 装备、使用、删除、Tooltip 都要回归测试。
- 验证方式
  - 使用消耗品。
  - 装备/卸下装备。
  - 删除整组物品。
  - Tooltip 正常显示/隐藏。
  - 背包刷新正确。
- 是否适合初学者立即执行
  - 适合，是 UI 架构最推荐的第一块。

### 第 4 阶段：仓库、商人、锻造 UI MVP

- 修改目标
  - 在背包试点稳定后，复用同样模式处理仓库、商人、锻造。
- 涉及文件
  - `UI_Storage`
  - `UI_StorageItemSlot`
  - `UI_Merchant`
  - `UI_MerchantSlot`
  - `UI_Craft`
  - `UI_CraftPreview`
  - 新增 `StoragePresenter`、`MerchantPresenter`、`CraftPresenter`
- 修改风险
  - 中到高。
  - 交易和材料消耗容易出数据 bug。
- 验证方式
  - 买入、卖出、整组买卖。
  - 背包到仓库、仓库到背包。
  - 制作物品、材料扣除、金币刷新。
- 是否适合初学者立即执行
  - 建议在第 3 阶段稳定后执行。

### 第 5 阶段：技能树 UI Controller

- 修改目标
  - 把技能树解锁规则从 `UI_TreeNode` 中拆到 Controller。
  - View 只负责节点显示和点击转发。
- 涉及文件
  - `UI_SkillTree`
  - `UI_TreeNode`
  - `UI_TreeConnectHandler`
  - `UI_TreeConnection`
  - `Player_SkillManager`
  - 新增 `SkillTreeController`
- 修改风险
  - 高。
  - 技能树涉及存档、冲突、默认解锁、技能应用。
- 验证方式
  - 默认技能解锁。
  - 消耗技能点。
  - 冲突分支锁定。
  - 退款。
  - 保存/读取技能树。
  - 技能槽图标和冷却正常。
- 是否适合初学者立即执行
  - 不建议第一轮做。

### 第 6 阶段：Frame 局部接入

- 修改目标
  - 在 UI 架构稳定后，选择性接入 Frame。
- 推荐顺序
  1. 修复 `EventCenter.Clear(E_EventType)`。
  2. 用 `EventCenter` 试点 PlayerDead 或 SkillUnlocked。
  3. 用 `PoolMgr` 试点 VFX。
  4. 评估新面板是否用 `UIMgr/BasePanel`。
  5. 评估音频 SFX 是否接对象池。
- 涉及文件
  - `Frame/EventCenter/EventCenter.cs`
  - `Frame/Pool/PoolMgr.cs`
  - VFX prefab
  - VFX 相关脚本
  - 新增或试点 UI 面板
- 修改风险
  - 中。
  - 主要风险在资源路径、Addressables key、池化对象 Reset。
- 验证方式
  - 单个 VFX 多次播放无残留。
  - 切场景后对象池清理正确。
  - 事件监听不重复。
- 是否适合初学者立即执行
  - 不建议作为第一步，适合作为 UI 重构稳定后的阶段。

### 第 7 阶段：场景流程与 Manager 瘦身

- 修改目标
  - 把 `GameManager` 中的场景切换、重生、存档等待、Fade 流程拆清楚。
  - `SceneMgr` 只作为底层加载工具。
- 涉及文件
  - `GameManager`
  - `SaveManager`
  - `UI_FadeScreen`
  - `Object_WayPoint`
  - `Object_Portal`
  - 新增 `SceneFlowController` 或 `GameFlowController`
- 修改风险
  - 高。
  - 容易影响存档、切场景、传送门。
- 验证方式
  - 主菜单继续游戏。
  - Level 间路点传送。
  - Portal 往返城镇。
  - 死亡重生。
  - 读取上次位置。
- 是否适合初学者立即执行
  - 不建议优先执行。

### 第 8 阶段：命名、注释、常量、目录整理

- 修改目标
  - 清理乱码注释。
  - 整理魔法字符串。
  - 统一命名。
  - 逐步整理目录。
- 涉及文件
  - 多数业务脚本。
- 修改风险
  - 中。
  - 改 serialized 字段名可能导致 prefab 引用丢失。
- 验证方式
  - Unity Inspector 引用不丢。
  - 所有核心流程回归。
- 是否适合初学者立即执行
  - 适合做注释和常量。
  - 不建议早期大规模重命名 public/serialized 字段。

## 附：最推荐的下一步

如果下一轮你希望开始实际修改，最推荐的安全顺序是：

1. 先确认 Unity Console 中 Frame 导入后的编译错误。
2. 修 `TextUtil.cs`、`UnityEditor` 引用、快捷栏空引用、输入重复订阅。
3. 从 `UI_Inventory` 开始做 MVP 试点。
4. 试点稳定后，再扩展到 Storage、Merchant、Craft。
5. 最后再接入 Frame 的 EventCenter、PoolMgr、UIMgr。

