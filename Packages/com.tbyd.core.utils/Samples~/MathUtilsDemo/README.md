# MathUtils 示例

本示例演示了`MathUtils`类的各种数学计算功能，包括：

## 功能演示

### 角度与弧度转换
- 角度转弧度（DegreesToRadians）
- 弧度转角度（RadiansToDegrees）

### 数值操作
- 值限制在指定范围内（Clamp）
- 值循环包装（Wrap）
- 值从一个范围映射到另一个范围（Remap）

### 插值功能
- 线性插值（Lerp）
- 缓入插值（EaseIn）
- 缓出插值（EaseOut）
- 缓入缓出插值（EaseInOut）
- 弹簧插值（Spring）
- 弹跳插值（Bounce）
- 颜色插值

### 几何计算
- 计算两点间距离（Distance）
- 计算两向量间角度（AngleBetween）
- 旋转点（RotatePoint）

### 2D网格计算
- 从索引计算网格位置（IndexToPosition）
- 从网格位置计算索引（PositionToIndex）

## 使用方法

1. 打开示例场景 `MathUtilsDemo.unity`
2. 场景包含多个UI面板，每个面板对应一个功能组：
   - 角度与弧度转换面板
   - 数值操作面板
   - 插值功能面板
   - 几何计算面板
   - 2D网格计算面板

3. 各功能说明：
   - 在输入框中输入参数值
   - 使用滑块动态调整插值值
   - 使用下拉菜单选择不同的插值方式
   - 点击按钮执行对应操作
   - 查看结果文本区域的输出信息和可视化效果

## 插值效果说明

- **线性插值(Linear)**: 匀速变化，适合简单动画
- **缓入插值(EaseIn)**: 开始慢，结束快，适合物体启动
- **缓出插值(EaseOut)**: 开始快，结束慢，适合物体减速
- **缓入缓出插值(EaseInOut)**: 开始和结束都慢，中间快，适合平滑过渡
- **弹簧插值(Spring)**: 超过目标后弹回，适合弹性效果
- **弹跳插值(Bounce)**: 在结束时反弹，适合物体落地效果

## 示例代码结构

```
MathUtilsDemo/
├── MathUtilsExample.cs - 主示例脚本
├── MathUtilsDemo.unity - 示例场景
└── README.md - 本说明文档
```

## 关键代码示例

### 角度与弧度转换

```csharp
// 角度转弧度
float radians = MathUtils.DegreesToRadians(90f); // 返回 π/2

// 弧度转角度
float degrees = MathUtils.RadiansToDegrees(Mathf.PI); // 返回 180
```

### 值的映射

```csharp
// 将0-10范围的值映射到0-100范围
float result = MathUtils.Remap(5f, 0f, 10f, 0f, 100f); // 返回 50
```

### 几何计算

```csharp
// 计算两点间距离
float distance = MathUtils.Distance(new Vector2(0, 0), new Vector2(3, 4)); // 返回 5

// 旋转点
Vector2 rotated = MathUtils.RotatePoint(
    new Vector2(1, 0),    // 要旋转的点
    Vector2.zero,         // 旋转中心
    MathUtils.DegreesToRadians(90)  // 旋转角度（弧度）
); // 返回接近 (0, 1) 的点
```

## 相关API文档

有关`MathUtils`类的完整API文档，请参阅`TByd.Core.Utils`包的使用手册。 