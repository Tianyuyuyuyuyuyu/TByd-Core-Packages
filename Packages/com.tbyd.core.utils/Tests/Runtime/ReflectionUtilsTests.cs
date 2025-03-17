using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TByd.Core.Utils.Runtime;
using UnityEngine;
using System.ComponentModel;

namespace TByd.Core.Utils.Tests.Runtime
{
    public class ReflectionUtilsTests
    {
        // 测试用的嵌套类
        [DisplayName("测试类")]
        private class TestClass
        {
            public int PublicField = 42;
            private string _privateField = "测试字段";
            
            public string PublicProperty { get; set; } = "测试属性";
            private bool PrivateProperty { get; set; } = true;
            
            public static float StaticField = 3.14f;
            public static bool StaticProperty { get; set; } = true;
            
            public TestClass() { }
            
            public TestClass(int value)
            {
                PublicField = value;
            }
            
            public TestClass(string text)
            {
                PublicProperty = text;
            }
            
            public string GetString()
            {
                return "测试方法";
            }
            
            public int Add(int a, int b)
            {
                return a + b;
            }
            
            private void PrivateMethod()
            {
                Debug.Log("私有方法");
            }
            
            public static string StaticMethod()
            {
                return "静态方法";
            }
        }
        
        // 测试用接口和实现
        private interface ITestInterface
        {
            void DoSomething();
        }
        
        private class TestImplementation : ITestInterface
        {
            public void DoSomething() { }
        }
        
        [Test]
        public void GetType_ReturnsCorrectType()
        {
            // 准备 & 测试
            Type type = ReflectionUtils.GetType(typeof(TestClass).FullName);
            
            // 验证
            Assert.NotNull(type);
            Assert.AreEqual(typeof(TestClass), type);
        }
        
        [Test]
        public void GetTypes_FiltersCorrectly()
        {
            // 准备
            Assembly assembly = Assembly.GetExecutingAssembly();
            
            // 测试 - 获取所有类型
            var allTypes = ReflectionUtils.GetTypes(assembly).ToList();
            
            // 验证
            Assert.NotNull(allTypes);
            Assert.Greater(allTypes.Count, 0);
            
            // 测试 - 使用谓词筛选
            var testClasses = ReflectionUtils.GetTypes(assembly, 
                t => t.Name.Contains("Test") && !t.IsInterface).ToList();
            
            // 验证
            Assert.NotNull(testClasses);
            Assert.Greater(testClasses.Count, 0);
            Assert.True(testClasses.Any(t => t == typeof(ReflectionUtilsTests)));
        }
        
        [Test]
        public void GetAllTypes_FiltersCorrectly()
        {
            // 测试 - 筛选所有使用DisplayName特性的类型
            var typesWithAttribute = ReflectionUtils.GetAllTypes(
                t => t.GetCustomAttributes(typeof(DisplayNameAttribute), false).Any()).ToList();
            
            // 验证
            Assert.NotNull(typesWithAttribute);
            // TestClass有DisplayName特性
            Assert.True(typesWithAttribute.Contains(typeof(TestClass)));
        }
        
        [Test]
        public void GetMethodNames_ReturnsCorrectNames()
        {
            // 测试
            var methodNames = ReflectionUtils.GetMethodNames(typeof(TestClass));
            
            // 验证
            Assert.NotNull(methodNames);
            Assert.True(methodNames.Contains("GetString"));
            Assert.True(methodNames.Contains("Add"));
            // 不应该包含私有方法
            Assert.False(methodNames.Contains("PrivateMethod"));
        }
        
        [Test]
        public void GetPropertyNames_ReturnsCorrectNames()
        {
            // 测试
            var propertyNames = ReflectionUtils.GetPropertyNames(typeof(TestClass));
            
            // 验证
            Assert.NotNull(propertyNames);
            Assert.True(propertyNames.Contains("PublicProperty"));
            // 不应该包含私有属性
            Assert.False(propertyNames.Contains("PrivateProperty"));
        }
        
        [Test]
        public void GetStaticFieldNames_ReturnsCorrectNames()
        {
            // 测试
            var staticFieldNames = ReflectionUtils.GetStaticFieldNames(typeof(TestClass));
            
            // 验证
            Assert.NotNull(staticFieldNames);
            Assert.True(staticFieldNames.Contains("StaticField"));
        }
        
        [Test]
        public void GetFieldInfo_ReturnsField()
        {
            // 测试
            FieldInfo publicField = ReflectionUtils.GetFieldInfo(typeof(TestClass), "PublicField");
            FieldInfo privateField = ReflectionUtils.GetFieldInfo(typeof(TestClass), "_privateField");
            FieldInfo nonExistentField = ReflectionUtils.GetFieldInfo(typeof(TestClass), "NonExistentField");
            
            // 验证
            Assert.NotNull(publicField);
            Assert.AreEqual("PublicField", publicField.Name);
            
            Assert.NotNull(privateField);
            Assert.AreEqual("_privateField", privateField.Name);
            
            Assert.Null(nonExistentField);
        }
        
        [Test]
        public void GetPropertyInfo_ReturnsProperty()
        {
            // 测试
            PropertyInfo publicProperty = ReflectionUtils.GetPropertyInfo(typeof(TestClass), "PublicProperty");
            PropertyInfo privateProperty = ReflectionUtils.GetPropertyInfo(typeof(TestClass), "PrivateProperty");
            PropertyInfo nonExistentProperty = ReflectionUtils.GetPropertyInfo(typeof(TestClass), "NonExistentProperty");
            
            // 验证
            Assert.NotNull(publicProperty);
            Assert.AreEqual("PublicProperty", publicProperty.Name);
            
            Assert.NotNull(privateProperty);
            Assert.AreEqual("PrivateProperty", privateProperty.Name);
            
            Assert.Null(nonExistentProperty);
        }
        
        [Test]
        public void GetMethodInfo_ReturnsMethod()
        {
            // 测试
            MethodInfo publicMethod = ReflectionUtils.GetMethodInfo(typeof(TestClass), "GetString");
            MethodInfo privateMethod = ReflectionUtils.GetMethodInfo(typeof(TestClass), "PrivateMethod");
            MethodInfo overloadedMethod = ReflectionUtils.GetMethodInfo(typeof(TestClass), "Add", 
                new Type[] { typeof(int), typeof(int) });
            MethodInfo nonExistentMethod = ReflectionUtils.GetMethodInfo(typeof(TestClass), "NonExistentMethod");
            
            // 验证
            Assert.NotNull(publicMethod);
            Assert.AreEqual("GetString", publicMethod.Name);
            
            Assert.NotNull(privateMethod);
            Assert.AreEqual("PrivateMethod", privateMethod.Name);
            
            Assert.NotNull(overloadedMethod);
            Assert.AreEqual("Add", overloadedMethod.Name);
            
            Assert.Null(nonExistentMethod);
        }
        
        [Test]
        public void GetConstructorInfo_ReturnsConstructor()
        {
            // 测试
            ConstructorInfo defaultCtor = ReflectionUtils.GetConstructorInfo(typeof(TestClass));
            ConstructorInfo paramCtor = ReflectionUtils.GetConstructorInfo(typeof(TestClass), new Type[] { typeof(int) });
            ConstructorInfo stringCtor = ReflectionUtils.GetConstructorInfo(typeof(TestClass), new Type[] { typeof(string) });
            ConstructorInfo nonExistentCtor = ReflectionUtils.GetConstructorInfo(typeof(TestClass), 
                new Type[] { typeof(float), typeof(double) });
            
            // 验证
            Assert.NotNull(defaultCtor);
            
            Assert.NotNull(paramCtor);
            Assert.AreEqual(1, paramCtor.GetParameters().Length);
            Assert.AreEqual(typeof(int), paramCtor.GetParameters()[0].ParameterType);
            
            Assert.NotNull(stringCtor);
            Assert.AreEqual(1, stringCtor.GetParameters().Length);
            Assert.AreEqual(typeof(string), stringCtor.GetParameters()[0].ParameterType);
            
            Assert.Null(nonExistentCtor);
        }
        
        [Test]
        public void CreateGetter_CreatesWorkingDelegate()
        {
            // 准备
            TestClass instance = new TestClass { PublicProperty = "测试值" };
            
            // 测试 - 属性获取
            var propertyGetter = ReflectionUtils.CreateGetter<TestClass, string>("PublicProperty");
            var fieldGetter = ReflectionUtils.CreateGetter<TestClass, int>("PublicField");
            
            // 验证
            Assert.NotNull(propertyGetter);
            Assert.NotNull(fieldGetter);
            
            Assert.AreEqual("测试值", propertyGetter(instance));
            Assert.AreEqual(42, fieldGetter(instance));
        }
        
        [Test]
        public void CreateSetter_CreatesWorkingDelegate()
        {
            // 准备
            TestClass instance = new TestClass();
            
            // 测试 - 属性设置
            var propertySetter = ReflectionUtils.CreateSetter<TestClass, string>("PublicProperty");
            var fieldSetter = ReflectionUtils.CreateSetter<TestClass, int>("PublicField");
            
            // 验证
            Assert.NotNull(propertySetter);
            Assert.NotNull(fieldSetter);
            
            propertySetter(instance, "新值");
            fieldSetter(instance, 99);
            
            Assert.AreEqual("新值", instance.PublicProperty);
            Assert.AreEqual(99, instance.PublicField);
        }
        
        [Test]
        public void CreateInstance_CreatesInstanceCorrectly()
        {
            // 测试 - 默认构造函数
            var instance1 = ReflectionUtils.CreateInstance<TestClass>();
            
            // 测试 - 带参数构造函数
            var instance2 = ReflectionUtils.CreateInstance(typeof(TestClass), 123);
            var instance3 = ReflectionUtils.CreateInstance<TestClass>("新实例");
            
            // 验证
            Assert.NotNull(instance1);
            Assert.NotNull(instance2);
            Assert.NotNull(instance3);
            
            Assert.AreEqual(42, instance1.PublicField);  // 默认值
            
            TestClass typedInstance2 = instance2 as TestClass;
            Assert.NotNull(typedInstance2);
            Assert.AreEqual(123, typedInstance2.PublicField);
            
            Assert.AreEqual("新实例", instance3.PublicProperty);
        }
        
        [Test]
        public void CreateMethodDelegate_WorksCorrectly()
        {
            // 准备
            TestClass instance = new TestClass();
            MethodInfo addMethod = ReflectionUtils.GetMethodInfo(typeof(TestClass), "Add");
            MethodInfo staticMethod = ReflectionUtils.GetMethodInfo(typeof(TestClass), "StaticMethod", null, 
                BindingFlags.Public | BindingFlags.Static);
            
            // 测试
            var addDelegate = ReflectionUtils.CreateMethodDelegate<Func<int, int, int>>(instance, addMethod);
            var staticDelegate = ReflectionUtils.CreateMethodDelegate<Func<string>>(null, staticMethod);
            
            // 验证
            Assert.NotNull(addDelegate);
            Assert.NotNull(staticDelegate);
            
            Assert.AreEqual(5, addDelegate(2, 3));
            Assert.AreEqual("静态方法", staticDelegate());
        }
        
        [Test]
        public void InvokeMethod_InvokesCorrectly()
        {
            // 准备
            TestClass instance = new TestClass();
            
            // 测试
            object result1 = ReflectionUtils.InvokeMethod(instance, "GetString");
            object result2 = ReflectionUtils.InvokeMethod(instance, "Add", 2, 3);
            
            // 验证
            Assert.AreEqual("测试方法", result1);
            Assert.AreEqual(5, result2);
        }
        
        [Test]
        public void InvokeStaticMethod_InvokesCorrectly()
        {
            // 测试
            object result = ReflectionUtils.InvokeStaticMethod(typeof(TestClass), "StaticMethod");
            
            // 验证
            Assert.AreEqual("静态方法", result);
        }
        
        [Test]
        public void GetAttribute_ReturnsAttribute()
        {
            // 测试
            var attribute = ReflectionUtils.GetAttribute<DisplayNameAttribute>(typeof(TestClass));
            
            // 验证
            Assert.NotNull(attribute);
            Assert.AreEqual("测试类", attribute.DisplayName);
        }
        
        [Test]
        public void GetAttributes_ReturnsAllAttributes()
        {
            // 测试
            var attributes = ReflectionUtils.GetAttributes<DisplayNameAttribute>(typeof(TestClass)).ToList();
            
            // 验证
            Assert.NotNull(attributes);
            Assert.AreEqual(1, attributes.Count);
            Assert.AreEqual("测试类", attributes[0].DisplayName);
        }
        
        [Test]
        public void HasAttribute_DetectsAttribute()
        {
            // 测试
            bool hasAttribute = ReflectionUtils.HasAttribute<DisplayNameAttribute>(typeof(TestClass));
            bool hasNoAttribute = ReflectionUtils.HasAttribute<ObsoleteAttribute>(typeof(TestClass));
            
            // 验证
            Assert.True(hasAttribute);
            Assert.False(hasNoAttribute);
        }
        
        [Test]
        public void TryConvert_ConvertsValues()
        {
            // 测试 - 有效转换
            bool success1 = ReflectionUtils.TryConvert<int>("42", out int intResult);
            bool success2 = ReflectionUtils.TryConvert<string>(42, out string stringResult);
            
            // 测试 - 无效转换
            bool failure = ReflectionUtils.TryConvert<DateTime>("not a date", out DateTime dateResult);
            
            // 验证
            Assert.True(success1);
            Assert.AreEqual(42, intResult);
            
            Assert.True(success2);
            Assert.AreEqual("42", stringResult);
            
            Assert.False(failure);
            Assert.AreEqual(default(DateTime), dateResult);
        }
        
        [Test]
        public void Convert_ConvertsValues()
        {
            // 测试 - 有效转换
            int intResult = ReflectionUtils.ConvertTo<int>("42");
            string stringResult = ReflectionUtils.ConvertTo<string>(42);
            
            // 验证
            Assert.AreEqual(42, intResult);
            Assert.AreEqual("42", stringResult);
            
            // 测试 - 无效转换应抛出异常
            Assert.Throws<InvalidCastException>(() => ReflectionUtils.ConvertTo<DateTime>("not a date"));
        }
        
        [Test]
        public void ClearAllCaches_ClearsCaches()
        {
            // 准备 - 填充缓存
            ReflectionUtils.GetFieldInfo(typeof(TestClass), "PublicField");
            ReflectionUtils.GetPropertyInfo(typeof(TestClass), "PublicProperty");
            
            // 测试
            ReflectionUtils.ClearAllCaches();
            
            // 验证 - 间接测试，当缓存被清除时应该仍能正常工作
            FieldInfo fieldInfo = ReflectionUtils.GetFieldInfo(typeof(TestClass), "PublicField");
            PropertyInfo propertyInfo = ReflectionUtils.GetPropertyInfo(typeof(TestClass), "PublicProperty");
            
            Assert.NotNull(fieldInfo);
            Assert.NotNull(propertyInfo);
        }
    }
} 