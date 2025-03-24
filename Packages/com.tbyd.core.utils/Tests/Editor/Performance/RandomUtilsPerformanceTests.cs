using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.PerformanceTesting;
using TByd.Core.Utils.Runtime;
using TByd.Core.Utils.Tests.Editor.Framework;
using Random = UnityEngine.Random;
using UnityEngine;

namespace TByd.Core.Utils.Tests.Editor.Performance
{
    /// <summary>
    /// RandomUtils类的性能测试
    /// </summary>
    [TestFixture]
    [Category("Performance")]
    public class RandomUtilsPerformanceTests : PerformanceTestBase
    {
        private const int BenchmarkIterations = 10000;
        
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            // 准备测试数据，如果需要
        }
        
        /// <summary>
        /// 测试RandomUtils.Bool方法的性能
        /// </summary>
        [Test, Performance]
        public void Bool_Performance()
        {
            // 测量随机布尔值生成性能
            MeasurePerformance(
                () => {
                    for (int i = 0; i < BenchmarkIterations; i++)
                    {
                        _ = RandomUtils.Bool();
                    }
                },
                "Bool(默认概率)"
            );
            
            // 测量指定概率的随机布尔值生成性能
            MeasurePerformance(
                () => {
                    for (int i = 0; i < BenchmarkIterations; i++)
                    {
                        _ = RandomUtils.Bool(0.3f);
                    }
                },
                "Bool(指定概率)"
            );
            
            // 比较与System.Random.NextDouble()的性能
            ComparePerformance(
                // 基准实现 - 使用System.Random
                () => {
                    var random = new System.Random();
                    for (int i = 0; i < BenchmarkIterations; i++)
                    {
                        _ = random.NextDouble() < 0.5;
                    }
                },
                
                // 优化实现 - 使用RandomUtils.Bool
                () => {
                    for (int i = 0; i < BenchmarkIterations; i++)
                    {
                        _ = RandomUtils.Bool();
                    }
                },
                
                "Bool vs Random.NextDouble",
                true // 测量GC分配
            );
        }
        
        /// <summary>
        /// 测试RandomUtils.WeightedRandom方法的性能
        /// </summary>
        [Test, Performance]
        public void WeightedRandom_Performance()
        {
            // 准备测试数据 - 不同大小的数组
            var smallItems = new int[] { 1, 2, 3 };
            var smallWeights = new float[] { 1f, 2f, 3f };
            
            var mediumItems = new int[10];
            var mediumWeights = new float[10];
            for (int i = 0; i < 10; i++)
            {
                mediumItems[i] = i + 1;
                mediumWeights[i] = i + 1;
            }
            
            var largeItems = new int[100];
            var largeWeights = new float[100];
            for (int i = 0; i < 100; i++)
            {
                largeItems[i] = i + 1;
                largeWeights[i] = i + 1;
            }
            
            // 测量小型数组性能
            MeasurePerformance(
                () => {
                    for (int i = 0; i < BenchmarkIterations / 10; i++)
                    {
                        _ = RandomUtils.WeightedRandom(smallItems, smallWeights);
                    }
                },
                "WeightedRandom(小型数组)"
            );
            
            // 测量中型数组性能
            MeasurePerformance(
                () => {
                    for (int i = 0; i < BenchmarkIterations / 10; i++)
                    {
                        _ = RandomUtils.WeightedRandom(mediumItems, mediumWeights);
                    }
                },
                "WeightedRandom(中型数组)"
            );
            
            // 测量大型数组性能
            MeasurePerformance(
                () => {
                    for (int i = 0; i < BenchmarkIterations / 10; i++)
                    {
                        _ = RandomUtils.WeightedRandom(largeItems, largeWeights);
                    }
                },
                "WeightedRandom(大型数组)"
            );
            
            // 测量不同类型性能 - 字符串数组
            var stringItems = new string[] { "A", "B", "C", "D", "E" };
            var stringWeights = new float[] { 1f, 2f, 3f, 4f, 5f };
            
            MeasurePerformance(
                () => {
                    for (int i = 0; i < BenchmarkIterations / 10; i++)
                    {
                        _ = RandomUtils.WeightedRandom(stringItems, stringWeights);
                    }
                },
                "WeightedRandom(字符串数组)"
            );
        }
        
        /// <summary>
        /// 测试RandomUtils.Gaussian方法的性能
        /// </summary>
        [Test, Performance]
        public void Gaussian_Performance()
        {
            // 测量标准正态分布生成性能
            MeasurePerformance(
                () => {
                    for (int i = 0; i < BenchmarkIterations; i++)
                    {
                        _ = RandomUtils.Gaussian();
                    }
                },
                "Gaussian(标准正态分布)"
            );
            
            // 测量自定义正态分布生成性能
            MeasurePerformance(
                () => {
                    for (int i = 0; i < BenchmarkIterations; i++)
                    {
                        _ = RandomUtils.Gaussian(5f, 2f);
                    }
                },
                "Gaussian(自定义正态分布)"
            );
            
            // 比较零标准差情况（优化路径）
            MeasurePerformance(
                () => {
                    for (int i = 0; i < BenchmarkIterations; i++)
                    {
                        _ = RandomUtils.Gaussian(5f, 0f);
                    }
                },
                "Gaussian(零标准差)"
            );

            // 比较与手动正态分布生成的性能
            ComparePerformance(
                // 基准实现 - 手动实现的Box-Muller变换
                () => {
                    var random = new System.Random();
                    for (int i = 0; i < BenchmarkIterations; i++)
                    {
                        float u1 = (float)random.NextDouble();
                        float u2 = (float)random.NextDouble();
                        float z0 = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * u2);
                        _ = z0;
                    }
                },
                
                // 优化实现 - 使用RandomUtils.Gaussian
                () => {
                    for (int i = 0; i < BenchmarkIterations; i++)
                    {
                        _ = RandomUtils.Gaussian();
                    }
                },
                
                "Gaussian vs 手动Box-Muller变换",
                true // 测量GC分配
            );
        }
        
        /// <summary>
        /// 测试RandomUtils.ColorHSV方法的性能
        /// </summary>
        [Test, Performance]
        public void ColorHSV_Performance()
        {
            // 测量默认参数的HSV颜色生成性能
            MeasurePerformance(
                () => {
                    for (int i = 0; i < BenchmarkIterations; i++)
                    {
                        _ = RandomUtils.ColorHSV();
                    }
                },
                "ColorHSV(默认参数)"
            );
            
            // 测量自定义参数的HSV颜色生成性能
            MeasurePerformance(
                () => {
                    for (int i = 0; i < BenchmarkIterations; i++)
                    {
                        _ = RandomUtils.ColorHSV(0.2f, 0.8f, 0.3f, 0.9f);
                    }
                },
                "ColorHSV(自定义参数)"
            );
            
            // 比较与Unity内置Random.ColorHSV的性能
            ComparePerformance(
                // 基准实现 - 使用Unity Random.ColorHSV
                () => {
                    for (int i = 0; i < BenchmarkIterations; i++)
                    {
                        _ = Random.ColorHSV();
                    }
                },
                
                // 优化实现 - 使用RandomUtils.ColorHSV
                () => {
                    for (int i = 0; i < BenchmarkIterations; i++)
                    {
                        _ = RandomUtils.ColorHSV();
                    }
                },
                
                "ColorHSV vs Unity Random.ColorHSV",
                true // 测量GC分配
            );
        }
        
        /// <summary>
        /// 测试RandomUtils.GenerateId方法的性能
        /// </summary>
        [Test, Performance]
        public void GenerateId_Performance()
        {
            // 测量不同长度ID生成的性能
            var lengths = new[] { 8, 16, 32, 64, 128 };
            
            foreach (int length in lengths)
            {
                MeasurePerformance(
                    () => {
                        for (int i = 0; i < BenchmarkIterations / 100; i++) // 减少迭代次数因为这是个重操作
                        {
                            _ = RandomUtils.GenerateId(length);
                        }
                    },
                    $"GenerateId(length={length})"
                );
            }
            
            // 测量包含特殊字符时的性能影响
            MeasurePerformance(
                () => {
                    for (int i = 0; i < BenchmarkIterations / 100; i++)
                    {
                        _ = RandomUtils.GenerateId(16, true);
                    }
                },
                "GenerateId(includeSpecialChars=true)"
            );
            
            MeasurePerformance(
                () => {
                    for (int i = 0; i < BenchmarkIterations / 100; i++)
                    {
                        _ = RandomUtils.GenerateId(16, false);
                    }
                },
                "GenerateId(includeSpecialChars=false)"
            );
            
            // 比较与Guid.NewGuid().ToString()的性能（生成16字符ID）
            ComparePerformance(
                // 基准实现 - 使用Guid生成
                () => {
                    for (int i = 0; i < BenchmarkIterations / 100; i++)
                    {
                        _ = System.Guid.NewGuid().ToString().Substring(0, 16);
                    }
                },
                
                // 优化实现 - 使用RandomUtils.GenerateId
                () => {
                    for (int i = 0; i < BenchmarkIterations / 100; i++)
                    {
                        _ = RandomUtils.GenerateId(16);
                    }
                },
                
                "GenerateId vs Guid生成16字符ID",
                true // 测量GC分配
            );
        }
        
        /// <summary>
        /// 测试RandomUtils各方法的GC分配
        /// </summary>
        [Test]
        [Performance]
        public void RandomUtils_GCAllocation()
        {
            // 测试RandomUtils.Range的内存分配（应该为0）
            MeasureGC.AssertNoAllocation(() => {
                for (int i = 0; i < 1000; i++)
                {
                    var value = RandomUtils.Range(0, 100);
                }
            }, "Range方法应该没有内存分配");
            
            // 测试RandomUtils.Bool的内存分配（应该为0）
            MeasureGC.AssertNoAllocation(() => {
                for (int i = 0; i < 1000; i++)
                {
                    var value = RandomUtils.Bool();
                }
            }, "Bool方法应该没有内存分配");
            
            // 测试RandomUtils.Range的内存分配（应小于16字节）
            MeasureGC.AssertMaxAllocation(() => {
                for (int i = 0; i < 100; i++)
                {
                    var value = RandomUtils.Range(-10, 10);
                }
            }, 16, "Range方法应该几乎没有内存分配");
            
            // 测试RandomUtils.Gaussian方法应该有少量GC分配
            MeasureGC.AssertMaxAllocation(() => {
                for (int i = 0; i < 10; i++)
                {
                    var value = RandomUtils.Gaussian();
                }
            }, 100, "Gaussian方法应该有少量GC分配");
            
            // 测试WeightedRandom方法的数组参数可能导致装箱，但应该有优化
            var items = new int[] { 1, 2, 3, 4, 5 };
            var weights = new float[] { 1f, 2f, 3f, 4f, 5f };
            
            MeasureGC.AssertMaxAllocation(() => {
                for (int i = 0; i < 10; i++)
                {
                    var value = RandomUtils.WeightedRandom(items, weights);
                }
            }, 200, "WeightedRandom方法应该有可控的GC分配");
            
            // 字符串生成方法必然有GC分配，但应该合理控制
            MeasureGC.AssertMaxAllocation(() => {
                var id = RandomUtils.GenerateId(10);
            }, 500, "GenerateId方法的GC分配应该在合理范围内");
        }
    }
} 