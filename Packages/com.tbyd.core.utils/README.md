# TByd Core Utils

<div align="center">

![版本](https://img.shields.io/badge/版本-0.5.0--rc.1-blue)
![Unity版本](https://img.shields.io/badge/Unity-2021.3.8f1+-brightgreen)
![许可证](https://img.shields.io/badge/许可证-MIT-green)
![测试覆盖率](https://img.shields.io/badge/测试覆盖率-95%25-success)

*为Unity开发者打造的高性能、易用工具集*

</div>

## 📋 概述

TByd Core Utils 是一个专为Unity开发者设计的实用工具库，提供常用的数学工具、字符串处理、随机功能、反射工具、时间工具、集合工具、IO操作和各种扩展方法，帮助开发者专注于游戏逻辑实现，减少编写重复代码的工作。

<div align="center">
  
| 🧮 数学工具 | 📝 字符串工具 | 🎲 随机工具 | ⏱️ 时间工具 | 🔍 反射工具 | 🔢 集合工具 | 💾 IO工具 | 🎮 扩展方法 |
|:-------------:|:-------------:|:-------------:|:-------------:|:-------------:|:-------------:|:-------------:|:-------------:|
| 平滑曲线插值 | 多语言文本处理 | 权重随机选择 | 时间格式化 | 高性能反射 | 批量数据处理 | 文件系统操作 | 链式变换操作 |
| 范围值重映射 | 智能字符串生成 | 正态分布随机 | 游戏时间系统 | 动态实例化 | 集合分页排序 | 异步文件读写 | 集合批量处理 |
| 矢量与旋转转换 | 高效文本解析 | 随机颜色生成 | 计时与测量 | 元数据访问 | 映射与过滤 | 文件监控 | 安全组件操作 |

</div>

## ✨ 核心特性

<table>
<tr>
<td width="33%">
<h3 align="center">🧮 MathUtils</h3>
<p align="center"></p>

```csharp
// 值范围重映射
float health = 75f;
float fillAmount = MathUtils.Remap(
    health, 0f, 100f, 0f, 1f);

// 平滑阻尼插值
Vector3 velocity = Vector3.zero;
transform.position = MathUtils.SmoothDamp(
    transform.position, 
    targetPosition, 
    ref velocity, 
    0.3f);
    
// 检测点是否在多边形内
bool isInside = MathUtils.IsPointInPolygon(
    playerPosition, polygonVertices);
```
</td>
<td width="33%">
<h3 align="center">📝 StringUtils</h3>
<p align="center"></p>

```csharp
// 生成随机字符串
string sessionId = StringUtils.GenerateRandom(
    32, includeSpecialChars: false);
    
// 转换为URL友好格式
string slug = StringUtils.ToSlug(
    "Hello World 2025!");
// 输出: "hello-world-2025"

// 智能截断长文本
string preview = StringUtils.Truncate(
    longDescription, 100, "...");
```
</td>
<td width="33%">
<h3 align="center">🎲 RandomUtils</h3>
<p align="center"></p>

```csharp
// 根据权重随机选择
string fruit = RandomUtils.WeightedRandom(
    new[] { "苹果", "香蕉", "樱桃" },
    new[] { 1f, 2f, 3f });
    
// 生成正态分布随机值
float iq = RandomUtils.Gaussian(
    mean: 100f, 
    standardDeviation: 15f);
    
// 随机打乱数组
string[] names = GetPlayerNames();
RandomUtils.Shuffle(names);
```
</td>
</tr>
<tr>
<td width="33%">
<h3 align="center">⏱️ TimeUtils</h3>
<p align="center"></p>

```csharp
// 格式化时间
string formatted = TimeUtils.FormatDateTime(
    DateTime.Now, "yyyy-MM-dd HH:mm");
    
// 相对时间描述
string relativeTime = TimeUtils.GetRelativeTimeDescription(
    DateTime.Now.AddDays(-2));
// 输出: "2天前"

// 游戏时间系统
TimeUtils.SetGameTimeScale(2.0f);
TimeUtils.UpdateGameTime();
DateTime gameTime = TimeUtils.GetCurrentGameTime();
```
</td>
<td width="33%">
<h3 align="center">🔍 ReflectionUtils</h3>
<p align="center"></p>

```csharp
// 高性能反射
var getter = ReflectionUtils.CreateGetter<Transform, Vector3>(
    "position");
var setter = ReflectionUtils.CreateSetter<Transform, Vector3>(
    "position");
    
// 动态实例创建
var instance = ReflectionUtils.CreateInstance(typeName);

// 元数据访问
bool hasAttribute = ReflectionUtils.HasAttribute<ObsoleteAttribute>(
    typeof(LegacyClass));
```
</td>
<td width="33%">
<h3 align="center">🎮 扩展方法</h3>
<p align="center"></p>

```csharp
// Transform扩展
transform
    .ResetLocal()
    .SetLocalX(5f)
    .SetLocalZ(3f);
    
// GameObject扩展
GameObject uiPanel = gameObject
    .FindOrCreateChild("UI_Panel")
    .SetLayerRecursively(
        LayerMask.NameToLayer("UI"));
    
// 集合扩展
List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };
numbers.Shuffle();
foreach (var batch in numbers.Batch(2)) {
    // 批量处理
}
```
</td>
</tr>
<tr>
<td width="33%">
<h3 align="center">🔢 CollectionUtils</h3>
<p align="center"></p>

```csharp
// 批量处理大集合
await CollectionUtils.BatchProcessAsync(
    largeList, 100, ProcessItem);
    
// 对集合进行分页
var page = CollectionUtils.Paginate(
    items, pageSize: 10, pageIndex: 2);
    
// 过滤和映射操作
var results = CollectionUtils
    .Filter(items, x => x.IsActive)
    .Map(x => x.Name);
    
// 比较两个集合
var differences = CollectionUtils.FindDifferences(
    oldItems, newItems);
```
</td>
<td width="33%">
<h3 align="center">💾 IOUtils</h3>
<p align="center"></p>

```csharp
// 异步读写文件
string json = await IOUtils.ReadAllTextAsync(
    savePath);
await IOUtils.WriteAllTextAsync(
    savePath, json);
    
// 规范化路径
string path = IOUtils.NormalizePath(
    "Assets/../Resources/Data.json");
    
// 监控文件变化
IOUtils.WatchFile(configPath, OnConfigChanged);

// 计算文件哈希
string hash = IOUtils.ComputeMD5(filePath);
```
</td>
<td width="33%">
<h3 align="center">🎮 扩展方法</h3>
<p align="center"></p>

```csharp
// Transform扩展
transform
    .ResetLocal()
    .SetLocalX(5f)
    .SetLocalZ(3f);
    
// GameObject扩展
GameObject uiPanel = gameObject
    .FindOrCreateChild("UI_Panel")
    .SetLayerRecursively(
        LayerMask.NameToLayer("UI"));
    
// 集合扩展
List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };
numbers.Shuffle();
foreach (var batch in numbers.Batch(2)) {
    // 批量处理
}
```
</td>
</tr>
</table>

## 📢 重要通知：API已冻结

**从0.5.0-rc.1版本开始，所有公共API已冻结，不会再有重大变更。**

- 后续1.0.0正式版本将保持与此版本API完全兼容
- 标记为`[Obsolete]`的API将在1.0.0版本中移除，请查看API文档中的替代方案
- 新功能将以不破坏现有API的方式添加
- 此版本已完成全面性能优化和测试，可放心用于生产环境

## 🚀 快速开始

### 安装

通过 Scoped Registry 安装：

1. 打开 **Edit > Project Settings > Package Manager**
2. 在 **Scoped Registries** 部分点击 **+** 按钮
3. 填写信息:
   - **Name**: `npmjs`
   - **URL**: `https://upm.tianyuyuyu.com`
   - **Scope(s)**: `com.tbyd`
4. 点击 **Apply** 保存设置
5. 打开 **Window > Package Manager**
6. 在左上角下拉菜单选择 **My Registries**
7. 找到并安装 **TByd.Core.Utils**

### 基本用法

```csharp
// 添加命名空间引用
using TByd.Core.Utils.Runtime;
using TByd.Core.Utils.Runtime.Extensions;

// 现在可以使用工具类了!
public class MyScript : MonoBehaviour
{
    void Start()
    {
        // 使用MathUtils
        float smoothValue = MathUtils.SmoothDamp(current, target, ref velocity, smoothTime);
        
        // 使用StringUtils
        string uniqueId = StringUtils.GenerateRandom(8);
        
        // 使用RandomUtils
        Color randomColor = RandomUtils.ColorHSV(0.7f, 1f, 0.7f, 1f);
        
        // 使用扩展方法
        transform.ResetLocal().SetLocalY(1.5f);
        GameObject child = gameObject.FindOrCreateChild("UI_Container");
        List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };
        int randomNumber = numbers.GetRandom();
    }
}
```

## ⚡ 性能对比

TByd Core Utils 专注于高性能实现，显著提升开发效率的同时保持卓越的运行时性能。

<table>
<tr>
<th>操作</th>
<th>标准Unity实现</th>
<th>TByd实现</th>
<th>性能提升</th>
</tr>
<tr>
<td>查找深层级子对象</td>
<td>~1.2ms</td>
<td>~0.3ms</td>
<td>🔥 4倍</td>
</tr>
<tr>
<td>批量字符串操作</td>
<td>~2.8ms</td>
<td>~0.9ms</td>
<td>🔥 3倍</td>
</tr>
<tr>
<td>多边形碰撞检测</td>
<td>~0.5ms</td>
<td>~0.15ms</td>
<td>🔥 3.3倍</td>
</tr>
<tr>
<td>集合随机打乱</td>
<td>~0.7ms</td>
<td>~0.2ms</td>
<td>🔥 3.5倍</td>
</tr>
<tr>
<td>大量数据分批处理</td>
<td>~2.1ms</td>
<td>~0.6ms</td>
<td>🔥 3.5倍</td>
</tr>
<tr>
<td>异步文件操作</td>
<td>线程阻塞</td>
<td>非阻塞</td>
<td>🔥 主线程零阻塞</td>
</tr>
</table>

## 📚 文档

详细文档可在安装包中找到:

- [**使用入门**](Documentation~/使用入门.md) - 快速入门指南
- [**使用手册**](Documentation~/使用手册.md) - 详细用法和示例
- [**API文档**](Documentation~/API文档.md) - 完整API参考
- [**示例场景说明**](Documentation~/示例场景说明.md) - 示例场景解释

## 🧪 示例

包含多个示例场景，展示核心功能的使用方法:

- **CoreUtilsShowcase** - 综合功能演示
- **MathUtilsDemo** - 数学工具演示场景
- **RandomUtilsDemo** - 随机功能演示场景
- **TimeUtilsDemo** - 时间工具演示场景
- **ReflectionUtilsDemo** - 反射工具演示场景
- **TransformExtensionsDemo** - Transform扩展方法演示场景
- **CollectionUtilsDemo** - 集合工具演示场景
- **IOUtilsDemo** - IO操作工具演示场景

要访问示例，请通过Package Manager导入。

## 📋 依赖项

- Unity 2021.3.8f1 或更高版本

## 🔄 版本信息

当前版本: **0.5.0-rc.1**

查看 [CHANGELOG.md](CHANGELOG.md) 获取详细更新记录。

## 📄 许可证

本项目基于 [MIT许可证](LICENSE.md) 发布。

## 🤝 贡献

欢迎贡献代码和提出建议！请查看 [开发者指南](Documentation~/开发者指南.md) 了解如何参与项目开发。

## 📞 支持

如有问题或建议，请通过以下方式联系我们:

- 提交 [GitHub Issue](https://github.com/tbyd/tbyd.core.utils/issues)
- 发送邮件至 support@tbyd.com

---

<div align="center">
  <sub>由TByd团队用 ❤️ 制作</sub>
  <br>
  <sub>Copyright © 2025 TByd团队</sub>
</div> 