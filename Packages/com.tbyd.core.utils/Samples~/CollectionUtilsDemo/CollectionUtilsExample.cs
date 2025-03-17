using UnityEngine;
using UnityEngine.UI;
using TByd.Core.Utils.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TByd.Core.Utils.Samples
{
    /// <summary>
    /// 展示CollectionUtils类的使用示例
    /// </summary>
    public class CollectionUtilsExample : MonoBehaviour
    {
        [Header("批处理")]
        [SerializeField] private Text _batchResultText;
        [SerializeField] private Button _processBatchButton;
        [SerializeField] private Slider _batchSizeSlider;
        [SerializeField] private Text _batchSizeText;
        [SerializeField] private Slider _itemCountSlider;
        [SerializeField] private Text _itemCountText;
        
        [Header("过滤和映射")]
        [SerializeField] private Text _filterMapResultText;
        [SerializeField] private Button _filterButton;
        [SerializeField] private Button _mapButton;
        [SerializeField] private Button _chainButton;
        [SerializeField] private Slider _minValueSlider;
        [SerializeField] private Text _minValueText;
        
        [Header("分页")]
        [SerializeField] private Text _paginationResultText;
        [SerializeField] private Button _prevPageButton;
        [SerializeField] private Button _nextPageButton;
        [SerializeField] private Text _pageInfoText;
        [SerializeField] private Slider _pageSizeSlider;
        [SerializeField] private Text _pageSizeText;
        
        [Header("对比")]
        [SerializeField] private Text _compareResultText;
        [SerializeField] private Button _compareButton;
        [SerializeField] private Button _findDifferencesButton;
        [SerializeField] private ToggleGroup _compareOptionsGroup;
        
        [Header("洗牌")]
        [SerializeField] private Text _shuffleResultText;
        [SerializeField] private Button _shuffleButton;
        [SerializeField] private Button _shuffleAndTakeButton;
        [SerializeField] private Slider _takeCountSlider;
        [SerializeField] private Text _takeCountText;
        
        // 示例数据
        private List<UserData> _userData;
        private List<int> _firstList;
        private List<int> _secondList;
        
        // 分页状态
        private int _currentPage = 1;
        private int _totalItems = 100;
        
        [System.Serializable]
        private class UserData
        {
            public int Id;
            public string Name;
            public int Age;
            public bool IsActive;
            
            public override string ToString()
            {
                return $"ID: {Id}, 姓名: {Name}, 年龄: {Age}, 状态: {(IsActive ? "活跃" : "非活跃")}";
            }
        }

        private void Start()
        {
            // 初始化示例数据
            GenerateUserData();
            GenerateCompareLists();
            
            // 初始化UI事件监听
            SetupUIEvents();
            
            // 初始化UI显示
            UpdateBatchSizeText(_batchSizeSlider.value);
            UpdateItemCountText(_itemCountSlider.value);
            UpdateMinValueText(_minValueSlider.value);
            UpdatePageSizeText(_pageSizeSlider.value);
            UpdateTakeCountText(_takeCountSlider.value);
            UpdatePageInfo();
        }
        
        private void SetupUIEvents()
        {
            if (_processBatchButton != null)
                _processBatchButton.onClick.AddListener(ProcessBatch);
                
            if (_filterButton != null)
                _filterButton.onClick.AddListener(FilterItems);
                
            if (_mapButton != null)
                _mapButton.onClick.AddListener(MapItems);
                
            if (_chainButton != null)
                _chainButton.onClick.AddListener(ChainOperations);
                
            if (_prevPageButton != null)
                _prevPageButton.onClick.AddListener(PreviousPage);
                
            if (_nextPageButton != null)
                _nextPageButton.onClick.AddListener(NextPage);
                
            if (_compareButton != null)
                _compareButton.onClick.AddListener(CompareCollections);
                
            if (_findDifferencesButton != null)
                _findDifferencesButton.onClick.AddListener(FindDifferences);
                
            if (_shuffleButton != null)
                _shuffleButton.onClick.AddListener(ShuffleCollection);
                
            if (_shuffleAndTakeButton != null)
                _shuffleAndTakeButton.onClick.AddListener(ShuffleAndTake);
                
            // 设置滑块事件
            if (_batchSizeSlider != null)
            {
                _batchSizeSlider.onValueChanged.AddListener(UpdateBatchSizeText);
            }
            
            if (_itemCountSlider != null)
            {
                _itemCountSlider.onValueChanged.AddListener(UpdateItemCountText);
            }
            
            if (_minValueSlider != null)
            {
                _minValueSlider.onValueChanged.AddListener(UpdateMinValueText);
            }
            
            if (_pageSizeSlider != null)
            {
                _pageSizeSlider.onValueChanged.AddListener((value) =>
                {
                    UpdatePageSizeText(value);
                    _currentPage = 1; // 重置页码
                    UpdatePageInfo();
                });
            }
            
            if (_takeCountSlider != null)
            {
                _takeCountSlider.onValueChanged.AddListener(UpdateTakeCountText);
            }
        }
        
        private void GenerateUserData()
        {
            string[] names = { "张三", "李四", "王五", "赵六", "钱七", "孙八", "周九", "吴十", 
                              "郑十一", "王十二", "冯十三", "陈十四", "楚十五", "魏十六", "蒋十七" };
            
            _userData = new List<UserData>();
            
            for (int i = 0; i < names.Length; i++)
            {
                _userData.Add(new UserData
                {
                    Id = i + 1000,
                    Name = names[i],
                    Age = 20 + i,
                    IsActive = (i % 2 == 0)
                });
            }
        }
        
        private void GenerateCompareLists()
        {
            _firstList = new List<int> { 1, 2, 3, 5, 7, 9 };
            _secondList = new List<int> { 2, 3, 4, 5, 8 };
        }
        
        private void UpdateBatchSizeText(float value)
        {
            int batchSize = Mathf.RoundToInt(value);
            if (_batchSizeText != null)
                _batchSizeText.text = batchSize.ToString();
        }
        
        private void UpdateItemCountText(float value)
        {
            int itemCount = Mathf.RoundToInt(value);
            if (_itemCountText != null)
                _itemCountText.text = itemCount.ToString();
            
            _totalItems = itemCount;
        }
        
        private void UpdateMinValueText(float value)
        {
            int minValue = Mathf.RoundToInt(value);
            if (_minValueText != null)
                _minValueText.text = minValue.ToString();
        }
        
        private void UpdatePageSizeText(float value)
        {
            int pageSize = Mathf.RoundToInt(value);
            if (_pageSizeText != null)
                _pageSizeText.text = pageSize.ToString();
        }
        
        private void UpdateTakeCountText(float value)
        {
            int takeCount = Mathf.RoundToInt(value);
            if (_takeCountText != null)
                _takeCountText.text = takeCount.ToString();
        }
        
        private void ProcessBatch()
        {
            int batchSize = Mathf.RoundToInt(_batchSizeSlider.value);
            int itemCount = Mathf.RoundToInt(_itemCountSlider.value);
            
            // 生成测试数据
            List<int> items = new List<int>(itemCount);
            for (int i = 0; i < itemCount; i++)
            {
                items.Add(i);
            }
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"批处理 {itemCount} 个项目，批大小: {batchSize}");
            sb.AppendLine();
            
            int batchCount = 0;
            
            // 使用CollectionUtils批处理
            CollectionUtils.BatchProcess(items, batchSize, batch =>
            {
                batchCount++;
                sb.AppendLine($"批次 {batchCount}: {batch.Count} 个项目");
                if (batchCount <= 3) // 仅显示前几批
                {
                    sb.AppendLine($"  内容: {string.Join(", ", batch.Take(10))}" + 
                                 (batch.Count > 10 ? "..." : ""));
                }
            });
            
            sb.AppendLine();
            sb.AppendLine($"总共处理了 {batchCount} 个批次");
            
            if (_batchResultText != null)
                _batchResultText.text = sb.ToString();
        }
        
        private void FilterItems()
        {
            int minValue = Mathf.RoundToInt(_minValueSlider.value);
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"过滤年龄 >= {minValue} 的用户");
            sb.AppendLine();
            
            // 使用CollectionUtils过滤
            var filtered = CollectionUtils.Filter(_userData, user => user.Age >= minValue);
            
            foreach (var user in filtered)
            {
                sb.AppendLine(user.ToString());
            }
            
            sb.AppendLine();
            sb.AppendLine($"结果: 找到 {filtered.Count()} 个符合条件的用户");
            
            if (_filterMapResultText != null)
                _filterMapResultText.text = sb.ToString();
        }
        
        private void MapItems()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("映射用户为简化表示");
            sb.AppendLine();
            
            // 使用CollectionUtils映射
            var mapped = CollectionUtils.Map(_userData, user => 
                new { ID = user.Id, 用户名 = user.Name, 状态 = user.IsActive ? "活跃" : "非活跃" });
            
            foreach (var item in mapped)
            {
                sb.AppendLine($"ID: {item.ID}, 用户名: {item.用户名}, 状态: {item.状态}");
            }
            
            if (_filterMapResultText != null)
                _filterMapResultText.text = sb.ToString();
        }
        
        private void ChainOperations()
        {
            int minValue = Mathf.RoundToInt(_minValueSlider.value);
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"链式操作: 过滤 -> 映射 -> 排序");
            sb.AppendLine($"1. 过滤年龄 >= {minValue} 的用户");
            sb.AppendLine($"2. 映射为简化表示");
            sb.AppendLine($"3. 按姓名排序");
            sb.AppendLine();
            
            // 使用CollectionUtils链式操作
            var result = CollectionUtils.Filter(_userData, user => user.Age >= minValue)
                .Map(user => new { ID = user.Id, 用户名 = user.Name, 年龄 = user.Age })
                .OrderBy(user => user.用户名);
            
            foreach (var item in result)
            {
                sb.AppendLine($"ID: {item.ID}, 用户名: {item.用户名}, 年龄: {item.年龄}");
            }
            
            sb.AppendLine();
            sb.AppendLine($"结果: 处理了 {result.Count()} 个用户");
            
            if (_filterMapResultText != null)
                _filterMapResultText.text = sb.ToString();
        }
        
        private void UpdatePageInfo()
        {
            int pageSize = Mathf.RoundToInt(_pageSizeSlider.value);
            int totalPages = Mathf.CeilToInt((float)_totalItems / pageSize);
            
            // 确保当前页在有效范围内
            _currentPage = Mathf.Clamp(_currentPage, 1, totalPages);
            
            if (_pageInfoText != null)
                _pageInfoText.text = $"第 {_currentPage} 页，共 {totalPages} 页";
                
            // 更新分页结果
            ShowPaginationResult();
            
            // 更新按钮状态
            if (_prevPageButton != null)
                _prevPageButton.interactable = (_currentPage > 1);
                
            if (_nextPageButton != null)
                _nextPageButton.interactable = (_currentPage < totalPages);
        }
        
        private void PreviousPage()
        {
            _currentPage--;
            UpdatePageInfo();
        }
        
        private void NextPage()
        {
            _currentPage++;
            UpdatePageInfo();
        }
        
        private void ShowPaginationResult()
        {
            int pageSize = Mathf.RoundToInt(_pageSizeSlider.value);
            
            // 生成测试数据
            List<int> allItems = new List<int>(_totalItems);
            for (int i = 0; i < _totalItems; i++)
            {
                allItems.Add(i + 1);
            }
            
            // 使用CollectionUtils分页
            var pagedItems = CollectionUtils.Paginate(allItems, _currentPage, pageSize);
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"分页: 每页 {pageSize} 个项目，总共 {_totalItems} 个");
            sb.AppendLine();
            sb.AppendLine($"当前页 ({_currentPage}) 内容:");
            
            foreach (var item in pagedItems)
            {
                sb.AppendLine($"  项目: {item}");
            }
            
            if (_paginationResultText != null)
                _paginationResultText.text = sb.ToString();
        }
        
        private void CompareCollections()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("比较两个集合是否相等");
            sb.AppendLine();
            sb.AppendLine($"第一个集合: {string.Join(", ", _firstList)}");
            sb.AppendLine($"第二个集合: {string.Join(", ", _secondList)}");
            sb.AppendLine();
            
            // 获取选中的比较选项
            bool ignoreOrder = GetSelectedCompareOption() == "ignore_order";
            
            // 使用CollectionUtils比较
            bool areEqual = CollectionUtils.Compare(_firstList, _secondList, ignoreOrder);
            
            sb.AppendLine($"比较选项: {(ignoreOrder ? "忽略顺序" : "考虑顺序")}");
            sb.AppendLine($"结果: 集合 {(areEqual ? "相等" : "不相等")}");
            
            if (_compareResultText != null)
                _compareResultText.text = sb.ToString();
        }
        
        private void FindDifferences()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("查找两个集合的差异");
            sb.AppendLine();
            sb.AppendLine($"第一个集合: {string.Join(", ", _firstList)}");
            sb.AppendLine($"第二个集合: {string.Join(", ", _secondList)}");
            sb.AppendLine();
            
            // 使用CollectionUtils找出差异
            var diff = CollectionUtils.FindDifferences(_firstList, _secondList);
            
            sb.AppendLine("差异分析:");
            sb.AppendLine($"  仅在第一个集合中: {string.Join(", ", diff.OnlyInFirst)}");
            sb.AppendLine($"  仅在第二个集合中: {string.Join(", ", diff.OnlyInSecond)}");
            sb.AppendLine($"  两个集合都有: {string.Join(", ", diff.InBoth)}");
            
            if (_compareResultText != null)
                _compareResultText.text = sb.ToString();
        }
        
        private string GetSelectedCompareOption()
        {
            if (_compareOptionsGroup == null)
                return "consider_order";
                
            foreach (Toggle toggle in _compareOptionsGroup.ActiveToggles())
            {
                return toggle.gameObject.name;
            }
            
            return "consider_order";
        }
        
        private void ShuffleCollection()
        {
            // 创建待洗牌的集合（使用用户数据）
            List<UserData> shuffleList = new List<UserData>(_userData);
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("洗牌前:");
            
            for (int i = 0; i < Mathf.Min(shuffleList.Count, 5); i++)
            {
                sb.AppendLine($"  {i+1}. {shuffleList[i].Name}");
            }
            
            // 使用CollectionUtils洗牌
            CollectionUtils.Shuffle(shuffleList);
            
            sb.AppendLine();
            sb.AppendLine("洗牌后:");
            
            for (int i = 0; i < Mathf.Min(shuffleList.Count, 5); i++)
            {
                sb.AppendLine($"  {i+1}. {shuffleList[i].Name}");
            }
            
            if (_shuffleResultText != null)
                _shuffleResultText.text = sb.ToString();
        }
        
        private void ShuffleAndTake()
        {
            int takeCount = Mathf.RoundToInt(_takeCountSlider.value);
            takeCount = Mathf.Min(takeCount, _userData.Count);
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"随机抽取 {takeCount} 个用户:");
            sb.AppendLine();
            
            // 洗牌并获取前几个元素
            List<UserData> shuffled = new List<UserData>(_userData);
            CollectionUtils.Shuffle(shuffled);
            var selected = shuffled.Take(takeCount).ToList();
            
            for (int i = 0; i < selected.Count; i++)
            {
                sb.AppendLine($"{i+1}. {selected[i].Name} (ID: {selected[i].Id})");
            }
            
            if (_shuffleResultText != null)
                _shuffleResultText.text = sb.ToString();
        }
    }
} 