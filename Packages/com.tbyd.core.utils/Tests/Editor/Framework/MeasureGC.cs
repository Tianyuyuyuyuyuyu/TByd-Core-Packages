using System;

namespace TByd.Core.Utils.Tests.Editor.Framework
{
    /// <summary>
    /// GC内存分配测量工具
    /// </summary>
    public static class MeasureGC
    {
        /// <summary>
        /// 测量指定方法的内存分配量(字节)
        /// </summary>
        /// <param name="action">要测量的方法</param>
        /// <returns>分配的内存字节数</returns>
        public static long Allocation(Action action)
        {
            return GCAllocationTester.MeasureAllocation(action);
        }
        
        /// <summary>
        /// 断言指定方法不产生任何GC分配
        /// </summary>
        /// <param name="action">要测试的方法</param>
        /// <param name="message">断言失败时的错误消息</param>
        public static void AssertNoAllocation(Action action, string message = null)
        {
            GCAllocationTester.AssertNoAllocation(action, message);
        }
        
        /// <summary>
        /// 断言指定方法的GC分配不超过指定值
        /// </summary>
        /// <param name="action">要测试的方法</param>
        /// <param name="maxBytes">最大允许分配字节数</param>
        /// <param name="message">断言失败时的错误消息</param>
        public static void AssertMaxAllocation(Action action, long maxBytes, string message = null)
        {
            GCAllocationTester.AssertMaxAllocation(action, maxBytes, message);
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
            GCAllocationTester.CompareAllocations(baseline, optimized, minImprovementRatio, message);
        }
        
        /// <summary>
        /// 运行一个方法多次，并报告每次调用的平均内存分配
        /// </summary>
        /// <param name="action">要测试的方法</param>
        /// <param name="iterations">迭代次数</param>
        /// <returns>平均每次调用的内存分配(字节)</returns>
        public static float AverageAllocation(Action action, int iterations = 1000)
        {
            return GCAllocationTester.MeasureAverageAllocation(action, iterations);
        }
    }
} 