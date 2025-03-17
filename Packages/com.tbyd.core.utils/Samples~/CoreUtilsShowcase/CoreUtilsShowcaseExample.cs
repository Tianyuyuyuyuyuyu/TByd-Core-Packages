using UnityEngine;
using UnityEngine.UI;
using TByd.Core.Utils.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace TByd.Core.Utils.Samples
{
    /// <summary>
    /// TByd.Core.Utils包所有工具类的综合展示
    /// </summary>
    public class CoreUtilsShowcaseExample : MonoBehaviour
    {
        [Header("界面控制")]
        [SerializeField] private Dropdown _utilsClassDropdown;
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _descriptionText;
        [SerializeField] private Button _showPerformanceButton;
        [SerializeField] private Button _showReferencesButton;
        
        [Header("示例面板")]
        [SerializeField] private RectTransform[] _showcasePanels;
        [SerializeField] private GameObject _mathUtilsPanel;
        [SerializeField] private GameObject _stringUtilsPanel;
        [SerializeField] private GameObject _timeUtilsPanel;
        [SerializeField] private GameObject _randomUtilsPanel;
        [SerializeField] private GameObject _reflectionUtilsPanel;
        [SerializeField] private GameObject _collectionUtilsPanel;
        [SerializeField] private GameObject _ioUtilsPanel;
        [SerializeField] private GameObject _transformExtensionsPanel;
        
        [Header("代码示例")]
        [SerializeField] private Text _codeExampleText;
        
        [Header("性能对比")]
        [SerializeField] private GameObject _performancePanel;
        [SerializeField] private Text _performanceResultText;
        [SerializeField] private Button _runTestButton;
        
        // 当前选中的工具类索引
        private int _currentUtilsIndex = 0;
        
        // 示例数据
        private List<string> _sampleStrings;
        private List<int> _sampleInts;
        private List<Vector3> _sampleVectors;
        private List<GameObject> _sampleGameObjects;
        
        // 各工具类的描述
        private readonly string[] _descriptions = new string[] {
            "MathUtils提供各种数学计算功能，包括插值、坐标转换、夹值和映射等，性能优化并减少GC压力。",
            "StringUtils提供高性能的字符串处理功能，包括分割、格式化、截断等，大幅减少GC分配。",
            "TimeUtils提供时间处理工具，包括时间格式化、相对时间描述、游戏时间系统和性能测量。",
            "RandomUtils提供增强的随机功能，包括范围随机、权重随机、随机颜色生成等。",
            "ReflectionUtils简化反射操作，提供类型查找、成员操作、属性获取设置和方法调用等功能。",
            "CollectionUtils提供集合处理工具，包括批处理、分页、过滤、映射和比较等功能。",
            "IOUtils提供文件和目录操作工具，包括读写、路径处理、监控和异步操作等。",
            "TransformExtensions为Transform类提供实用扩展方法，简化常见操作。"
        };
        
        // 示例代码
        private readonly string[] _codeExamples = new string[] {
            "// 将值从一个范围映射到另一个范围\nfloat remapped = MathUtils.Remap(value, 0, 10, 0, 1);\n\n// 平滑阻尼插值（类似SmoothDamp但有更多控制）\nVector3 smoothed = MathUtils.SmoothDamp(current, target, ref velocity, smoothTime);\n\n// 缓动插值函数\nfloat eased = MathUtils.EaseInOut(start, end, t);",
            
            "// 高性能字符串分割（减少GC分配）\nforeach (var part in StringUtils.Split(text, ',')) {\n    // 使用part...\n}\n\n// 格式化字符串，支持参数\nstring formatted = StringUtils.Format(\"{0} - {1}\", \"标题\", 123);\n\n// 截断字符串并添加省略号\nstring truncated = StringUtils.Truncate(longText, 20);",
            
            "// 将时间格式化为友好字符串\nstring timeStr = TimeUtils.FormatDateTime(DateTime.Now, \"yyyy-MM-dd HH:mm:ss\");\n\n// 获取相对时间描述\nstring relativeTime = TimeUtils.GetRelativeTimeDescription(timestamp);\n// 输出：\"3小时前\"、\"2天后\"等\n\n// 测量代码执行时间\nfloat ms = TimeUtils.MeasureExecutionTime(() => {\n    // 要测量的代码...\n});",
            
            "// 生成指定范围的随机整数\nint randomInt = RandomUtils.Range(1, 100);\n\n// 生成随机颜色\nColor randomColor = RandomUtils.ColorHSV();\n\n// 带权重的随机选择\nstring item = RandomUtils.WeightedRandom(\n    new string[] { \"普通\", \"稀有\", \"史诗\" },\n    new float[] { 70, 25, 5 }\n);",
            
            "// 获取类型\nType type = ReflectionUtils.GetType(\"UnityEngine.GameObject\");\n\n// 获取属性值\nobject value = ReflectionUtils.GetPropertyValue(obj, \"propertyName\");\n\n// 设置属性值\nReflectionUtils.SetPropertyValue(obj, \"propertyName\", newValue);\n\n// 调用方法\nobject result = ReflectionUtils.InvokeMethod(obj, \"MethodName\", parameters);",
            
            "// 批量处理集合，每批100个元素\nCollectionUtils.BatchProcess(items, 100, batch => {\n    foreach (var item in batch) {\n        // 处理每个item...\n    }\n});\n\n// 过滤集合\nvar filtered = CollectionUtils.Filter(users, user => user.Age >= 18);\n\n// 分页数据\nvar page = CollectionUtils.Paginate(items, pageIndex, pageSize);",
            
            "// 读取文本文件\nstring content = IOUtils.ReadAllText(filePath);\n\n// 异步写入文件\nawait IOUtils.WriteAllTextAsync(filePath, content);\n\n// 监视文件变化\nstring watcherId = IOUtils.StartWatching(directoryPath, \n    (path, changeType) => {\n        Debug.Log($\"文件改变：{path}, 类型：{changeType}\");\n    }\n);",
            
            "// 重置Transform的本地位置、旋转和缩放\ntransform.ResetLocal();\n\n// 查找或创建子物体\nTransform child = transform.FindOrCreateChild(\"UI\");\n\n// 获取所有子物体（包括非激活的）\nTransform[] children = transform.GetAllChildren();\n\n// 销毁所有子物体\ntransform.DestroyAllChildren();"
        };

        private void Start()
        {
            // 初始化下拉菜单
            SetupDropdown();
            
            // 初始化示例数据
            InitializeSampleData();
            
            // 设置按钮事件
            SetupButtons();
            
            // 默认显示第一个工具类
            ShowUtilsPanel(0);
        }
        
        private void SetupDropdown()
        {
            if (_utilsClassDropdown != null)
            {
                _utilsClassDropdown.ClearOptions();
                _utilsClassDropdown.AddOptions(new List<string> {
                    "MathUtils",
                    "StringUtils",
                    "TimeUtils",
                    "RandomUtils",
                    "ReflectionUtils",
                    "CollectionUtils",
                    "IOUtils",
                    "TransformExtensions"
                });
                
                _utilsClassDropdown.onValueChanged.AddListener(ShowUtilsPanel);
            }
        }
        
        private void InitializeSampleData()
        {
            // 示例字符串
            _sampleStrings = new List<string> {
                "这是第一个示例字符串",
                "这是第二个,包含逗号的字符串",
                "这是一个很长很长很长很长很长很长很长很长很长很长很长很长很长很长的字符串，需要被截断"
            };
            
            // 示例整数
            _sampleInts = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                _sampleInts.Add(i);
            }
            
            // 示例向量
            _sampleVectors = new List<Vector3> {
                new Vector3(1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 0, 1),
                new Vector3(1, 1, 1)
            };
            
            // 示例游戏对象
            _sampleGameObjects = new List<GameObject>();
            for (int i = 0; i < 3; i++)
            {
                GameObject go = new GameObject($"Sample_{i}");
                go.transform.SetParent(transform);
                _sampleGameObjects.Add(go);
            }
        }
        
        private void SetupButtons()
        {
            if (_showPerformanceButton != null)
                _showPerformanceButton.onClick.AddListener(TogglePerformancePanel);
                
            if (_showReferencesButton != null)
                _showReferencesButton.onClick.AddListener(ShowReferences);
                
            if (_runTestButton != null)
                _runTestButton.onClick.AddListener(RunPerformanceTest);
        }
        
        private void ShowUtilsPanel(int index)
        {
            _currentUtilsIndex = index;
            
            // 隐藏所有面板
            foreach (var panel in _showcasePanels)
            {
                if (panel != null)
                    panel.gameObject.SetActive(false);
            }
            
            // 显示选中的面板
            GameObject selectedPanel = null;
            
            switch (index)
            {
                case 0: selectedPanel = _mathUtilsPanel; break;
                case 1: selectedPanel = _stringUtilsPanel; break;
                case 2: selectedPanel = _timeUtilsPanel; break;
                case 3: selectedPanel = _randomUtilsPanel; break;
                case 4: selectedPanel = _reflectionUtilsPanel; break;
                case 5: selectedPanel = _collectionUtilsPanel; break;
                case 6: selectedPanel = _ioUtilsPanel; break;
                case 7: selectedPanel = _transformExtensionsPanel; break;
            }
            
            if (selectedPanel != null)
                selectedPanel.SetActive(true);
                
            // 更新标题和描述
            if (_titleText != null)
                _titleText.text = index < _utilsClassDropdown.options.Count ? 
                    _utilsClassDropdown.options[index].text : "未知工具类";
                
            if (_descriptionText != null)
                _descriptionText.text = index < _descriptions.Length ? 
                    _descriptions[index] : "没有可用的描述";
                    
            // 更新代码示例
            if (_codeExampleText != null)
                _codeExampleText.text = index < _codeExamples.Length ? 
                    _codeExamples[index] : "没有可用的代码示例";
                    
            // 隐藏性能面板
            if (_performancePanel != null)
                _performancePanel.SetActive(false);
        }
        
        private void TogglePerformancePanel()
        {
            if (_performancePanel != null)
                _performancePanel.SetActive(!_performancePanel.activeSelf);
                
            // 初始化性能结果文本
            if (_performanceResultText != null)
                _performanceResultText.text = "点击 \"运行测试\" 按钮开始性能测试...";
        }
        
        private void ShowReferences()
        {
            // 显示当前工具类的相关参考信息
            string[] references = new string[] {
                "MathUtils相关参考资料:\n- Unity Mathf 文档\n- Unity Mathematics 库\n- 缓动函数参考: https://easings.net/",
                "StringUtils相关参考资料:\n- C# String 类文档\n- StringBuilder 文档\n- .NET Span<T> 文档",
                "TimeUtils相关参考资料:\n- C# DateTime 文档\n- C# TimeSpan 文档\n- Unity Time 类文档",
                "RandomUtils相关参考资料:\n- Unity Random 类文档\n- C# System.Random 文档\n- 随机分布算法参考",
                "ReflectionUtils相关参考资料:\n- C# Reflection 文档\n- System.Reflection 命名空间\n- IL2CPP 反射限制",
                "CollectionUtils相关参考资料:\n- C# LINQ 文档\n- IEnumerable<T> 接口\n- 集合操作性能优化参考",
                "IOUtils相关参考资料:\n- C# System.IO 文档\n- Unity 文件操作指南\n- 异步IO操作最佳实践",
                "TransformExtensions相关参考资料:\n- Unity Transform 类文档\n- Unity GameObject 类文档\n- C# 扩展方法文档"
            };
            
            if (_currentUtilsIndex < references.Length && _performanceResultText != null)
            {
                _performanceResultText.text = references[_currentUtilsIndex];
                
                // 显示性能面板（复用来显示参考信息）
                if (_performancePanel != null && !_performancePanel.activeSelf)
                    _performancePanel.SetActive(true);
            }
        }
        
        private void RunPerformanceTest()
        {
            if (_performanceResultText == null)
                return;
                
            _performanceResultText.text = "正在运行性能测试...";
            
            // 延迟执行以便UI能够更新
            StartCoroutine(RunPerformanceTestCoroutine());
        }
        
        private IEnumerator RunPerformanceTestCoroutine()
        {
            yield return null;
            
            string result = "性能测试结果:\n\n";
            
            switch (_currentUtilsIndex)
            {
                case 0: // MathUtils
                    result += TestMathUtilsPerformance();
                    break;
                    
                case 1: // StringUtils
                    result += TestStringUtilsPerformance();
                    break;
                    
                case 5: // CollectionUtils
                    result += TestCollectionUtilsPerformance();
                    break;
                    
                case 6: // IOUtils
                    result += "IO操作性能测试需要实际文件系统交互，\n请参考IOUtilsDemo中的异步操作示例。";
                    break;
                    
                default:
                    result += "当前工具类没有性能测试。";
                    break;
            }
            
            if (_performanceResultText != null)
                _performanceResultText.text = result;
        }
        
        private string TestMathUtilsPerformance()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            
            // 测试1: Remap vs 手动计算
            float[] values = new float[1000];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = i / 10f;
            }
            
            float[] result1 = new float[values.Length];
            float[] result2 = new float[values.Length];
            
            // 测试MathUtils.Remap
            sw.Reset();
            sw.Start();
            for (int i = 0; i < values.Length; i++)
            {
                result1[i] = MathUtils.Remap(values[i], 0, 100, 0, 1);
            }
            sw.Stop();
            long remapTime = sw.ElapsedTicks;
            
            // 测试手动计算
            sw.Reset();
            sw.Start();
            for (int i = 0; i < values.Length; i++)
            {
                result2[i] = (values[i] - 0) / (100 - 0) * (1 - 0) + 0;
            }
            sw.Stop();
            long manualTime = sw.ElapsedTicks;
            
            string output = $"Remap 1000个值:\n";
            output += $"MathUtils.Remap: {remapTime} ticks\n";
            output += $"手动计算: {manualTime} ticks\n";
            output += $"性能比: {manualTime / (float)remapTime:F2}x\n\n";
            
            // 测试2: EaseInOut vs 线性插值
            sw.Reset();
            sw.Start();
            for (int i = 0; i < 10000; i++)
            {
                float t = i / 10000f;
                float eased = MathUtils.EaseInOut(0, 1, t);
            }
            sw.Stop();
            long easeTime = sw.ElapsedTicks;
            
            sw.Reset();
            sw.Start();
            for (int i = 0; i < 10000; i++)
            {
                float t = i / 10000f;
                float linear = Mathf.Lerp(0, 1, t);
            }
            sw.Stop();
            long lerpTime = sw.ElapsedTicks;
            
            output += $"插值 10000次:\n";
            output += $"MathUtils.EaseInOut: {easeTime} ticks\n";
            output += $"Mathf.Lerp: {lerpTime} ticks\n";
            output += $"EaseInOut相对开销: {easeTime / (float)lerpTime:F2}x\n";
            output += "(注意: EaseInOut包含更复杂的曲线计算，预期更慢)";
            
            return output;
        }
        
        private string TestStringUtilsPerformance()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            
            // 生成测试数据
            string testString = "这是,一个,用于,测试,字符串,分割,性能,的,字符串";
            
            int iterations = 10000;
            
            // 测试1: Split性能对比
            sw.Reset();
            sw.Start();
            
            // 预热
            for (int i = 0; i < 100; i++)
            {
                foreach (var part in StringUtils.Split(testString, ','))
                {
                    // 确保使用结果，防止编译器优化掉
                    _ = part.Length;
                }
            }
            
            long startMemory = GC.GetTotalMemory(true);
            
            for (int i = 0; i < iterations; i++)
            {
                foreach (var part in StringUtils.Split(testString, ','))
                {
                    _ = part.Length;
                }
            }
            
            long endMemory = GC.GetTotalMemory(false);
            sw.Stop();
            long customSplitTime = sw.ElapsedTicks;
            long customSplitMemory = endMemory - startMemory;
            
            // 测试原生String.Split
            sw.Reset();
            sw.Start();
            
            // 预热
            for (int i = 0; i < 100; i++)
            {
                string[] parts = testString.Split(',');
                foreach (string part in parts)
                {
                    _ = part.Length;
                }
            }
            
            startMemory = GC.GetTotalMemory(true);
            
            for (int i = 0; i < iterations; i++)
            {
                string[] parts = testString.Split(',');
                foreach (string part in parts)
                {
                    _ = part.Length;
                }
            }
            
            endMemory = GC.GetTotalMemory(false);
            sw.Stop();
            long standardSplitTime = sw.ElapsedTicks;
            long standardSplitMemory = endMemory - startMemory;
            
            string output = $"字符串分割 ({iterations}次):\n";
            output += $"StringUtils.Split 时间: {customSplitTime} ticks\n";
            output += $"String.Split 时间: {standardSplitTime} ticks\n";
            output += $"性能比: {standardSplitTime / (float)customSplitTime:F2}x\n\n";
            
            output += $"内存分配:\n";
            output += $"StringUtils.Split: {customSplitMemory / 1024f:F2} KB\n";
            output += $"String.Split: {standardSplitMemory / 1024f:F2} KB\n";
            output += $"内存减少: {(1 - (customSplitMemory / (float)standardSplitMemory)) * 100:F1}%\n";
            
            return output;
        }
        
        private string TestCollectionUtilsPerformance()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            
            // 生成大量测试数据
            List<int> testData = new List<int>();
            for (int i = 0; i < 100000; i++)
            {
                testData.Add(i);
            }
            
            // 记录初始内存
            long startMemory = GC.GetTotalMemory(true);
            
            // 测试1: 批处理vs普通循环
            int batchSize = 1000;
            int sum1 = 0;
            int sum2 = 0;
            
            // 测试BatchProcess
            sw.Reset();
            sw.Start();
            
            CollectionUtils.BatchProcess(testData, batchSize, batch => {
                foreach (var item in batch)
                {
                    sum1 += item;
                }
            });
            
            sw.Stop();
            long batchTime = sw.ElapsedTicks;
            long batchMemory = GC.GetTotalMemory(false) - startMemory;
            
            // 测试普通for循环
            startMemory = GC.GetTotalMemory(true);
            sw.Reset();
            sw.Start();
            
            for (int i = 0; i < testData.Count; i++)
            {
                sum2 += testData[i];
            }
            
            sw.Stop();
            long normalTime = sw.ElapsedTicks;
            long normalMemory = GC.GetTotalMemory(false) - startMemory;
            
            // 确认结果一致性
            bool resultsMatch = sum1 == sum2;
            
            string output = $"处理100,000个整数:\n";
            output += $"BatchProcess (批量{batchSize}): {batchTime} ticks\n";
            output += $"普通for循环: {normalTime} ticks\n";
            output += $"结果一致: {resultsMatch}\n\n";
            
            // 测试2: Filter vs LINQ Where
            startMemory = GC.GetTotalMemory(true);
            sw.Reset();
            sw.Start();
            
            var filtered1 = CollectionUtils.Filter(testData, x => x % 2 == 0);
            
            sw.Stop();
            long filterTime = sw.ElapsedTicks;
            long filterMemory = GC.GetTotalMemory(false) - startMemory;
            
            // 测试LINQ Where
            startMemory = GC.GetTotalMemory(true);
            sw.Reset();
            sw.Start();
            
            var filtered2 = testData.Where(x => x % 2 == 0).ToList();
            
            sw.Stop();
            long whereTime = sw.ElapsedTicks;
            long whereMemory = GC.GetTotalMemory(false) - startMemory;
            
            // 确认结果一致性
            bool filterResultsMatch = filtered1.Count == filtered2.Count;
            
            output += $"过滤100,000个整数:\n";
            output += $"CollectionUtils.Filter: {filterTime} ticks\n";
            output += $"LINQ Where: {whereTime} ticks\n";
            output += $"性能比: {whereTime / (float)filterTime:F2}x\n\n";
            
            output += $"内存分配:\n";
            output += $"CollectionUtils.Filter: {filterMemory / 1024f:F2} KB\n";
            output += $"LINQ Where: {whereMemory / 1024f:F2} KB\n";
            
            return output;
        }
        
        // 在销毁时清理资源
        private void OnDestroy()
        {
            // 清理示例游戏对象
            foreach (var go in _sampleGameObjects)
            {
                if (go != null)
                    Destroy(go);
            }
        }
    }
} 