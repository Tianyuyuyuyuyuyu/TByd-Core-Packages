using System;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using UnityEngine.Profiling;

namespace TByd.Core.Utils.Tests.Editor.Framework
{
    /// <summary>
    /// GC分配测试工具，用于测量方法的内存分配情况
    /// </summary>
    public static class GCAllocationTester
    {
        /// <summary>
        /// 测量指定方法的内存分配量(字节)
        /// </summary>
        /// <param name="action">要测量的方法</param>
        /// <returns>分配的内存字节数</returns>
        public static long MeasureAllocation(Action action)
        {
            // 强制GC收集，确保测量前内存状态干净
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            
            // 记录初始内存使用量
            long startMemory = Profiler.GetTotalAllocatedMemoryLong();
            
            // 执行测试方法
            action();
            
            // 记录执行后的内存使用量
            long endMemory = Profiler.GetTotalAllocatedMemoryLong();
            
            // 返回分配的内存量
            return endMemory - startMemory;
        }
        
        /// <summary>
        /// 断言指定方法不产生任何GC分配
        /// </summary>
        /// <param name="action">要测试的方法</param>
        /// <param name="message">断言失败时的错误消息</param>
        public static void AssertNoAllocation(Action action, string message = null)
        {
#if UNITY_EDITOR
            // 预热
            for (int i = 0; i < 3; i++)
            {
                action();
            }
            
            long allocated = MeasureAllocation(action);
            
            string errorMessage = message ?? $"预期不产生GC分配，但实际分配了{allocated}字节";
            Assert.AreEqual(0, allocated, errorMessage);
#else
            Debug.LogWarning("GC分配测试只能在编辑器中运行。");
#endif
        }
        
        /// <summary>
        /// 断言指定方法的GC分配不超过指定值
        /// </summary>
        /// <param name="action">要测试的方法</param>
        /// <param name="maxBytes">最大允许分配字节数</param>
        /// <param name="message">断言失败时的错误消息</param>
        public static void AssertMaxAllocation(Action action, long maxBytes, string message = null)
        {
#if UNITY_EDITOR
            // 预热
            for (int i = 0; i < 3; i++)
            {
                action();
            }
            
            long allocated = MeasureAllocation(action);
            
            string errorMessage = message ?? $"GC分配量({allocated}字节)超过最大允许值({maxBytes}字节)";
            Assert.LessOrEqual(allocated, maxBytes, errorMessage);
#else
            Debug.LogWarning("GC分配测试只能在编辑器中运行。");
#endif
        }
        
        /// <summary>
        /// 比较两个方法的内存分配，断言优化版本至少减少指定百分比的分配
        /// </summary>
        /// <param name="baseline">基准方法</param>
        /// <param name="optimized">优化方法</param>
        /// <param name="minImprovementRatio">最小改进比例(0-1之间，例如0.5表示减少50%)</param>
        /// <param name="message">断言失败时的错误消息</param>
        public static void CompareAllocations(Action baseline, Action optimized, float minImprovementRatio = 0.2f, string message = null)
        {
#if UNITY_EDITOR
            if (minImprovementRatio < 0f || minImprovementRatio > 1f)
            {
                throw new ArgumentException("改进比例必须在0到1之间", nameof(minImprovementRatio));
            }
            
            // 预热
            for (int i = 0; i < 3; i++)
            {
                baseline();
                optimized();
            }
            
            // 测量基准方法的分配
            long baselineAllocation = MeasureAllocation(baseline);
            
            // 如果基准方法没有分配，则优化版本也不应有分配
            if (baselineAllocation == 0)
            {
                AssertNoAllocation(optimized, "基准方法没有GC分配，优化版本也不应有GC分配");
                return;
            }
            
            // 测量优化方法的分配
            long optimizedAllocation = MeasureAllocation(optimized);
            
            // 计算实际改进比例
            float actualImprovement = 1f - ((float)optimizedAllocation / baselineAllocation);
            
            string errorMessage = message ?? 
                $"优化版本未达到预期的内存分配改进目标(期望减少: {minImprovementRatio*100}%, 实际减少: {actualImprovement*100}%)，" +
                $"基准方法分配: {baselineAllocation}字节，优化方法分配: {optimizedAllocation}字节";
                
            Assert.GreaterOrEqual(actualImprovement, minImprovementRatio, errorMessage);
#else
            Debug.LogWarning("GC分配测试只能在编辑器中运行。");
#endif
        }
        
        /// <summary>
        /// 运行一个方法多次，并报告每次调用的平均内存分配
        /// </summary>
        /// <param name="action">要测试的方法</param>
        /// <param name="iterations">迭代次数</param>
        /// <returns>平均每次调用的内存分配(字节)</returns>
        public static float MeasureAverageAllocation(Action action, int iterations = 1000)
        {
#if UNITY_EDITOR
            if (iterations <= 0)
            {
                throw new ArgumentException("迭代次数必须大于0", nameof(iterations));
            }
            
            // 预热
            for (int i = 0; i < Math.Min(iterations / 10, 100); i++)
            {
                action();
            }
            
            // 强制GC收集
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            
            // 记录初始内存使用量
            long startMemory = Profiler.GetTotalAllocatedMemoryLong();
            
            // 执行测试方法多次
            for (int i = 0; i < iterations; i++)
            {
                action();
            }
            
            // 记录执行后的内存使用量
            long endMemory = Profiler.GetTotalAllocatedMemoryLong();
            
            // 计算平均每次调用的分配量
            float averageAllocation = (float)(endMemory - startMemory) / iterations;
            
            return averageAllocation;
#else
            Debug.LogWarning("GC分配测试只能在编辑器中运行。");
            return 0f;
#endif
        }
        
        /// <summary>
        /// 防止方法被内联，确保能够准确测量其分配
        /// </summary>
        /// <param name="action">要测量的方法</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void DoNotInline(Action action)
        {
            action();
        }
    }
} 