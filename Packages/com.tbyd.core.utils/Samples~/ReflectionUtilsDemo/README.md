# ReflectionUtils 示例

本示例演示了`ReflectionUtils`类的各种反射功能，包括：

## 功能演示

### 类型查找
- 根据类型名称获取Type对象（GetType）
- 显示类型的详细信息（基类、接口等）

### 成员查找
- 查找字段（GetField、GetFields）
- 查找属性（GetProperty、GetProperties）
- 查找方法（GetMethod、GetMethods）
- 查找构造函数（GetConstructors）
- 查找所有成员（GetMember、GetMembers）

### 属性操作
- 获取属性值（GetPropertyValue）
- 设置属性值（SetPropertyValue）
- 自动进行类型转换

### 方法调用
- 动态调用方法（InvokeMethod）
- 参数解析与传递

### 特性查找
- 查找类上的特性（GetAttributes）
- 查找成员上的特性
- 筛选特定类型的特性

## 使用方法

1. 打开示例场景 `ReflectionUtilsDemo.unity`
2. 场景包含多个UI面板，每个面板对应一个功能组：
   - 类型查找面板
   - 成员查找面板
   - 属性操作面板
   - 方法调用面板
   - 特性查找面板

3. 各功能说明：
   - 在输入框中输入类型名称、成员名称或参数值
   - 选择成员类型（字段、属性、方法等）
   - 点击按钮执行对应操作
   - 查看结果文本区域的输出信息

## 性能与安全注意事项

- 反射操作相对较慢，不建议在性能关键代码中频繁使用
- 示例使用了绑定标志组合（BindingFlags），可以控制成员的可见性范围
- 在实际项目中，应当谨慎使用反射访问非公开成员，可能破坏封装性
- 使用SetPropertyValue和InvokeMethod时，必须确保有正确的权限

## 示例代码结构

```
ReflectionUtilsDemo/
├── ReflectionUtilsExample.cs - 主示例脚本
├── ReflectionUtilsDemo.unity - 示例场景
└── README.md - 本说明文档
```

## 关键代码示例

### 类型查找

```csharp
// 根据类型名称获取Type对象
Type type = ReflectionUtils.GetType("System.String");
```

### 成员查找

```csharp
// 获取类型的所有公共方法
MethodInfo[] methods = ReflectionUtils.GetMethods(typeof(string), BindingFlags.Public | BindingFlags.Instance);
```

### 属性操作

```csharp
// 获取属性值
object value = ReflectionUtils.GetPropertyValue(obj, "PropertyName");

// 设置属性值
ReflectionUtils.SetPropertyValue(obj, "PropertyName", newValue);
```

### 方法调用

```csharp
// 调用方法
object result = ReflectionUtils.InvokeMethod(obj, "MethodName", parameters);
```

### 特性查找

```csharp
// 获取类型上的特定特性
ObsoleteAttribute[] attributes = ReflectionUtils.GetAttributes<ObsoleteAttribute>(typeof(MyClass));
```

## 相关API文档

有关`ReflectionUtils`类的完整API文档，请参阅`TByd.Core.Utils`包的使用手册。 