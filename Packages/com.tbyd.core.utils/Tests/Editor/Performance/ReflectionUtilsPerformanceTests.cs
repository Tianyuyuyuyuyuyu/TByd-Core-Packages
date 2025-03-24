using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TByd.Core.Utils.Runtime;
using TByd.Core.Utils.Tests.Editor.Framework;
using Unity.PerformanceTesting;

namespace TByd.Core.Utils.Tests.Editor.Performance
{
    /// <summary>
    /// ReflectionUtils 性能测试类
    /// </summary>
    public class ReflectionUtilsPerformanceTests : PerformanceTestBase
    {
        #region 测试类和接口
        
        private interface ITestInterface
        {
            string GetValue();
        }
        
        private class TestBaseClass
        {
            public string PublicField = "BasePublicField";
            private string privateField = "BasePrivateField";
            protected string protectedField = "BaseProtectedField";
            
            public string PublicProperty { get; set; } = "BasePublicProperty";
            private string PrivateProperty { get; set; } = "BasePrivateProperty";
            protected string ProtectedProperty { get; set; } = "BaseProtectedProperty";
            
            public string PublicMethod() => "BasePublicMethod";
            private string PrivateMethod() => "BasePrivateMethod";
            protected string ProtectedMethod() => "BaseProtectedMethod";
            
            public virtual string VirtualMethod() => "BaseVirtualMethod";
            
            public static string StaticMethod() => "BaseStaticMethod";
            
            public string MethodWithParameters(string param1, int param2) => $"{param1}_{param2}";
        }
        
        private class TestDerivedClass : TestBaseClass, ITestInterface
        {
            public string PublicField = "DerivedPublicField";
            private string privateField = "DerivedPrivateField";
            protected string protectedField = "DerivedProtectedField";
            
            public new string PublicProperty { get; set; } = "DerivedPublicProperty";
            private string PrivateProperty { get; set; } = "DerivedPrivateProperty";
            protected string ProtectedProperty { get; set; } = "DerivedProtectedProperty";
            
            public new string PublicMethod() => "DerivedPublicMethod";
            private string PrivateMethod() => "DerivedPrivateMethod";
            protected string ProtectedMethod() => "DerivedProtectedMethod";
            
            public override string VirtualMethod() => "DerivedVirtualMethod";
            
            public static new string StaticMethod() => "DerivedStaticMethod";
            
            // 实现接口方法
            public string GetValue() => "InterfaceMethod";
        }
        
        [Serializable]
        private class TestSerializableClass
        {
            public string Name;
            public int Age;
            
            [NonSerialized]
            public string Ignored;
        }
        
        #endregion
        
        private TestDerivedClass _testInstance;
        private const int TestIterations = 10000;
        
        [SetUp]
        public void SetUp()
        {
            _testInstance = new TestDerivedClass();
        }
        
        #region GetField Performance Tests
        
        [Test]
        [Performance]
        public void GetFieldInfo_Performance()
        {
            // 定义测试配置
            var config = new PerformanceTestConfig
            {
                TestName = "GetFieldInfo_性能",
                MeasurementCount = 10,
                WarmupCount = 3,
                MeasureGC = true
            };
            
            // 定义基准实现（直接访问）
            Action baseline = () =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    string value = _testInstance.PublicField;
                }
            };
            
            // 定义ReflectionUtils实现
            Action reflection = () =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    var fieldInfo = ReflectionUtils.GetFieldInfo(typeof(TestDerivedClass), "PublicField");
                    string value = (string)fieldInfo.GetValue(_testInstance);
                }
            };
            
            // 比较性能
            RunPerformanceTest(baseline, new PerformanceTestConfig 
            {
                TestName = config.TestName + "_Baseline",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = true
            });
            
            RunPerformanceTest(reflection, new PerformanceTestConfig
            {
                TestName = config.TestName + "_Reflection",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = false
            });
        }
        
        [Test]
        [Performance]
        public void GetFieldInfo_CachedReflection_Performance()
        {
            // 定义测试配置
            var config = new PerformanceTestConfig
            {
                TestName = "GetFieldInfo_缓存反射性能",
                MeasurementCount = 10,
                WarmupCount = 3,
                MeasureGC = true
            };
            
            // 定义基准实现（直接访问）
            Action baseline = () =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    string value = _testInstance.PublicField;
                }
            };
            
            // 准备缓存的反射信息
            FieldInfo fieldInfo = ReflectionUtils.GetFieldInfo(typeof(TestDerivedClass), "PublicField");
            
            // 定义手动缓存反射实现
            Action cachedReflection = () =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    string value = (string)fieldInfo.GetValue(_testInstance);
                }
            };
            
            // 比较性能
            RunPerformanceTest(baseline, new PerformanceTestConfig 
            {
                TestName = config.TestName + "_Baseline",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = true
            });
            
            RunPerformanceTest(cachedReflection, new PerformanceTestConfig
            {
                TestName = config.TestName + "_CachedReflection",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = false
            });
        }
        
        #endregion
        
        #region SetField Performance Tests
        
        [Test]
        [Performance]
        public void SetFieldValue_Performance()
        {
            // 定义测试配置
            var config = new PerformanceTestConfig
            {
                TestName = "SetFieldValue_性能",
                MeasurementCount = 10,
                WarmupCount = 3,
                MeasureGC = true
            };
            
            // 定义基准实现（直接访问）
            Action baseline = () =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    _testInstance.PublicField = "NewValue";
                }
            };
            
            // 定义ReflectionUtils实现
            Action reflection = () =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    var fieldInfo = ReflectionUtils.GetFieldInfo(typeof(TestDerivedClass), "PublicField");
                    fieldInfo.SetValue(_testInstance, "NewValue");
                }
            };
            
            // 比较性能
            RunPerformanceTest(baseline, new PerformanceTestConfig 
            {
                TestName = config.TestName + "_Baseline",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = true
            });
            
            RunPerformanceTest(reflection, new PerformanceTestConfig
            {
                TestName = config.TestName + "_Reflection",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = false
            });
        }
        
        #endregion
        
        #region GetProperty Performance Tests
        
        [Test]
        [Performance]
        public void GetPropertyInfo_Performance()
        {
            // 定义测试配置
            var config = new PerformanceTestConfig
            {
                TestName = "GetPropertyInfo_性能",
                MeasurementCount = 10,
                WarmupCount = 3,
                MeasureGC = true
            };
            
            // 定义基准实现（直接访问）
            Action baseline = () =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    string value = _testInstance.PublicProperty;
                }
            };
            
            // 定义ReflectionUtils实现
            Action reflection = () =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    var propertyInfo = ReflectionUtils.GetPropertyInfo(typeof(TestDerivedClass), "PublicProperty");
                    string value = (string)propertyInfo.GetValue(_testInstance);
                }
            };
            
            // 比较性能
            RunPerformanceTest(baseline, new PerformanceTestConfig 
            {
                TestName = config.TestName + "_Baseline",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = true
            });
            
            RunPerformanceTest(reflection, new PerformanceTestConfig
            {
                TestName = config.TestName + "_Reflection",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = false
            });
        }
        
        #endregion
        
        #region SetProperty Performance Tests
        
        [Test]
        [Performance]
        public void SetPropertyValue_Performance()
        {
            // 定义测试配置
            var config = new PerformanceTestConfig
            {
                TestName = "SetPropertyValue_性能",
                MeasurementCount = 10,
                WarmupCount = 3,
                MeasureGC = true
            };
            
            // 定义基准实现（直接访问）
            Action baseline = () =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    _testInstance.PublicProperty = "NewValue";
                }
            };
            
            // 定义ReflectionUtils实现
            Action reflection = () =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    var propertyInfo = ReflectionUtils.GetPropertyInfo(typeof(TestDerivedClass), "PublicProperty");
                    propertyInfo.SetValue(_testInstance, "NewValue");
                }
            };
            
            // 比较性能
            RunPerformanceTest(baseline, new PerformanceTestConfig 
            {
                TestName = config.TestName + "_Baseline",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = true
            });
            
            RunPerformanceTest(reflection, new PerformanceTestConfig
            {
                TestName = config.TestName + "_Reflection",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = false
            });
        }
        
        #endregion
        
        #region InvokeMethod Performance Tests
        
        [Test]
        [Performance]
        public void InvokeMethod_Performance()
        {
            // 定义测试配置
            var config = new PerformanceTestConfig
            {
                TestName = "InvokeMethod_性能",
                MeasurementCount = 10,
                WarmupCount = 3,
                MeasureGC = true
            };
            
            // 定义基准实现（直接调用）
            Action baseline = () =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    string value = _testInstance.PublicMethod();
                }
            };
            
            // 定义ReflectionUtils实现
            Action reflection = () =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    object value = ReflectionUtils.InvokeMethod(_testInstance, "PublicMethod");
                }
            };
            
            // 比较性能
            RunPerformanceTest(baseline, new PerformanceTestConfig 
            {
                TestName = config.TestName + "_Baseline",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = true
            });
            
            RunPerformanceTest(reflection, new PerformanceTestConfig
            {
                TestName = config.TestName + "_Reflection",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = false
            });
        }
        
        [Test]
        [Performance]
        public void InvokeMethod_WithParameters_Performance()
        {
            // 定义测试配置
            var config = new PerformanceTestConfig
            {
                TestName = "InvokeMethod_WithParameters_性能",
                MeasurementCount = 10,
                WarmupCount = 3,
                MeasureGC = true
            };
            
            // 定义基准实现（直接调用）
            Action baseline = () =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    string value = _testInstance.MethodWithParameters("Hello", 123);
                }
            };
            
            // 定义ReflectionUtils实现
            Action reflection = () =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    object value = ReflectionUtils.InvokeMethod(_testInstance, "MethodWithParameters", 
                        new object[] { "Hello", 123 });
                }
            };
            
            // 比较性能
            RunPerformanceTest(baseline, new PerformanceTestConfig 
            {
                TestName = config.TestName + "_Baseline",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = true
            });
            
            RunPerformanceTest(reflection, new PerformanceTestConfig
            {
                TestName = config.TestName + "_Reflection",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = false
            });
        }
        
        #endregion
        
        #region GetAttribute Performance Tests
        
        [Test]
        [Performance]
        public void GetAttribute_Performance()
        {
            // 定义测试配置
            var config = new PerformanceTestConfig
            {
                TestName = "GetAttribute_性能",
                MeasurementCount = 10,
                WarmupCount = 3,
                MeasureGC = true
            };
            
            Type serializableType = typeof(TestSerializableClass);
            
            // 定义基准实现（直接获取特性）
            Action baseline = () =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    SerializableAttribute attr = serializableType.GetCustomAttribute<SerializableAttribute>();
                }
            };
            
            // 定义ReflectionUtils实现
            Action reflection = () =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    SerializableAttribute attr = ReflectionUtils.GetAttribute<SerializableAttribute>(serializableType);
                }
            };
            
            // 比较性能
            RunPerformanceTest(baseline, new PerformanceTestConfig 
            {
                TestName = config.TestName + "_Baseline",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = true
            });
            
            RunPerformanceTest(reflection, new PerformanceTestConfig
            {
                TestName = config.TestName + "_Reflection",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = false
            });
        }
        
        #endregion
        
        #region CreateInstance Performance Tests
        
        [Test]
        [Performance]
        public void CreateInstance_Performance()
        {
            // 定义测试配置
            var config = new PerformanceTestConfig
            {
                TestName = "CreateInstance_性能",
                MeasurementCount = 10,
                WarmupCount = 3,
                MeasureGC = true
            };
            
            // 定义基准实现（直接实例化）
            Action baseline = () =>
            {
                for (int i = 0; i < TestIterations / 10; i++) // 减少迭代次数，因为创建实例较耗时
                {
                    var instance = new TestDerivedClass();
                }
            };
            
            // 定义ReflectionUtils实现
            Action reflection = () =>
            {
                for (int i = 0; i < TestIterations / 10; i++)
                {
                    var instance = ReflectionUtils.CreateInstance<TestDerivedClass>();
                }
            };
            
            // 比较性能
            RunPerformanceTest(baseline, new PerformanceTestConfig 
            {
                TestName = config.TestName + "_Baseline",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = true
            });
            
            RunPerformanceTest(reflection, new PerformanceTestConfig
            {
                TestName = config.TestName + "_Reflection",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = false
            });
        }
        
        [Test]
        [Performance]
        public void CreateInstance_WithParameters_Performance()
        {
            // 定义测试配置
            var config = new PerformanceTestConfig
            {
                TestName = "CreateInstance_WithParameters_性能",
                MeasurementCount = 10,
                WarmupCount = 3,
                MeasureGC = true
            };
            
            // 定义基准实现（直接实例化）
            Action baseline = () =>
            {
                for (int i = 0; i < TestIterations / 10; i++)
                {
                    var instance = new List<string>(5);
                }
            };
            
            // 定义ReflectionUtils实现
            Action reflection = () =>
            {
                for (int i = 0; i < TestIterations / 10; i++)
                {
                    var instance = ReflectionUtils.CreateInstance<List<string>>(new object[] { 5 });
                }
            };
            
            // 比较性能
            RunPerformanceTest(baseline, new PerformanceTestConfig 
            {
                TestName = config.TestName + "_Baseline",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = true
            });
            
            RunPerformanceTest(reflection, new PerformanceTestConfig
            {
                TestName = config.TestName + "_Reflection",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = false
            });
        }
        
        #endregion
        
        #region GetTypes Performance Tests
        
        [Test]
        [Performance]
        public void GetTypes_Performance()
        {
            // 定义测试配置
            var config = new PerformanceTestConfig
            {
                TestName = "GetTypes_性能",
                MeasurementCount = 5,
                WarmupCount = 2,
                MeasureGC = true
            };
            
            // 定义基准实现（使用LINQ）
            Action baseline = () =>
            {
                for (int i = 0; i < 10; i++) // 减少迭代次数，因为这个操作很耗时
                {
                    var types = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => a.GetTypes())
                        .Where(t => typeof(ITestInterface).IsAssignableFrom(t) && !t.IsInterface)
                        .ToList();
                }
            };
            
            // 定义ReflectionUtils实现
            Action reflection = () =>
            {
                for (int i = 0; i < 10; i++)
                {
                    var types = ReflectionUtils.GetAllTypes(
                        t => typeof(ITestInterface).IsAssignableFrom(t) && !t.IsInterface);
                }
            };
            
            // 比较性能
            RunPerformanceTest(baseline, new PerformanceTestConfig 
            {
                TestName = config.TestName + "_Baseline",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = true
            });
            
            RunPerformanceTest(reflection, new PerformanceTestConfig
            {
                TestName = config.TestName + "_Reflection",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = false
            });
        }
        
        #endregion
        
        #region Memory Allocation Tests
        
        [Test]
        [Performance]
        public void ReflectionUtils_GetFieldInfo_GCAllocation()
        {
            MeasureGC.AssertMaxAllocation(() => 
            {
                ReflectionUtils.GetFieldInfo(typeof(TestDerivedClass), "PublicField");
            }, 200);
        }
        
        [Test]
        [Performance]
        public void ReflectionUtils_FieldSetValue_GCAllocation()
        {
            var fieldInfo = ReflectionUtils.GetFieldInfo(typeof(TestDerivedClass), "PublicField");
            
            MeasureGC.AssertMaxAllocation(() => 
            {
                fieldInfo.SetValue(_testInstance, "NewValue");
            }, 200);
        }
        
        [Test]
        [Performance]
        public void ReflectionUtils_GetPropertyInfo_GCAllocation()
        {
            MeasureGC.AssertMaxAllocation(() => 
            {
                ReflectionUtils.GetPropertyInfo(typeof(TestDerivedClass), "PublicProperty");
            }, 200);
        }
        
        [Test]
        [Performance]
        public void ReflectionUtils_PropertySetValue_GCAllocation()
        {
            var propertyInfo = ReflectionUtils.GetPropertyInfo(typeof(TestDerivedClass), "PublicProperty");
            
            MeasureGC.AssertMaxAllocation(() => 
            {
                propertyInfo.SetValue(_testInstance, "NewValue");
            }, 200);
        }
        
        [Test]
        [Performance]
        public void ReflectionUtils_InvokeMethod_GCAllocation()
        {
            MeasureGC.AssertMaxAllocation(() => 
            {
                object result = ReflectionUtils.InvokeMethod(_testInstance, "PublicMethod");
            }, 300);
        }
        
        [Test]
        [Performance]
        public void ReflectionUtils_InvokeMethod_WithParameters_GCAllocation()
        {
            object[] parameters = new object[] { "Hello", 123 };
            
            MeasureGC.AssertMaxAllocation(() => 
            {
                object result = ReflectionUtils.InvokeMethod(_testInstance, "MethodWithParameters", parameters);
            }, 400);
        }
        
        [Test]
        [Performance]
        public void ReflectionUtils_GetAttribute_GCAllocation()
        {
            Type serializableType = typeof(TestSerializableClass);
            
            MeasureGC.AssertMaxAllocation(() => 
            {
                ReflectionUtils.GetAttribute<SerializableAttribute>(serializableType);
            }, 200);
        }
        
        [Test]
        [Performance]
        public void ReflectionUtils_CreateInstance_GCAllocation()
        {
            // 实例化必然会分配内存，所以我们只需要确保它不超过合理的量
            MeasureGC.AssertMaxAllocation(() => 
            {
                ReflectionUtils.CreateInstance<object>();
            }, 500);
        }
        
        #endregion
    }
} 