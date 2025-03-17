# TByd.Core.Utils工具包综合展示

## 概述

本场景是TByd.Core.Utils工具包的综合展示演示，提供对包中所有核心工具类的功能概览和性能对比。通过交互式界面，您可以快速了解各工具类的主要功能、使用方法和性能优势。

TByd.Core.Utils包含以下核心工具类：

- **MathUtils**: 数学计算工具类，提供插值、坐标转换、数值映射等功能
- **StringUtils**: 字符串处理工具类，提供高性能字符串分割、格式化等功能
- **TimeUtils**: 时间处理工具类，提供时间格式化、相对时间描述等功能
- **RandomUtils**: 随机工具类，提供增强的随机数生成和随机选择功能
- **ReflectionUtils**: 反射工具类，简化类型查找、属性操作等功能
- **CollectionUtils**: 集合处理工具类，提供批处理、过滤、映射等功能
- **IOUtils**: 文件和目录操作工具类，提供文件读写、监控等功能
- **TransformExtensions**: Transform扩展工具类，简化Transform相关操作

## 功能展示内容

本展示包含以下内容：

1. **工具类选择器**: 通过下拉菜单选择要查看的工具类
2. **功能展示面板**: 展示选中工具类的主要功能和演示效果
3. **代码示例显示**: 提供常用功能的代码片段，方便直接复制使用
4. **性能对比**: 提供与原生方法的性能对比测试，展示优化效果

## 如何使用

1. 在Unity编辑器中打开CoreUtilsShowcase场景
2. 通过顶部的下拉菜单选择要查看的工具类
3. 在功能展示面板中交互查看各个功能的效果
4. 查看代码示例了解如何在自己的项目中使用这些功能
5. 点击"性能对比"按钮查看与原生方法的性能测试结果
6. 点击"参考资料"按钮查看相关文档和资源链接

## 工具类说明

### MathUtils

数学计算工具类，提供一系列优化的数学操作函数，包括:
- 值映射(Remap)
- 平滑阻尼插值(SmoothDamp)
- 缓动函数(Easing Functions)
- 向量操作扩展

更详细的信息请查看[MathUtilsDemo](../MathUtilsDemo/)演示场景。

### StringUtils

字符串处理工具类，提供高性能、低GC的字符串操作，包括:
- 高性能字符串分割(Split)
- 字符串格式化(Format)
- 字符串截断(Truncate)
- 字符串构建器池(StringBuilderPool)

更详细的信息请查看[StringUtilsDemo](../StringUtilsDemo/)演示场景。

### TimeUtils

时间处理工具类，提供时间相关的实用功能，包括:
- 时间格式化(FormatDateTime)
- 相对时间描述(GetRelativeTimeDescription)
- 游戏时间系统(GameTime)
- 性能测量(MeasureExecutionTime)

更详细的信息请查看[TimeUtilsDemo](../TimeUtilsDemo/)演示场景。

### RandomUtils

随机工具类，提供增强的随机功能，包括:
- 范围随机数生成(Range)
- 随机颜色生成(ColorHSV)
- 带权重的随机选择(WeightedRandom)
- 随机元素选择(RandomElement)

更详细的信息请查看[RandomUtilsDemo](../RandomUtilsDemo/)演示场景。

### ReflectionUtils

反射工具类，简化反射操作，包括:
- 类型查找(GetType)
- 属性获取和设置(GetPropertyValue/SetPropertyValue)
- 方法调用(InvokeMethod)
- 缓存机制优化反射性能

更详细的信息请查看[ReflectionUtilsDemo](../ReflectionUtilsDemo/)演示场景。

### CollectionUtils

集合处理工具类，提供高效的集合操作，包括:
- 批量处理(BatchProcess)
- 集合过滤(Filter)
- 集合映射(Map)
- 集合分页(Paginate)
- 集合比较和差异查找(Compare/FindDifferences)

更详细的信息请查看[CollectionUtilsDemo](../CollectionUtilsDemo/)演示场景。

### IOUtils

文件和目录操作工具类，提供文件系统操作功能，包括:
- 文件读写(ReadAllText/WriteAllText)
- 文件监控(StartWatching/StopWatching)
- 异步文件操作(ReadAllTextAsync/WriteAllTextAsync)
- 文件哈希计算(CalculateFileHash)

更详细的信息请查看[IOUtilsDemo](../IOUtilsDemo/)演示场景。

### TransformExtensions

Transform扩展方法集，简化常见的Transform操作，包括:
- 重置变换(ResetLocal)
- 查找或创建子物体(FindOrCreateChild)
- 获取所有子物体(GetAllChildren)
- 销毁所有子物体(DestroyAllChildren)

更详细的信息请查看[TransformExtensionsDemo](../TransformExtensionsDemo/)演示场景。

## 示例代码文件

本目录包含以下文件:

- `CoreUtilsShowcaseExample.cs`: 主要示例脚本，包含所有工具类的展示逻辑
- `README.md`: 本文档，提供综合展示的说明

## 性能对比

本展示包含部分工具类与Unity/C#原生方法的性能对比，主要包括:

- **MathUtils** vs 原生数学计算: 
  - `MathUtils.Remap` vs 手动映射计算
  - `MathUtils.EaseInOut` vs `Mathf.Lerp`

- **StringUtils** vs 原生字符串操作:
  - `StringUtils.Split` vs `String.Split`
  - 对比GC分配和执行时间

- **CollectionUtils** vs 原生集合操作:
  - `CollectionUtils.BatchProcess` vs 普通for循环
  - `CollectionUtils.Filter` vs LINQ `Where`

运行性能测试可以看到TByd.Core.Utils工具类在某些场景下的性能优势，特别是在GC分配减少方面。

## 相关API文档

更完整的API文档请参阅TByd.Core.Utils包中的Documentation目录下的使用手册。 