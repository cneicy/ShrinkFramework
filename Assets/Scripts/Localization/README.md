# Localization 使用文档

## 概述
本本地化系统提供了基于 Unity 的多语言管理解决方案，支持以下核心功能：
- 多语言动态切换
- 智能占位符替换
- 跨场景事件通知
- 系统语言自动适配
- 文本组件自动更新

## 快速开始

### 1. 初始化配置
```csharp
// 在游戏初始化时调用
LocalizationManager.Instance.Initialize();
```

### 2. 加载语言
```csharp
// 加载简体中文
LocalizationManager.Instance.LoadLanguage(Language.zh_cn);

// 自动检测系统语言
LocalizationManager.Instance.UseSystemLanguage = true;
```

### 3. 获取本地化文本
```csharp
// 基础文本获取
string text = LocalizationManager.Instance.Get("ui.main.title");

// 带参数文本
string welcomeText = LocalizationManager.Instance.Get(
    "game.welcome", 
    "Player1", 
    DateTime.Now.ToString("HH:mm")
);
```

### 4. 绑定UI组件
```csharp
// 为Text组件添加LocalizedText脚本
public LocalizedText titleText;

void Start()
{
    titleText.SetKey("ui.main.title");
    titleText.SetArguments(100);
}
```

## 高级功能

### 参数说明
| 配置项 | 类型 | 说明 |
|--------|------|------|
| AutoUpdateTexts | bool | 自动更新绑定组件 (默认true) |
| MissingKeyPrefix | string | 缺失键标识前缀 (默认"[MISSING]") |
| PlaceholderStart | char | 占位符起始字符 (默认'{') |
| PlaceholderEnd | char | 占位符结束字符 (默认'}') |

### 事件系统集成
```csharp
// 订阅语言变更事件
EventManager.Instance.RegisterEvent<Language>(
    "OnLanguageChanged",
    lang => Debug.Log($"语言已切换到 {lang}")
);

// 触发语言切换请求
EventManager.Instance.TriggerEvent(
    "RequestLanguageChange", 
    Language.en_us
);
```

### 动态参数更新
```csharp
// 实时更新文本参数
LocalizedText scoreText;

void UpdateScore(int newScore)
{
    scoreText.UpdateArguments(newScore);
    scoreText.Refresh();
}
```

## 最佳实践
1. **键命名规范**：使用`<模块>.<分类>.<名称>`结构（例：dialogue.npc.greeting）
2. **参数管理**：避免在频繁更新的文本中使用复杂格式化
3. **资源组织**：按语言代码分目录存储翻译文件
4. **字体管理**：为不同语言绑定对应字体资源

## 注意事项
⚠️ **重要限制**：
- 语言文件必须存放于 Resources/Localization 目录
- 占位符索引必须从0开始连续编号
- 同一场景内避免重复注册事件监听
- 文本更新操作应在主线程执行
- 语言文件编码必须为 UTF-8

## 示例场景
```csharp
// 初始化配置
void Initialize()
{
    LocalizationManager.Instance.Config = new LocalizationConfig {
        MissingKeyPrefix = "[L10N_ERROR]",
        AutoUpdateTexts = true
    };
}

// 动态切换语言
public void OnLanguageDropdownChanged(int index)
{
    var selectedLang = (Language)index;
    LocalizationManager.Instance.LoadLanguage(selectedLang);
}

// 复杂文本应用
void ShowQuestInfo(Quest quest)
{
    string desc = LocalizationManager.Instance.Get(
        "quest.description",
        quest.Name,
        quest.RewardGold,
        quest.Deadline.ToString("yyyy-MM-dd")
    );
}
```

## 错误处理
系统自动处理以下异常情况：
- 缺失语言文件
- 无效占位符格式
- 参数数量不匹配
- 重复事件订阅
- 无效键值访问

所有错误将通过 Unity Console 输出带堆栈跟踪的详细信息，建议开启以下调试模式：
```csharp
[SerializeField] private bool _enableDebugMode = true;

void Awake()
{
    LocalizationManager.Instance.EnableDebugLog = _enableDebugMode;
}
```

## API 参考

### LocalizationManager
```csharp
void Initialize()
void LoadLanguage(Language language)
string Get(string key, params object[] args)
void RegisterText(LocalizedText text)
void UnregisterText(LocalizedText text)
```

### LocalizedText
```csharp
void SetKey(string key)
void SetArguments(params object[] args)
void Refresh()
void UpdateArguments(int index, object value)
```

## 性能建议
- 高频更新文本使用对象缓存
- 复杂格式化操作预先生成模板
- 批量文本更新使用事件延迟机制
- 长期闲置语言资源主动卸载
- 配合 Addressables 实现动态加载
