# ObjectPool 使用文档

## 概述
本对象池管理系统提供了基于 Unity 的 GameObject 高效复用能力，支持以下核心功能：
- 多类型对象池管理
- 自动容量维护
- 跨场景持久化
- 活跃对象数量限制
- 异常安全机制

## 快速开始

### 1. 创建对象池
```csharp
// 在游戏初始化时调用
PoolManager.CreatePool(
    "bullet_pool",    // 唯一池标识
    bulletPrefab,     // 预制体引用
    defaultCapacity: 5,  // 初始容量
    maxSize: 20       // 最大容量
);
```

### 2. 获取对象实例
```csharp
// 从对象池获取实例
var bullet = PoolManager.Get<Bullet>("bullet_pool", weaponTransform);

// 使用默认父节点
var bullet = PoolManager.Get<Bullet>("bullet_pool");
```

### 3. 归还对象
```csharp
// 当对象不再使用时
PoolManager.Release("bullet_pool", bullet);
```

### 4. 销毁对象池
```csharp
// 销毁单个池（场景切换时）
PoolManager.DisposePool("bullet_pool");

// 销毁所有池（游戏退出时）
PoolManager.DisposeAllPools();
```

## 高级功能

### 参数说明
| 参数 | 类型 | 说明 |
|------|------|------|
| poolId | string | 唯一池标识（大小写敏感） |
| defaultCapacity | int | 初始预创建数量 (默认10) |
| maxSize | int | 最大活跃对象数 (默认20) |

### 自动回收机制
当活跃对象超过 maxSize 时，系统会自动回收最早激活的对象

### 生命周期控制
- 所有池对象在 DontDestroyOnLoad 场景中持久化
- 对象实例默认禁用状态存入池中
- 回收时自动重置父节点到池容器

## 最佳实践
1. **池标识规范**：使用`<系统名称><对象类型>`命名规则（例：FxExplosion）
2. **容量规划**：根据对象使用频率设置合理容量
3. **类型安全**：确保 Get/Release 操作的类型与创建池时一致
4. **异常处理**：通过 Debug 日志监控池操作错误

## 注意事项
⚠️ **重要限制**：
- 同一 poolId 不能重复创建
- 释放对象时必须使用原始池
- 预制体必须包含指定类型的组件
- 不可直接 Destroy 池对象，必须通过 Release 归还

## 示例场景
```csharp
// 创建敌人池
PoolManager.CreatePool("EntityEnemy", enemyPrefab, 10, 30);

// 战斗系统
void SpawnEnemy()
{
    var enemy = PoolManager.Get<Enemy>("EntityEnemy", spawnPoint);
    enemy.Initialize();
}

void OnEnemyDefeated(Enemy enemy)
{
    PoolManager.Release("EntityEnemy", enemy);
}

// 场景清理
void OnSceneUnload()
{
    PoolManager.DisposePool("EntityEnemy");
}
```

## 错误处理
系统会自动捕获以下异常情况：
- 空池ID创建请求
- 重复池ID创建
- 类型不匹配操作
- 访问不存在对象池
- 预制体丢失情况

所有错误将通过 Unity Console 输出，建议在开发阶段开启 Error Pause 功能。

## API 参考

### PoolManager
```csharp
static void CreatePool<T>(string poolId, T prefab, int defaultCapacity, int maxSize)
static T Get<T>(string poolId, Transform parent = null)
static void Release<T>(string poolId, T obj)
static void DisposePool(string poolId)
static void DisposeAllPools()
```

### 性能建议
- 高频对象优先使用对象池
- 长期闲置池建议主动销毁
- 避免在 Update 中频繁创建/销毁池
- 合理设置 maxSize 防止内存膨胀
