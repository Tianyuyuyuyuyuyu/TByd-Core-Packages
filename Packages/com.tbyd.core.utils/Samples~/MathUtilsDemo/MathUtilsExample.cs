using UnityEngine;
using UnityEngine.UI;
using TByd.Core.Utils.Runtime;
using System;
using System.Collections.Generic;

namespace TByd.Core.Utils.Samples
{
    /// <summary>
    /// 展示MathUtils类的功能和用法
    /// </summary>
    public class MathUtilsExample : MonoBehaviour
    {
        [Header("角度与弧度转换")]
        [SerializeField] private InputField _angleInputField;
        [SerializeField] private Text _radianResultText;
        [SerializeField] private Button _toRadiansButton;
        [SerializeField] private InputField _radianInputField;
        [SerializeField] private Text _angleResultText;
        [SerializeField] private Button _toDegreesButton;
        
        [Header("数值操作")]
        [SerializeField] private InputField _value1InputField;
        [SerializeField] private InputField _value2InputField;
        [SerializeField] private Text _numberResultText;
        [SerializeField] private Button _clampButton;
        [SerializeField] private Button _wrapButton;
        [SerializeField] private Button _remapButton;
        
        [Header("插值")]
        [SerializeField] private Slider _lerpSlider;
        [SerializeField] private Text _lerpValueText;
        [SerializeField] private Text _lerpResultText;
        [SerializeField] private Dropdown _lerpTypeDropdown;
        [SerializeField] private RawImage _colorDisplay;
        
        [Header("几何计算")]
        [SerializeField] private InputField _geometryX1Input;
        [SerializeField] private InputField _geometryY1Input;
        [SerializeField] private InputField _geometryX2Input;
        [SerializeField] private InputField _geometryY2Input;
        [SerializeField] private Text _geometryResultText;
        [SerializeField] private Button _distanceButton;
        [SerializeField] private Button _angleButton;
        [SerializeField] private Button _rotatePointButton;
        
        [Header("2D网格计算")]
        [SerializeField] private InputField _gridWidthInput;
        [SerializeField] private InputField _gridHeightInput;
        [SerializeField] private InputField _gridIndexInput;
        [SerializeField] private Text _gridResultText;
        [SerializeField] private Button _indexToPositionButton;
        [SerializeField] private Button _positionToIndexButton;
        
        private enum LerpType
        {
            Linear,
            EaseIn,
            EaseOut,
            EaseInOut,
            Spring,
            Bounce,
            ColorLerp
        }

        private void Start()
        {
            // 初始化下拉菜单
            SetupDropdown();
            
            // 设置UI事件监听
            SetupUIEvents();
            
            // 设置初始值
            SetupInitialValues();
            
            // 更新初始插值显示
            UpdateLerpDisplay(_lerpSlider.value);
        }
        
        private void SetupDropdown()
        {
            if (_lerpTypeDropdown != null)
            {
                _lerpTypeDropdown.ClearOptions();
                _lerpTypeDropdown.AddOptions(new List<string>
                {
                    "线性插值",
                    "缓入插值",
                    "缓出插值",
                    "缓入缓出插值",
                    "弹簧插值",
                    "弹跳插值",
                    "颜色插值"
                });
                
                _lerpTypeDropdown.onValueChanged.AddListener(value => UpdateLerpDisplay(_lerpSlider.value));
            }
        }
        
        private void SetupUIEvents()
        {
            // 角度与弧度转换
            if (_toRadiansButton != null)
                _toRadiansButton.onClick.AddListener(ConvertToRadians);
                
            if (_toDegreesButton != null)
                _toDegreesButton.onClick.AddListener(ConvertToDegrees);
                
            // 数值操作
            if (_clampButton != null)
                _clampButton.onClick.AddListener(ClampValue);
                
            if (_wrapButton != null)
                _wrapButton.onClick.AddListener(WrapValue);
                
            if (_remapButton != null)
                _remapButton.onClick.AddListener(RemapValue);
                
            // 插值
            if (_lerpSlider != null)
                _lerpSlider.onValueChanged.AddListener(UpdateLerpDisplay);
                
            // 几何计算
            if (_distanceButton != null)
                _distanceButton.onClick.AddListener(CalculateDistance);
                
            if (_angleButton != null)
                _angleButton.onClick.AddListener(CalculateAngle);
                
            if (_rotatePointButton != null)
                _rotatePointButton.onClick.AddListener(RotatePoint);
                
            // 2D网格计算
            if (_indexToPositionButton != null)
                _indexToPositionButton.onClick.AddListener(IndexToPosition);
                
            if (_positionToIndexButton != null)
                _positionToIndexButton.onClick.AddListener(PositionToIndex);
        }
        
        private void SetupInitialValues()
        {
            // 角度与弧度初始值
            if (_angleInputField != null)
                _angleInputField.text = "45";
                
            if (_radianInputField != null)
                _radianInputField.text = "1.5708";
                
            // 数值操作初始值
            if (_value1InputField != null)
                _value1InputField.text = "5";
                
            if (_value2InputField != null)
                _value2InputField.text = "10";
                
            // 几何计算初始值
            if (_geometryX1Input != null)
                _geometryX1Input.text = "0";
                
            if (_geometryY1Input != null)
                _geometryY1Input.text = "0";
                
            if (_geometryX2Input != null)
                _geometryX2Input.text = "3";
                
            if (_geometryY2Input != null)
                _geometryY2Input.text = "4";
                
            // 2D网格计算初始值
            if (_gridWidthInput != null)
                _gridWidthInput.text = "10";
                
            if (_gridHeightInput != null)
                _gridHeightInput.text = "10";
                
            if (_gridIndexInput != null)
                _gridIndexInput.text = "15";
        }
        
        #region 角度与弧度转换
        
        private void ConvertToRadians()
        {
            if (string.IsNullOrEmpty(_angleInputField.text))
                return;
                
            if (float.TryParse(_angleInputField.text, out float degrees))
            {
                float radians = MathUtils.DegreesToRadians(degrees);
                
                if (_radianResultText != null)
                    _radianResultText.text = $"{degrees}° = {radians:F4} 弧度";
            }
            else
            {
                if (_radianResultText != null)
                    _radianResultText.text = "请输入有效的角度值";
            }
        }
        
        private void ConvertToDegrees()
        {
            if (string.IsNullOrEmpty(_radianInputField.text))
                return;
                
            if (float.TryParse(_radianInputField.text, out float radians))
            {
                float degrees = MathUtils.RadiansToDegrees(radians);
                
                if (_angleResultText != null)
                    _angleResultText.text = $"{radians} 弧度 = {degrees:F2}°";
            }
            else
            {
                if (_angleResultText != null)
                    _angleResultText.text = "请输入有效的弧度值";
            }
        }
        
        #endregion
        
        #region 数值操作
        
        private void ClampValue()
        {
            if (string.IsNullOrEmpty(_value1InputField.text))
                return;
                
            if (float.TryParse(_value1InputField.text, out float value) && 
                float.TryParse(_value2InputField.text, out float limit))
            {
                float min = 0;
                float max = limit;
                
                if (limit < 0)
                {
                    min = limit;
                    max = 0;
                }
                
                float clampedValue = MathUtils.Clamp(value, min, max);
                
                if (_numberResultText != null)
                    _numberResultText.text = $"Clamp({value}, {min}, {max}) = {clampedValue}";
            }
            else
            {
                if (_numberResultText != null)
                    _numberResultText.text = "请输入有效的数值";
            }
        }
        
        private void WrapValue()
        {
            if (string.IsNullOrEmpty(_value1InputField.text) || string.IsNullOrEmpty(_value2InputField.text))
                return;
                
            if (float.TryParse(_value1InputField.text, out float value) && 
                float.TryParse(_value2InputField.text, out float max))
            {
                float min = 0;
                
                float wrappedValue = MathUtils.Wrap(value, min, max);
                
                if (_numberResultText != null)
                    _numberResultText.text = $"Wrap({value}, {min}, {max}) = {wrappedValue}";
            }
            else
            {
                if (_numberResultText != null)
                    _numberResultText.text = "请输入有效的数值";
            }
        }
        
        private void RemapValue()
        {
            if (string.IsNullOrEmpty(_value1InputField.text) || string.IsNullOrEmpty(_value2InputField.text))
                return;
                
            if (float.TryParse(_value1InputField.text, out float value) && 
                float.TryParse(_value2InputField.text, out float outputMax))
            {
                float inputMin = 0;
                float inputMax = 10;
                float outputMin = 0;
                
                float remappedValue = MathUtils.Remap(value, inputMin, inputMax, outputMin, outputMax);
                
                if (_numberResultText != null)
                    _numberResultText.text = $"Remap({value}, [{inputMin}, {inputMax}], [{outputMin}, {outputMax}]) = {remappedValue:F2}";
            }
            else
            {
                if (_numberResultText != null)
                    _numberResultText.text = "请输入有效的数值";
            }
        }
        
        #endregion
        
        #region 插值
        
        private void UpdateLerpDisplay(float t)
        {
            if (_lerpValueText != null)
                _lerpValueText.text = $"t = {t:F2}";
                
            LerpType lerpType = (LerpType)(_lerpTypeDropdown != null ? _lerpTypeDropdown.value : 0);
            
            float result = 0;
            
            switch (lerpType)
            {
                case LerpType.Linear:
                    result = MathUtils.Lerp(0, 1, t);
                    SetLerpResult(t, result, "Linear");
                    break;
                    
                case LerpType.EaseIn:
                    result = MathUtils.EaseIn(0, 1, t);
                    SetLerpResult(t, result, "EaseIn");
                    break;
                    
                case LerpType.EaseOut:
                    result = MathUtils.EaseOut(0, 1, t);
                    SetLerpResult(t, result, "EaseOut");
                    break;
                    
                case LerpType.EaseInOut:
                    result = MathUtils.EaseInOut(0, 1, t);
                    SetLerpResult(t, result, "EaseInOut");
                    break;
                    
                case LerpType.Spring:
                    result = MathUtils.Spring(0, 1, t);
                    SetLerpResult(t, result, "Spring");
                    break;
                    
                case LerpType.Bounce:
                    result = MathUtils.Bounce(0, 1, t);
                    SetLerpResult(t, result, "Bounce");
                    break;
                    
                case LerpType.ColorLerp:
                    ShowColorLerp(t);
                    break;
            }
        }
        
        private void SetLerpResult(float t, float result, string methodName)
        {
            string text = $"{methodName}(0, 1, {t:F2}) = {result:F4}";
            if (_lerpResultText != null)
                _lerpResultText.text = text;
                
            // 更新颜色显示
            if (_colorDisplay != null)
            {
                _colorDisplay.color = new Color(result, result, result);
            }
        }
        
        private void ShowColorLerp(float t)
        {
            Color colorA = Color.blue;
            Color colorB = Color.red;
            
            Color result = MathUtils.Lerp(colorA, colorB, t);
            
            if (_lerpResultText != null)
                _lerpResultText.text = $"Lerp(蓝色, 红色, {t:F2}) = RGB({result.r:F2}, {result.g:F2}, {result.b:F2})";
                
            // 更新颜色显示
            if (_colorDisplay != null)
            {
                _colorDisplay.color = result;
            }
        }
        
        #endregion
        
        #region 几何计算
        
        private void CalculateDistance()
        {
            if (TryGetPoints(out Vector2 pointA, out Vector2 pointB))
            {
                float distance = MathUtils.Distance(pointA, pointB);
                
                if (_geometryResultText != null)
                    _geometryResultText.text = $"点 {PointToString(pointA)} 和点 {PointToString(pointB)} 之间的距离是: {distance:F2}";
            }
        }
        
        private void CalculateAngle()
        {
            if (TryGetPoints(out Vector2 pointA, out Vector2 pointB))
            {
                // 将点B视为向量（相对于原点）
                float angle = MathUtils.AngleBetween(pointA, pointB);
                
                if (_geometryResultText != null)
                    _geometryResultText.text = $"向量 {PointToString(pointA)} 和向量 {PointToString(pointB)} 之间的角度是: {angle:F2}°";
            }
        }
        
        private void RotatePoint()
        {
            if (TryGetPoints(out Vector2 point, out Vector2 center))
            {
                // 使用Y坐标作为旋转角度
                float angle = center.y;
                
                Vector2 rotated = MathUtils.RotatePoint(point, Vector2.zero, MathUtils.DegreesToRadians(angle));
                
                if (_geometryResultText != null)
                    _geometryResultText.text = $"点 {PointToString(point)} 绕原点旋转 {angle:F2}° 后的位置是: {PointToString(rotated)}";
            }
        }
        
        private bool TryGetPoints(out Vector2 pointA, out Vector2 pointB)
        {
            pointA = Vector2.zero;
            pointB = Vector2.zero;
            
            bool success = true;
            
            if (!float.TryParse(_geometryX1Input.text, out float x1))
            {
                SetGeometryResult("第一个点的X坐标无效");
                success = false;
            }
            
            if (!float.TryParse(_geometryY1Input.text, out float y1))
            {
                SetGeometryResult("第一个点的Y坐标无效");
                success = false;
            }
            
            if (!float.TryParse(_geometryX2Input.text, out float x2))
            {
                SetGeometryResult("第二个点的X坐标无效");
                success = false;
            }
            
            if (!float.TryParse(_geometryY2Input.text, out float y2))
            {
                SetGeometryResult("第二个点的Y坐标无效");
                success = false;
            }
            
            if (success)
            {
                pointA = new Vector2(x1, y1);
                pointB = new Vector2(x2, y2);
            }
            
            return success;
        }
        
        private string PointToString(Vector2 point)
        {
            return $"({point.x:F2}, {point.y:F2})";
        }
        
        private void SetGeometryResult(string message)
        {
            if (_geometryResultText != null)
                _geometryResultText.text = message;
        }
        
        #endregion
        
        #region 2D网格计算
        
        private void IndexToPosition()
        {
            if (!TryGetGridParameters(out int width, out int height, out int index))
                return;
                
            // 计算从索引到位置的转换
            Vector2Int position = MathUtils.IndexToPosition(index, width);
            
            if (_gridResultText != null)
                _gridResultText.text = $"在 {width}x{height} 的网格中，索引 {index} 对应的位置是: ({position.x}, {position.y})";
        }
        
        private void PositionToIndex()
        {
            if (!TryGetGridParameters(out int width, out int height, out _))
                return;
                
            // 使用X输入和Y输入作为位置
            if (!int.TryParse(_geometryX1Input.text, out int x) || !int.TryParse(_geometryY1Input.text, out int y))
            {
                if (_gridResultText != null)
                    _gridResultText.text = "请在几何计算的第一个点输入框中输入有效的网格位置坐标";
                return;
            }
            
            // 计算从位置到索引的转换
            int index = MathUtils.PositionToIndex(new Vector2Int(x, y), width);
            
            if (_gridResultText != null)
                _gridResultText.text = $"在 {width}x{height} 的网格中，位置 ({x}, {y}) 对应的索引是: {index}";
        }
        
        private bool TryGetGridParameters(out int width, out int height, out int index)
        {
            width = 0;
            height = 0;
            index = 0;
            
            if (!int.TryParse(_gridWidthInput.text, out width))
            {
                if (_gridResultText != null)
                    _gridResultText.text = "请输入有效的网格宽度";
                return false;
            }
            
            if (!int.TryParse(_gridHeightInput.text, out height))
            {
                if (_gridResultText != null)
                    _gridResultText.text = "请输入有效的网格高度";
                return false;
            }
            
            if (!int.TryParse(_gridIndexInput.text, out index))
            {
                if (_gridResultText != null)
                    _gridResultText.text = "请输入有效的网格索引";
                return false;
            }
            
            return true;
        }
        
        #endregion
    }
} 