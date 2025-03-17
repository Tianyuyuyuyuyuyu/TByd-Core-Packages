# IOUtils 示例

本示例演示了`IOUtils`类的各种文件和目录操作功能，包括：

## 功能演示

### 路径处理
- 路径规范化（NormalizePath）
- 获取相对路径（GetRelativePath）
- 获取文件扩展名（GetExtension）
- 获取文件名（GetFileName/GetFileNameWithoutExtension）

### 文件操作
- 读取文件内容（ReadAllText）
- 写入文件内容（WriteAllText）
- 追加文件内容（AppendAllText）
- 复制文件（CopyFile）
- 移动文件（MoveFile）
- 删除文件（DeleteFile）

### 异步文件操作
- 异步读取文件（ReadAllTextAsync）
- 异步写入文件（WriteAllTextAsync）
- 计算文件哈希值（CalculateMD5Async）
- 大文件性能测试

### 文件监控
- 监听文件/目录变化（StartWatching）
- 停止监控（StopWatching）
- 文件变化事件处理

## 使用方法

1. 打开示例场景 `IOUtilsDemo.unity`
2. 场景包含多个UI面板，每个面板对应一个功能组：
   - 路径处理面板
   - 文件操作面板
   - 异步文件操作面板
   - 文件监控面板

3. 各功能说明：
   - 在输入框中输入相应参数
   - 点击按钮执行对应操作
   - 查看结果文本区域的输出信息

## 注意事项

- 示例使用Unity的`Application.persistentDataPath`作为测试目录，所有文件操作将在此目录下进行
- 文件监控需要在使用完毕后手动停止，避免不必要的资源占用
- 对于大文件操作，示例展示了异步API的使用，避免阻塞主线程
- 示例包含一个简单的`UnityMainThreadDispatcher`，用于从后台线程安全地更新UI

## 示例代码结构

```
IOUtilsDemo/
├── IOUtilsExample.cs - 主示例脚本
├── IOUtilsDemo.unity - 示例场景
└── README.md - 本说明文档
```

## 相关API文档

有关`IOUtils`类的完整API文档，请参阅`TByd.Core.Utils`包的使用手册。 