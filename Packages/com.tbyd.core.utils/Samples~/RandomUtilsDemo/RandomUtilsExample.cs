using UnityEngine;
using UnityEngine.UI;
using TByd.Core.Utils.Runtime;
using System.Collections.Generic;

namespace TByd.Core.Utils.Samples
{
    /// <summary>
    /// 展示RandomUtils类的使用示例
    /// </summary>
    public class RandomUtilsExample : MonoBehaviour
    {
        [Header("随机整数")]
        [SerializeField] private Text _randomIntText;
        [SerializeField] private Button _generateIntButton;
        [SerializeField] private Slider _minIntSlider;
        [SerializeField] private Slider _maxIntSlider;
        [SerializeField] private Text _minIntText;
        [SerializeField] private Text _maxIntText;
        
        [Header("随机浮点数")]
        [SerializeField] private Text _randomFloatText;
        [SerializeField] private Button _generateFloatButton;
        [SerializeField] private Slider _minFloatSlider;
        [SerializeField] private Slider _maxFloatSlider;
        [SerializeField] private Text _minFloatText;
        [SerializeField] private Text _maxFloatText;
        
        [Header("随机颜色")]
        [SerializeField] private Image _randomColorImage;
        [SerializeField] private Button _generateColorButton;
        [SerializeField] private Toggle _useHSVToggle;
        
        [Header("带权重随机")]
        [SerializeField] private Text _weightedRandomText;
        [SerializeField] private Button _generateWeightedRandomButton;
        [SerializeField] private Slider[] _weightSliders;
        [SerializeField] private Text[] _weightTexts;
        [SerializeField] private Text[] _itemTexts;
        
        [Header("随机元素")]
        [SerializeField] private Text _randomElementText;
        [SerializeField] private Button _pickRandomElementButton;
        [SerializeField] private Button _shuffleButton;
        [SerializeField] private Transform _elementsContainer;
        
        // 示例数据
        private string[] _items = { "苹果", "香蕉", "橙子", "葡萄", "西瓜" };
        private List<GameObject> _elements = new List<GameObject>();

        private void Start()
        {
            // 初始化UI事件监听
            if (_generateIntButton != null)
                _generateIntButton.onClick.AddListener(GenerateRandomInt);
                
            if (_generateFloatButton != null)
                _generateFloatButton.onClick.AddListener(GenerateRandomFloat);
                
            if (_generateColorButton != null)
                _generateColorButton.onClick.AddListener(GenerateRandomColor);
                
            if (_generateWeightedRandomButton != null)
                _generateWeightedRandomButton.onClick.AddListener(GenerateWeightedRandom);
                
            if (_pickRandomElementButton != null)
                _pickRandomElementButton.onClick.AddListener(PickRandomElement);
                
            if (_shuffleButton != null)
                _shuffleButton.onClick.AddListener(ShuffleElements);
                
            // 初始化滑块事件
            SetupSliders();
            
            // 初始化元素
            InitializeElements();
        }

        private void SetupSliders()
        {
            if (_minIntSlider != null)
            {
                _minIntSlider.onValueChanged.AddListener((value) => 
                {
                    UpdateIntSliderValues();
                });
            }
            
            if (_maxIntSlider != null)
            {
                _maxIntSlider.onValueChanged.AddListener((value) => 
                {
                    UpdateIntSliderValues();
                });
            }
            
            if (_minFloatSlider != null)
            {
                _minFloatSlider.onValueChanged.AddListener((value) => 
                {
                    UpdateFloatSliderValues();
                });
            }
            
            if (_maxFloatSlider != null)
            {
                _maxFloatSlider.onValueChanged.AddListener((value) => 
                {
                    UpdateFloatSliderValues();
                });
            }
            
            // 设置权重滑块
            if (_weightSliders != null && _weightTexts != null)
            {
                for (int i = 0; i < _weightSliders.Length && i < _weightTexts.Length; i++)
                {
                    int index = i; // 捕获变量
                    if (_weightSliders[i] != null)
                    {
                        _weightSliders[i].onValueChanged.AddListener((value) => 
                        {
                            UpdateWeightText(index, value);
                        });
                        UpdateWeightText(i, _weightSliders[i].value);
                    }
                }
            }
            
            // 初始更新滑块值
            UpdateIntSliderValues();
            UpdateFloatSliderValues();
        }
        
        private void UpdateIntSliderValues()
        {
            int minValue = Mathf.RoundToInt(_minIntSlider.value);
            int maxValue = Mathf.RoundToInt(_maxIntSlider.value);
            
            // 确保最小值不大于最大值
            if (minValue > maxValue)
            {
                minValue = maxValue;
                _minIntSlider.value = minValue;
            }
            
            if (_minIntText != null)
                _minIntText.text = minValue.ToString();
                
            if (_maxIntText != null)
                _maxIntText.text = maxValue.ToString();
        }
        
        private void UpdateFloatSliderValues()
        {
            float minValue = _minFloatSlider.value;
            float maxValue = _maxFloatSlider.value;
            
            // 确保最小值不大于最大值
            if (minValue > maxValue)
            {
                minValue = maxValue;
                _minFloatSlider.value = minValue;
            }
            
            if (_minFloatText != null)
                _minFloatText.text = minValue.ToString("F2");
                
            if (_maxFloatText != null)
                _maxFloatText.text = maxValue.ToString("F2");
        }
        
        private void UpdateWeightText(int index, float value)
        {
            if (_weightTexts != null && index < _weightTexts.Length && _weightTexts[index] != null)
            {
                _weightTexts[index].text = value.ToString("F1");
            }
        }
        
        private void InitializeElements()
        {
            if (_elementsContainer != null && _itemTexts != null)
            {
                _elements.Clear();
                
                for (int i = 0; i < _itemTexts.Length; i++)
                {
                    if (_itemTexts[i] != null)
                    {
                        _itemTexts[i].text = _items[i % _items.Length];
                        _elements.Add(_itemTexts[i].gameObject);
                    }
                }
            }
        }
        
        private void GenerateRandomInt()
        {
            int min = Mathf.RoundToInt(_minIntSlider.value);
            int max = Mathf.RoundToInt(_maxIntSlider.value);
            
            int randomInt = RandomUtils.Range(min, max + 1); // +1 因为Range是max排除的
            
            if (_randomIntText != null)
                _randomIntText.text = randomInt.ToString();
        }
        
        private void GenerateRandomFloat()
        {
            float min = _minFloatSlider.value;
            float max = _maxFloatSlider.value;
            
            float randomFloat = RandomUtils.Range(min, max);
            
            if (_randomFloatText != null)
                _randomFloatText.text = randomFloat.ToString("F4");
        }
        
        private void GenerateRandomColor()
        {
            Color randomColor;
            
            if (_useHSVToggle != null && _useHSVToggle.isOn)
            {
                // 使用HSV生成颜色
                randomColor = RandomUtils.ColorHSV(0f, 1f, 0.7f, 1f, 0.7f, 1f);
            }
            else
            {
                // 使用RGB生成颜色
                randomColor = new Color(
                    RandomUtils.Range(0f, 1f),
                    RandomUtils.Range(0f, 1f),
                    RandomUtils.Range(0f, 1f)
                );
            }
            
            if (_randomColorImage != null)
                _randomColorImage.color = randomColor;
        }
        
        private void GenerateWeightedRandom()
        {
            if (_weightSliders == null || _weightSliders.Length < _items.Length) 
                return;
                
            // 收集权重
            float[] weights = new float[_items.Length];
            for (int i = 0; i < _items.Length; i++)
            {
                weights[i] = i < _weightSliders.Length ? _weightSliders[i].value : 1f;
            }
            
            // 使用权重随机
            string randomItem = RandomUtils.WeightedRandom(_items, weights);
            
            if (_weightedRandomText != null)
                _weightedRandomText.text = randomItem;
        }
        
        private void PickRandomElement()
        {
            if (_elements.Count == 0) return;
            
            // 随机选择元素
            GameObject randomElement = RandomUtils.GetRandom(_elements);
            
            // 高亮显示随机选中的元素
            foreach (var element in _elements)
            {
                if (element.GetComponent<Image>() != null)
                {
                    element.GetComponent<Image>().color = (element == randomElement) 
                        ? Color.green 
                        : Color.white;
                }
            }
            
            if (_randomElementText != null)
                _randomElementText.text = randomElement.GetComponentInChildren<Text>()?.text ?? "未知";
        }
        
        private void ShuffleElements()
        {
            if (_elements.Count == 0) return;
            
            // 洗牌元素
            List<GameObject> shuffled = new List<GameObject>(_elements);
            RandomUtils.Shuffle(shuffled);
            
            // 重新排列UI
            for (int i = 0; i < shuffled.Count; i++)
            {
                if (i < _elementsContainer.childCount)
                {
                    shuffled[i].transform.SetSiblingIndex(i);
                }
            }
            
            // 更新元素列表
            _elements = shuffled;
            
            // 更新文本显示
            if (_randomElementText != null)
                _randomElementText.text = "已洗牌";
        }
    }
} 