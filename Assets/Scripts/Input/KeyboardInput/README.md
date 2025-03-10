# 键盘输入管理系统使用文档

## 概述
本系统提供完整的键盘按键配置管理解决方案，包含按键映射、持久化存储、事件通知、冲突检测等功能。适用于需要可定制按键操作的游戏项目。

---

## 功能特性
- **动态按键配置**：实时修改和保存按键设置
- **冲突检测机制**：自动防止按键重复绑定
- **事件驱动架构**：关键操作均有事件通知
- **默认配置保护**：自动补全缺失的默认按键
- **输入状态管理**：内置方向输入处理逻辑

---

## 快速开始

### 1. 初始化配置
```csharp
// 自动执行流程：
// 1. Awake时加载KeySettings.json
// 2. 补全默认配置
// 3. 初始化方向输入检测
```

### 2. 基本使用
```csharp
// 获取攻击按键
KeyCode? attackKey = KeySettingManager.Instance.GetKey("Attack");

// 修改移动按键
bool success = KeySettingManager.Instance.SetKey("Left", KeyCode.LeftArrow);

// 添加新按键
KeySettingManager.Instance.AddKeyMapping("SkillQ", KeyCode.Q);
```

---

## 核心功能说明

### 1. 按键映射管理
| 方法 | 说明 |
|------|------|
| `AddKeyMapping()` | 添加新按键映射 |
| `RemoveKeyMapping()` | 移除现有映射 |
| `SetKey()` | 修改已有映射 |
| `GetAllMappings()` | 获取所有配置 |

### 2. 方向输入处理
```csharp
// 实时获取方向输入
Vector2 moveInput = KeySettingManager.Instance.Direction;

// 等效于检测：
float h = Input.GetKey(Left) ? -1 : (Input.GetKey(Right) ? 1 : 0;
float v = Input.GetKey(Down) ? -1 : (Input.GetKey(Up) ? 1 : 0;
```

### 3. 配置持久化
- 存储路径：`Application.persistentDataPath/KeySettings.json`
- 自动保存时机：
    - 添加/删除按键
    - 修改按键绑定
    - 重置默认设置

---

## 事件系统

### 事件类型列表
| 事件常量 | 触发时机 |
|----------|----------|
| KeyAdded | 新增按键映射 |
| KeyRemoved | 移除按键映射 |
| KeyChanged | 修改按键绑定 |
| KeyConflict | 检测到按键冲突 |
| KeySettingsLoaded | 配置加载完成 |
| KeySettingsSaved | 配置保存成功 |
| KeySettingsReset | 重置为默认配置 |

### 事件订阅示例
```csharp
void OnEnable()
{
    EventManager.Instance.RegisterEvent<KeyEventArgs>(KeySettingManager.KeyEvents.KeyChanged, OnKeyChanged);
}

void OnKeyChanged(KeyEventArgs args)
{
    Debug.Log($"按键修改: {args.ActionName} {args.OldKey}=>{args.NewKey}");
}

void OnConflict(object data)
{
    var conflictInfo = new { data.AttemptedAction, data.ConflictingAction };
    Debug.Log($"按键冲突: {conflictInfo.AttemptedAction} 与 {conflictInfo.ConflictingAction}");
}
```

---

## 高级配置

### 1. 修改默认配置
```csharp
// 在Awake前修改默认配置
void SetupCustomDefaults()
{
    KeySettingManager.Instance.OverrideDefaults(new[]
    {
        new KeyMapping("Jump", KeyCode.Space),
        new KeyMapping("Crouch", KeyCode.C)
    });
}
```

### 2. 自定义存储路径
```csharp
KeySettingManager.Instance.SetCustomSavePath("/SaveData/Controls.json");
```

### 3. 运行时热更新
```csharp
IEnumerator ReloadConfig()
{
    KeySettingManager.Instance.ForceReload();
    yield return new WaitUntil(() => 
        EventManager.Instance.GetEventCount(KeySettingManager.KeyEvents.SettingsLoaded) > 0);
}
```

---

## 最佳实践

### 1. UI配置界面实现
```csharp
public class KeyConfigUI : MonoBehaviour
{
    public Dropdown keyDropdown;
    public Text conflictWarning;

    void Start()
    {
        EventManager.Instance.RegisterEvent(KeySettingManager.KeyEvents.KeyConflict, 
            data => ShowConflictWarning(data));
    }

    public void OnKeySelected(string actionName)
    {
        var selectedKey = (KeyCode)keyDropdown.value;
        KeySettingManager.Instance.SetKey(actionName, selectedKey);
    }

    void ShowConflictWarning(object data)
    {
        conflictWarning.text = $"按键冲突! 已绑定到: {data.ConflictingAction}";
    }
}
```

### 2. 输入缓冲系统
```csharp
public class InputBuffer : MonoBehaviour
{
    private Dictionary<string, float> _bufferTimes = new();

    void Update()
    {
        foreach (var mapping in KeySettingManager.Instance.GetAllMappings())
        {
            if (Input.GetKeyDown(mapping.keyCode))
            {
                _bufferTimes[mapping.actionName] = Time.time + 0.2f;
            }
        }
    }

    public bool GetBufferedInput(string actionName)
    {
        return _bufferTimes.TryGetValue(actionName, out var time) && Time.time <= time;
    }
}
```

### 3. 平台差异化配置
```csharp
#if UNITY_STANDALONE_WIN
    KeySettingManager.Instance.AddKeyMapping("AltFire", KeyCode.Mouse1);
#elif UNITY_PS4
    KeySettingManager.Instance.AddKeyMapping("AltFire", KeyCode.JoystickButton2);
#endif
```

---

## 注意事项

### 1. 按键冲突处理
- 使用`SetKey`时会自动检测冲突
- 可通过事件系统监听KeyConflict事件
- 强制覆盖使用：
  ```csharp
  KeySettingManager.Instance.ForceSetKey("Attack", KeyCode.K, true);
  ```

### 2. 特殊按键支持
- 支持组合键检测：
  ```csharp
  if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.S))
  {
      // 保存操作
  }
  ```

### 3. 移动端适配
```csharp
// 在移动设备禁用键盘配置
if (Application.isMobilePlatform)
{
    KeySettingManager.Instance.DisableKeyboardInput();
    ShowTouchControls();
}
```

---

## 故障排查

### Q1: 配置未正确保存
- 检查应用数据目录的写入权限
- 确认没有多个单例实例
- 查看控制台错误日志

### Q2: 事件未触发
- 确认事件名称拼写完全一致
- 检查订阅方法的参数类型匹配
- 确保EventManager已初始化

### Q3: 方向输入不更新
- 确认Update方法正常执行
- 检查按键映射是否存在Left/Right/Up/Down
- 验证GetKeyState的检测逻辑

---

## 性能优化建议

1. **按需更新检测**：
```csharp
// 在配置界面打开时启用检测
void OnConfigOpen()
{
    KeySettingManager.Instance.EnableInputDetection();
}

void OnConfigClose()
{
    KeySettingManager.Instance.DisableInputDetection();
}
```

2. **缓存常用查询**：
```csharp
private Dictionary<string, KeyCode> _keyCache = new();

void BuildKeyCache()
{
    foreach (var mapping in KeySettingManager.Instance.GetAllMappings())
    {
        _keyCache[mapping.actionName] = mapping.keyCode;
    }
}
```

3. **批量操作优化**：
```csharp
IEnumerator BatchUpdateKeys(List<KeyValuePair<string, KeyCode>> updates)
{
    KeySettingManager.Instance.BeginBatchUpdate();
    foreach (var update in updates)
    {
        KeySettingManager.Instance.SetKey(update.Key, update.Value);
    }
    yield return new WaitForEndOfFrame();
    KeySettingManager.Instance.EndBatchUpdate();
}
```

---

## 扩展接口

### 自定义序列化
```csharp
public interface IKeySettingsSerializer
{
    void Save(Dictionary<string, KeyMapping> mappings);
    Dictionary<string, KeyMapping> Load();
}

public class EncryptedSerializer : IKeySettingsSerializer
{
    // 实现加密存储逻辑
}
```

### 输入历史记录
```csharp
public class InputRecorder : MonoBehaviour
{
    void Update()
    {
        var state = KeySettingManager.Instance.GetCurrentInputState();
        RecordInput(state);
    }
}
```

### 云端同步
```csharp
public class CloudSync : MonoBehaviour
{
    void SyncKeys()
    {
        var json = KeySettingManager.Instance.ExportJson();
        CloudService.Upload("keySettings", json);
    }
}
```