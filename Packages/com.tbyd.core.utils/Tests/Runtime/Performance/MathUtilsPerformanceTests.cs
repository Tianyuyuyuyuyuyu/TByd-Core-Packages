using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TByd.Core.Utils.Runtime;
using System.Diagnostics;

namespace TByd.Core.Utils.Tests.Runtime.Performance
{
    /// <summary>
    /// MathUtils性能测试类，使用PerformanceTestFramework进行测试
    /// </summary>
    public class MathUtilsPerformanceTests
    {
        private PerformanceTestFramework _testFramework;
        private Vector3 _startPos;
        private Vector3 _targetPos;
        private Vector3 _velocity;
        private float _testValue;

        [SetUp]
        public void Setup()
        {
            _testFramework = new PerformanceTestFramework();
            _startPos = new Vector3(1, 2, 3);
            _targetPos = new Vector3(4, 5, 6);
            _velocity = Vector3.zero;
            _testValue = 0.5f;
        }

        [Test]
        public void Test_SmoothDamp_Performance()
        {
            _testFramework.RunTest(
                "MathUtils.SmoothDamp",
                () => 
                {
                    Vector3 vel = _velocity;
                    MathUtils.SmoothDamp(_startPos, _targetPos, ref vel, 0.1f);
                },
                "Vector3.SmoothDamp",
                () => 
                {
                    Vector3 vel = _velocity;
                    Vector3.SmoothDamp(_startPos, _targetPos, ref vel, 0.1f, Mathf.Infinity, Time.deltaTime);
                },
                10000
            );
            
            // 测试不同的阻尼值
            _testFramework.RunTest(
                "MathUtils.SmoothDamp (低阻尼)",
                () => 
                {
                    Vector3 vel = _velocity;
                    MathUtils.SmoothDamp(_startPos, _targetPos, ref vel, 0.01f);
                },
                "Vector3.SmoothDamp (低阻尼)",
                () => 
                {
                    Vector3 vel = _velocity;
                    Vector3.SmoothDamp(_startPos, _targetPos, ref vel, 0.01f, Mathf.Infinity, Time.deltaTime);
                },
                10000
            );
            
            _testFramework.RunTest(
                "MathUtils.SmoothDamp (高阻尼)",
                () => 
                {
                    Vector3 vel = _velocity;
                    MathUtils.SmoothDamp(_startPos, _targetPos, ref vel, 1.0f);
                },
                "Vector3.SmoothDamp (高阻尼)",
                () => 
                {
                    Vector3 vel = _velocity;
                    Vector3.SmoothDamp(_startPos, _targetPos, ref vel, 1.0f, Mathf.Infinity, Time.deltaTime);
                },
                10000
            );
        }

        [Test]
        public void Test_Remap_Performance()
        {
            float result = 0f;
            _testFramework.RunTest(
                "MathUtils.Remap",
                () => { result = MathUtils.Remap(_testValue, 0, 1, 0, 100); },
                "手动实现Remap",
                () => 
                {
                    float normalizedValue = (_testValue - 0) / (1 - 0);
                    result = Mathf.Lerp(0, 100, normalizedValue);
                },
                10000
            );
            
            // 测试不同范围的重映射
            _testFramework.RunTest(
                "MathUtils.Remap (大范围)",
                () => { result = MathUtils.Remap(_testValue, 0, 1, 0, 10000); },
                "手动实现Remap (大范围)",
                () => 
                {
                    float normalizedValue = (_testValue - 0) / (1 - 0);
                    result = Mathf.Lerp(0, 10000, normalizedValue);
                },
                10000
            );
            
            _testFramework.RunTest(
                "MathUtils.Remap (负范围)",
                () => { result = MathUtils.Remap(_testValue, -1, 1, -100, 100); },
                "手动实现Remap (负范围)",
                () => 
                {
                    float normalizedValue = (_testValue - (-1)) / (1 - (-1));
                    result = Mathf.Lerp(-100, 100, normalizedValue);
                },
                10000
            );
        }

        [Test]
        public void Test_DirectionToRotation_Performance()
        {
            Vector3 direction = _targetPos - _startPos;
            Quaternion result;
            
            _testFramework.RunTest(
                "MathUtils.DirectionToRotation",
                () => { result = MathUtils.DirectionToRotation(direction); },
                "Quaternion.LookRotation",
                () => { result = Quaternion.LookRotation(direction); },
                10000
            );
            
            // 测试不同方向
            Vector3 upDirection = Vector3.up;
            _testFramework.RunTest(
                "MathUtils.DirectionToRotation (上方向)",
                () => { result = MathUtils.DirectionToRotation(upDirection); },
                "Quaternion.LookRotation (上方向)",
                () => { result = Quaternion.LookRotation(upDirection); },
                10000
            );
            
            Vector3 randomDirection = new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f)
            ).normalized;
            
            _testFramework.RunTest(
                "MathUtils.DirectionToRotation (随机方向)",
                () => { result = MathUtils.DirectionToRotation(randomDirection); },
                "Quaternion.LookRotation (随机方向)",
                () => { result = Quaternion.LookRotation(randomDirection); },
                10000
            );
        }

        [Test]
        public void Test_Clamp_Performance()
        {
            float value = 150f;
            float result = 0f;
            
            _testFramework.RunTest(
                "Mathf.Clamp",
                () => { result = Mathf.Clamp(value, 0, 100); },
                "手动实现Clamp",
                () => { result = (value < 0) ? 0 : ((value > 100) ? 100 : value); },
                10000
            );
            
            // 测试不同范围的Clamp
            _testFramework.RunTest(
                "Mathf.Clamp (负范围)",
                () => { result = Mathf.Clamp(value, -100, -50); },
                "手动实现Clamp (负范围)",
                () => { result = (value < -100) ? -100 : ((value > -50) ? -50 : value); },
                10000
            );
            
            // 测试接近边界值
            float borderValue = 99.9f;
            _testFramework.RunTest(
                "Mathf.Clamp (边界值)",
                () => { result = Mathf.Clamp(borderValue, 0, 100); },
                "手动实现Clamp (边界值)",
                () => { result = (borderValue < 0) ? 0 : ((borderValue > 100) ? 100 : borderValue); },
                10000
            );
        }

        [Test]
        public void Test_Lerp_Performance()
        {
            float result = 0f;
            
            _testFramework.RunTest(
                "Mathf.Lerp",
                () => { result = Mathf.Lerp(0, 100, _testValue); },
                "手动实现Lerp",
                () => { result = 0 + (_testValue * (100 - 0)); },
                10000
            );
            
            // 测试接近边界值
            _testFramework.RunTest(
                "Mathf.Lerp (接近0)",
                () => { result = Mathf.Lerp(0, 100, 0.001f); },
                "手动实现Lerp (接近0)",
                () => { result = 0 + (0.001f * (100 - 0)); },
                10000
            );
            
            _testFramework.RunTest(
                "Mathf.Lerp (接近1)",
                () => { result = Mathf.Lerp(0, 100, 0.999f); },
                "手动实现Lerp (接近1)",
                () => { result = 0 + (0.999f * (100 - 0)); },
                10000
            );
            
            _testFramework.RunTest(
                "Mathf.LerpUnclamped",
                () => { result = Mathf.LerpUnclamped(0, 100, 1.5f); },
                "手动实现LerpUnclamped",
                () => { result = 0 + (1.5f * (100 - 0)); },
                10000
            );
        }

        [Test]
        public void Test_CalculateDistance_Performance()
        {
            float result = 0f;
            
            _testFramework.RunTest(
                "Vector3.Distance",
                () => { result = Vector3.Distance(_startPos, _targetPos); },
                "手动计算距离",
                () => { result = (_targetPos - _startPos).magnitude; },
                10000
            );
            
            _testFramework.RunTest(
                "Vector3.sqrMagnitude",
                () => { result = (_targetPos - _startPos).sqrMagnitude; },
                "手动计算平方距离",
                () => 
                { 
                    Vector3 diff = _targetPos - _startPos;
                    result = diff.x * diff.x + diff.y * diff.y + diff.z * diff.z; 
                },
                10000
            );
        }

        [Test]
        public void Test_Memory_Allocation()
        {
            // 测试内存分配
            _testFramework.RunMemoryTest(
                "MathUtils.SmoothDamp 内存分配",
                () => 
                {
                    Vector3 vel = _velocity;
                    for (int i = 0; i < 1000; i++)
                    {
                        MathUtils.SmoothDamp(_startPos, _targetPos, ref vel, 0.1f);
                    }
                },
                "Vector3.SmoothDamp 内存分配",
                () => 
                {
                    Vector3 vel = _velocity;
                    for (int i = 0; i < 1000; i++)
                    {
                        Vector3.SmoothDamp(_startPos, _targetPos, ref vel, 0.1f, Mathf.Infinity, Time.deltaTime);
                    }
                }
            );
            
            _testFramework.RunMemoryTest(
                "MathUtils.DirectionToRotation 内存分配",
                () => 
                {
                    Vector3 direction = _targetPos - _startPos;
                    for (int i = 0; i < 1000; i++)
                    {
                        MathUtils.DirectionToRotation(direction);
                    }
                },
                "Quaternion.LookRotation 内存分配",
                () => 
                {
                    Vector3 direction = _targetPos - _startPos;
                    for (int i = 0; i < 1000; i++)
                    {
                        Quaternion.LookRotation(direction);
                    }
                }
            );
        }

        [Test]
        public void GeneratePerformanceReport()
        {
            // 生成性能测试报告
            PerformanceTestFramework.GenerateReport("MathUtils性能测试报告");
        }
    }
} 