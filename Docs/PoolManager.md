# Unity Object Pool System

## 1. 模块介绍

Object Pool（对象池）是一种用于优化 Unity 游戏运行性能的内存管理方案。

在游戏运行过程中，大量频繁创建和销毁 GameObject 会导致：

-   GC（Garbage Collection）压力增加
-   CPU 消耗增加
-   内存碎片产生
-   游戏出现卡顿

对象池通过提前创建对象并循环利用，减少运行时 Instantiate 和 Destroy
带来的开销。

常见应用：

-   技能特效
-   子弹
-   怪物单位
-   飘字
-   UI元素
-   临时战斗对象

------------------------------------------------------------------------

# 2. 设计目标

## 2.1 对象复用

对象生命周期：

    Create
      ↓
    使用
      ↓
    回收
      ↓
    重新使用

避免：

    Instantiate
          ↓
    Destroy
          ↓
    Instantiate
          ↓
    Destroy

导致频繁内存分配。

## 2.2 数量限制

每种对象通过 PoolObj 配置最大数量：

    Bullet
    {
        poolMax = 50
    }

规则：

1.  空闲池有对象：
    -   直接取出
2.  空闲池为空，未达到最大数量：
    -   创建新对象
3.  达到最大数量：
    -   复用当前使用时间最长对象

------------------------------------------------------------------------

# 3. 系统结构

    PoolManager

        |
        |
        +---- Dictionary<string, PoolData>


                    PoolData

                    |
                    +---- Stack
                    |     空闲对象池
                    |
                    +---- List
                          使用中对象列表

## PoolManager

负责：

-   管理所有对象池
-   创建对象池
-   获取对象
-   回收对象

## PoolData

负责：

-   管理单个类型对象
-   保存空闲对象
-   保存使用对象
-   控制最大数量

------------------------------------------------------------------------

# 4. 数据结构

## 空闲对象池

``` csharp
private Stack poolStack = new Stack();
```

使用 Stack：

-   获取速度 O(1)
-   适合对象快速取出和回收

## 使用对象列表

``` csharp
private List usedList = new List();
```

作用：

-   保存正在使用对象
-   记录对象使用顺序
-   达到最大数量时进行复用

------------------------------------------------------------------------

# 5. 获取对象流程

调用：

``` csharp
PoolManager.Instance.getObject("Bullet");
```

流程：

    GetObject

        |
        |
    是否存在对象池

        |
     +--+--+

     No    Yes

     |      |
    创建   检查空闲池

              |
           +--+--+

          有     无

          |       |
        Pop    判断数量

                  |
              +---+---+

            未达到   达到

              |       |

            创建    复用最旧对象

------------------------------------------------------------------------

# 6. 回收对象流程

调用：

``` csharp
PoolManager.Instance.putObject("Bullet", bullet);
```

流程：

    Object

      |
    SetActive(false)

      |
    加入 Stack

      |
    移除 UsedList

------------------------------------------------------------------------

# 7. 最大数量控制

对象预制体挂载：

``` csharp
PoolObj
{
    poolMax = 50
}
```

初始化时读取：

``` csharp
usedMax = poolObj.poolMax;
```

------------------------------------------------------------------------

# 8. 对象池满处理

当：

    emptyCount == 0

    usedCount >= usedMax

不会继续创建。

而是：

获取：

``` csharp
usedList[0]
```

复用最早使用对象。

例如：

    UsedList

    A
    B
    C

再次请求：

    复用 A

结果：

    UsedList

    B
    C
    A

------------------------------------------------------------------------

# 9. Hierarchy整理

支持：

``` csharp
public bool useLayout;
```

开启：

    Scene

     |
     PoolRoot

          |
          Bullet

              Bullet01
              Bullet02

优点：

-   调试方便
-   层级清晰

------------------------------------------------------------------------

# 10. 使用示例

创建：

``` csharp
GameObject obj =
PoolManager.Instance.getObject("Bullet");
```

回收：

``` csharp
PoolManager.Instance.putObject(
    "Bullet",
    obj
);
```

------------------------------------------------------------------------

# 11. 资源目录

    Resources

        Pool

            Bullet.prefab
            Enemy.prefab
            Effect.prefab

加载：

``` csharp
Resources.Load<GameObject>(
    "Pool/Bullet"
);
```

------------------------------------------------------------------------

# 12. 当前版本特点

## 优点

-   减少GC压力
-   支持多类型对象管理
-   支持对象数量限制
-   结构简单易扩展

------------------------------------------------------------------------

# 13. 后续优化方向

## 泛型对象池

当前：

``` csharp
Dictionary<string,PoolData>
```

可优化：

``` csharp
ObjectPool<T>
```

## Queue优化

当前 List：

``` csharp
RemoveAt(0)
```

复杂度：

    O(n)

可改：

    Queue

实现 O(1)。

## Addressables支持

替换：

    Resources.Load

支持：

-   异步加载
-   AssetBundle
-   热更新

## 自动扩容

增加：

    expandSize

达到上限后自动增加对象数量。

------------------------------------------------------------------------

# 14. 总结

该对象池系统实现：

-   多对象池管理
-   空闲对象缓存
-   使用对象追踪
-   最大数量控制
-   对象循环复用
-   Hierarchy整理

可以作为 Unity MMORPG Framework 基础模块：

    Framework

     |
     +-- ResourceManager

     +-- PoolManager

     +-- EventBus

     +-- UIManager

     +-- NetworkManager

用于管理：

-   战斗单位
-   技能特效
-   子弹
-   飘字
-   UI缓存

降低GC压力，提高游戏运行稳定性。
