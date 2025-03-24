# TByd UPM包测试框架

本文档介绍 TByd UPM包的测试框架，包括测试类型、目录结构、工具类和使用方法。

## 测试目录结构

```
Tests/
├── Editor/                    # 编辑器模式测试
│   ├── Unit/                  # 单元测试
│   ├── Integration/           # 集成测试
│   ├── Performance/           # 编辑器性能测试
│   └── Framework/             # 测试框架代码
├── Runtime/                   # 运行时测试
│   ├── Unit/                  # 运行时单元测试
│   ├── Integration/           # 运行时集成测试
│   └── Performance/           # 运行时性能测试
├── TestResources/             # 测试资源
└── README.md                  # 测试文档
```

## 测试框架核心类

### 1. TestBase

所有测试类的基类，提供通用的测试设置和清理逻辑。

```csharp
public class MyTests : TestBase
{
    [Test]
    public void MyTest()
    {
        // 使用基类方法创建测试对象，测试结束后会自动清理
        GameObject go = CreateGameObject("TestObject");
        
        // 执行测试...
    }
}
```

### 2. TestUtils

提供通用的测试辅助功能：

- 生成随机数据
- 反射操作
- 近似值比较
- GameObject操作

```csharp
// 生成随机字符串
string random = TestUtils.GenerateRandomString(10);

// 获取私有字段值
int value = TestUtils.GetPrivateField<int>(obj, "privateField");

// 近似比较向量
TestUtils.AssertApproximatelyEqual(expectedVector, actualVector, 0.001f);
```

### 3. GCAllocationTester

测量方法的内存分配情况：

```csharp
// 测量分配的内存
long allocated = GCAllocationTester.MeasureAllocation(() => {
    // 被测方法
    StringUtils.GenerateRandom(100);
});

// 断言无GC分配
GCAllocationTester.AssertNoAllocation(() => {
    // 被测方法应该不产生GC分配
    vector.Normalize();
});

// 比较两个实现的内存分配
GCAllocationTester.CompareAllocations(
    () => { /* 基准实现 */ },
    () => { /* 优化实现 */ },
    0.5f  // 期望优化版本至少减少50%分配
);
```

### 4. TestDataGenerator

生成各种类型的测试数据：

```csharp
// 生成随机字符串
string text = TestDataGenerator.GenerateString(100);

// 生成随机中文字符串
string chinese = TestDataGenerator.GenerateChineseString(50);

// 生成随机Vector3数组
Vector3[] vectors = TestDataGenerator.GenerateArray(10, i => 
    TestDataGenerator.GenerateVector3());

// 生成大型文本块
string largeText = TestDataGenerator.GenerateLargeText(
    paragraphCount: 5,
    sentencesPerParagraph: 8,
    averageWordsPerSentence: 15
);
```

### 5. PerformanceTestBase

性能测试基类，提供性能测试基础设施：

```csharp
public class MyPerformanceTests : PerformanceTestBase
{
    [Test, Performance]
    public void CompareImplementations()
    {
        // 比较两个实现的性能
        ComparePerformance(
            // 基准实现
            () => { /* 标准方法 */ },
            
            // 优化实现
            () => { /* 优化方法 */ },
            
            "方法名比较",
            measureGC: true
        );
    }
    
    [Test, Performance]
    public void MeasureMethod()
    {
        // 测量单个方法的性能
        MeasurePerformance(
            () => {
                // 被测方法
                for (int i = 0; i < 100; i++)
                {
                    MyMethod();
                }
            },
            "MyMethod性能测试"
        );
    }
}
```

## 测试规范

### 1. 单元测试命名规范

使用`方法名_测试场景_预期结果`模式命名测试方法：

```csharp
[Test]
public void IsNullOrWhiteSpace_WithNullString_ReturnsTrue()
{
    bool result = StringUtils.IsNullOrWhiteSpace(null);
    Assert.IsTrue(result);
}
```

### 2. 测试结构规范

使用AAA(安排-执行-断言)模式组织测试：

```csharp
[Test]
public void MethodName_TestCase_ExpectedResult()
{
    // 安排(Arrange) - 准备测试数据和环境
    string input = "Test";
    
    // 执行(Act) - 调用被测方法
    string result = StringUtils.ProcessString(input);
    
    // 断言(Assert) - 验证结果
    Assert.AreEqual("PROCESSED: TEST", result);
}
```

### 3. 性能测试规范

性能测试应该：

- 使用`[Category("Performance")]`标记
- 使用`[Test, Performance]`特性
- 包含预热阶段
- 多次测量取平均值
- 与基准实现比较(如适用)
- 测量内存分配(如适用)

## 本地运行测试

### 单元测试

在Unity编辑器中使用Test Runner窗口运行测试：

1. 打开 Window > General > Test Runner
2. 选择 EditMode 或 PlayMode 标签页
3. 点击 Run All 或选择特定测试运行

### 命令行运行测试

```bash
# 运行所有EditMode测试
unity -batchmode -projectPath . -runTests -testPlatform EditMode -logFile test_results.log

# 运行所有PlayMode测试
unity -batchmode -projectPath . -runTests -testPlatform PlayMode -logFile test_results.log

# 运行特定类别的测试
unity -batchmode -projectPath . -runTests -testPlatform EditMode -testCategory "UnitTests" -logFile test_results.log
```

## 自动化测试

项目使用GitHub Actions自动运行测试。每当推送到`main`或`develop`分支，或创建Pull Request时，都会触发测试流水线。

流水线包括：

1. 运行所有单元测试
2. 收集代码覆盖率报告
3. 运行性能测试
4. 构建UPM包(针对`main`和`develop`分支的推送)

测试结果和构建的包可以在GitHub Actions的Artifacts中找到。

## 编写新测试

1. 在适当的目录创建测试类（继承自TestBase或PerformanceTestBase）
2. 添加测试方法，使用[Test]特性标记
3. 使用测试工具类简化测试编写
4. 遵循命名和结构规范
5. 本地运行测试验证
6. 提交代码触发自动化测试

## 覆盖率目标

- 单元测试覆盖率：>=95%
- 性能测试覆盖率：所有公共API的关键性能路径
- 集成测试覆盖率：所有主要功能场景

## 常见问题

**Q: 如何调试测试?**
A: 在Test Runner窗口中选择测试，点击"Run Selected(Debugging)"选项运行。

**Q: 如何跳过某些测试?**
A: 使用`[Ignore("原因")]`特性标记测试方法。

**Q: 如何创建参数化测试?**
A: 使用`[TestCase]`或`[ValueSource]`特性提供不同的测试数据。

**Q: 如何测试异步方法?**
A: 使用`[UnityTest]`特性和`IEnumerator`返回类型创建异步测试。 