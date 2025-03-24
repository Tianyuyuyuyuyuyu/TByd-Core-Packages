using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using TByd.Core.Utils.Tests.Editor.Framework;
using Unity.PerformanceTesting;

namespace TByd.Core.Utils.Tests.Editor.Performance
{
    /// <summary>
    /// CollectionUtils类的性能测试
    /// </summary>
    [TestFixture]
    public class CollectionUtilsPerformanceTests : PerformanceTestBase
    {
        private const int BenchmarkIterations = 10000;
        private const int SmallCollectionSize = 10;
        private const int MediumCollectionSize = 100;
        private const int LargeCollectionSize = 1000;

        private List<int> _smallCollection;
        private List<int> _mediumCollection;
        private List<int> _largeCollection;
        private List<string> _stringCollection;

        [SetUp]
        public void Setup()
        {
            // 初始化测试集合
            _smallCollection = Enumerable.Range(1, SmallCollectionSize).ToList();
            _mediumCollection = Enumerable.Range(1, MediumCollectionSize).ToList();
            _largeCollection = Enumerable.Range(1, LargeCollectionSize).ToList();
            _stringCollection = new List<string>();
            for (int i = 0; i < MediumCollectionSize; i++)
            {
                _stringCollection.Add($"项目{i}");
            }
        }

        #region IsNullOrEmpty Performance Tests

        /// <summary>
        /// 测试IsNullOrEmpty方法的性能
        /// </summary>
        [Test, Performance]
        public void IsNullOrEmpty_Performance()
        {
            var emptyCollection = new List<int>();
            
            // 测试非空集合
            Measure.Method(() =>
            {
                for (int i = 0; i < BenchmarkIterations; i++)
                {
                    CollectionUtils.IsNullOrEmpty(_smallCollection);
                }
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
            
            // 测试空集合
            Measure.Method(() =>
            {
                for (int i = 0; i < BenchmarkIterations; i++)
                {
                    CollectionUtils.IsNullOrEmpty(emptyCollection);
                }
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
            
            // 比较: 手动实现的非空检查
            Measure.Method(() =>
            {
                for (int i = 0; i < BenchmarkIterations; i++)
                {
                    var result = _smallCollection == null || !_smallCollection.Any();
                }
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        /// <summary>
        /// 测试IsNullOrEmpty方法的GC分配
        /// </summary>
        [Test, Performance]
        public void IsNullOrEmpty_GCAllocation()
        {
            // 空集合
            var emptyAllocation = MeasureGC.Allocation(() =>
            {
                bool result = CollectionUtils.IsNullOrEmpty(new List<int>());
            });
            
            // null集合
            var nullAllocation = MeasureGC.Allocation(() =>
            {
                bool result = CollectionUtils.IsNullOrEmpty((List<int>)null);
            });
            
            // 验证结果 - 应该非常少或零分配
            Assert.That(emptyAllocation, Is.LessThan(16), "IsNullOrEmpty对空集合的内存分配应很小");
            Assert.That(nullAllocation, Is.LessThan(16), "IsNullOrEmpty对null集合的内存分配应很小");
        }

        #endregion

        #region ForEach Performance Tests

        /// <summary>
        /// 测试ForEach方法的性能
        /// </summary>
        [Test, Performance]
        public void ForEach_Performance()
        {
            // 小集合
            Measure.Method(() =>
            {
                int sum = 0;
                CollectionUtils.ForEach(_smallCollection, item => sum += item);
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
            
            // 中集合
            Measure.Method(() =>
            {
                int sum = 0;
                CollectionUtils.ForEach(_mediumCollection, item => sum += item);
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
            
            // 比较: 使用foreach循环
            Measure.Method(() =>
            {
                int sum = 0;
                foreach (var item in _mediumCollection)
                {
                    sum += item;
                }
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
            
            // 比较: 使用LINQ的ForEach扩展方法
            Measure.Method(() =>
            {
                int sum = 0;
                _mediumCollection.ForEach(item => sum += item);
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        /// <summary>
        /// 测试ForEach方法的GC分配
        /// </summary>
        [Test, Performance]
        public void ForEach_GCAllocation()
        {
            // 准备测试数据
            var collection = new List<int> { 1, 2, 3, 4, 5 };
            int sum = 0;
            
            var allocation = MeasureGC.Allocation(() =>
            {
                CollectionUtils.ForEach(collection, item => sum += item);
            });
            
            // 验证结果 - 应该是零分配
            Assert.That(allocation, Is.EqualTo(0), "ForEach方法应该是零内存分配");
        }

        /// <summary>
        /// 测试ForEach方法的带索引版本性能
        /// </summary>
        [Test, Performance]
        public void ForEach_WithIndex_Performance()
        {
            // 使用CollectionUtils.ForEach带索引版本
            Measure.Method(() =>
            {
                int sum = 0;
                CollectionUtils.ForEach(_mediumCollection, (item, index) => sum += item + index);
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
            
            // 比较: 使用for循环
            Measure.Method(() =>
            {
                int sum = 0;
                for (int i = 0; i < _mediumCollection.Count; i++)
                {
                    sum += _mediumCollection[i] + i;
                }
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        #endregion

        #region Shuffle Performance Tests

        /// <summary>
        /// 测试Shuffle方法的性能
        /// </summary>
        [Test, Performance]
        public void Shuffle_Performance()
        {
            // 准备测试集合，每次都复制一份新的，避免影响测试结果
            var smallCollectionCopy = new List<int>(_smallCollection);
            var mediumCollectionCopy = new List<int>(_mediumCollection);
            var largeCollectionCopy = new List<int>(_largeCollection);
            
            // 小集合
            Measure.Method(() =>
            {
                CollectionUtils.Shuffle(smallCollectionCopy);
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
            
            // 中集合
            Measure.Method(() =>
            {
                CollectionUtils.Shuffle(mediumCollectionCopy);
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
            
            // 大集合
            Measure.Method(() =>
            {
                CollectionUtils.Shuffle(largeCollectionCopy);
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        /// <summary>
        /// 测试Shuffle方法的GC分配
        /// </summary>
        [Test, Performance]
        public void Shuffle_GCAllocation()
        {
            // 准备测试数据
            var collection = new List<int> { 1, 2, 3, 4, 5 };
            
            var allocation = MeasureGC.Allocation(() =>
            {
                CollectionUtils.Shuffle(collection);
            });
            
            // 验证结果 - 洗牌操作应该有限制的内存分配
            Assert.That(allocation, Is.LessThan(1024), "Shuffle方法的内存分配应控制在合理范围内");
        }

        #endregion

        #region GetRandomElement Performance Tests

        /// <summary>
        /// 测试GetRandomElement方法的性能
        /// </summary>
        [Test, Performance]
        public void GetRandomElement_Performance()
        {
            // 定义测试配置
            var config = new PerformanceTestConfig
            {
                TestName = "GetRandomElement_性能",
                MeasurementCount = 10,
                WarmupCount = 3,
                MeasureGC = true
            };
            
            // 定义基准实现（直接使用Random.Range）
            Action baseline = () =>
            {
                for (int i = 0; i < BenchmarkIterations; i++)
                {
                    int index = UnityEngine.Random.Range(0, _smallCollection.Count);
                    int result = _smallCollection[index];
                }
            };
            
            // 定义CollectionUtils实现
            Action collectionUtils = () =>
            {
                for (int i = 0; i < BenchmarkIterations; i++)
                {
                    int result = CollectionUtils.GetRandomElement(_smallCollection);
                }
            };
            
            // 比较性能
            RunPerformanceTest(baseline, config);
            RunPerformanceTest(collectionUtils, new PerformanceTestConfig
            {
                TestName = config.TestName + "_优化",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC
            });
        }

        /// <summary>
        /// 测试GetRandomElement方法在各种大小集合上的性能
        /// </summary>
        [Test, Performance]
        public void GetRandomElement_VariousSizes_Performance()
        {
            // 小集合
            Measure.Method(() =>
            {
                for (int i = 0; i < BenchmarkIterations; i++)
                {
                    _ = CollectionUtils.GetRandomElement(_smallCollection);
                }
            })
            .WarmupCount(5)
            .MeasurementCount(5)
            .Run();
            
            // 中集合
            Measure.Method(() =>
            {
                for (int i = 0; i < BenchmarkIterations; i++)
                {
                    _ = CollectionUtils.GetRandomElement(_mediumCollection);
                }
            })
            .WarmupCount(5)
            .MeasurementCount(5)
            .Run();
            
            // 大集合
            Measure.Method(() =>
            {
                for (int i = 0; i < BenchmarkIterations; i++)
                {
                    _ = CollectionUtils.GetRandomElement(_largeCollection);
                }
            })
            .WarmupCount(5)
            .MeasurementCount(5)
            .Run();
        }

        /// <summary>
        /// 测试GetRandomElement方法的GC分配
        /// </summary>
        [Test, Performance]
        public void GetRandomElement_GCAllocation()
        {
            // 测量GetRandomElement操作的GC分配
            double smallCollectionAllocation = MeasureGC.Allocation(() => 
                CollectionUtils.GetRandomElement(_smallCollection));
            
            double mediumCollectionAllocation = MeasureGC.Allocation(() => 
                CollectionUtils.GetRandomElement(_mediumCollection));
            
            // 验证
            Assert.Less(smallCollectionAllocation, 128, "从小集合获取随机元素的GC分配应该低于128字节");
        }

        #endregion

        #region Join Performance Tests

        /// <summary>
        /// 测试Join方法的性能
        /// </summary>
        [Test, Performance]
        public void Join_Performance()
        {
            // 准备测试数据
            var target = new List<int>();
            
            // 小集合
            Measure.Method(() =>
            {
                target.Clear();
                CollectionUtils.Join(target, _smallCollection);
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
            
            // 中集合
            Measure.Method(() =>
            {
                target.Clear();
                CollectionUtils.Join(target, _mediumCollection);
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
            
            // 比较: 使用List<T>.AddRange
            Measure.Method(() =>
            {
                target.Clear();
                target.AddRange(_mediumCollection);
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
            
            // 比较: 使用循环添加
            Measure.Method(() =>
            {
                target.Clear();
                foreach (var item in _mediumCollection)
                {
                    target.Add(item);
                }
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        /// <summary>
        /// 测试Join方法的GC分配
        /// </summary>
        [Test, Performance]
        public void Join_GCAllocation()
        {
            // 准备
            var source = new List<int> { 1, 2, 3, 4, 5 };
            var toAdd = new List<int> { 6, 7, 8, 9, 10 };
            
            // 测量CollectionUtils.Join的GC分配
            double joinAllocation = MeasureGC.Allocation(() => 
                CollectionUtils.Join(source, toAdd));
            
            // 测量List<T>.AddRange的GC分配
            double listAddRangeAllocation = MeasureGC.Allocation(() =>
            {
                var result = new List<int>(source);
                result.AddRange(toAdd);
            });
            
            // 验证
            Assert.Less(joinAllocation, 512, "合并两个集合的GC分配应该低于512字节");
        }

        #endregion

        #region JoinToString Performance Tests

        /// <summary>
        /// 测试JoinToString方法的性能
        /// </summary>
        [Test, Performance]
        public void JoinToString_Performance()
        {
            // 小集合
            Measure.Method(() =>
            {
                for (int i = 0; i < 100; i++) // 使用较小的迭代次数，避免字符串操作的开销太大
                {
                    CollectionUtils.JoinToString(_smallCollection, ",");
                }
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .GC()
            .Run();
            
            // 中集合
            Measure.Method(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    CollectionUtils.JoinToString(_mediumCollection, ",");
                }
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
            
            // 比较: 使用string.Join
            Measure.Method(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    string.Join(",", _mediumCollection);
                }
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
            
            // 比较: 使用StringBuilder手动拼接
            Measure.Method(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    StringBuilder sb = new StringBuilder();
                    bool first = true;
                    foreach (var item in _mediumCollection)
                    {
                        if (!first)
                            sb.Append(",");
                        sb.Append(item);
                        first = false;
                    }
                    string result = sb.ToString();
                }
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        /// <summary>
        /// 测试JoinToString带转换器的性能
        /// </summary>
        [Test, Performance]
        public void JoinToString_WithConverter_Performance()
        {
            Func<int, string> converter = i => $"数字{i}";
            
            // 使用CollectionUtils.JoinToString
            Measure.Method(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    // 由于JoinToString没有转换器参数，自己实现转换
                    _ = CollectionUtils.JoinToString(_smallCollection.Select(x => x.ToString("X")).ToList(), ",");
                }
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .Run();
            
            // 使用string.Join
            Measure.Method(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    _ = string.Join(",", _smallCollection.Select(x => x.ToString("X")));
                }
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .Run();
        }

        #endregion

        #region DeepCopy Performance Tests

        /// <summary>
        /// 测试DeepCopy方法的性能
        /// </summary>
        [Test, Performance]
        public void DeepCopy_Performance()
        {
            // 定义测试配置
            var config = new PerformanceTestConfig
            {
                TestName = "DeepCopy_性能",
                MeasurementCount = 10,
                WarmupCount = 3,
                MeasureGC = true
            };
            
            // 小集合
            List<int> smallCollection = new List<int>(Enumerable.Range(0, SmallCollectionSize));
            
            // 定义基准实现（手动复制）
            Action baseline = () =>
            {
                for (int i = 0; i < BenchmarkIterations / 10; i++)
                {
                    var copy = new List<int>(smallCollection);
                }
            };
            
            // 定义CollectionUtils实现
            Action collectionUtils = () =>
            {
                for (int i = 0; i < BenchmarkIterations / 10; i++)
                {
                    _ = CollectionUtils.DeepCopy(smallCollection);
                }
            };
            
            // 比较性能
            RunPerformanceTest(baseline, config);
            RunPerformanceTest(collectionUtils, new PerformanceTestConfig
            {
                TestName = config.TestName + "_优化",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC
            });
        }

        /// <summary>
        /// 测试DeepCopy方法的GC分配
        /// </summary>
        [Test, Performance]
        public void DeepCopy_GCAllocation()
        {
            // 测量不同集合大小的复制操作GC分配
            double smallCollectionAllocation = MeasureGC.Allocation(() => 
                CollectionUtils.DeepCopy(_smallCollection));
            
            double mediumCollectionAllocation = MeasureGC.Allocation(() => 
                CollectionUtils.DeepCopy(_mediumCollection));
            
            // 验证
            Assert.Less(smallCollectionAllocation, 1024, "复制小集合的GC分配应该低于1024字节");
        }

        #endregion

        #region Collection Methods Comparison Tests

        /// <summary>
        /// 比较各种集合操作方法在不同集合大小下的性能
        /// </summary>
        [Test, Performance]
        public void CollectionMethods_SizeComparison_Performance()
        {
            // 在小集合上执行各种操作
            Measure.Method(() =>
            {
                var result = CollectionUtils.IsNullOrEmpty(_smallCollection);
                CollectionUtils.Shuffle(_smallCollection);
                CollectionUtils.GetRandomElement(_smallCollection);
                CollectionUtils.JoinToString(_smallCollection, ",");
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
            
            // 在中集合上执行各种操作
            Measure.Method(() =>
            {
                var result = CollectionUtils.IsNullOrEmpty(_mediumCollection);
                CollectionUtils.Shuffle(_mediumCollection);
                CollectionUtils.GetRandomElement(_mediumCollection);
                CollectionUtils.JoinToString(_mediumCollection, ",");
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
            
            // 在大集合上执行各种操作
            Measure.Method(() =>
            {
                var result = CollectionUtils.IsNullOrEmpty(_largeCollection);
                CollectionUtils.Shuffle(_largeCollection);
                CollectionUtils.GetRandomElement(_largeCollection);
                CollectionUtils.JoinToString(_largeCollection, ",");
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        #endregion

        /// <summary>
        /// 测试CollectionUtils所有相关方法的GC分配情况
        /// </summary>
        [Test]
        [Performance]
        public void CollectionUtils_GCAllocation()
        {
            // IsNullOrEmpty
            MeasureGC.AssertNoAllocation(() => {
                var result = CollectionUtils.IsNullOrEmpty(_smallCollection);
            });
            MeasureGC.AssertNoAllocation(() => {
                var result = CollectionUtils.IsNullOrEmpty(new List<int>());
            });
            
            // ForEach
            MeasureGC.AssertNoAllocation(() => {
                CollectionUtils.ForEach(_smallCollection, x => { var temp = x; });
            });
            
            // Shuffle
            MeasureGC.AssertMaxAllocation(() => {
                var list = new List<int>(_smallCollection);
                CollectionUtils.Shuffle(list);
            }, 1024);
            
            // GetRandomElement
            MeasureGC.AssertMaxAllocation(() => {
                var result = CollectionUtils.GetRandomElement(_smallCollection);
            }, 128);
            
            // Join
            MeasureGC.AssertMaxAllocation(() => {
                var result = CollectionUtils.Join(_smallCollection, _smallCollection);
            }, 512);
            
            // DeepCopy
            MeasureGC.AssertMaxAllocation(() => {
                var result = CollectionUtils.DeepCopy(_smallCollection);
            }, 1024);
        }

        /// <summary>
        /// 序列化测试类
        /// </summary>
        [Serializable]
        private class TestClass
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
} 