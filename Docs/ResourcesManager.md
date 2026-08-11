# ResourcesManager

Unity `Resources` 资源管理器，用于统一管理 `Resources` 目录下资源的加载、缓存和释放。

## 核心功能

- **同步加载**：通过 `Load<T>()` 加载资源。
- **异步加载**：通过 `LoadAsync<T>()` 加载资源。
- **请求合并**：相同路径和类型的资源只创建一次异步加载任务，多个请求共享同一个 `ResInfo`。
- **引用计数**：每次 `Load / LoadAsync` 增加引用计数，`Release` 减少引用计数。
- **资源释放**：引用计数归零后自动释放资源。
- **异步生命周期管理**：异步加载过程中即使引用归零，也会等待加载完成后再决定是否释放。
- **批量清理**：通过 `UnloadUnusedAssets()` 清理没有引用的资源。
- **状态查询**：可以查询资源当前的引用计数和加载状态。

## 核心结构

```text
ResourcesManager
│
├── resDic
│     └── path + Type
│            ↓
│         ResInfo<T>
│         ├── asset
│         ├── callback
│         ├── coroutine
│         ├── refCount
│         └── state
│
├── Load<T>()
├── LoadAsync<T>()
├── Release<T>()
├── UnloadUnusedAssets()
├── GetResourcesRefCount<T>()
└── GetLoadState<T>()
```

## 资源生命周期

### 同步加载

```text
Load
 ↓
Resources.Load
 ↓
缓存 ResInfo
 ↓
refCount++
 ↓
返回资源
```

### 异步加载

```text
LoadAsync
 ↓
检查缓存
 ↓
不存在 → 创建 ResInfo
 ↓
Loading
 ↓
Resources.LoadAsync
 ↓
Success
 ↓
执行所有 callback
```

相同资源的多个异步请求会合并：

```text
LoadAsync A ─┐
LoadAsync B ─┼→ 一个 ResourceRequest → A/B/C callback
LoadAsync C ─┘
```

### 资源释放

```text
Release
 ↓
refCount--
 ↓
refCount > 0 → 保留资源
 ↓
refCount == 0
 ↓
UnloadResource
```

如果资源仍处于 `Loading` 状态，则不会立即停止加载，而是等待异步加载完成后，根据 `refCount` 决定是否释放。

## 加载状态

```csharp
public enum ResLoadState
{
    Loading,
    Success,
    None
}
```

- `Loading`：正在异步加载。
- `Success`：资源加载成功并已缓存。
- `None`：资源当前不存在于 `ResourcesManager` 中。

## 使用示例

### 异步加载

```csharp
ResourcesManager.Instance.LoadAsync<GameObject>(
    "Prefabs/Player",
    prefab =>
    {
        if (prefab == null)
            return;

        Debug.Log("加载完成");
    });
```

### 同步加载

```csharp
GameObject prefab =
    ResourcesManager.Instance.Load<GameObject>("Prefabs/Player");
```

### 释放资源

```csharp
ResourcesManager.Instance.Release<GameObject>(
    "Prefabs/Player");
```

### 查询引用计数

```csharp
int count =
    ResourcesManager.Instance.GetResourcesRefCount<GameObject>(
        "Prefabs/Player");
```

## 设计要点

### 1. Path + Type 作为资源唯一 Key

```text
资源路径 + 资源类型
```

避免相同路径下不同类型资源产生冲突。

### 2. 引用计数管理生命周期

资源只有在：

```text
refCount == 0
```

时才允许真正释放，避免多个系统共享资源时被提前卸载。

### 3. 异步请求合并

同一个资源只维护一个异步加载流程，后续请求只增加引用计数并注册 callback，避免重复加载。

### 4. 特殊资源安全处理

`GameObject`、`Component`、`AssetBundle` 不直接调用 `Resources.UnloadAsset()`，避免错误卸载。

## 注意事项

`Load / LoadAsync` 与 `Release` 应保持匹配：

```text
Load        → Release
LoadAsync   → Release
```

资源加载失败时不会产生有效引用，因此不需要调用 `Release`。

> 当前版本主要用于学习 Unity 资源管理、引用计数和异步加载流程。大型项目可以进一步替换底层实现为 AssetBundle 或 Addressables，而上层资源管理接口可以保持类似设计。