# 单例基类与自动初始化文档

## 概述
本系统提供在Unity编辑器模式下自动初始化单例对象的功能，以及一个安全的运行时单例基类。适用于需要全局访问且保持唯一实例的组件。

---

## 功能特性
- **编辑器自动初始化**：场景加载后自动创建未实例化的单例对象
- **运行时安全访问**：防止重复创建和空引用
- **DontDestroyOnLoad支持**：在场景切换时保持实例
- **自定义控制**：可通过特性禁用自动创建

---

## 快速开始

### 1. 创建单例类
```csharp
using Singleton;
using UnityEngine;

// 基础单例示例
public class GameManager : Singleton<GameManager>
{
    public void Init()
    {
        Debug.Log("GameManager initialized!");
    }
}

// 禁用自动创建的单例示例
[DisableAutoCreate]
public class AudioManager : Singleton<AudioManager>
{
    // 此单例不会自动创建
}
```

### 2. 访问单例实例
```csharp
// 在任何脚本中访问
void Start()
{
    GameManager.Instance.Init();
    
    // 安全访问方式
    if (AudioManager.Instance != null)
    {
        AudioManager.Instance.PlayBGM();
    }
}
```

---

## 编辑器功能说明

### 自动初始化流程
1. 当场景加载完成时
2. 扫描所有继承 `Singleton<T>` 的类
3. 检查场景中是否已存在实例
4. 对未实例化的单例：
    - 创建同名GameObject
    - 添加对应组件
    - 标记场景需要保存

### 手动触发初始化
在编辑器窗口执行：
```csharp
UnityEditor.EditorApplication.delayCall += SingletonAutoInitializer.InitializeAllSingletonTypes;
```

---

## 注意事项

### 1. 编辑器模式限制
- 自动创建仅在编辑器模式下生效
- 运行时需通过 `Instance` 属性访问

### 2. 组件要求
- 必须继承自 `Singleton<T>`
- 必须是具体类（非abstract）
- 需要挂载在GameObject上

### 3. 生命周期管理
- 使用 `DisableAutoCreate` 特性可禁用自动创建
- 在Awake中初始化数据而非构造函数
- 重写Awake时需调用base.Awake()

```csharp
protected override void Awake()
{
    base.Awake(); // 必须调用基类方法
    // 自定义初始化代码
}
```

### 4. 场景操作
- 自动创建后需手动保存场景
- 多个场景切换时建议使用DontDestroyOnLoad

---

## 常见问题

### Q1: 如何防止自动创建？
```csharp
[DisableAutoCreate]
public class CustomManager : Singleton<CustomManager>
{
    // 此单例不会自动创建
}
```

### Q2: 运行时出现重复实例？
- 确保继承关系正确
- 检查Awake方法是否调用了base.Awake()
- 确认没有手动创建实例

### Q3: 编辑器没有自动创建？
- 确认类未使用DisableAutoCreate特性
- 检查场景是否已保存
- 查看控制台日志输出

---

## 技术实现细节

### 编辑器部分
- 使用 `InitializeOnLoad` 特性注册初始化回调
- 延迟执行确保场景加载完成
- 反射扫描所有程序集查找单例类型

### 运行时部分
- 双检锁模式保证线程安全
- 应用退出状态标记防止空引用
- 自动销毁重复实例

---

## 最佳实践
1. 对需要持久化数据的单例使用自动创建
2. 对场景相关单例使用DisableAutoCreate
3. 在Instance属性访问时进行空值检查
4. 使用独立GameObject存放重要单例
5. 在OnApplicationQuit中清理资源

```csharp
private void OnApplicationQuit()
{
    // 清理资源代码
    _isQuitting = true; // 自动设置，无需手动处理
}
```