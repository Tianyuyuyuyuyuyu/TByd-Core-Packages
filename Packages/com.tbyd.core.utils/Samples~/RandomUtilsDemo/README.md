# RandomUtils 示例

本示例演示了`RandomUtils`类的各种随机数生成和随机化功能，包括：

## 功能演示

### 随机整数
- 生成指定范围内的随机整数（Range）
- 控制最小值和最大值

### 随机浮点数
- 生成指定范围内的随机浮点数（Range）
- 精确控制浮点数范围

### 随机颜色
- 生成随机RGB颜色
- 生成随机HSV颜色（ColorHSV）
- 控制色相、饱和度、亮度范围

### 带权重随机
- 基于权重的随机选择（WeightedRandom）
- 直观展示不同权重的影响

### 随机元素
- 从集合中随机选择元素（GetRandom）
- 随机打乱集合顺序（Shuffle）

## 使用方法

1. 打开示例场景 `RandomUtilsDemo.unity`
2. 场景包含多个UI面板，每个面板对应一个功能组：
   - 随机整数面板
   - 随机浮点数面板
   - 随机颜色面板
   - 带权重随机面板
   - 随机元素面板

3. 各功能说明：
   - 使用滑块调整生成范围和权重值
   - 切换HSV/RGB颜色生成模式
   - 点击按钮执行随机生成
   - 可视化展示随机结果

## 应用场景

- 游戏中的随机掉落系统
- 程序化生成内容
- 随机关卡设计
- AI行为的随机化
- UI元素的随机颜色和排列
- 卡牌游戏的洗牌功能

## 示例代码结构

```
RandomUtilsDemo/
├── RandomUtilsExample.cs - 主示例脚本
├── RandomUtilsDemo.unity - 示例场景
└── README.md - 本说明文档
```

## 关键代码示例

### 随机整数和浮点数

```csharp
// 生成1到10之间的随机整数
int randomInt = RandomUtils.Range(1, 11); // 注意：最大值是排除的

// 生成0.0到1.0之间的随机浮点数
float randomFloat = RandomUtils.Range(0f, 1f);
```

### 随机颜色

```csharp
// 生成完全随机的RGB颜色
Color randomColor = new Color(
    RandomUtils.Range(0f, 1f),
    RandomUtils.Range(0f, 1f),
    RandomUtils.Range(0f, 1f)
);

// 使用HSV生成随机颜色（控制色相、饱和度和亮度范围）
Color randomHSVColor = RandomUtils.ColorHSV(
    0f, 1f,       // 色相范围（0-1）
    0.7f, 1f,     // 饱和度范围（0.7-1）
    0.7f, 1f      // 亮度范围（0.7-1）
);
```

### 带权重随机

```csharp
// 定义项目和对应的权重
string[] items = { "普通", "稀有", "史诗", "传说" };
float[] weights = { 70f, 20f, 8f, 2f };  // 权重分别为70%, 20%, 8%, 2%

// 使用权重随机选择一个项目
string randomItem = RandomUtils.WeightedRandom(items, weights);
```

### 随机元素和洗牌

```csharp
// 从列表中随机选择一个元素
List<string> options = new List<string> { "选项A", "选项B", "选项C" };
string randomOption = RandomUtils.GetRandom(options);

// 随机打乱一个列表
List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };
RandomUtils.Shuffle(numbers);
// numbers顺序会被随机打乱
```

## 相关API文档

有关`RandomUtils`类的完整API文档，请参阅`TByd.Core.Utils`包的使用手册。 