using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace TByd.Core.Utils.Tests.Runtime.Performance
{
    /// <summary>
    /// 性能测试框架，用于自动化测试各工具类的性能并生成报告
    /// </summary>
    public class PerformanceTestFramework
    {
        // 测试配置
        private const int WarmupCount = 3;
        private const int MeasurementCount = 10;
        private const string ReportDirectory = "PerformanceReports";
        
        // 测试结果存储
        private static Dictionary<string, TestResult> testResults = new Dictionary<string, TestResult>();
        
        /// <summary>
        /// 测试结果类，存储性能测试的详细信息
        /// </summary>
        public class TestResult
        {
            public string TestName { get; set; }
            public string Category { get; set; }
            public double ExecutionTimeMs { get; set; }
            public long MemoryAllocationBytes { get; set; }
            public int GCCallCount { get; set; }
            public string BaselineTestName { get; set; }
            public double BaselineExecutionTimeMs { get; set; }
            public long BaselineMemoryAllocationBytes { get; set; }
            public int BaselineGCCallCount { get; set; }
            
            public double ExecutionTimeImprovement => 
                BaselineExecutionTimeMs > 0 ? 
                (BaselineExecutionTimeMs - ExecutionTimeMs) / BaselineExecutionTimeMs * 100 : 0;
                
            public double MemoryAllocationImprovement => 
                BaselineMemoryAllocationBytes > 0 ? 
                (BaselineMemoryAllocationBytes - MemoryAllocationBytes) / (double)BaselineMemoryAllocationBytes * 100 : 0;
                
            public double GCCallImprovement => 
                BaselineGCCallCount > 0 ? 
                (BaselineGCCallCount - GCCallCount) / (double)BaselineGCCallCount * 100 : 0;
        }
        
        /// <summary>
        /// 运行性能测试并记录结果
        /// </summary>
        /// <param name="testName">测试名称</param>
        /// <param name="category">测试类别</param>
        /// <param name="testAction">测试方法</param>
        /// <param name="baselineTestName">基准测试名称（可选）</param>
        /// <param name="iterations">迭代次数</param>
        public static void RunPerformanceTest(
            string testName, 
            string category, 
            Action testAction, 
            string baselineTestName = null,
            int iterations = 1000)
        {
            // 预热
            for (int i = 0; i < WarmupCount; i++)
            {
                testAction();
            }
            
            // 测量执行时间
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            // 测量内存分配
            long startMemory = GC.GetTotalMemory(true);
            int startGCCount = GC.CollectionCount(0) + GC.CollectionCount(1) + GC.CollectionCount(2);
            
            // 执行测试
            for (int i = 0; i < iterations; i++)
            {
                testAction();
            }
            
            // 计算结果
            long endMemory = GC.GetTotalMemory(false);
            int endGCCount = GC.CollectionCount(0) + GC.CollectionCount(1) + GC.CollectionCount(2);
            stopwatch.Stop();
            
            // 记录结果
            TestResult result = new TestResult
            {
                TestName = testName,
                Category = category,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                MemoryAllocationBytes = endMemory - startMemory,
                GCCallCount = endGCCount - startGCCount
            };
            
            // 如果有基准测试，关联基准测试结果
            if (!string.IsNullOrEmpty(baselineTestName) && testResults.TryGetValue(baselineTestName, out TestResult baseline))
            {
                result.BaselineTestName = baselineTestName;
                result.BaselineExecutionTimeMs = baseline.ExecutionTimeMs;
                result.BaselineMemoryAllocationBytes = baseline.MemoryAllocationBytes;
                result.BaselineGCCallCount = baseline.GCCallCount;
            }
            
            // 存储结果
            testResults[testName] = result;
            
            // 输出结果
            UnityEngine.Debug.Log($"性能测试 [{category}] {testName}: " +
                                 $"执行时间 = {result.ExecutionTimeMs}ms, " +
                                 $"内存分配 = {result.MemoryAllocationBytes / 1024.0:F2}KB, " +
                                 $"GC调用 = {result.GCCallCount}");
                                 
            if (!string.IsNullOrEmpty(baselineTestName) && testResults.ContainsKey(baselineTestName))
            {
                UnityEngine.Debug.Log($"与基准测试 {baselineTestName} 相比: " +
                                     $"执行时间提升 = {result.ExecutionTimeImprovement:F2}%, " +
                                     $"内存分配减少 = {result.MemoryAllocationImprovement:F2}%, " +
                                     $"GC调用减少 = {result.GCCallImprovement:F2}%");
            }
        }
        
        /// <summary>
        /// 运行对比测试，比较两个实现的性能差异
        /// </summary>
        /// <param name="testName">测试名称</param>
        /// <param name="testAction">测试方法</param>
        /// <param name="baselineTestName">基准测试名称</param>
        /// <param name="baselineAction">基准测试方法</param>
        /// <param name="iterations">迭代次数</param>
        public void RunTest(
            string testName,
            Action testAction,
            string baselineTestName,
            Action baselineAction,
            int iterations = 1000)
        {
            // 先运行基准测试
            PerformanceTestFramework.RunPerformanceTest(
                baselineTestName,
                "性能对比",
                baselineAction,
                null,
                iterations);
                
            // 再运行测试方法
            PerformanceTestFramework.RunPerformanceTest(
                testName,
                "性能对比",
                testAction,
                baselineTestName,
                iterations);
        }
        
        /// <summary>
        /// 运行内存分配测试，比较两个实现的内存分配差异
        /// </summary>
        /// <param name="testName">测试名称</param>
        /// <param name="testAction">测试方法</param>
        /// <param name="baselineTestName">基准测试名称</param>
        /// <param name="baselineAction">基准测试方法</param>
        /// <param name="iterations">迭代次数</param>
        public void RunMemoryTest(
            string testName,
            Action testAction,
            string baselineTestName,
            Action baselineAction,
            int iterations = 1000)
        {
            // 先运行基准测试
            PerformanceTestFramework.RunPerformanceTest(
                baselineTestName,
                "内存分配对比",
                baselineAction,
                null,
                iterations);
                
            // 再运行测试方法
            PerformanceTestFramework.RunPerformanceTest(
                testName,
                "内存分配对比",
                testAction,
                baselineTestName,
                iterations);
        }
        
        /// <summary>
        /// 生成性能测试报告
        /// </summary>
        /// <param name="reportName">报告名称</param>
        public static void GenerateReport(string reportName)
        {
            // 确保报告目录存在
            string reportPath = Path.Combine(Application.persistentDataPath, ReportDirectory);
            if (!Directory.Exists(reportPath))
            {
                Directory.CreateDirectory(reportPath);
            }
            
            // 创建报告文件
            string filePath = Path.Combine(reportPath, $"{reportName}_{DateTime.Now:yyyyMMdd_HHmmss}.md");
            StringBuilder report = new StringBuilder();
            
            // 添加报告标题
            report.AppendLine($"# {reportName} 性能测试报告");
            report.AppendLine($"生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();
            
            // 按类别分组测试结果
            Dictionary<string, List<TestResult>> resultsByCategory = new Dictionary<string, List<TestResult>>();
            foreach (var result in testResults.Values)
            {
                if (!resultsByCategory.TryGetValue(result.Category, out List<TestResult> categoryResults))
                {
                    categoryResults = new List<TestResult>();
                    resultsByCategory[result.Category] = categoryResults;
                }
                
                categoryResults.Add(result);
            }
            
            // 生成每个类别的报告
            foreach (var category in resultsByCategory.Keys)
            {
                report.AppendLine($"## {category}");
                report.AppendLine();
                
                // 创建表格
                report.AppendLine("| 测试名称 | 执行时间 (ms) | 内存分配 (KB) | GC调用次数 | 基准测试 | 执行时间提升 | 内存分配减少 | GC调用减少 |");
                report.AppendLine("|---------|--------------|--------------|------------|----------|--------------|--------------|------------|");
                
                foreach (var result in resultsByCategory[category])
                {
                    string baselineInfo = string.IsNullOrEmpty(result.BaselineTestName) ? "-" : result.BaselineTestName;
                    string timeImprovement = string.IsNullOrEmpty(result.BaselineTestName) ? "-" : $"{result.ExecutionTimeImprovement:F2}%";
                    string memoryImprovement = string.IsNullOrEmpty(result.BaselineTestName) ? "-" : $"{result.MemoryAllocationImprovement:F2}%";
                    string gcImprovement = string.IsNullOrEmpty(result.BaselineTestName) ? "-" : $"{result.GCCallImprovement:F2}%";
                    
                    report.AppendLine($"| {result.TestName} | {result.ExecutionTimeMs:F2} | {result.MemoryAllocationBytes / 1024.0:F2} | {result.GCCallCount} | {baselineInfo} | {timeImprovement} | {memoryImprovement} | {gcImprovement} |");
                }
                
                report.AppendLine();
            }
            
            // 添加测试环境信息
            report.AppendLine("## 测试环境");
            report.AppendLine();
            report.AppendLine($"- 操作系统: {SystemInfo.operatingSystem}");
            report.AppendLine($"- 处理器: {SystemInfo.processorType}");
            report.AppendLine($"- 内存: {SystemInfo.systemMemorySize} MB");
            report.AppendLine($"- Unity版本: {Application.unityVersion}");
            report.AppendLine($"- 测试时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            
            // 写入文件
            File.WriteAllText(filePath, report.ToString());
            
            UnityEngine.Debug.Log($"性能测试报告已生成: {filePath}");
        }
        
        /// <summary>
        /// 清除所有测试结果
        /// </summary>
        public static void ClearResults()
        {
            testResults.Clear();
        }
        
        /// <summary>
        /// 获取指定测试的结果
        /// </summary>
        /// <param name="testName">测试名称</param>
        /// <returns>测试结果，如果不存在则返回null</returns>
        public static TestResult GetTestResult(string testName)
        {
            testResults.TryGetValue(testName, out TestResult result);
            return result;
        }
        
        /// <summary>
        /// 获取所有测试结果
        /// </summary>
        /// <returns>所有测试结果的字典</returns>
        public static Dictionary<string, TestResult> GetAllTestResults()
        {
            return new Dictionary<string, TestResult>(testResults);
        }
    }
} 