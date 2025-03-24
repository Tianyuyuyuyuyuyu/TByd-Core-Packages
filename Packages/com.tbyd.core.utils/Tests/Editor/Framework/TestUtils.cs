using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace TByd.Core.Utils.Tests.Editor.Framework
{
    /// <summary>
    /// 提供测试过程中的各种辅助功能
    /// </summary>
    public static class TestUtils
    {
        /// <summary>
        /// 临时文件夹路径，用于测试文件操作
        /// </summary>
        public static readonly string TempFolderPath = Path.Combine(Application.temporaryCachePath, "Tests");

        /// <summary>
        /// 在测试前初始化测试环境
        /// </summary>
        /// <param name="testName">测试名称，用于创建特定的测试子文件夹</param>
        /// <returns>此测试使用的临时目录路径</returns>
        public static string SetupTestEnvironment(string testName)
        {
            string testPath = Path.Combine(TempFolderPath, testName);
            
            // 确保目录存在且为空
            if (Directory.Exists(testPath))
            {
                try
                {
                    Directory.Delete(testPath, true);
                }
                catch (IOException)
                {
                    // 如果文件被占用，生成一个唯一的路径名
                    testPath = Path.Combine(TempFolderPath, $"{testName}_{Guid.NewGuid().ToString("N").Substring(0, 8)}");
                }
            }
            
            Directory.CreateDirectory(testPath);
            return testPath;
        }

        /// <summary>
        /// 清理测试环境，删除测试过程中创建的临时文件和目录
        /// </summary>
        /// <param name="testPath">测试临时目录路径</param>
        public static void CleanupTestEnvironment(string testPath)
        {
            if (Directory.Exists(testPath))
            {
                try
                {
                    Directory.Delete(testPath, true);
                }
                catch (IOException ex)
                {
                    Debug.LogWarning($"无法删除测试目录 {testPath}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 在测试完成后清理整个测试临时文件夹
        /// </summary>
        public static void CleanupAllTestEnvironments()
        {
            if (Directory.Exists(TempFolderPath))
            {
                try
                {
                    Directory.Delete(TempFolderPath, true);
                }
                catch (IOException ex)
                {
                    Debug.LogWarning($"无法删除全局测试目录 {TempFolderPath}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 创建测试用临时文件
        /// </summary>
        /// <param name="testPath">测试目录路径</param>
        /// <param name="fileName">文件名</param>
        /// <param name="content">文件内容</param>
        /// <returns>临时文件的完整路径</returns>
        public static string CreateTempFile(string testPath, string fileName, string content)
        {
            string filePath = Path.Combine(testPath, fileName);
            File.WriteAllText(filePath, content, Encoding.UTF8);
            return filePath;
        }

        /// <summary>
        /// 断言两个数组包含相同的元素（不考虑顺序）
        /// </summary>
        /// <typeparam name="T">数组元素类型</typeparam>
        /// <param name="expected">期望的数组</param>
        /// <param name="actual">实际的数组</param>
        /// <param name="message">断言失败时的消息</param>
        public static void AssertContainsSameElements<T>(IEnumerable<T> expected, IEnumerable<T> actual, string message = null)
        {
            if (expected == null)
            {
                Assert.IsNull(actual, message ?? "期望为null，但实际不为null");
                return;
            }

            Assert.IsNotNull(actual, message ?? "期望不为null，但实际为null");

            // 转换为列表以便多次迭代
            var expectedList = expected.ToList();
            var actualList = actual.ToList();

            Assert.AreEqual(expectedList.Count, actualList.Count, message ?? $"数组长度不同: 期望{expectedList.Count}个元素，实际{actualList.Count}个元素");

            // 检查每个期望的元素是否在实际数组中存在相同数量
            foreach (var expectedItem in expectedList.GroupBy(x => x).Select(g => new { Item = g.Key, Count = g.Count() }))
            {
                int actualCount = actualList.Count(item => Equals(item, expectedItem.Item));
                Assert.AreEqual(expectedItem.Count, actualCount, 
                    message ?? $"元素 {expectedItem.Item} 出现次数不同: 期望{expectedItem.Count}次，实际{actualCount}次");
            }
        }

        /// <summary>
        /// 等待指定秒数
        /// </summary>
        /// <param name="seconds">等待时间（秒）</param>
        /// <returns></returns>
        public static IEnumerator WaitForSeconds(float seconds)
        {
            float startTime = Time.time;
            while (Time.time - startTime < seconds)
            {
                yield return null;
            }
        }

        /// <summary>
        /// 等待直到条件满足或超时
        /// </summary>
        /// <param name="condition">条件函数</param>
        /// <param name="timeoutSeconds">超时时间（秒）</param>
        /// <returns>条件是否在超时前满足</returns>
        public static IEnumerator WaitUntil(Func<bool> condition, float timeoutSeconds = 5f)
        {
            float startTime = Time.time;
            while (!condition() && Time.time - startTime < timeoutSeconds)
            {
                yield return null;
            }

            Assert.IsTrue(condition(), $"条件在 {timeoutSeconds} 秒内未满足");
        }

        /// <summary>
        /// 断言两个集合内容相同（不考虑顺序）
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="expected">期望的集合</param>
        /// <param name="actual">实际的集合</param>
        /// <param name="message">断言失败时的消息</param>
        public static void AssertCollectionsEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, string message = null)
        {
            if (expected == null)
            {
                Assert.IsNull(actual, message ?? "期望集合为null，但实际集合不为null");
                return;
            }

            Assert.IsNotNull(actual, message ?? "期望集合不为null，但实际集合为null");
            
            var expectedList = expected.ToList();
            var actualList = actual.ToList();
            
            if (expectedList.Count != actualList.Count)
            {
                Assert.Fail(message ?? $"集合长度不同: 期望{expectedList.Count}个元素，实际{actualList.Count}个元素");
            }
            
            var comparer = EqualityComparer<T>.Default;
            var unmatchedActual = new List<T>(actualList);
            
            foreach (var expectedItem in expectedList)
            {
                bool found = false;
                for (int i = 0; i < unmatchedActual.Count; i++)
                {
                    if (comparer.Equals(expectedItem, unmatchedActual[i]))
                    {
                        unmatchedActual.RemoveAt(i);
                        found = true;
                        break;
                    }
                }
                
                if (!found)
                {
                    Assert.Fail(message ?? $"期望元素 {expectedItem} 在实际集合中不存在");
                }
            }
        }

        /// <summary>
        /// 通过反射来测试私有方法
        /// </summary>
        /// <param name="instance">要测试的实例，为null表示测试静态方法</param>
        /// <param name="methodName">方法名</param>
        /// <param name="parameters">方法参数</param>
        /// <returns>方法返回值</returns>
        public static object InvokePrivateMethod(object instance, string methodName, params object[] parameters)
        {
            Type type = instance?.GetType() ?? throw new ArgumentNullException(nameof(instance));
            
            MethodInfo method = type.GetMethod(methodName, 
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            
            if (method == null)
            {
                throw new ArgumentException($"方法 '{methodName}' 在类型 '{type.FullName}' 中不存在");
            }
            
            return method.Invoke(instance, parameters);
        }

        /// <summary>
        /// 通过反射获取私有字段值
        /// </summary>
        /// <typeparam name="T">字段类型</typeparam>
        /// <param name="instance">要获取字段的实例，为null表示获取静态字段</param>
        /// <param name="fieldName">字段名</param>
        /// <returns>字段值</returns>
        public static T GetPrivateField<T>(object instance, string fieldName)
        {
            Type type = instance?.GetType() ?? throw new ArgumentNullException(nameof(instance));
            
            FieldInfo field = type.GetField(fieldName, 
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            
            if (field == null)
            {
                throw new ArgumentException($"字段 '{fieldName}' 在类型 '{type.FullName}' 中不存在");
            }
            
            return (T)field.GetValue(instance);
        }

        /// <summary>
        /// 通过反射设置私有字段值
        /// </summary>
        /// <typeparam name="T">字段类型</typeparam>
        /// <param name="instance">要设置字段的实例，为null表示设置静态字段</param>
        /// <param name="fieldName">字段名</param>
        /// <param name="value">要设置的值</param>
        public static void SetPrivateField<T>(object instance, string fieldName, T value)
        {
            Type type = instance?.GetType() ?? throw new ArgumentNullException(nameof(instance));
            
            FieldInfo field = type.GetField(fieldName, 
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            
            if (field == null)
            {
                throw new ArgumentException($"字段 '{fieldName}' 在类型 '{type.FullName}' 中不存在");
            }
            
            field.SetValue(instance, value);
        }

        /// <summary>
        /// 断言两个浮点数在给定精度范围内相等
        /// </summary>
        /// <param name="expected">期望值</param>
        /// <param name="actual">实际值</param>
        /// <param name="epsilon">允许的误差</param>
        /// <param name="message">断言失败时的消息</param>
        public static void AssertApproximatelyEqual(float expected, float actual, float epsilon = 0.0001f, string message = null)
        {
            Assert.LessOrEqual(Mathf.Abs(expected - actual), epsilon, 
                message ?? $"浮点数不在误差范围内: 期望 {expected}, 实际 {actual}, 误差 {epsilon}");
        }

        /// <summary>
        /// 断言两个向量在给定精度范围内相等
        /// </summary>
        /// <param name="expected">期望的向量</param>
        /// <param name="actual">实际的向量</param>
        /// <param name="epsilon">允许的误差</param>
        /// <param name="message">断言失败时的消息</param>
        public static void AssertVectorApproximatelyEqual(Vector3 expected, Vector3 actual, float epsilon = 0.0001f, string message = null)
        {
            Assert.LessOrEqual(Vector3.Distance(expected, actual), epsilon, 
                message ?? $"向量不在误差范围内: 期望 {expected}, 实际 {actual}, 误差 {epsilon}");
        }

        /// <summary>
        /// 断言两个四元数在给定精度范围内相等
        /// </summary>
        /// <param name="expected">期望的四元数</param>
        /// <param name="actual">实际的四元数</param>
        /// <param name="epsilon">允许的误差</param>
        /// <param name="message">断言失败时的消息</param>
        public static void AssertQuaternionApproximatelyEqual(Quaternion expected, Quaternion actual, float epsilon = 0.0001f, string message = null)
        {
            // 四元数的距离用点积的绝对值来表示
            float dot = Mathf.Abs(Quaternion.Dot(expected, actual));
            // dot接近1表示四元数接近
            Assert.LessOrEqual(1f - dot, epsilon, 
                message ?? $"四元数不在误差范围内: 期望 {expected}, 实际 {actual}, 误差 {epsilon}");
        }

        /// <summary>
        /// 断言字符串匹配正则表达式
        /// </summary>
        /// <param name="pattern">正则表达式模式</param>
        /// <param name="actual">实际字符串</param>
        /// <param name="message">断言失败时的消息</param>
        public static void AssertMatchesRegex(string pattern, string actual, string message = null)
        {
            Assert.IsTrue(Regex.IsMatch(actual, pattern), 
                message ?? $"字符串 '{actual}' 不匹配正则表达式 '{pattern}'");
        }

        /// <summary>
        /// 断言方法在指定时间内执行完成
        /// </summary>
        /// <param name="action">要测试的方法</param>
        /// <param name="maxMilliseconds">最大执行时间（毫秒）</param>
        /// <param name="message">断言失败时的消息</param>
        public static void AssertExecutionTime(Action action, int maxMilliseconds, string message = null)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            
            action();
            
            stopwatch.Stop();
            long elapsedMs = stopwatch.ElapsedMilliseconds;
            
            Assert.LessOrEqual(elapsedMs, maxMilliseconds, 
                message ?? $"方法执行时间 ({elapsedMs}ms) 超过预期的 {maxMilliseconds}ms");
        }
    }
} 