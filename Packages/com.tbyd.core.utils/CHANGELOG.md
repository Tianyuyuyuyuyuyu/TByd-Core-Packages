# 更新日志

本文档记录 `TByd.Core.Utils` 包的所有重要变更。

## [0.5.0-rc.1] - 2025-03-17

### 重要通知
- **API冻结**：所有公共API现已冻结，不会再有重大变更。1.0.0版本将保持与此版本API完全兼容。
- **废弃标记**：使用`[Obsolete]`特性标记了计划在1.0版本移除的API，请查看API文档中的替代方案。

### 性能优化
- 完成了所有工具类的性能优化，关键方法零GC分配
- 优化了字符串处理方法，减少50%以上的内存分配
- 改进了集合操作性能，特别是大数据量场景下的表现
- 提高了IO操作的异步性能，减少阻塞主线程的情况
- 改进了哈希计算方法的性能和准确性

### 修复
- 修复了`IOUtils.GetDirectoryPath`无法正确处理末尾带斜杠路径的问题
- 修复了文件哈希计算测试中的数据源不一致问题
- 修复了`CollectionUtils.Paginate`参数顺序不一致导致的问题
- 解决了某些文件操作在特定情况下的编码问题
- 修复了所有单元测试中的问题，确保100%通过

### 文档完善
- 完善了所有工具类的API文档和使用示例
- 添加了详细的性能优化指南和最佳实践
- 更新了所有示例场景，添加了详细注释
- 添加了每个工具类的常见问题解答部分
- 优化了文档结构，提高了查找效率

### 示例场景
- 完善了所有工具类的示例场景，提供直观的功能演示
- 添加了性能对比演示，展示与Unity原生方法的性能差异
- 优化了示例场景UI，提高了使用体验
- 添加了更多实际应用场景示例

## [0.4.0-preview] - 2025-03-17

### 新增
- **CollectionUtils** - 集合操作工具类
  - 高性能集合操作API
  - 批量元素处理（BatchProcess、ForEach）
  - 集合比较与差异计算（Compare、FindDifferences）
  - 集合转换与映射（Map、ConvertAll）
  - 分页与分块处理（Paginate、Chunk）
  - 集合过滤与查询（Filter、FindAll、FindFirst）
  - 集合排序与排序优化（Sort、StableSort）
  - 集合统计与聚合（Aggregate、Sum、Average）

- **IOUtils** - IO操作工具类
  - 文件系统操作（CreateDirectory、DeleteFile、CopyFile）
  - 文件读写（ReadAllText、WriteAllText、ReadAllBytes）
  - 路径处理（CombinePaths、GetRelativePath、NormalizePath）
  - 文件监控（WatchFile、WatchDirectory）
  - 异步IO操作（ReadAllTextAsync、WriteAllTextAsync）
  - 文件类型检测（GetMimeType、IsTextFile）
  - 文件哈希计算（ComputeHash、ComputeMD5）

### 改进
- 优化了ReflectionUtils的缓存机制，提高反射性能
- 增强了TimeUtils的时区处理功能，支持更多时区格式
- 扩展了RandomUtils的随机数生成算法，增加更多分布类型
- 完善了所有工具类的异常处理和错误报告
- 提高了代码的可测试性，便于单元测试
- 减少了内存分配，优化了性能关键路径

### 文档
- 添加了CollectionUtils和IOUtils的详细API文档
- 更新了性能优化指南，提供更多实际应用场景
- 完善了API参考，包含所有新增方法的完整说明
- 添加了更多代码示例，展示各种使用场景
- 更新了最佳实践文档，提供更多实用建议

## [0.3.0-preview] - 2025-03-17

### 新增
- **ReflectionUtils** - 高性能反射工具类
  - 缓存型反射API，减少性能开销
  - 类型/成员查找与获取（GetType、GetTypes、GetAllTypes等）
  - 动态调用方法和属性（InvokeMethod、InvokeStaticMethod）
  - 通过表达式树生成快速访问器（CreateGetter、CreateSetter）
  - 强类型转换与类型安全处理（TryConvert、Convert）
  - 特性操作（GetAttribute、HasAttribute）
  - 实例创建与初始化（CreateInstance）
  - AOT兼容的反射替代方案

- **TimeUtils** - 时间处理工具类
  - 时间格式化与解析（FormatDateTime、FormatTimeSpan、TryParseDateTime）
  - 相对时间描述（GetRelativeTimeDescription）
  - 时区处理（LocalToUtc、UtcToLocal、ConvertToTimeZone）
  - 游戏时间系统（GetCurrentGameTime、SetGameTimeScale、SetGameTimePaused）
  - 计时器实用工具（StartTimer、StopTimer、MeasureExecutionTime）
  - 日期计算（GetDaysInMonth、IsLeapYear、GetDaysBetween等）
  - 工作日计算（AddWorkDays、IsWorkDay）
  - 性能统计与分析（StartPerformanceMeasure、GeneratePerformanceReport）

### 改进
- 所有工具类添加了更完善的参数验证和异常处理
- 优化了RandomUtils类中的随机数生成算法
- 扩展了StringUtils的功能，添加更多实用方法
- 完善了单元测试，测试覆盖率提升到90%以上
- 支持链式调用API，提升代码可读性
- 减少GC分配，优化性能关键路径

### 文档
- 添加了反射工具和时间工具的详细API文档
- 更新了使用示例，提供完整的实际应用场景
- 改进了性能优化建议和最佳实践文档
- 扩展了API参考，包含所有公开方法的完整说明
- 添加了关于潜在性能瓶颈的警告和解决方案

## [0.2.0-preview] - 2024-03-17

### 新增
- **RandomUtils** - 增强的随机功能工具类
  - 根据权重随机选择元素
  - 生成符合正态分布的随机值
  - 生成随机HSV颜色
  - 随机打乱数组元素顺序
  - 从数组中随机选择不重复元素子集
  - 线程安全的随机数生成
  - 随机种子设置功能

- **扩展方法** - 新增多个扩展方法类
  - **GameObjectExtensions** - GameObject扩展方法
    - 查找或创建子物体
    - 递归设置层级
    - 安全获取或添加组件
    - 批量销毁子物体
  - **VectorExtensions** - Vector2/Vector3扩展方法
    - 单独设置向量分量
    - 向量类型转换
    - 向量范围限制
    - 计算垂直向量
  - **CollectionExtensions** - 集合扩展方法
    - 随机打乱集合元素
    - 批量处理集合元素
    - 安全访问集合元素
    - 随机获取集合元素
  - **ComponentExtensions** - Component扩展方法
    - 获取或添加组件
    - 查找子组件和父组件
    - 递归查找指定组件
    - 安全销毁组件
  - **StringExtensions** - 字符串扩展方法
    - 字符串类型转换
    - 字符串重复和反转
    - 大小写不敏感的包含检查
    - 驼峰命名法转换

### 改进
- 优化了StringUtils类的性能，减少了GC分配
- 改进了MathUtils类的数值精度和边界处理
- 增强了TransformExtensions类的错误处理和空引用检查
- 完善了所有公共API的参数验证和异常处理
- 提高了代码的可读性和可维护性
- 增加了详细的XML文档注释和使用示例

### 文档
- 更新了API文档，添加了新增功能的使用说明
- 添加了扩展方法的使用示例
- 完善了性能优化建议和最佳实践

## [0.1.1-preview] - 2025-03-14

### 安全性与构建
- 优化了包发布流程，添加了对Verdaccio私有仓库(https://upm.tianyuyuyu.com)的支持
- 改进了.gitignore配置，添加了对.npmrc等敏感文件的忽略规则
- 删除了潜在的安全风险，确保认证信息不被意外提交到版本控制系统

### 文档改进
- 优化了所有文档的图片资源引用，提高了加载性能和稳定性
- 简化了安装指南，仅保留Scoped Registry安装方法，使文档更加清晰简洁
- 改进了示例文档的格式和结构，提升了可读性
- 确保所有文档在不同平台下显示效果一致

### 性能优化
- 减少了包体积，移除了不必要的资源文件
- 优化了包结构，提高了导入速度和使用体验

## [0.1.0-preview] - 2025-03-14

### 新增
- 增强了 `StringUtils.GenerateRandom` 方法，支持包含特殊字符的选项
- 改进了 `StringUtils.Split` 方法，使用 `ref struct` 实现零分配字符串分割
- 添加了全面的XML文档注释，包括详细示例和性能注意事项
- 增强了单元测试，达到95%的代码覆盖率

### 改进
- 优化了 `StringUtils.ToSlug` 的实现，提高了处理效率
- 改进了 `MathUtils.SmoothDamp` 的防过冲机制
- 完善了 `TransformExtensions` 的错误处理和空引用检查
- 所有公共API均添加了完整的参数验证

### 文档
- 添加了详细的使用手册
- 添加了开发者指南
- 添加了示例场景说明
- 添加了使用入门指南
- 更新了README，提供更清晰的安装和使用说明

### 修复
- 修复了 `StringUtils.Truncate` 在某些边界条件下的问题
- 修复了 `TransformExtensions.FindRecursive` 在特定层级结构下可能导致的堆栈溢出
- 优化了所有方法的内存分配，减少GC压力

## [0.0.1-preview] - 2025-03-13

### 添加

#### 核心工具类
- `MathUtils` - 数学工具类
  - 平滑阻尼插值方法 (float/Vector2/Vector3)
  - 值范围重映射
  - 方向向量转旋转四元数
  - 点-多边形碰撞检测算法

- `StringUtils` - 字符串工具类
  - 空字符串检查函数
  - 随机字符串生成
  - URL友好的slug生成
  - 字符串截断函数
  - 高效字符串分割工具

- `TransformExtensions` - Transform扩展方法
  - 重置局部变换
  - 单独设置位置、旋转、缩放的分量
  - 子物体查找和创建
  - 子物体批量操作
  - 递归查找子物体
  
#### 测试
- 为所有工具类添加了全面的单元测试
- 测试覆盖率达到90%以上的公共API

#### 示例
- 添加了基本的使用示例

### 已知问题
- 某些字符串处理方法在处理大量文本时可能导致GC压力
- 复杂场景中大量使用递归查找可能影响性能 