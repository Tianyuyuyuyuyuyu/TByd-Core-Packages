using UnityEngine;
using UnityEngine.UI;
using TByd.Core.Utils.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace TByd.Core.Utils.Samples
{
    /// <summary>
    /// 展示ReflectionUtils类的功能和用法
    /// </summary>
    public class ReflectionUtilsExample : MonoBehaviour
    {
        [Header("类型查找")]
        [SerializeField] private InputField _typeNameInputField;
        [SerializeField] private Button _findTypeButton;
        [SerializeField] private Text _typeResultText;
        
        [Header("成员查找")]
        [SerializeField] private Dropdown _memberTypeDropdown;
        [SerializeField] private InputField _memberNameInputField;
        [SerializeField] private Button _findMemberButton;
        [SerializeField] private Text _memberResultText;
        
        [Header("属性操作")]
        [SerializeField] private InputField _propertyNameInputField;
        [SerializeField] private InputField _propertyValueInputField;
        [SerializeField] private Button _getPropertyButton;
        [SerializeField] private Button _setPropertyButton;
        [SerializeField] private Text _propertyResultText;
        
        [Header("方法调用")]
        [SerializeField] private InputField _methodNameInputField;
        [SerializeField] private InputField _methodParamsInputField;
        [SerializeField] private Button _invokeMethodButton;
        [SerializeField] private Text _methodResultText;
        
        [Header("特性查找")]
        [SerializeField] private InputField _attributeTypeInputField;
        [SerializeField] private Button _findAttributesButton;
        [SerializeField] private Text _attributeResultText;

        // 示例目标对象
        private ExampleTarget _target;
        
        // 当前选择的类型
        private Type _selectedType;
        
        // 定义成员类型枚举
        private enum MemberType
        {
            Field,
            Property,
            Method,
            Constructor,
            All
        }

        private void Start()
        {
            // 创建示例目标对象
            _target = new ExampleTarget();
            
            // 设置下拉菜单选项
            SetupDropdown();
            
            // 设置UI事件监听
            SetupUIEvents();
            
            // 默认输入值
            if (_typeNameInputField != null)
                _typeNameInputField.text = typeof(ExampleTarget).FullName;
                
            // 初始选择类型
            _selectedType = typeof(ExampleTarget);
            
            // 显示初始类型信息
            DisplayTypeInfo(_selectedType);
        }
        
        private void SetupDropdown()
        {
            if (_memberTypeDropdown != null)
            {
                _memberTypeDropdown.ClearOptions();
                _memberTypeDropdown.AddOptions(new List<string>
                {
                    "字段",
                    "属性",
                    "方法",
                    "构造函数",
                    "所有成员"
                });
            }
        }
        
        private void SetupUIEvents()
        {
            // 类型查找按钮
            if (_findTypeButton != null)
                _findTypeButton.onClick.AddListener(FindType);
                
            // 成员查找按钮
            if (_findMemberButton != null)
                _findMemberButton.onClick.AddListener(FindMember);
                
            // 属性操作按钮
            if (_getPropertyButton != null)
                _getPropertyButton.onClick.AddListener(GetProperty);
                
            if (_setPropertyButton != null)
                _setPropertyButton.onClick.AddListener(SetProperty);
                
            // 方法调用按钮
            if (_invokeMethodButton != null)
                _invokeMethodButton.onClick.AddListener(InvokeMethod);
                
            // 特性查找按钮
            if (_findAttributesButton != null)
                _findAttributesButton.onClick.AddListener(FindAttributes);
        }
        
        #region 类型查找
        
        private void FindType()
        {
            string typeName = _typeNameInputField.text.Trim();
            if (string.IsNullOrEmpty(typeName))
            {
                SetTypeResult("请输入类型名称");
                return;
            }
            
            try
            {
                // 使用ReflectionUtils查找类型
                Type type = ReflectionUtils.GetType(typeName);
                
                if (type != null)
                {
                    _selectedType = type;
                    DisplayTypeInfo(type);
                }
                else
                {
                    SetTypeResult($"未找到类型: {typeName}");
                }
            }
            catch (Exception ex)
            {
                SetTypeResult($"查找类型时出错: {ex.Message}");
            }
        }
        
        private void DisplayTypeInfo(Type type)
        {
            if (type == null)
            {
                SetTypeResult("类型为空");
                return;
            }
            
            string result = $"类型信息: {type.FullName}\n";
            result += $"是否是类: {type.IsClass}\n";
            result += $"是否是接口: {type.IsInterface}\n";
            result += $"是否是值类型: {type.IsValueType}\n";
            result += $"程序集名称: {type.Assembly.GetName().Name}\n";
            
            if (type.BaseType != null)
                result += $"基类: {type.BaseType.FullName}\n";
                
            var interfaces = type.GetInterfaces();
            if (interfaces.Length > 0)
            {
                result += "实现的接口:\n";
                foreach (var interfaceType in interfaces)
                {
                    result += $"- {interfaceType.FullName}\n";
                }
            }
            
            SetTypeResult(result);
        }
        
        private void SetTypeResult(string result)
        {
            if (_typeResultText != null)
                _typeResultText.text = result;
        }
        
        #endregion
        
        #region 成员查找
        
        private void FindMember()
        {
            if (_selectedType == null)
            {
                SetMemberResult("请先查找并选择一个类型");
                return;
            }
            
            string memberName = _memberNameInputField != null ? _memberNameInputField.text.Trim() : "";
            MemberType memberType = (MemberType)(_memberTypeDropdown != null ? _memberTypeDropdown.value : 0);
            
            try
            {
                BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | 
                                            BindingFlags.Instance | BindingFlags.Static;
                
                string result = "";
                
                switch (memberType)
                {
                    case MemberType.Field:
                        result = DisplayFields(_selectedType, memberName, bindingFlags);
                        break;
                        
                    case MemberType.Property:
                        result = DisplayProperties(_selectedType, memberName, bindingFlags);
                        break;
                        
                    case MemberType.Method:
                        result = DisplayMethods(_selectedType, memberName, bindingFlags);
                        break;
                        
                    case MemberType.Constructor:
                        result = DisplayConstructors(_selectedType, bindingFlags);
                        break;
                        
                    case MemberType.All:
                        result = DisplayAllMembers(_selectedType, memberName, bindingFlags);
                        break;
                }
                
                SetMemberResult(result);
            }
            catch (Exception ex)
            {
                SetMemberResult($"查找成员时出错: {ex.Message}");
            }
        }
        
        private string DisplayFields(Type type, string fieldName, BindingFlags bindingFlags)
        {
            FieldInfo[] fields;
            
            if (string.IsNullOrEmpty(fieldName))
            {
                fields = ReflectionUtils.GetFields(type, bindingFlags);
            }
            else
            {
                var field = ReflectionUtils.GetField(type, fieldName, bindingFlags);
                fields = field != null ? new FieldInfo[] { field } : new FieldInfo[0];
            }
            
            if (fields.Length == 0)
                return $"未找到字段{(string.IsNullOrEmpty(fieldName) ? "" : ": " + fieldName)}";
                
            string result = $"找到 {fields.Length} 个字段:\n";
            
            foreach (var field in fields)
            {
                result += $"{GetAccessModifier(field)} {field.FieldType.Name} {field.Name}\n";
            }
            
            return result;
        }
        
        private string DisplayProperties(Type type, string propertyName, BindingFlags bindingFlags)
        {
            PropertyInfo[] properties;
            
            if (string.IsNullOrEmpty(propertyName))
            {
                properties = ReflectionUtils.GetProperties(type, bindingFlags);
            }
            else
            {
                var property = ReflectionUtils.GetProperty(type, propertyName, bindingFlags);
                properties = property != null ? new PropertyInfo[] { property } : new PropertyInfo[0];
            }
            
            if (properties.Length == 0)
                return $"未找到属性{(string.IsNullOrEmpty(propertyName) ? "" : ": " + propertyName)}";
                
            string result = $"找到 {properties.Length} 个属性:\n";
            
            foreach (var property in properties)
            {
                string accessors = "";
                if (property.CanRead) accessors += "get; ";
                if (property.CanWrite) accessors += "set; ";
                
                result += $"{GetAccessModifier(property)} {property.PropertyType.Name} {property.Name} {{ {accessors}}}\n";
            }
            
            return result;
        }
        
        private string DisplayMethods(Type type, string methodName, BindingFlags bindingFlags)
        {
            MethodInfo[] methods;
            
            if (string.IsNullOrEmpty(methodName))
            {
                methods = ReflectionUtils.GetMethods(type, bindingFlags)
                    .Where(m => !m.IsSpecialName) // 过滤掉特殊方法（如属性访问器）
                    .ToArray();
            }
            else
            {
                methods = ReflectionUtils.GetMethods(type, methodName, bindingFlags);
            }
            
            if (methods.Length == 0)
                return $"未找到方法{(string.IsNullOrEmpty(methodName) ? "" : ": " + methodName)}";
                
            string result = $"找到 {methods.Length} 个方法:\n";
            
            foreach (var method in methods)
            {
                string parameters = string.Join(", ", method.GetParameters()
                    .Select(p => $"{p.ParameterType.Name} {p.Name}"));
                    
                result += $"{GetAccessModifier(method)} {method.ReturnType.Name} {method.Name}({parameters})\n";
            }
            
            return result;
        }
        
        private string DisplayConstructors(Type type, BindingFlags bindingFlags)
        {
            var constructors = ReflectionUtils.GetConstructors(type, bindingFlags);
            
            if (constructors.Length == 0)
                return "未找到构造函数";
                
            string result = $"找到 {constructors.Length} 个构造函数:\n";
            
            foreach (var constructor in constructors)
            {
                string parameters = string.Join(", ", constructor.GetParameters()
                    .Select(p => $"{p.ParameterType.Name} {p.Name}"));
                    
                result += $"{GetAccessModifier(constructor)} {type.Name}({parameters})\n";
            }
            
            return result;
        }
        
        private string DisplayAllMembers(Type type, string memberName, BindingFlags bindingFlags)
        {
            string result = "";
            
            if (string.IsNullOrEmpty(memberName))
            {
                result += DisplayFields(type, "", bindingFlags) + "\n";
                result += DisplayProperties(type, "", bindingFlags) + "\n";
                result += DisplayMethods(type, "", bindingFlags) + "\n";
                result += DisplayConstructors(type, bindingFlags);
            }
            else
            {
                var member = ReflectionUtils.GetMember(type, memberName, bindingFlags);
                
                if (member != null)
                {
                    result = $"找到成员 {memberName}:\n";
                    result += $"类型: {member.MemberType}\n";
                    
                    switch (member.MemberType)
                    {
                        case MemberTypes.Field:
                            var field = member as FieldInfo;
                            result += $"{GetAccessModifier(field)} {field.FieldType.Name} {field.Name}";
                            break;
                            
                        case MemberTypes.Property:
                            var property = member as PropertyInfo;
                            string accessors = "";
                            if (property.CanRead) accessors += "get; ";
                            if (property.CanWrite) accessors += "set; ";
                            result += $"{GetAccessModifier(property)} {property.PropertyType.Name} {property.Name} {{ {accessors}}}";
                            break;
                            
                        case MemberTypes.Method:
                            var method = member as MethodInfo;
                            string parameters = string.Join(", ", method.GetParameters()
                                .Select(p => $"{p.ParameterType.Name} {p.Name}"));
                            result += $"{GetAccessModifier(method)} {method.ReturnType.Name} {method.Name}({parameters})";
                            break;
                            
                        case MemberTypes.Constructor:
                            var constructor = member as ConstructorInfo;
                            string ctorParams = string.Join(", ", constructor.GetParameters()
                                .Select(p => $"{p.ParameterType.Name} {p.Name}"));
                            result += $"{GetAccessModifier(constructor)} {type.Name}({ctorParams})";
                            break;
                    }
                }
                else
                {
                    result = $"未找到成员: {memberName}";
                }
            }
            
            return result;
        }
        
        private string GetAccessModifier(MemberInfo member)
        {
            if (member is FieldInfo field)
            {
                if (field.IsPublic) return "public";
                if (field.IsPrivate) return "private";
                if (field.IsFamily) return "protected";
                if (field.IsAssembly) return "internal";
                if (field.IsFamilyOrAssembly) return "protected internal";
                return "";
            }
            else if (member is MethodBase method)
            {
                if (method.IsPublic) return "public";
                if (method.IsPrivate) return "private";
                if (method.IsFamily) return "protected";
                if (method.IsAssembly) return "internal";
                if (method.IsFamilyOrAssembly) return "protected internal";
                return "";
            }
            else if (member is PropertyInfo property)
            {
                var getMethod = property.GetMethod;
                var setMethod = property.SetMethod;
                
                if (getMethod != null && getMethod.IsPublic) return "public";
                if (setMethod != null && setMethod.IsPublic) return "public";
                if (getMethod != null && getMethod.IsFamily) return "protected";
                if (setMethod != null && setMethod.IsFamily) return "protected";
                if (getMethod != null && getMethod.IsAssembly) return "internal";
                if (setMethod != null && setMethod.IsAssembly) return "internal";
                if (getMethod != null && getMethod.IsFamilyOrAssembly) return "protected internal";
                if (setMethod != null && setMethod.IsFamilyOrAssembly) return "protected internal";
                if (getMethod != null && getMethod.IsPrivate) return "private";
                if (setMethod != null && setMethod.IsPrivate) return "private";
                return "";
            }
            
            return "";
        }
        
        private void SetMemberResult(string result)
        {
            if (_memberResultText != null)
                _memberResultText.text = result;
        }
        
        #endregion
        
        #region 属性操作
        
        private void GetProperty()
        {
            if (_target == null)
            {
                SetPropertyResult("目标对象为空");
                return;
            }
            
            string propertyName = _propertyNameInputField != null ? _propertyNameInputField.text.Trim() : "";
            
            if (string.IsNullOrEmpty(propertyName))
            {
                SetPropertyResult("请输入属性名称");
                return;
            }
            
            try
            {
                // 使用ReflectionUtils获取属性值
                object value = ReflectionUtils.GetPropertyValue(_target, propertyName);
                
                SetPropertyResult($"属性 {propertyName} 的值是: {value}");
            }
            catch (Exception ex)
            {
                SetPropertyResult($"获取属性值时出错: {ex.Message}");
            }
        }
        
        private void SetProperty()
        {
            if (_target == null)
            {
                SetPropertyResult("目标对象为空");
                return;
            }
            
            string propertyName = _propertyNameInputField != null ? _propertyNameInputField.text.Trim() : "";
            string propertyValue = _propertyValueInputField != null ? _propertyValueInputField.text.Trim() : "";
            
            if (string.IsNullOrEmpty(propertyName))
            {
                SetPropertyResult("请输入属性名称");
                return;
            }
            
            try
            {
                // 获取属性信息
                PropertyInfo property = ReflectionUtils.GetProperty(typeof(ExampleTarget), propertyName);
                
                if (property == null)
                {
                    SetPropertyResult($"属性 {propertyName} 不存在");
                    return;
                }
                
                // 转换值的类型
                object value = ConvertValue(propertyValue, property.PropertyType);
                
                // 使用ReflectionUtils设置属性值
                ReflectionUtils.SetPropertyValue(_target, propertyName, value);
                
                // 读取设置后的值
                object newValue = ReflectionUtils.GetPropertyValue(_target, propertyName);
                
                SetPropertyResult($"已将属性 {propertyName} 的值设置为: {newValue}");
            }
            catch (Exception ex)
            {
                SetPropertyResult($"设置属性值时出错: {ex.Message}");
            }
        }
        
        private void SetPropertyResult(string result)
        {
            if (_propertyResultText != null)
                _propertyResultText.text = result;
        }
        
        #endregion
        
        #region 方法调用
        
        private void InvokeMethod()
        {
            if (_target == null)
            {
                SetMethodResult("目标对象为空");
                return;
            }
            
            string methodName = _methodNameInputField != null ? _methodNameInputField.text.Trim() : "";
            string paramsText = _methodParamsInputField != null ? _methodParamsInputField.text.Trim() : "";
            
            if (string.IsNullOrEmpty(methodName))
            {
                SetMethodResult("请输入方法名称");
                return;
            }
            
            try
            {
                // 解析参数
                object[] parameters = ParseParameters(paramsText);
                
                // 使用ReflectionUtils调用方法
                object result = ReflectionUtils.InvokeMethod(_target, methodName, parameters);
                
                SetMethodResult($"调用方法 {methodName} 成功!\n返回值: {(result ?? "无返回值")}");
            }
            catch (Exception ex)
            {
                SetMethodResult($"调用方法时出错: {ex.Message}");
            }
        }
        
        private object[] ParseParameters(string paramsText)
        {
            if (string.IsNullOrEmpty(paramsText))
                return new object[0];
                
            // 简单参数解析，以逗号分隔
            string[] paramStrings = paramsText.Split(',');
            object[] parameters = new object[paramStrings.Length];
            
            for (int i = 0; i < paramStrings.Length; i++)
            {
                string param = paramStrings[i].Trim();
                
                // 尝试解析为常见类型
                if (int.TryParse(param, out int intValue))
                    parameters[i] = intValue;
                else if (float.TryParse(param, out float floatValue))
                    parameters[i] = floatValue;
                else if (bool.TryParse(param, out bool boolValue))
                    parameters[i] = boolValue;
                else
                    parameters[i] = param; // 默认作为字符串
            }
            
            return parameters;
        }
        
        private void SetMethodResult(string result)
        {
            if (_methodResultText != null)
                _methodResultText.text = result;
        }
        
        #endregion
        
        #region 特性查找
        
        private void FindAttributes()
        {
            if (_selectedType == null)
            {
                SetAttributeResult("请先查找并选择一个类型");
                return;
            }
            
            string attributeTypeName = _attributeTypeInputField != null ? _attributeTypeInputField.text.Trim() : "";
            
            try
            {
                Type attributeType = null;
                
                if (!string.IsNullOrEmpty(attributeTypeName))
                {
                    // 如果没有指定完整的特性名称，尝试添加Attribute后缀
                    if (!attributeTypeName.EndsWith("Attribute"))
                        attributeTypeName += "Attribute";
                        
                    attributeType = ReflectionUtils.GetType(attributeTypeName);
                    
                    if (attributeType == null)
                    {
                        SetAttributeResult($"未找到特性类型: {attributeTypeName}");
                        return;
                    }
                }
                
                string result = "";
                
                // 查找类上的特性
                result += $"类 {_selectedType.Name} 上的特性:\n";
                var typeAttributes = attributeType != null
                    ? ReflectionUtils.GetAttributes(_selectedType, attributeType)
                    : ReflectionUtils.GetAttributes(_selectedType);
                    
                if (typeAttributes.Length == 0)
                    result += "无\n";
                else
                {
                    foreach (var attr in typeAttributes)
                    {
                        result += $"- {attr.GetType().Name}\n";
                    }
                }
                
                // 查找成员上的特性
                result += $"\n成员上的特性:\n";
                var members = ReflectionUtils.GetMembers(_selectedType)
                    .Where(m => m.DeclaringType == _selectedType) // 只显示当前类型直接声明的成员
                    .ToArray();
                    
                bool foundAny = false;
                
                foreach (var member in members)
                {
                    var memberAttributes = attributeType != null
                        ? ReflectionUtils.GetAttributes(member, attributeType)
                        : ReflectionUtils.GetAttributes(member);
                        
                    if (memberAttributes.Length > 0)
                    {
                        foundAny = true;
                        result += $"成员 {member.Name}:\n";
                        
                        foreach (var attr in memberAttributes)
                        {
                            result += $"- {attr.GetType().Name}\n";
                        }
                    }
                }
                
                if (!foundAny)
                    result += "没有成员拥有" + (attributeType != null ? $"{attributeType.Name}" : "任何") + "特性";
                    
                SetAttributeResult(result);
            }
            catch (Exception ex)
            {
                SetAttributeResult($"查找特性时出错: {ex.Message}");
            }
        }
        
        private void SetAttributeResult(string result)
        {
            if (_attributeResultText != null)
                _attributeResultText.text = result;
        }
        
        #endregion
        
        private object ConvertValue(string value, Type targetType)
        {
            if (string.IsNullOrEmpty(value))
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
                
            if (targetType == typeof(string))
                return value;
                
            if (targetType == typeof(int) || targetType == typeof(int?))
                return int.Parse(value);
                
            if (targetType == typeof(float) || targetType == typeof(float?))
                return float.Parse(value);
                
            if (targetType == typeof(double) || targetType == typeof(double?))
                return double.Parse(value);
                
            if (targetType == typeof(bool) || targetType == typeof(bool?))
                return bool.Parse(value);
                
            if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                return DateTime.Parse(value);
                
            if (targetType.IsEnum)
                return Enum.Parse(targetType, value);
                
            // 默认情况，尝试使用Convert.ChangeType
            return Convert.ChangeType(value, targetType);
        }
    }
    
    /// <summary>
    /// 用于反射演示的示例类
    /// </summary>
    [ExampleClass("这是一个示例类")]
    public class ExampleTarget
    {
        // 字段
        public int PublicField = 42;
        private string _privateField = "私有字段";
        protected float _protectedField = 3.14f;
        
        // 属性
        public string Name { get; set; } = "示例对象";
        
        [ExampleProperty("年龄属性")]
        public int Age { get; private set; } = 10;
        
        public bool IsActive { get; set; } = true;
        
        public ExampleTarget()
        {
            // 默认构造函数
        }
        
        public ExampleTarget(string name)
        {
            Name = name;
        }
        
        public ExampleTarget(string name, int age)
        {
            Name = name;
            Age = age;
        }
        
        // 方法
        public void SayHello()
        {
            Debug.Log($"你好，我是 {Name}");
        }
        
        [ExampleMethod("问候方法")]
        public string Greet(string visitorName)
        {
            return $"你好，{visitorName}！我是 {Name}，今年 {Age} 岁了。";
        }
        
        public int Add(int a, int b)
        {
            return a + b;
        }
        
        private void SecretMethod()
        {
            Debug.Log("这是一个私有方法");
        }
        
        protected virtual void OnUpdate()
        {
            // 模拟的受保护虚方法
        }
    }
    
    // 自定义特性
    [AttributeUsage(AttributeTargets.Class)]
    public class ExampleClassAttribute : Attribute
    {
        public string Description { get; }
        
        public ExampleClassAttribute(string description)
        {
            Description = description;
        }
    }
    
    [AttributeUsage(AttributeTargets.Property)]
    public class ExamplePropertyAttribute : Attribute
    {
        public string Description { get; }
        
        public ExamplePropertyAttribute(string description)
        {
            Description = description;
        }
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class ExampleMethodAttribute : Attribute
    {
        public string Description { get; }
        
        public ExampleMethodAttribute(string description)
        {
            Description = description;
        }
    }
} 