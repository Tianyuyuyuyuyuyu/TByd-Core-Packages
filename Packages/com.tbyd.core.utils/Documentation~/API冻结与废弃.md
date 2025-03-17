# API冻结与废弃策略

## API冻结声明

从`0.5.0-rc.1`版本开始，TByd.Core.Utils包的所有公共API已经冻结。这意味着：

1. **不会删除现有的公共API**：所有现有的公共方法、属性和类型将在未来版本中保持可用
2. **不会更改现有API的签名**：方法参数、返回类型和行为不会发生破坏性变更
3. **新功能添加方式**：如果需要添加新功能，将通过新方法、属性或类实现，不会修改现有API

此API冻结承诺确保开发者可以安全地依赖当前版本的API，升级到未来版本时不会遇到重大兼容性问题。

## 废弃API清单

以下API已被标记为废弃(`[Obsolete]`)，将在`1.0.0`正式版中移除。如果您的代码使用了这些API，请迁移到推荐的替代方案。

### StringUtils

| 废弃API | 替代方案 | 废弃原因 |
|---------|----------|----------|
| `Split(string value, params char[] separators)` | `Split(string value, char separator)` | 性能优化，新API零GC分配 |
| `ToBase64(string input)` | `EncodeToBase64(string input)` | 方法命名标准化 |
| `FromBase64(string base64)` | `DecodeFromBase64(string base64)` | 方法命名标准化 |

### MathUtils

| 废弃API | 替代方案 | 废弃原因 |
|---------|----------|----------|
| `AngleBetween(Vector3 from, Vector3 to)` | `GetAngleBetween(Vector3 from, Vector3 to)` | 方法命名标准化 |
| `ClampAngle(float angle)` | `ClampAngleTo360(float angle)` | 方法名称不够明确 |

### RandomUtils

| 废弃API | 替代方案 | 废弃原因 |
|---------|----------|----------|
| `GetRandom(int min, int max)` | `Range(int min, int max)` | 与Unity API风格统一 |
| `GetRandom(float min, float max)` | `Range(float min, float max)` | 与Unity API风格统一 |

### TransformExtensions

| 废弃API | 替代方案 | 废弃原因 |
|---------|----------|----------|
| `FindInChildren(this Transform transform, string name)` | `FindChild(this Transform transform, string name)` | 方法命名更简洁 |
| `Reset(this Transform transform)` | `ResetLocal(this Transform transform)` | 明确方法作用 |

## 废弃API使用指南

1. **编译警告**：使用被废弃的API时，编译器会生成警告信息
2. **警告内容**：警告消息中会提供推荐的替代API信息
3. **过渡期**：0.5.0-rc.1 到 1.0.0 版本之间为过渡期，废弃API依然可用
4. **移除时间**：所有废弃API将在1.0.0版本中移除

## 迁移建议

1. 立即开始迁移到替代API
2. 使用IDE的全局搜索找到所有使用废弃API的地方
3. 参考使用手册中的示例代码了解新API的使用方法
4. 如有任何问题，请在[GitHub Issues](https://github.com/Tianyuyuyuyuyuyu/TByd/issues)提问 