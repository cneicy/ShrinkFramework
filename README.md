# ShrinkFramework - Unity开发工具脚手架

![Unity Version](https://img.shields.io/badge/Unity-2022.3%2B-blue)
![License](https://img.shields.io/badge/License-MIT-green)

一个轻量级的Unity开发工具脚手架，提供游戏开发常用功能模块的标准化实现。包含事件管理、输入控制、本地化、对象池等核心系统，支持快速构建可维护的Unity应用。

📖 文档见各模块下README | 
[💡 提交Issue](https://github.com/cneicy/ShrinkFramework/issues)

_本仓库所有此README由Deepseek生成，某些功能别信它吹得这么邪乎，我自己的水平我还是有底的。_
## 核心功能

| 模块 | 功能亮点 | 关键特性 |
|------|----------|----------|
| **事件系统** | 松耦合通信 | 特性驱动订阅、多参数事件、自动生命周期管理 |
| **输入管理** | 可配置按键 | 动态绑定、冲突检测、多平台支持 |
| **本地化** | 多语言支持 | 智能占位符、UI自动更新、系统语言适配 |
| **对象池** | 高效对象复用 | 自动扩容、异常处理、跨场景持久化 |
| **屏幕适配** | 智能宽高比 | 自动黑边处理、编辑器实时预览 |
| **单例系统** | 全局访问 | 自动初始化、线程安全、场景持久化 |

## 快速开始

### 环境要求
- Unity 2022.3+
- .NET 4.x

### 安装方式
1. 克隆仓库到Unity项目：
```bash
git clone https://github.com/cneicy/ShrinkFramework.git
```
2. 将 `Assets/Scripts` 目录导入Unity工程

### 基础使用示例
```csharp
// 初始化框架核心
SingletonAutoInitializer.InitializeAllSingletonTypes();

// 事件注册与触发
EventManager.Instance.RegisterEvent<int>("PlayerAttack", damage => {
    Debug.Log($"造成伤害: {damage}");
    return null;
});
EventManager.Instance.TriggerEvent("PlayerAttack", 50);

// 本地化文本获取
string welcome = LocalizationManager.Instance.Get("ui.welcome", "Player1");

// 对象池使用
var bullet = PoolManager.Get<Bullet>("bullet_pool");
PoolManager.Release("bullet_pool", bullet);
```

## 模块概览

### 1. 事件管理系统 (`Events/`)
```csharp
[EventSubscribe("OnLevelUp")]
public object HandleLevelUp(int newLevel)
{
    // 处理升级事件
    return new { success = true };
}
```
- 特性驱动自动订阅
- 支持泛型参数和返回值
- 对象级生命周期管理

### 2. 输入管理系统 (`Input/`)
```csharp
// 动态修改按键绑定
KeySettingManager.Instance.SetKey("Jump", KeyCode.Space);

// 获取当前输入方向
Vector2 moveInput = KeySettingManager.Instance.Direction;
```
- 支持按键冲突检测
- 配置自动持久化
- 多设备输入适配

### 3. 本地化系统 (`Localization/`)
```csharp
// 带参数的本地化文本
string text = LocalizationManager.Instance.Get(
    "quest.reward", 
    100, 
    DateTime.Now.ToString("d")
);
```
- 支持动态参数替换
- 自动检测系统语言
- 实时UI更新

### 4. 对象池系统 (`ObjectPool/`)
```csharp
PoolManager.CreatePool("enemies", prefab, 10, 20);
var enemy = PoolManager.Get<Enemy>("enemies");
PoolManager.Release("enemies", enemy);
```
- 自动容量维护
- 异常安全机制
- 支持批量操作

### 5. 屏幕适配系统 (`ScreenAspect/`)
```csharp
// 设置21:9超宽屏适配
GetComponent<ScreenAspector>().targetAspect = 21f / 9f;
```
- 智能黑边处理
- 编辑器实时预览
- 动态分辨率适配

## 目录结构
```
Assets/Scripts/
├
├── Events/            # 事件管理系统
├── Input/             # 输入管理系统
├── Localization/      # 本地化系统
├── Singleton/         # 单例基类
├── ObjectPool/        # 对象池系统 
└── ScreenAspect/      # 屏幕适配
```

## 最佳实践
1. **事件命名规范**：使用`模块.动作`格式（例：`Player.Jump`）
2. **按键配置**：通过事件系统监听配置变更
3. **本地化键管理**：使用分层命名（`ui.menu.start`）
4. **对象池选择**：高频创建对象优先使用池
5. **单例使用**：核心管理类继承`Singleton<T>`

## 贡献指南
欢迎通过Issue和PR参与贡献：
1. Fork仓库并创建特性分支
2. 遵循现有代码风格（C#命名规范）
3. 添加必要的单元测试
4. 更新相关文档
5. 提交Pull Request

## 许可协议
本项目采用 [MIT License](LICENSE)，可自由用于商业项目。

---

> 更多详细用法请参考各模块子文档：[事件管理](Events/README.md) | [输入系统](Input/README.md) | [本地化](Localization/README.md) | [对象池](ObjectPool/README.md) | [宽高适配](ScreenAspect/README.md) | [单例](Singleton/README.md)
