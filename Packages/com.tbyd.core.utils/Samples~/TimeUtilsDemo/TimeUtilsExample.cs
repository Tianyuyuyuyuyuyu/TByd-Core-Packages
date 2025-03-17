using UnityEngine;
using UnityEngine.UI;
using TByd.Core.Utils.Runtime;
using System;
using System.Collections;

namespace TByd.Core.Utils.Samples
{
    /// <summary>
    /// 展示TimeUtils类的使用示例
    /// </summary>
    public class TimeUtilsExample : MonoBehaviour
    {
        [Header("时间格式化")]
        [SerializeField] private Text _currentTimeText;
        [SerializeField] private Text _formattedTimeText;
        [SerializeField] private Dropdown _formatDropdown;
        
        [Header("相对时间")]
        [SerializeField] private Text _relativeTimeText;
        [SerializeField] private Slider _timeOffsetSlider;
        [SerializeField] private Text _timeOffsetText;
        
        [Header("游戏时间系统")]
        [SerializeField] private Text _gameTimeText;
        [SerializeField] private Slider _timeScaleSlider;
        [SerializeField] private Text _timeScaleText;
        [SerializeField] private Button _pauseResumeButton;
        
        [Header("计时器")]
        [SerializeField] private Text _timerText;
        [SerializeField] private Button _startTimerButton;
        [SerializeField] private Button _stopTimerButton;
        [SerializeField] private Button _resetTimerButton;
        [SerializeField] private Slider _timerDurationSlider;
        [SerializeField] private Text _timerDurationText;
        
        [Header("性能测量")]
        [SerializeField] private Text _performanceResultText;
        [SerializeField] private Button _measurePerformanceButton;
        [SerializeField] private Slider _iterationsSlider;
        [SerializeField] private Text _iterationsText;
        
        // 示例时间格式
        private string[] _timeFormats = {
            "yyyy-MM-dd HH:mm:ss",
            "MM/dd/yyyy hh:mm tt",
            "dddd, MMMM d, yyyy",
            "HH:mm:ss.fff",
            "yyyy年MM月dd日 HH时mm分ss秒"
        };
        
        private bool _gameTimePaused = false;
        private bool _timerRunning = false;
        private float _elapsedTime = 0f;
        private Coroutine _timerCoroutine;

        private void Start()
        {
            // 初始化UI事件监听
            SetupUIEvents();
            
            // 初始化下拉框选项
            InitializeFormatDropdown();
            
            // 初始化时间系统
            TimeUtils.InitializeGameTime();
            
            // 启动时间更新
            StartCoroutine(UpdateTimeDisplay());
        }
        
        private void SetupUIEvents()
        {
            if (_pauseResumeButton != null)
                _pauseResumeButton.onClick.AddListener(ToggleGameTimePause);
                
            if (_startTimerButton != null)
                _startTimerButton.onClick.AddListener(StartTimer);
                
            if (_stopTimerButton != null)
                _stopTimerButton.onClick.AddListener(StopTimer);
                
            if (_resetTimerButton != null)
                _resetTimerButton.onClick.AddListener(ResetTimer);
                
            if (_measurePerformanceButton != null)
                _measurePerformanceButton.onClick.AddListener(MeasurePerformance);
                
            // 设置滑块事件
            if (_timeOffsetSlider != null)
            {
                _timeOffsetSlider.onValueChanged.AddListener(OnTimeOffsetChanged);
                OnTimeOffsetChanged(_timeOffsetSlider.value);
            }
            
            if (_timeScaleSlider != null)
            {
                _timeScaleSlider.onValueChanged.AddListener(OnTimeScaleChanged);
                OnTimeScaleChanged(_timeScaleSlider.value);
            }
            
            if (_timerDurationSlider != null)
            {
                _timerDurationSlider.onValueChanged.AddListener(OnTimerDurationChanged);
                OnTimerDurationChanged(_timerDurationSlider.value);
            }
            
            if (_iterationsSlider != null)
            {
                _iterationsSlider.onValueChanged.AddListener(OnIterationsChanged);
                OnIterationsChanged(_iterationsSlider.value);
            }
            
            if (_formatDropdown != null)
            {
                _formatDropdown.onValueChanged.AddListener(OnFormatChanged);
            }
        }
        
        private void InitializeFormatDropdown()
        {
            if (_formatDropdown != null)
            {
                _formatDropdown.ClearOptions();
                _formatDropdown.AddOptions(new System.Collections.Generic.List<string>(_timeFormats));
            }
        }
        
        private IEnumerator UpdateTimeDisplay()
        {
            while (true)
            {
                UpdateCurrentTime();
                UpdateFormattedTime();
                UpdateRelativeTime();
                UpdateGameTime();
                
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        private void UpdateCurrentTime()
        {
            if (_currentTimeText != null)
            {
                _currentTimeText.text = DateTime.Now.ToString();
            }
        }
        
        private void UpdateFormattedTime()
        {
            if (_formattedTimeText != null && _formatDropdown != null)
            {
                int formatIndex = _formatDropdown.value;
                if (formatIndex >= 0 && formatIndex < _timeFormats.Length)
                {
                    string format = _timeFormats[formatIndex];
                    string formattedTime = TimeUtils.FormatDateTime(DateTime.Now, format);
                    _formattedTimeText.text = formattedTime;
                }
            }
        }
        
        private void UpdateRelativeTime()
        {
            if (_relativeTimeText != null && _timeOffsetSlider != null)
            {
                float offsetDays = _timeOffsetSlider.value;
                DateTime offsetTime = DateTime.Now.AddDays(offsetDays);
                string relativeTime = TimeUtils.GetRelativeTimeDescription(offsetTime);
                _relativeTimeText.text = relativeTime;
            }
        }
        
        private void UpdateGameTime()
        {
            if (_gameTimeText != null)
            {
                // 更新游戏时间
                TimeUtils.UpdateGameTime();
                DateTime gameTime = TimeUtils.GetCurrentGameTime();
                _gameTimeText.text = gameTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        
        private void OnTimeOffsetChanged(float value)
        {
            if (_timeOffsetText != null)
            {
                _timeOffsetText.text = value.ToString("F1") + " 天";
            }
        }
        
        private void OnTimeScaleChanged(float value)
        {
            if (_timeScaleText != null)
            {
                _timeScaleText.text = value.ToString("F1") + "x";
            }
            
            TimeUtils.SetGameTimeScale(value);
        }
        
        private void OnTimerDurationChanged(float value)
        {
            if (_timerDurationText != null)
            {
                _timerDurationText.text = value.ToString("F1") + " 秒";
            }
        }
        
        private void OnIterationsChanged(float value)
        {
            if (_iterationsText != null)
            {
                _iterationsText.text = Mathf.RoundToInt(value).ToString() + " 次";
            }
        }
        
        private void OnFormatChanged(int value)
        {
            UpdateFormattedTime();
        }
        
        private void ToggleGameTimePause()
        {
            _gameTimePaused = !_gameTimePaused;
            TimeUtils.SetGameTimePaused(_gameTimePaused);
            
            if (_pauseResumeButton != null)
            {
                _pauseResumeButton.GetComponentInChildren<Text>().text = _gameTimePaused ? "恢复" : "暂停";
            }
        }
        
        private void StartTimer()
        {
            if (_timerRunning) return;
            
            _timerRunning = true;
            
            if (_timerCoroutine != null)
                StopCoroutine(_timerCoroutine);
                
            _timerCoroutine = StartCoroutine(RunTimer());
        }
        
        private void StopTimer()
        {
            _timerRunning = false;
            
            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
                _timerCoroutine = null;
            }
        }
        
        private void ResetTimer()
        {
            StopTimer();
            _elapsedTime = 0f;
            
            if (_timerText != null)
            {
                _timerText.text = TimeUtils.FormatTimeSpan(TimeSpan.FromSeconds(_elapsedTime));
            }
        }
        
        private IEnumerator RunTimer()
        {
            float duration = _timerDurationSlider != null ? _timerDurationSlider.value : 10f;
            _elapsedTime = 0f;
            
            while (_timerRunning && _elapsedTime < duration)
            {
                _elapsedTime += Time.deltaTime;
                
                if (_timerText != null)
                {
                    _timerText.text = TimeUtils.FormatTimeSpan(TimeSpan.FromSeconds(_elapsedTime));
                }
                
                yield return null;
            }
            
            _timerRunning = false;
            
            if (_timerText != null && _elapsedTime >= duration)
            {
                _timerText.text = "计时结束!";
            }
        }
        
        private void MeasurePerformance()
        {
            int iterations = Mathf.RoundToInt(_iterationsSlider != null ? _iterationsSlider.value : 1000);
            
            // 使用TimeUtils测量性能
            float elapsedMs = TimeUtils.MeasureExecutionTime(() => 
            {
                for (int i = 0; i < iterations; i++)
                {
                    // 执行一些操作作为测试
                    string s = "";
                    for (int j = 0; j < 100; j++)
                    {
                        s += j.ToString();
                    }
                }
            });
            
            if (_performanceResultText != null)
            {
                _performanceResultText.text = string.Format(
                    "执行 {0} 次迭代\n耗时: {1:F2} 毫秒\n平均每次: {2:F5} 毫秒",
                    iterations,
                    elapsedMs,
                    elapsedMs / iterations
                );
            }
        }
    }
} 