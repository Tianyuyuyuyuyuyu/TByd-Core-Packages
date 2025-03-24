using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace TByd.Core.Utils.Tests.Editor.Framework
{
    /// <summary>
    /// 测试基类，提供通用的测试设置和清理逻辑
    /// </summary>
    public abstract class TestBase
    {
        /// <summary>
        /// 临时创建的游戏对象列表，用于测试后自动清理
        /// </summary>
        protected readonly List<GameObject> TestGameObjects = new List<GameObject>();
        
        /// <summary>
        /// 在每个测试之前运行
        /// </summary>
        [SetUp]
        public virtual void SetUp()
        {
            // 清空测试对象列表
            TestGameObjects.Clear();
        }
        
        /// <summary>
        /// 在每个测试之后运行
        /// </summary>
        [TearDown]
        public virtual void TearDown()
        {
            // 销毁所有测试过程中创建的游戏对象
            foreach (var go in TestGameObjects)
            {
                if (go != null)
                {
                    Object.DestroyImmediate(go);
                }
            }
            
            // 清空列表
            TestGameObjects.Clear();
            
            // 强制GC收集，减少测试间的干扰
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
        }
        
        /// <summary>
        /// 创建测试用GameObject，并添加到自动清理列表
        /// </summary>
        /// <param name="name">GameObject的名称</param>
        /// <returns>创建的GameObject</returns>
        protected GameObject CreateGameObject(string name = "TestObject")
        {
            var go = new GameObject(name);
            TestGameObjects.Add(go);
            return go;
        }
        
        /// <summary>
        /// 创建测试用GameObject并添加指定组件，然后添加到自动清理列表
        /// </summary>
        /// <typeparam name="T">要添加的组件类型</typeparam>
        /// <param name="name">GameObject的名称</param>
        /// <returns>添加的组件</returns>
        protected T CreateGameObjectWithComponent<T>(string name = "TestObject") where T : Component
        {
            var go = CreateGameObject(name);
            return go.AddComponent<T>();
        }
        
        /// <summary>
        /// 记录开始时间，用于性能测试
        /// </summary>
        /// <returns>当前时间戳</returns>
        protected float StartTimer()
        {
            return Time.realtimeSinceStartup;
        }
        
        /// <summary>
        /// 计算从开始时间到现在的耗时
        /// </summary>
        /// <param name="startTime">开始时间戳</param>
        /// <returns>耗时（秒）</returns>
        protected float EndTimer(float startTime)
        {
            return Time.realtimeSinceStartup - startTime;
        }
        
        /// <summary>
        /// 记录测试日志
        /// </summary>
        /// <param name="message">日志消息</param>
        protected void LogInfo(string message)
        {
            TestContext.WriteLine($"[信息] {message}");
            Debug.Log($"[测试信息] {message}");
        }
        
        /// <summary>
        /// 记录测试警告
        /// </summary>
        /// <param name="message">警告消息</param>
        protected void LogWarning(string message)
        {
            TestContext.WriteLine($"[警告] {message}");
            Debug.LogWarning($"[测试警告] {message}");
        }
        
        /// <summary>
        /// 记录测试错误
        /// </summary>
        /// <param name="message">错误消息</param>
        protected void LogError(string message)
        {
            TestContext.WriteLine($"[错误] {message}");
            Debug.LogError($"[测试错误] {message}");
        }
        
        /// <summary>
        /// 比较预期值和实际值的差距百分比
        /// </summary>
        /// <param name="expected">预期值</param>
        /// <param name="actual">实际值</param>
        /// <returns>差距百分比</returns>
        protected float CalculatePercentageDifference(float expected, float actual)
        {
            if (Mathf.Approximately(expected, 0f))
            {
                return actual;
            }
            
            return Mathf.Abs((actual - expected) / expected) * 100f;
        }
    }
} 