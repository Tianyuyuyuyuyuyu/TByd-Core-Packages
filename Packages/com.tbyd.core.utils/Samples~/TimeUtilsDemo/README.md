# TimeUtils 示例

本示例演示了`TimeUtils`类的各种时间相关功能，包括：

## 功能演示

### 时间格式化
- 格式化日期时间（FormatDateTime）
- 支持多种格式模板
- 自定义格式化

### 相对时间
- 获取相对时间描述（GetRelativeTimeDescription）
- 以人类可读方式显示时间差异（如"3天前"、"1小时后"等）

### 游戏时间系统
- 游戏时间初始化与更新（InitializeGameTime/UpdateGameTime）
- 获取当前游戏时间（GetCurrentGameTime）
- 设置游戏时间缩放（SetGameTimeScale）
- 暂停/恢复游戏时间（SetGameTimePaused）

### 计时器功能
- 运行计时器
- 暂停/停止计时器
- 重置计时器
- 格式化时间间隔（FormatTimeSpan）

### 性能测量
- 测量代码执行时间（MeasureExecutionTime）
- 计算平均执行时间

## 使用方法

1. 打开示例场景 `TimeUtilsDemo.unity`
2. 场景包含多个UI面板，每个面板对应一个功能组：
   - 时间格式化面板
   - 相对时间面板
   - 游戏时间系统面板
   - 计时器面板
   - 性能测量面板

3. 各功能说明：
   - 使用下拉菜单选择不同的时间格式
   - 使用滑块调整时间偏移、时间缩放等参数
   - 点击按钮执行暂停/恢复、启动/停止等操作
   - 查看结果文本区域的输出信息

## 应用场景

- 游戏中的倒计时、计时器功能
- 游戏内时间系统（可加速、减速或暂停）
- UI中显示相对时间（如"3分钟前发布"）
- 性能分析和优化
- 游戏存档中的时间戳处理

## 示例代码结构

```
TimeUtilsDemo/
├── TimeUtilsExample.cs - 主示例脚本
├── TimeUtilsDemo.unity - 示例场景
└── README.md - 本说明文档
```

## 关键代码示例

### 时间格式化

```csharp
// 使用自定义格式格式化当前时间
string formattedTime = TimeUtils.FormatDateTime(DateTime.Now, "yyyy-MM-dd HH:mm:ss");
```

### 相对时间描述

```csharp
// 获取相对于当前时间的描述
string relativeTime = TimeUtils.GetRelativeTimeDescription(DateTime.Now.AddHours(-2));
// 输出: "2小时前"
```

### 游戏时间系统

```csharp
// 初始化游戏时间系统
TimeUtils.InitializeGameTime();

// 设置游戏时间流速（2倍速）
TimeUtils.SetGameTimeScale(2.0f);

// 更新游戏时间（在Update中调用）
TimeUtils.UpdateGameTime();

// 获取当前游戏时间
DateTime gameTime = TimeUtils.GetCurrentGameTime();
```

### 性能测量

```csharp
// 测量某段代码的执行时间
float elapsedMs = TimeUtils.MeasureExecutionTime(() => {
    // 要测量的代码
    for (int i = 0; i < 1000; i++) {
        // 执行某些操作
    }
});

Debug.Log($"执行耗时: {elapsedMs} 毫秒");
```

## 相关API文档

有关`TimeUtils`类的完整API文档，请参阅`TByd.Core.Utils`包的使用手册。 