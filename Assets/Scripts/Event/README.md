# 事件管理使用文档

## 概述
本系统提供基于单例模式的事件管理机制，支持动态注册/注销事件、自动订阅检测、多参数事件触发等功能。适用于实现模块间松耦合通信。

---

## 功能特性
- **单例安全访问**：继承自`Singleton<EventManager>`保证全局唯一
- **泛型事件支持**：支持带参数和返回值的事件处理
- **特性驱动订阅**：通过`[EventSubscribe]`自动注册处理方法
- **生命周期管理**：提供对象级事件注销功能
- **调试支持**：内置事件取消和全局清理日志

---

## 快速开始

### 1. 基础事件使用
```csharp
// 注册事件
EventManager.Instance.RegisterEvent<int>("PlayerAttack", damage => {
    Debug.Log($"造成伤害: {damage}");
    return null;
});

// 触发事件
EventManager.Instance.TriggerEvent("PlayerAttack", 50);

// 注销事件
EventManager.Instance.UnregisterEvent<int>("PlayerAttack", handler);
```

### 2. 自动订阅特性
```csharp
public class PlayerController : MonoBehaviour
{
    private void OnEnable()
    {
        EventManager.Instance.RegisterEventHandlersFromAttributes(this);
    }

    [EventSubscribe("OnPlayerHurt")]
    public object HandleHurtEvent(int damage)
    {
        Debug.Log($"受到伤害: {damage}");
        return new { Success = true };
    }
}

// 触发特性事件
var result = EventManager.Instance.TriggerEvent<int>("OnPlayerHurt", 30);
```

---

## 核心方法说明

### 1. 事件注册
```csharp
// 泛型注册方法
public void RegisterEvent<T>(string eventName, Func<T, object> handler)
```
- `eventName`: 事件唯一标识
- `handler`: 事件处理委托，接受泛型参数并返回object

### 2. 事件触发
```csharp
public object TriggerEvent<T>(string eventName, T args)
```
- 返回最后一个处理程序的执行结果
- 支持传递任意类型参数

### 3. 批量注销
```csharp
public void UnregisterAllEventsForObject(object targetObject)
```
- 自动清理指定对象关联的所有事件
- 建议在`OnDestroy`中调用

---

## 特性使用指南

### EventSubscribeAttribute
标记自动注册的事件处理方法：
```csharp
[EventSubscribe("事件名称")]
public object MethodName(参数类型 args)
{
    // 处理逻辑
    return 返回值; // 可返回null
}
```
**特性要求**：
1. 方法必须为实例方法
2. 必须包含且只能包含一个参数
3. 返回值必须为object类型
4. 方法访问级别限制为public

---

## 生命周期管理

### 推荐使用模式
```csharp
public class EventUser : MonoBehaviour
{
    private void OnEnable()
    {
        // 自动注册特性方法
        EventManager.Instance.RegisterEventHandlersFromAttributes(this);
        
        // 手动注册
        EventManager.Instance.RegisterEvent<string>("UIUpdate", OnUIUpdate);
    }

    private void OnDisable()
    {
        // 自动清理本对象所有事件
        EventManager.Instance.UnregisterAllEventsForObject(this);
    }

    private object OnUIUpdate(string msg)
    {
        Debug.Log(msg);
        return null;
    }
}
```

---

## 注意事项

### 1. 参数类型匹配
- 注册与触发时必须使用相同的泛型类型参数
- 错误示例：
  ```csharp
  // 注册
  RegisterEvent<int>("Test", ...)
  // 触发（错误）
  TriggerEvent<float>("Test", ...)
  ```

### 2. 返回值处理
- 多个处理程序时返回最后一个有效结果
- 使用返回值时应进行空值检查

### 3. 性能优化
- 避免在高频事件中进行复杂操作
- 对高频事件使用特定的事件池
- 及时注销不再使用的事件

### 4. 场景切换
- 使用`DontDestroyOnLoad`保持事件管理器
- 切换场景时建议调用`UnregisterAllEvents()`

---

## 高级用法示例

### 1. 带返回值的事件
```csharp
// 注册验证处理器
EventManager.Instance.RegisterEvent<LoginData>("BeforeLogin", data => {
    return new { IsValid = !string.IsNullOrEmpty(data.Account) };
});

// 触发并获取结果
var result = EventManager.Instance.TriggerEvent<LoginData>("BeforeLogin", loginData);
if ((result as dynamic).IsValid == false)
{
    // 处理验证失败
}
```

### 2. 组合事件处理
```csharp
void RegisterCombatEvents()
{
    // 物理伤害
    EventManager.Instance.RegisterEvent<DamageInfo>("CalculateDamage", info => {
        info.Amount += info.Source.Strength * 0.5f;
        return info;
    });

    // 元素加成
    EventManager.Instance.RegisterEvent<DamageInfo>("CalculateDamage", info => {
        info.Amount *= ElementMultiplier[info.ElementType];
        return info;
    });
}

// 触发处理链
var finalDamage = EventManager.Instance.TriggerEvent<DamageInfo>("CalculateDamage", baseDamage);
```

---

## 常见问题

### Q1: 为什么特性订阅不生效？
- 检查方法签名是否符合要求
- 确认已调用`RegisterEventHandlersFromAttributes()`
- 确保事件名称拼写一致

### Q2: 如何处理多个返回值？
- 返回元组类型：
  ```csharp
  return (result1, result2);
  ```
- 使用out参数（需包装为对象）

### Q3: 事件冲突如何调试？
- 使用唯一事件命名规范（例：模块_事件名）
- 触发前检查事件是否存在：
  ```csharp
  if (EventManager.Instance.ContainsEvent("MyEvent"))
  {
      // 安全触发
  }
  ```

---

## 技术实现细节

### 1. 事件存储结构
- 使用`Dictionary<string, Delegate>`存储事件委托
- 支持同一事件名的多播委托

### 2. 反射机制
- 通过`GetCustomAttributes`扫描特性方法
- 使用`Delegate.CreateDelegate`动态创建委托

### 3. 类型安全
- 泛型方法确保参数类型一致性
- 运行时类型检查保障系统稳定性

---

## 性能建议

1. **高频事件优化**：
```csharp
// 缓存事件触发委托
private Func<int, object> _cachedTrigger;

void Start()
{
    _cachedTrigger = val => EventManager.Instance.TriggerEvent<int>("HighFrequencyEvent", val);
}

void Update()
{
    _cachedTrigger(Time.frameCount);
}
```

2. **使用值类型参数**：
```csharp
// 使用struct减少GC
public struct DamageInfo
{
    public int BaseDamage;
    public float Multiplier;
}

// 注册
RegisterEvent<DamageInfo>("CalculateDamage", ...);
```

---

## 最佳实践

1. **事件命名规范**：
```csharp
// 使用 [模块][事件类型] 格式
const string EVENT_PLAYER_HEALTH_CHANGE = "PlayerHealthChange";
const string EVENT_UI_SCORE_UPDATE = "UIScoreUpdate";
```

2. **错误处理增强**：
```csharp
public object SafeTrigger<T>(string eventName, T args)
{
    try 
    {
        return EventManager.Instance.TriggerEvent<T>(eventName, args);
    }
    catch (Exception e)
    {
        Debug.LogError($"事件处理错误: {eventName}\n{e}");
        return null;
    }
}
```

3. **编辑器扩展建议**：
- 添加事件监控窗口
- 实现事件历史记录
- 开发可视化事件流调试工具