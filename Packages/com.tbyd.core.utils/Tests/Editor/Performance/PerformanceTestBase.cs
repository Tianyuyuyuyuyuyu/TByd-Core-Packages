using System;
using NUnit.Framework;
using TByd.Core.Utils.Tests.Editor.Framework;
using Unity.PerformanceTesting;
using UnityEngine;

namespace TByd.Core.Utils.Tests.Editor.Performance
{
    /// <summary>
    /// 性能测试基类，提供性能测试基础设施
    /// </summary>
    public abstract class PerformanceTestBase : TestBase
    {
        /// <summary>
        /// 性能测试的默认测量次数
        /// </summary>
        protected const int DefaultMeasurementCount = 10;
        
        /// <summary>
        /// 性能测试的默认预热次数
        /// </summary>
        protected const int DefaultWarmupCount = 3;
        
        /// <summary>
        /// 运行性能测试的配置
        /// </summary>
        protected struct PerformanceTestConfig
        {
            /// <summary>
            /// 测试名称
            /// </summary>
            public string TestName;
            
            /// <summary>
            /// 测量次数
            /// </summary>
            public int MeasurementCount;
            
            /// <summary>
            /// 预热次数
            /// </summary>
            public int WarmupCount;
            
            /// <summary>
            /// 是否测量GC分配
            /// </summary>
            public bool MeasureGC;
            
            /// <summary>
            /// 是否为基准测试
            /// </summary>
            public bool IsBaseline;
        }
        
        /// <summary>
        /// 运行性能测试
        /// </summary>
        /// <param name="action">待测试的方法</param>
        /// <param name="config">测试配置</param>
        protected void RunPerformanceTest(Action action, PerformanceTestConfig config)
        {
#if UNITY_PERFORMANCE_TESTS
            var testName = config.TestName ?? TestContext.CurrentContext.Test.Name;
            
            // 创建性能测试
            var sampleGroup = new SampleGroup(testName, SampleUnit.Millisecond);
            
            // 添加GC采样组
            SampleGroup gcSampleGroup = null;
            if (config.MeasureGC)
            {
                gcSampleGroup = new SampleGroup($"{testName}_GC", SampleUnit.Byte);
            }
            
            // 预热
            for (int i = 0; i < config.WarmupCount; i++)
            {
                action();
            }
            
            // 测量性能
            for (int i = 0; i < config.MeasurementCount; i++)
            {
                if (config.MeasureGC)
                {
                    // 测量GC分配
                    long memoryBefore = GC.GetTotalMemory(true);
                    
                    // 测量时间
                    float startTime = Time.realtimeSinceStartup;
                    action();
                    float endTime = Time.realtimeSinceStartup;
                    
                    // 计算GC分配
                    long memoryAfter = GC.GetTotalMemory(false);
                    long allocated = memoryAfter - memoryBefore;
                    
                    // 记录样本
                    Measure.Custom(sampleGroup, (endTime - startTime) * 1000f);
                    Measure.Custom(gcSampleGroup, allocated);
                }
                else
                {
                    // 仅测量时间
                    float startTime = Time.realtimeSinceStartup;
                    action();
                    float endTime = Time.realtimeSinceStartup;
                    
                    // 记录样本
                    Measure.Custom(sampleGroup, (endTime - startTime) * 1000f);
                }
            }
#else
            // 非性能测试环境下的简单计时
            // 预热
            for (int i = 0; i < config.WarmupCount; i++)
            {
                action();
            }
            
            // 记录测试结果
            List<float> measurements = new List<float>();
            
            // 测量
            for (int i = 0; i < config.MeasurementCount; i++)
            {
                float startTime = Time.realtimeSinceStartup;
                action();
                float endTime = Time.realtimeSinceStartup;
                
                measurements.Add((endTime - startTime) * 1000f);
            }
            
            // 计算平均值和标准差
            float sum = 0f;
            foreach (float measurement in measurements)
            {
                sum += measurement;
            }
            
            float average = sum / measurements.Count;
            
            // 计算标准差
            float varianceSum = 0f;
            foreach (float measurement in measurements)
            {
                float diff = measurement - average;
                varianceSum += diff * diff;
            }
            
            float stdDev = Mathf.Sqrt(varianceSum / measurements.Count);
            
            // 输出结果
            LogInfo($"性能测试 {config.TestName}: 平均时间 {average:F2} ms, 标准差 {stdDev:F2} ms");
#endif
        }
        
        /// <summary>
        /// 运行带参数的性能测试
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="action">待测试的方法</param>
        /// <param name="parameter">参数</param>
        /// <param name="config">测试配置</param>
        protected void RunPerformanceTest<T>(Action<T> action, T parameter, PerformanceTestConfig config)
        {
            RunPerformanceTest(() => action(parameter), config);
        }
        
        /// <summary>
        /// 对比两个实现的性能差异
        /// </summary>
        /// <param name="baseline">基准实现</param>
        /// <param name="optimized">优化实现</param>
        /// <param name="testName">测试名称</param>
        /// <param name="measureGC">是否测量GC分配</param>
        /// <param name="measurementCount">测量次数</param>
        /// <param name="warmupCount">预热次数</param>
        protected void ComparePerformance(Action baseline, Action optimized, string testName, 
                                          bool measureGC = true, int measurementCount = DefaultMeasurementCount, 
                                          int warmupCount = DefaultWarmupCount)
        {
            // 运行基准测试
            RunPerformanceTest(baseline, new PerformanceTestConfig
            {
                TestName = $"{testName}_Baseline",
                MeasurementCount = measurementCount,
                WarmupCount = warmupCount,
                MeasureGC = measureGC,
                IsBaseline = true
            });
            
            // 运行优化测试
            RunPerformanceTest(optimized, new PerformanceTestConfig
            {
                TestName = $"{testName}_Optimized",
                MeasurementCount = measurementCount,
                WarmupCount = warmupCount,
                MeasureGC = measureGC,
                IsBaseline = false
            });
        }
        
        /// <summary>
        /// 运行简单性能测试
        /// </summary>
        /// <param name="action">待测试的方法</param>
        /// <param name="testName">测试名称</param>
        /// <param name="measureGC">是否测量GC分配</param>
        protected void MeasurePerformance(Action action, string testName, bool measureGC = true)
        {
            RunPerformanceTest(action, new PerformanceTestConfig
            {
                TestName = testName,
                MeasurementCount = DefaultMeasurementCount,
                WarmupCount = DefaultWarmupCount,
                MeasureGC = measureGC,
                IsBaseline = false
            });
        }

        /// <summary>
        /// 测量指定操作的垃圾回收分配量（字节）
        /// </summary>
        /// <param name="action">要测量的操作</param>
        /// <returns>分配的内存量（字节）</returns>
        protected double MeasureGCAllocation(Action action)
        {
            // 使用MeasureGC测量内存分配
            return MeasureGC.Allocation(action);
        }

        /// <summary>
        /// 测量指定操作的平均垃圾回收分配量（字节）
        /// </summary>
        /// <param name="action">要测量的操作</param>
        /// <param name="iterations">迭代次数</param>
        /// <returns>平均分配的内存量（字节）</returns>
        protected double MeasureAverageGCAllocation(Action action, int iterations = 1000)
        {
            // 使用MeasureGC测量平均内存分配
            return MeasureGC.AverageAllocation(action, iterations);
        }

        /// <summary>
        /// 断言指定操作的最大内存分配量
        /// </summary>
        /// <param name="action">要测量的操作</param>
        /// <param name="maxBytes">最大允许分配的内存量（字节）</param>
        /// <param name="message">断言失败时的消息</param>
        protected void AssertMaxGCAllocation(Action action, double maxBytes, string message = null)
        {
            // 使用MeasureGC断言最大内存分配
            MeasureGC.AssertMaxAllocation(action, (long)maxBytes, message);
        }
    }
} 