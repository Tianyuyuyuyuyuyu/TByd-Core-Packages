# CollectionUtils 示例

本示例演示了`CollectionUtils`类的各种集合处理功能，包括：

## 功能演示

### 批处理操作
- 批量处理大量数据（BatchProcess）
- 控制批处理大小和处理逻辑

### 过滤与映射
- 集合过滤（Filter）
- 集合映射（Map）
- 链式操作（链式调用多个集合操作）

### 分页功能
- 分页数据（Paginate）
- 计算总页数
- 页面导航

### 集合比较
- 集合相等性比较（AreEqual）
- 忽略顺序比较（AreEqualIgnoreOrder）
- 查找集合差异（FindDifferences）

### 随机化
- 集合随机排序（Shuffle）
- 随机选取N个元素（Shuffle和Take组合使用）

## 使用方法

1. 打开示例场景 `CollectionUtilsDemo.unity`
2. 场景包含多个UI面板，每个面板对应一个功能组：
   - 批处理操作面板
   - 过滤与映射面板
   - 分页功能面板
   - 集合比较面板
   - 随机化功能面板

3. 各功能说明：
   - 使用滑块调整参数（数据量、批次大小、页码等）
   - 点击按钮执行对应操作
   - 查看结果文本区域的输出信息

## 性能注意事项

- 批处理功能适用于处理大量数据，避免一次性处理导致的UI卡顿
- 链式操作可以减少中间集合的创建，提高性能
- 对于大型集合，`FindDifferences`操作可能较为耗时
- 使用`Shuffle`时，注意原集合会被修改，如需保留原始集合，请先创建副本

## 示例代码结构

```
CollectionUtilsDemo/
├── CollectionUtilsExample.cs - 主示例脚本
├── CollectionUtilsDemo.unity - 示例场景
└── README.md - 本说明文档
```

## 关键代码示例

### 批处理

```csharp
// 将1000个项目分批处理，每批100个
List<int> items = Enumerable.Range(1, 1000).ToList();
CollectionUtils.BatchProcess(items, 100, batch => {
    // 处理当前批次
    foreach (var item in batch) {
        // 处理每个项目
    }
});
```

### 链式操作

```csharp
// 过滤、映射和排序组合使用
var result = CollectionUtils.Filter(users, u => u.Age >= minAge)
    .Then(filtered => CollectionUtils.Map(filtered, u => new { Name = u.Name, Age = u.Age }))
    .Then(mapped => CollectionUtils.Sort(mapped, (a, b) => a.Age.CompareTo(b.Age)));
```

## 相关API文档

有关`CollectionUtils`类的完整API文档，请参阅`TByd.Core.Utils`包的使用手册。 