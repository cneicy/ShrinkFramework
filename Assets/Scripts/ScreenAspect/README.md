# 屏幕宽高比适配使用文档

## 概述
本系统提供自动化的相机宽高比适配解决方案，包含编辑器工具链支持。主要功能包括：
- 自动保持目标宽高比显示
- 智能黑边处理
- 编辑器自动组件附加
- 动态分辨率适配

---

## 功能特性
- **自动宽高比适配**：根据目标比例动态调整视口
- **智能组件附加**：自动挂载到主相机
- **编辑器集成**：场景加载时自动配置
- **安全检测机制**：缺失主相机时自动创建
- **实时预览**：编辑器模式下即时生效

---

## 快速开始

### 基础配置
1. 添加组件到场景
```csharp
// 手动添加方式
var mainCamera = Camera.main.gameObject;
mainCamera.AddComponent<ScreenAspector>();
```

2. 配置目标比例（默认16:9）
```csharp
// 代码动态修改
GetComponent<ScreenAspector>().targetAspect = 21f / 9f; 
```

3. 启用自动附加（编辑器自动完成）
```csharp
[AutoAttachToMainCamera(CreateIfMissing = false)]
public class CustomAspectAdapter : MonoBehaviour 
{
    // 仅在存在主相机时附加
}
```

---

## 核心组件说明

### ScreenAspector 组件
| 属性 | 类型 | 说明 |
|------|------|------|
| targetAspect | float | 目标宽高比（0.1-3.0）|

**核心方法**：
- `UpdateAspect()`: 强制刷新视口计算

### AutoAttachToMainCamera 特性
```csharp
[AutoAttachToMainCamera(
    CreateIfMissing = true // 是否自动创建主相机
)]
```
- 标记需要自动附加到主相机的组件类
- 支持继承检测（默认不继承）

---

## 编辑器行为

### 自动初始化流程
1. 场景加载完成后检测主相机
2. 不存在主相机时自动创建（带AudioListener）
3. 扫描所有带特性的组件类型
4. 附加缺失组件到主相机

**触发条件**：
- 新建场景
- 重新加载场景
- 手动保存场景时

---

## 注意事项

### 1. 主相机要求
- 必须正确设置`MainCamera`标签
- 建议保持默认相机配置
- 多相机场景需手动管理

### 2. 动态分辨率适配
```csharp
// 在分辨率变化时调用
void OnResolutionChanged()
{
    GetComponent<ScreenAspector>().UpdateAspect();
}
```

### 3. 编辑器限制
- 自动附加仅在编辑器模式生效
- 运行时不自动创建组件
- 修改特性参数需重新加载场景

---

## 高级配置

### 禁用自动创建相机
```csharp
[AutoAttachToMainCamera(CreateIfMissing = false)]
public class SafeAspector : MonoBehaviour 
{
    // 仅在存在主相机时附加
}
```

### 手动附加组件
```csharp
#if UNITY_EDITOR
[MenuItem("Tools/Attach Aspect Components")]
static void ForceAttach()
{
    AutoCameraAttachInitializer.ProcessCameraAttachments();
}
#endif
```

### 多目标比例切换
```csharp
IEnumerator DynamicAspectRoutine()
{
    yield return new WaitForSeconds(5);
    targetAspect = 4f / 3f;  // 切换到4:3比例
    UpdateAspect();
}
```

---

## 常见问题

### Q1: 组件未自动附加
- 确认类已添加`[AutoAttachToMainCamera]`特性
- 检查主相机标签是否正确
- 尝试手动执行初始化工具

### Q2: 黑边显示不正确
- 确认ScreenAspector组件已附加
- 检查目标比例计算是否正确
- 调用`UpdateAspect()`强制刷新

### Q3: 运行时修改无效
- 确保在UpdateAspect()后调用
- 检查相机rect参数是否被其他系统修改
- 确认未禁用组件

---

## 技术实现细节

### 视口计算逻辑
```
当前比例 = 屏幕宽度 / 屏幕高度
缩放比例 = 当前比例 / 目标比例

if 缩放比例 < 1.0:
    垂直黑边
else:
    水平黑边
```

### 编辑器初始化机制
- 使用`[InitializeOnLoad]`注册回调
- 延迟执行至场景完全加载
- 反射扫描带特性组件

---

## 最佳实践

1. **测试分辨率方案**：
```csharp
// 编辑器测试脚本
[ExecuteInEditMode]
public class AspectTester : MonoBehaviour
{
    [SerializeField] Vector2[] testResolutions = {
        new(1920, 1080),
        new(2560, 1440),
        new(1280, 720)
    };

    void Update()
    {
        foreach (var res in testResolutions)
        {
            // 模拟分辨率变化
        }
    }
}
```

2. **场景配置检查表**：
- [ ] MainCamera标签正确
- [ ] ScreenAspector组件存在
- [ ] 目标比例参数合理
- [ ] 无其他相机系统干扰

3. **UI适配建议**：
- 结合Canvas Scaler使用
- 重要UI元素保持在安全区内
- 使用Anchors适应不同比例

---

## 性能优化

### 1. 避免频繁更新
```csharp
void Update()
{
    // 错误示范：每帧检测
    if (Screen.width != lastWidth || Screen.height != lastHeight)
    {
        UpdateAspect();
    }
}

// 正确方式：使用事件监听
private void OnEnable()
{
    StartCoroutine(AspectCheckRoutine());
}

IEnumerator AspectCheckRoutine()
{
    var lastSize = new Vector2(Screen.width, Screen.height);
    while (true)
    {
        yield return new WaitForSeconds(0.5f);
        if ((Vector2)ScreenSize != lastSize)
        {
            UpdateAspect();
            lastSize = ScreenSize;
        }
    }
}
```

### 2. 多相机优化策略
```csharp
// 分离渲染相机
public class BackgroundCamera : MonoBehaviour
{
    void Start()
    {
        GetComponent<Camera>().rect = Camera.main.rect;
    }
}
```

---

## 扩展建议

### 1. 添加调试面板
```csharp
#if UNITY_EDITOR
[CustomEditor(typeof(ScreenAspector))]
public class ScreenAspectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Test Aspect"))
        {
            ((ScreenAspector)target).UpdateAspect();
        }
    }
}
#endif
```

### 2. 集成设备信息
```csharp
public class DeviceAspectHelper : MonoBehaviour
{
    void Start()
    {
        var aspector = GetComponent<ScreenAspector>();
        aspector.targetAspect = DetectIdealAspect();
    }

    float DetectIdealAspect()
    {
        // 根据设备类型返回最佳比例
    }
}
```