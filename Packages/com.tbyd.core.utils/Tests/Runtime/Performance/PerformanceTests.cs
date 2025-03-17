using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TByd.Core.Utils.Runtime;
using System.Diagnostics;
using System.Linq;

namespace TByd.Core.Utils.Tests.Runtime.Performance
{
    /// <summary>
    /// 性能测试类，用于测试和比较各个工具类的性能
    /// 注意：此类中的测试正在逐步迁移到使用PerformanceTestFramework的专用测试类中
    /// 请参考IOUtilsPerformanceTests、StringUtilsPerformanceTests等类
    /// </summary>
    public class PerformanceTests
    {
        private const int IterationCount = 10000;
        private const int WarmupCount = 100;
        
        [Test]
        public void StringUtils_Performance_Test()
        {
            // 准备测试数据
            string testString = "这是一个用于性能测试的字符串，包含中文和English以及数字123456789";
            
            // 预热
            for (int i = 0; i < WarmupCount; i++)
            {
                StringUtils.IsNullOrEmpty(testString);
                StringUtils.IsNullOrWhiteSpace(testString);
                StringUtils.Truncate(testString, 10);
                StringUtils.EncodeToBase64(testString);
                StringUtils.DecodeFromBase64(StringUtils.EncodeToBase64(testString));
            }
            
            // 测试IsNullOrEmpty性能
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < IterationCount; i++)
            {
                StringUtils.IsNullOrEmpty(testString);
            }
            sw.Stop();
            UnityEngine.Debug.Log($"StringUtils.IsNullOrEmpty: {sw.ElapsedMilliseconds}ms for {IterationCount} iterations");
            
            // 测试原生string.IsNullOrEmpty性能
            sw.Reset();
            sw.Start();
            for (int i = 0; i < IterationCount; i++)
            {
                string.IsNullOrEmpty(testString);
            }
            sw.Stop();
            UnityEngine.Debug.Log($"string.IsNullOrEmpty: {sw.ElapsedMilliseconds}ms for {IterationCount} iterations");
            
            // 测试IsNullOrWhiteSpace性能
            sw.Reset();
            sw.Start();
            for (int i = 0; i < IterationCount; i++)
            {
                StringUtils.IsNullOrWhiteSpace(testString);
            }
            sw.Stop();
            UnityEngine.Debug.Log($"StringUtils.IsNullOrWhiteSpace: {sw.ElapsedMilliseconds}ms for {IterationCount} iterations");
            
            // 测试原生string.IsNullOrWhiteSpace性能
            sw.Reset();
            sw.Start();
            for (int i = 0; i < IterationCount; i++)
            {
                string.IsNullOrWhiteSpace(testString);
            }
            sw.Stop();
            UnityEngine.Debug.Log($"string.IsNullOrWhiteSpace: {sw.ElapsedMilliseconds}ms for {IterationCount} iterations");
            
            // 测试Truncate性能
            sw.Reset();
            sw.Start();
            for (int i = 0; i < IterationCount; i++)
            {
                StringUtils.Truncate(testString, 10);
            }
            sw.Stop();
            UnityEngine.Debug.Log($"StringUtils.Truncate: {sw.ElapsedMilliseconds}ms for {IterationCount} iterations");
            
            // 测试原生Substring性能
            sw.Reset();
            sw.Start();
            for (int i = 0; i < IterationCount; i++)
            {
                string result = testString.Length > 10 ? testString.Substring(0, 10) : testString;
            }
            sw.Stop();
            UnityEngine.Debug.Log($"string.Substring: {sw.ElapsedMilliseconds}ms for {IterationCount} iterations");
            
            // 测试Base64编码性能
            sw.Reset();
            sw.Start();
            for (int i = 0; i < IterationCount; i++)
            {
                StringUtils.EncodeToBase64(testString);
            }
            sw.Stop();
            UnityEngine.Debug.Log($"StringUtils.EncodeToBase64: {sw.ElapsedMilliseconds}ms for {IterationCount} iterations");
            
            // 测试原生Base64编码性能
            sw.Reset();
            sw.Start();
            for (int i = 0; i < IterationCount; i++)
            {
                Convert.ToBase64String(Encoding.UTF8.GetBytes(testString));
            }
            sw.Stop();
            UnityEngine.Debug.Log($"Convert.ToBase64String: {sw.ElapsedMilliseconds}ms for {IterationCount} iterations");
            
            // 测试内存分配
            long beforeMemory = GC.GetTotalMemory(true);
            for (int i = 0; i < IterationCount; i++)
            {
                StringUtils.Truncate(testString, 10);
            }
            long afterMemory = GC.GetTotalMemory(true);
            UnityEngine.Debug.Log($"StringUtils.Truncate memory allocation: {(afterMemory - beforeMemory) / 1024.0f}KB for {IterationCount} iterations");
            
            beforeMemory = GC.GetTotalMemory(true);
            for (int i = 0; i < IterationCount; i++)
            {
                string result = testString.Length > 10 ? testString.Substring(0, 10) : testString;
            }
            afterMemory = GC.GetTotalMemory(true);
            UnityEngine.Debug.Log($"string.Substring memory allocation: {(afterMemory - beforeMemory) / 1024.0f}KB for {IterationCount} iterations");
        }
        
        [Test]
        public void MathUtils_Performance_Test()
        {
            // 准备测试数据
            Vector3 startPos = new Vector3(1, 2, 3);
            Vector3 targetPos = new Vector3(4, 5, 6);
            Vector3 velocity = Vector3.zero;
            
            // 预热
            for (int i = 0; i < WarmupCount; i++)
            {
                MathUtils.SmoothDamp(startPos, targetPos, ref velocity, 0.1f);
                MathUtils.Remap(0.5f, 0, 1, 0, 100);
                MathUtils.DirectionToRotation(targetPos - startPos);
            }
            
            // 测试SmoothDamp性能
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < IterationCount; i++)
            {
                MathUtils.SmoothDamp(startPos, targetPos, ref velocity, 0.1f);
            }
            sw.Stop();
            UnityEngine.Debug.Log($"MathUtils.SmoothDamp: {sw.ElapsedMilliseconds}ms for {IterationCount} iterations");
            
            // 测试原生Vector3.SmoothDamp性能
            velocity = Vector3.zero;
            sw.Reset();
            sw.Start();
            for (int i = 0; i < IterationCount; i++)
            {
                Vector3.SmoothDamp(startPos, targetPos, ref velocity, 0.1f, Mathf.Infinity, Time.deltaTime);
            }
            sw.Stop();
            UnityEngine.Debug.Log($"Vector3.SmoothDamp: {sw.ElapsedMilliseconds}ms for {IterationCount} iterations");
            
            // 测试Remap性能
            sw.Reset();
            sw.Start();
            for (int i = 0; i < IterationCount; i++)
            {
                MathUtils.Remap(0.5f, 0, 1, 0, 100);
            }
            sw.Stop();
            UnityEngine.Debug.Log($"MathUtils.Remap: {sw.ElapsedMilliseconds}ms for {IterationCount} iterations");
            
            // 测试手动实现Remap性能
            sw.Reset();
            sw.Start();
            for (int i = 0; i < IterationCount; i++)
            {
                float normalizedValue = (0.5f - 0) / (1 - 0);
                Mathf.Lerp(0, 100, normalizedValue);
            }
            sw.Stop();
            UnityEngine.Debug.Log($"Manual Remap: {sw.ElapsedMilliseconds}ms for {IterationCount} iterations");
            
            // 测试DirectionToRotation性能
            sw.Reset();
            sw.Start();
            for (int i = 0; i < IterationCount; i++)
            {
                MathUtils.DirectionToRotation(targetPos - startPos);
            }
            sw.Stop();
            UnityEngine.Debug.Log($"MathUtils.DirectionToRotation: {sw.ElapsedMilliseconds}ms for {IterationCount} iterations");
            
            // 测试原生Quaternion.LookRotation性能
            sw.Reset();
            sw.Start();
            for (int i = 0; i < IterationCount; i++)
            {
                Quaternion.LookRotation(targetPos - startPos);
            }
            sw.Stop();
            UnityEngine.Debug.Log($"Quaternion.LookRotation: {sw.ElapsedMilliseconds}ms for {IterationCount} iterations");
        }
        
        [Test]
        public void CollectionUtils_Performance_Test()
        {
            // 准备测试数据
            List<int> testList = new List<int>();
            for (int i = 0; i < 1000; i++)
            {
                testList.Add(UnityEngine.Random.Range(0, 1000));
            }
            
            // 预热
            for (int i = 0; i < WarmupCount; i++)
            {
                CollectionUtils.IsNullOrEmpty(testList);
                CollectionUtils.GetRandomElement(testList);
                CollectionUtils.Shuffle(testList);
            }
            
            // 测试IsNullOrEmpty性能
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < IterationCount; i++)
            {
                CollectionUtils.IsNullOrEmpty(testList);
            }
            sw.Stop();
            UnityEngine.Debug.Log($"CollectionUtils.IsNullOrEmpty: {sw.ElapsedMilliseconds}ms for {IterationCount} iterations");
            
            // 测试手动检查IsNullOrEmpty性能
            sw.Reset();
            sw.Start();
            for (int i = 0; i < IterationCount; i++)
            {
                bool isEmpty = testList == null || testList.Count == 0;
            }
            sw.Stop();
            UnityEngine.Debug.Log($"Manual IsNullOrEmpty: {sw.ElapsedMilliseconds}ms for {IterationCount} iterations");
            
            // 测试GetRandomElement性能
            sw.Reset();
            sw.Start();
            for (int i = 0; i < IterationCount; i++)
            {
                CollectionUtils.GetRandomElement(testList);
            }
            sw.Stop();
            UnityEngine.Debug.Log($"CollectionUtils.GetRandomElement: {sw.ElapsedMilliseconds}ms for {IterationCount} iterations");
            
            // 测试手动获取随机元素性能
            sw.Reset();
            sw.Start();
            for (int i = 0; i < IterationCount; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, testList.Count);
                int element = testList[randomIndex];
            }
            sw.Stop();
            UnityEngine.Debug.Log($"Manual GetRandomElement: {sw.ElapsedMilliseconds}ms for {IterationCount} iterations");
            
            // 测试Shuffle性能
            sw.Reset();
            sw.Start();
            for (int i = 0; i < 100; i++) // 减少迭代次数，因为Shuffle操作较重
            {
                CollectionUtils.Shuffle(testList);
            }
            sw.Stop();
            UnityEngine.Debug.Log($"CollectionUtils.Shuffle: {sw.ElapsedMilliseconds}ms for 100 iterations");
            
            // 测试LINQ性能
            sw.Reset();
            sw.Start();
            for (int i = 0; i < 100; i++) // 减少迭代次数，因为LINQ操作较重
            {
                var result = testList.OrderBy(x => UnityEngine.Random.value).ToList();
            }
            sw.Stop();
            UnityEngine.Debug.Log($"LINQ Shuffle: {sw.ElapsedMilliseconds}ms for 100 iterations");
        }
        
        [Test]
        public void TimeUtils_Performance_Test()
        {
            // 准备测试数据
            DateTime now = DateTime.Now;
            
            // 预热
            for (int i = 0; i < WarmupCount; i++)
            {
                TimeUtils.FormatDateTime(now);
                TimeUtils.GetRelativeTimeDescription(now.AddDays(-1));
                TimeUtils.MeasureExecutionTime("预热测试", () => { }, true);
            }
            
            // 测试FormatDateTime性能
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < IterationCount; i++)
            {
                TimeUtils.FormatDateTime(now);
            }
            sw.Stop();
            UnityEngine.Debug.Log($"TimeUtils.FormatDateTime: {sw.ElapsedMilliseconds}ms for {IterationCount} iterations");
            
            // 测试原生DateTime.ToString性能
            sw.Reset();
            sw.Start();
            for (int i = 0; i < IterationCount; i++)
            {
                now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            sw.Stop();
            UnityEngine.Debug.Log($"DateTime.ToString: {sw.ElapsedMilliseconds}ms for {IterationCount} iterations");
            
            // 测试GetRelativeTimeDescription性能
            sw.Reset();
            sw.Start();
            for (int i = 0; i < IterationCount; i++)
            {
                TimeUtils.GetRelativeTimeDescription(now.AddDays(-1));
            }
            sw.Stop();
            UnityEngine.Debug.Log($"TimeUtils.GetRelativeTimeDescription: {sw.ElapsedMilliseconds}ms for {IterationCount} iterations");
            
            // 测试MeasureExecutionTime性能
            sw.Reset();
            sw.Start();
            for (int i = 0; i < IterationCount; i++)
            {
                TimeUtils.MeasureExecutionTime("测试", () => { }, false);
            }
            sw.Stop();
            UnityEngine.Debug.Log($"TimeUtils.MeasureExecutionTime: {sw.ElapsedMilliseconds}ms for {IterationCount} iterations");
            
            // 测试手动测量执行时间性能
            sw.Reset();
            sw.Start();
            for (int i = 0; i < IterationCount; i++)
            {
                Stopwatch innerSw = new Stopwatch();
                innerSw.Start();
                // 执行操作
                innerSw.Stop();
                long elapsedMs = innerSw.ElapsedMilliseconds;
            }
            sw.Stop();
            UnityEngine.Debug.Log($"Manual MeasureExecutionTime: {sw.ElapsedMilliseconds}ms for {IterationCount} iterations");
        }
    }
} 