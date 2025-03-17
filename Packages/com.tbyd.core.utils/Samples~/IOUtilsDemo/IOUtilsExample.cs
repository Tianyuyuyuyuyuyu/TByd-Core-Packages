using UnityEngine;
using UnityEngine.UI;
using TByd.Core.Utils.Runtime;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

namespace TByd.Core.Utils.Samples
{
    /// <summary>
    /// 展示IOUtils类的使用示例
    /// </summary>
    public class IOUtilsExample : MonoBehaviour
    {
        [Header("路径处理")]
        [SerializeField] private InputField _pathInputField;
        [SerializeField] private Text _pathResultText;
        [SerializeField] private Button _normalizePathButton;
        [SerializeField] private Button _getRelativePathButton;
        [SerializeField] private Button _getExtensionButton;
        [SerializeField] private Button _getFileNameButton;
        
        [Header("文件操作")]
        [SerializeField] private InputField _fileContentInputField;
        [SerializeField] private Text _fileResultText;
        [SerializeField] private Button _readFileButton;
        [SerializeField] private Button _writeFileButton;
        [SerializeField] private Button _appendFileButton;
        [SerializeField] private Button _deleteFileButton;
        [SerializeField] private Button _copyFileButton;
        [SerializeField] private Button _moveFileButton;
        
        [Header("异步文件操作")]
        [SerializeField] private InputField _asyncFileContentInputField;
        [SerializeField] private Text _asyncFileResultText;
        [SerializeField] private Button _readAsyncButton;
        [SerializeField] private Button _writeAsyncButton;
        [SerializeField] private Button _calculatedHashButton;
        [SerializeField] private Slider _fileSizeSlider;
        [SerializeField] private Text _fileSizeText;
        
        [Header("文件监控")]
        [SerializeField] private Text _monitorResultText;
        [SerializeField] private Button _startMonitoringButton;
        [SerializeField] private Button _stopMonitoringButton;
        [SerializeField] private Button _modifyMonitoredFileButton;
        [SerializeField] private Toggle _includeSubdirectoriesToggle;
        
        // 示例文件路径
        private string _testDirectoryPath;
        private string _testFilePath;
        private string _testCopyFilePath;
        private string _testMoveFilePath;
        private string _testLargeFilePath;
        private string _testMonitorFilePath;
        
        // 文件监控ID
        private string _watcherId;
        
        // 文件变化日志
        private List<string> _fileChangeLog = new List<string>();

        private void Start()
        {
            // 初始化示例路径
            InitializePaths();
            
            // 创建示例目录
            EnsureDirectoryExists(_testDirectoryPath);
            
            // 初始化UI事件监听
            SetupUIEvents();
            
            // 初始化文件大小滑块
            UpdateFileSizeText(_fileSizeSlider.value);
            
            // 显示初始路径
            if (_pathInputField != null)
                _pathInputField.text = _testFilePath;
        }
        
        private void OnDestroy()
        {
            // 停止所有文件监控
            if (!string.IsNullOrEmpty(_watcherId))
            {
                IOUtils.StopWatching(_watcherId);
            }
        }
        
        private void InitializePaths()
        {
            // 使用应用程序的持久化数据路径作为测试目录
            _testDirectoryPath = Path.Combine(Application.persistentDataPath, "IOUtilsDemo");
            _testFilePath = Path.Combine(_testDirectoryPath, "test.txt");
            _testCopyFilePath = Path.Combine(_testDirectoryPath, "test_copy.txt");
            _testMoveFilePath = Path.Combine(_testDirectoryPath, "test_moved.txt");
            _testLargeFilePath = Path.Combine(_testDirectoryPath, "large_file.dat");
            _testMonitorFilePath = Path.Combine(_testDirectoryPath, "monitored_file.txt");
        }
        
        private void SetupUIEvents()
        {
            // 路径处理按钮
            if (_normalizePathButton != null)
                _normalizePathButton.onClick.AddListener(NormalizePath);
                
            if (_getRelativePathButton != null)
                _getRelativePathButton.onClick.AddListener(GetRelativePath);
                
            if (_getExtensionButton != null)
                _getExtensionButton.onClick.AddListener(GetExtension);
                
            if (_getFileNameButton != null)
                _getFileNameButton.onClick.AddListener(GetFileName);
                
            // 文件操作按钮
            if (_readFileButton != null)
                _readFileButton.onClick.AddListener(ReadFile);
                
            if (_writeFileButton != null)
                _writeFileButton.onClick.AddListener(WriteFile);
                
            if (_appendFileButton != null)
                _appendFileButton.onClick.AddListener(AppendFile);
                
            if (_deleteFileButton != null)
                _deleteFileButton.onClick.AddListener(DeleteFile);
                
            if (_copyFileButton != null)
                _copyFileButton.onClick.AddListener(CopyFile);
                
            if (_moveFileButton != null)
                _moveFileButton.onClick.AddListener(MoveFile);
                
            // 异步文件操作按钮
            if (_readAsyncButton != null)
                _readAsyncButton.onClick.AddListener(() => StartCoroutine(ReadFileAsync()));
                
            if (_writeAsyncButton != null)
                _writeAsyncButton.onClick.AddListener(() => StartCoroutine(WriteFileAsync()));
                
            if (_calculatedHashButton != null)
                _calculatedHashButton.onClick.AddListener(() => StartCoroutine(CalculateHashAsync()));
                
            // 文件监控按钮
            if (_startMonitoringButton != null)
                _startMonitoringButton.onClick.AddListener(StartMonitoring);
                
            if (_stopMonitoringButton != null)
                _stopMonitoringButton.onClick.AddListener(StopMonitoring);
                
            if (_modifyMonitoredFileButton != null)
                _modifyMonitoredFileButton.onClick.AddListener(ModifyMonitoredFile);
                
            // 滑块事件
            if (_fileSizeSlider != null)
            {
                _fileSizeSlider.onValueChanged.AddListener(UpdateFileSizeText);
            }
        }
        
        private void EnsureDirectoryExists(string directoryPath)
        {
            if (!IOUtils.DirectoryExists(directoryPath))
            {
                IOUtils.CreateDirectory(directoryPath);
            }
        }
        
        private void UpdateFileSizeText(float value)
        {
            int fileSizeKB = Mathf.RoundToInt(value);
            if (_fileSizeText != null)
            {
                if (fileSizeKB < 1024)
                    _fileSizeText.text = $"{fileSizeKB} KB";
                else
                    _fileSizeText.text = $"{fileSizeKB / 1024f:F2} MB";
            }
        }
        
        #region 路径处理
        
        private void NormalizePath()
        {
            string path = GetInputPath();
            if (string.IsNullOrEmpty(path)) return;
            
            string normalizedPath = IOUtils.NormalizePath(path);
            
            SetPathResult($"规范化路径:\n原始路径: {path}\n规范化结果: {normalizedPath}");
        }
        
        private void GetRelativePath()
        {
            string path = GetInputPath();
            if (string.IsNullOrEmpty(path)) return;
            
            string basePath = Application.persistentDataPath;
            string relativePath = IOUtils.GetRelativePath(basePath, path);
            
            SetPathResult($"获取相对路径:\n基准路径: {basePath}\n目标路径: {path}\n相对路径: {relativePath}");
        }
        
        private void GetExtension()
        {
            string path = GetInputPath();
            if (string.IsNullOrEmpty(path)) return;
            
            string extension = IOUtils.GetExtension(path);
            
            SetPathResult($"获取文件扩展名:\n文件路径: {path}\n扩展名: {extension}");
        }
        
        private void GetFileName()
        {
            string path = GetInputPath();
            if (string.IsNullOrEmpty(path)) return;
            
            string fileName = IOUtils.GetFileName(path);
            string fileNameWithoutExtension = IOUtils.GetFileNameWithoutExtension(path);
            
            SetPathResult($"获取文件名:\n文件路径: {path}\n文件名: {fileName}\n无扩展名文件名: {fileNameWithoutExtension}");
        }
        
        private string GetInputPath()
        {
            return _pathInputField != null ? _pathInputField.text : _testFilePath;
        }
        
        private void SetPathResult(string result)
        {
            if (_pathResultText != null)
                _pathResultText.text = result;
        }
        
        #endregion
        
        #region 文件操作
        
        private void ReadFile()
        {
            try
            {
                if (!IOUtils.FileExists(_testFilePath))
                {
                    SetFileResult($"文件不存在: {_testFilePath}");
                    return;
                }
                
                string content = IOUtils.ReadAllText(_testFilePath);
                
                SetFileResult($"读取文件内容:\n文件: {_testFilePath}\n内容: \n{content}");
            }
            catch (Exception ex)
            {
                SetFileResult($"读取文件失败: {ex.Message}");
            }
        }
        
        private void WriteFile()
        {
            try
            {
                string content = GetFileContent();
                
                // 确保目录存在
                EnsureDirectoryExists(_testDirectoryPath);
                
                // 写入文件
                IOUtils.WriteAllText(_testFilePath, content);
                
                SetFileResult($"写入文件成功!\n文件: {_testFilePath}\n内容: \n{content}");
            }
            catch (Exception ex)
            {
                SetFileResult($"写入文件失败: {ex.Message}");
            }
        }
        
        private void AppendFile()
        {
            try
            {
                string content = GetFileContent();
                
                // 确保目录存在
                EnsureDirectoryExists(_testDirectoryPath);
                
                // 附加到文件
                IOUtils.AppendAllText(_testFilePath, "\n" + content);
                
                string newContent = IOUtils.ReadAllText(_testFilePath);
                
                SetFileResult($"附加文件成功!\n文件: {_testFilePath}\n当前内容: \n{newContent}");
            }
            catch (Exception ex)
            {
                SetFileResult($"附加文件失败: {ex.Message}");
            }
        }
        
        private void DeleteFile()
        {
            try
            {
                if (!IOUtils.FileExists(_testFilePath))
                {
                    SetFileResult($"文件不存在: {_testFilePath}");
                    return;
                }
                
                IOUtils.DeleteFile(_testFilePath);
                
                SetFileResult($"删除文件成功!\n文件: {_testFilePath}");
            }
            catch (Exception ex)
            {
                SetFileResult($"删除文件失败: {ex.Message}");
            }
        }
        
        private void CopyFile()
        {
            try
            {
                if (!IOUtils.FileExists(_testFilePath))
                {
                    SetFileResult($"源文件不存在: {_testFilePath}");
                    return;
                }
                
                IOUtils.CopyFile(_testFilePath, _testCopyFilePath, true);
                
                SetFileResult($"复制文件成功!\n源文件: {_testFilePath}\n目标文件: {_testCopyFilePath}");
            }
            catch (Exception ex)
            {
                SetFileResult($"复制文件失败: {ex.Message}");
            }
        }
        
        private void MoveFile()
        {
            try
            {
                if (!IOUtils.FileExists(_testFilePath))
                {
                    SetFileResult($"源文件不存在: {_testFilePath}");
                    return;
                }
                
                if (IOUtils.FileExists(_testMoveFilePath))
                {
                    IOUtils.DeleteFile(_testMoveFilePath);
                }
                
                IOUtils.MoveFile(_testFilePath, _testMoveFilePath);
                
                SetFileResult($"移动文件成功!\n源文件: {_testFilePath}\n目标文件: {_testMoveFilePath}");
            }
            catch (Exception ex)
            {
                SetFileResult($"移动文件失败: {ex.Message}");
            }
        }
        
        private string GetFileContent()
        {
            return _fileContentInputField != null ? _fileContentInputField.text : "测试内容";
        }
        
        private void SetFileResult(string result)
        {
            if (_fileResultText != null)
                _fileResultText.text = result;
        }
        
        #endregion
        
        #region 异步文件操作
        
        private IEnumerator ReadFileAsync()
        {
            SetAsyncFileResult("正在异步读取文件...");
            
            if (!IOUtils.FileExists(_testLargeFilePath))
            {
                SetAsyncFileResult($"文件不存在: {_testLargeFilePath}\n请先写入大文件");
                yield break;
            }
            
            // 开始计时
            float startTime = Time.realtimeSinceStartup;
            
            try
            {
                Task<string> readTask = IOUtils.ReadAllTextAsync(_testLargeFilePath);
                
                // 等待任务完成
                while (!readTask.IsCompleted)
                {
                    SetAsyncFileResult($"正在异步读取文件... 已用时: {Time.realtimeSinceStartup - startTime:F2}秒");
                    yield return null;
                }
                
                if (readTask.IsFaulted)
                {
                    SetAsyncFileResult($"读取文件失败: {readTask.Exception.Message}");
                    yield break;
                }
                
                string content = readTask.Result;
                float elapsedTime = Time.realtimeSinceStartup - startTime;
                
                SetAsyncFileResult($"异步读取文件成功!\n文件: {_testLargeFilePath}\n大小: {IOUtils.GetFileSize(_testLargeFilePath) / 1024f:F2} KB\n用时: {elapsedTime:F2}秒\n内容 (前100字符): \n{content.Substring(0, Math.Min(100, content.Length))}...");
            }
            catch (Exception ex)
            {
                SetAsyncFileResult($"读取文件失败: {ex.Message}");
            }
        }
        
        private IEnumerator WriteFileAsync()
        {
            int fileSizeKB = Mathf.RoundToInt(_fileSizeSlider.value);
            string asyncContent = GenerateLargeContent(fileSizeKB);
            
            SetAsyncFileResult($"正在异步写入 {fileSizeKB} KB 文件...");
            
            // 确保目录存在
            EnsureDirectoryExists(_testDirectoryPath);
            
            // 开始计时
            float startTime = Time.realtimeSinceStartup;
            
            try
            {
                Task writeTask = IOUtils.WriteAllTextAsync(_testLargeFilePath, asyncContent);
                
                // 等待任务完成
                while (!writeTask.IsCompleted)
                {
                    SetAsyncFileResult($"正在异步写入 {fileSizeKB} KB 文件... 已用时: {Time.realtimeSinceStartup - startTime:F2}秒");
                    yield return null;
                }
                
                if (writeTask.IsFaulted)
                {
                    SetAsyncFileResult($"写入文件失败: {writeTask.Exception.Message}");
                    yield break;
                }
                
                float elapsedTime = Time.realtimeSinceStartup - startTime;
                
                SetAsyncFileResult($"异步写入文件成功!\n文件: {_testLargeFilePath}\n大小: {fileSizeKB} KB\n用时: {elapsedTime:F2}秒");
            }
            catch (Exception ex)
            {
                SetAsyncFileResult($"写入文件失败: {ex.Message}");
            }
        }
        
        private IEnumerator CalculateHashAsync()
        {
            if (!IOUtils.FileExists(_testLargeFilePath))
            {
                SetAsyncFileResult($"文件不存在: {_testLargeFilePath}\n请先写入大文件");
                yield break;
            }
            
            SetAsyncFileResult("正在计算文件哈希值...");
            
            // 开始计时
            float startTime = Time.realtimeSinceStartup;
            
            try
            {
                Task<string> hashTask = IOUtils.CalculateMD5Async(_testLargeFilePath);
                
                // 等待任务完成
                while (!hashTask.IsCompleted)
                {
                    SetAsyncFileResult($"正在计算文件哈希值... 已用时: {Time.realtimeSinceStartup - startTime:F2}秒");
                    yield return null;
                }
                
                if (hashTask.IsFaulted)
                {
                    SetAsyncFileResult($"计算哈希值失败: {hashTask.Exception.Message}");
                    yield break;
                }
                
                string hash = hashTask.Result;
                float elapsedTime = Time.realtimeSinceStartup - startTime;
                
                SetAsyncFileResult($"计算哈希值成功!\n文件: {_testLargeFilePath}\n大小: {IOUtils.GetFileSize(_testLargeFilePath) / 1024f:F2} KB\nMD5: {hash}\n用时: {elapsedTime:F2}秒");
            }
            catch (Exception ex)
            {
                SetAsyncFileResult($"计算哈希值失败: {ex.Message}");
            }
        }
        
        private string GenerateLargeContent(int sizeKB)
        {
            StringBuilder sb = new StringBuilder(sizeKB * 1024);
            string line = "这是一行测试数据，用于生成足够大的文本内容来测试异步文件操作性能。";
            
            int lineLength = line.Length + Environment.NewLine.Length;
            int neededLines = (sizeKB * 1024) / lineLength + 1;
            
            for (int i = 0; i < neededLines; i++)
            {
                sb.AppendLine($"{i}: {line}");
            }
            
            return sb.ToString();
        }
        
        private string GetAsyncFileContent()
        {
            return _asyncFileContentInputField != null ? _asyncFileContentInputField.text : "异步测试内容";
        }
        
        private void SetAsyncFileResult(string result)
        {
            if (_asyncFileResultText != null)
                _asyncFileResultText.text = result;
        }
        
        #endregion
        
        #region 文件监控
        
        private void StartMonitoring()
        {
            if (!string.IsNullOrEmpty(_watcherId))
            {
                AddToFileMonitorLog("已经在监视文件，请先停止当前监视");
                return;
            }
            
            try
            {
                // 确保测试目录存在
                EnsureDirectoryExists(_testDirectoryPath);
                
                // 确保被监视的文件存在
                if (!IOUtils.FileExists(_testMonitorFilePath))
                {
                    IOUtils.WriteAllText(_testMonitorFilePath, "这是一个被监视的文件。");
                }
                
                bool includeSubdirs = _includeSubdirectoriesToggle != null && _includeSubdirectoriesToggle.isOn;
                
                // 清空日志
                _fileChangeLog.Clear();
                AddToFileMonitorLog($"开始监视: {_testMonitorFilePath}");
                
                // 开始监视
                _watcherId = IOUtils.StartWatching(
                    includeSubdirs ? _testDirectoryPath : _testMonitorFilePath,
                    onChange: (path, changeType) => 
                    {
                        // 在主线程上更新UI
                        UnityMainThreadDispatcher.Instance().Enqueue(() => 
                        {
                            AddToFileMonitorLog($"检测到变化: {path}\n类型: {changeType}");
                        });
                    },
                    includeSubdirectories: includeSubdirs
                );
                
                AddToFileMonitorLog("监视已开始！请尝试修改文件查看效果");
                
                // 更新按钮状态
                if (_startMonitoringButton != null)
                    _startMonitoringButton.interactable = false;
                    
                if (_stopMonitoringButton != null)
                    _stopMonitoringButton.interactable = true;
            }
            catch (Exception ex)
            {
                AddToFileMonitorLog($"启动监视失败: {ex.Message}");
            }
        }
        
        private void StopMonitoring()
        {
            if (string.IsNullOrEmpty(_watcherId))
            {
                AddToFileMonitorLog("没有正在进行的监视");
                return;
            }
            
            try
            {
                // 停止监视
                IOUtils.StopWatching(_watcherId);
                _watcherId = null;
                
                AddToFileMonitorLog("监视已停止");
                
                // 更新按钮状态
                if (_startMonitoringButton != null)
                    _startMonitoringButton.interactable = true;
                    
                if (_stopMonitoringButton != null)
                    _stopMonitoringButton.interactable = false;
            }
            catch (Exception ex)
            {
                AddToFileMonitorLog($"停止监视失败: {ex.Message}");
            }
        }
        
        private void ModifyMonitoredFile()
        {
            try
            {
                // 修改被监视的文件
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                IOUtils.AppendAllText(_testMonitorFilePath, $"\n修改于: {timestamp}");
                
                AddToFileMonitorLog($"手动修改文件: {_testMonitorFilePath}");
            }
            catch (Exception ex)
            {
                AddToFileMonitorLog($"修改文件失败: {ex.Message}");
            }
        }
        
        private void AddToFileMonitorLog(string message)
        {
            _fileChangeLog.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            
            // 保持日志不超过20条
            while (_fileChangeLog.Count > 20)
            {
                _fileChangeLog.RemoveAt(0);
            }
            
            // 更新UI
            UpdateMonitorResultText();
        }
        
        private void UpdateMonitorResultText()
        {
            if (_monitorResultText != null)
            {
                _monitorResultText.text = string.Join("\n\n", _fileChangeLog);
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 一个简单的Unity主线程调度器，用于从后台线程更新UI
    /// </summary>
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static UnityMainThreadDispatcher _instance;
        private readonly Queue<Action> _executionQueue = new Queue<Action>();
        private readonly object _lock = new object();
        
        public static UnityMainThreadDispatcher Instance()
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("UnityMainThreadDispatcher");
                _instance = go.AddComponent<UnityMainThreadDispatcher>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
        
        public void Enqueue(Action action)
        {
            lock (_lock)
            {
                _executionQueue.Enqueue(action);
            }
        }
        
        void Update()
        {
            lock (_lock)
            {
                while (_executionQueue.Count > 0)
                {
                    _executionQueue.Dequeue().Invoke();
                }
            }
        }
    }
} 