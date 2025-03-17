using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TByd.Core.Utils.Runtime;
using System.Diagnostics;

namespace TByd.Core.Utils.Tests.Runtime.Performance
{
    /// <summary>
    /// CollectionUtils性能测试类，使用PerformanceTestFramework进行测试
    /// </summary>
    public class CollectionUtilsPerformanceTests
    {
        private PerformanceTestFramework _testFramework;
        private List<int> _smallList;
        private List<int> _mediumList;
        private List<int> _largeList;
        private Dictionary<string, int> _dictionary;
        private int[] _array;

        [SetUp]
        public void Setup()
        {
            _testFramework = new PerformanceTestFramework();
            
            // 准备小型列表（100个元素）
            _smallList = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                _smallList.Add(UnityEngine.Random.Range(0, 1000));
            }
            
            // 准备中型列表（1000个元素）
            _mediumList = new List<int>();
            for (int i = 0; i < 1000; i++)
            {
                _mediumList.Add(UnityEngine.Random.Range(0, 10000));
            }
            
            // 准备大型列表（10000个元素）
            _largeList = new List<int>();
            for (int i = 0; i < 10000; i++)
            {
                _largeList.Add(UnityEngine.Random.Range(0, 100000));
            }
            
            // 准备字典
            _dictionary = new Dictionary<string, int>();
            for (int i = 0; i < 1000; i++)
            {
                _dictionary["key" + i] = i;
            }
            
            // 准备数组
            _array = new int[1000];
            for (int i = 0; i < 1000; i++)
            {
                _array[i] = UnityEngine.Random.Range(0, 10000);
            }
        }

        [Test]
        public void Test_IsNullOrEmpty_Performance()
        {
            bool result = false;
            _testFramework.RunTest(
                "CollectionUtils.IsNullOrEmpty (List)",
                () => { result = CollectionUtils.IsNullOrEmpty(_smallList); },
                "手动检查IsNullOrEmpty (List)",
                () => { result = _smallList == null || _smallList.Count == 0; },
                10000
            );
            
            _testFramework.RunTest(
                "CollectionUtils.IsNullOrEmpty (Array)",
                () => { result = CollectionUtils.IsNullOrEmpty(_array); },
                "手动检查IsNullOrEmpty (Array)",
                () => { result = _array == null || _array.Length == 0; },
                10000
            );
            
            _testFramework.RunTest(
                "CollectionUtils.IsNullOrEmpty (Dictionary)",
                () => { result = CollectionUtils.IsNullOrEmpty(_dictionary); },
                "手动检查IsNullOrEmpty (Dictionary)",
                () => { result = _dictionary == null || _dictionary.Count == 0; },
                10000
            );
            
            // 测试空集合
            List<int> emptyList = new List<int>();
            _testFramework.RunTest(
                "CollectionUtils.IsNullOrEmpty (空List)",
                () => { result = CollectionUtils.IsNullOrEmpty(emptyList); },
                "手动检查IsNullOrEmpty (空List)",
                () => { result = emptyList == null || emptyList.Count == 0; },
                10000
            );
            
            // 测试null集合
            List<int> nullList = null;
            _testFramework.RunTest(
                "CollectionUtils.IsNullOrEmpty (null List)",
                () => { result = CollectionUtils.IsNullOrEmpty(nullList); },
                "手动检查IsNullOrEmpty (null List)",
                () => { result = nullList == null || nullList.Count == 0; },
                10000
            );
        }

        [Test]
        public void Test_GetRandomElement_Performance()
        {
            int randomElement = 0;
            _testFramework.RunTest(
                "CollectionUtils.GetRandomElement (小型List)",
                () => { randomElement = CollectionUtils.GetRandomElement(_smallList); },
                "手动获取随机元素 (小型List)",
                () => 
                {
                    int randomIndex = UnityEngine.Random.Range(0, _smallList.Count);
                    randomElement = _smallList[randomIndex];
                },
                10000
            );
            
            _testFramework.RunTest(
                "CollectionUtils.GetRandomElement (中型List)",
                () => { randomElement = CollectionUtils.GetRandomElement(_mediumList); },
                "手动获取随机元素 (中型List)",
                () => 
                {
                    int randomIndex = UnityEngine.Random.Range(0, _mediumList.Count);
                    randomElement = _mediumList[randomIndex];
                },
                10000
            );
            
            _testFramework.RunTest(
                "CollectionUtils.GetRandomElement (Array)",
                () => { randomElement = CollectionUtils.GetRandomElement(_array); },
                "手动获取随机元素 (Array)",
                () => 
                {
                    int randomIndex = UnityEngine.Random.Range(0, _array.Length);
                    randomElement = _array[randomIndex];
                },
                10000
            );
        }

        [Test]
        public void Test_Shuffle_Performance()
        {
            // 创建测试列表的副本，避免修改原始列表
            List<int> testList = new List<int>(_smallList);
            List<int> tempList = null;
            
            _testFramework.RunTest(
                "CollectionUtils.Shuffle (小型List)",
                () => CollectionUtils.Shuffle(testList),
                "LINQ Shuffle (小型List)",
                () => { tempList = testList.OrderBy(x => UnityEngine.Random.value).ToList(); },
                100
            );
            
            // 中型列表
            List<int> mediumTestList = new List<int>(_mediumList);
            
            _testFramework.RunTest(
                "CollectionUtils.Shuffle (中型List)",
                () => CollectionUtils.Shuffle(mediumTestList),
                "LINQ Shuffle (中型List)",
                () => { tempList = mediumTestList.OrderBy(x => UnityEngine.Random.value).ToList(); },
                20
            );
            
            // 大型列表
            List<int> largeTestList = new List<int>(_largeList);
            
            _testFramework.RunTest(
                "CollectionUtils.Shuffle (大型List)",
                () => CollectionUtils.Shuffle(largeTestList),
                "LINQ Shuffle (大型List)",
                () => { tempList = largeTestList.OrderBy(x => UnityEngine.Random.value).ToList(); },
                5
            );
        }

        [Test]
        public void Test_FindAll_Performance()
        {
            List<int> result = null;
            
            _testFramework.RunTest(
                "CollectionUtils.FindAll (小型List)",
                () => { result = CollectionUtils.FindAll(_smallList, x => x > 500); },
                "List.FindAll (小型List)",
                () => { result = _smallList.FindAll(x => x > 500); },
                1000
            );
            
            _testFramework.RunTest(
                "CollectionUtils.FindAll (中型List)",
                () => { result = CollectionUtils.FindAll(_mediumList, x => x > 5000); },
                "List.FindAll (中型List)",
                () => { result = _mediumList.FindAll(x => x > 5000); },
                100
            );
            
            _testFramework.RunTest(
                "CollectionUtils.FindAll (大型List)",
                () => { result = CollectionUtils.FindAll(_largeList, x => x > 50000); },
                "List.FindAll (大型List)",
                () => { result = _largeList.FindAll(x => x > 50000); },
                10
            );
        }

        [Test]
        public void Test_ForEach_Performance()
        {
            int sum = 0;
            
            _testFramework.RunTest(
                "CollectionUtils.ForEach (小型List)",
                () => 
                {
                    sum = 0;
                    CollectionUtils.ForEach(_smallList, x => sum += x);
                },
                "List.ForEach (小型List)",
                () => 
                {
                    sum = 0;
                    _smallList.ForEach(x => sum += x);
                },
                1000
            );
            
            _testFramework.RunTest(
                "CollectionUtils.ForEach (中型List)",
                () => 
                {
                    sum = 0;
                    CollectionUtils.ForEach(_mediumList, x => sum += x);
                },
                "List.ForEach (中型List)",
                () => 
                {
                    sum = 0;
                    _mediumList.ForEach(x => sum += x);
                },
                100
            );
            
            _testFramework.RunTest(
                "CollectionUtils.ForEach (Array)",
                () => 
                {
                    sum = 0;
                    CollectionUtils.ForEach(_array, x => sum += x);
                },
                "foreach循环 (Array)",
                () => 
                {
                    sum = 0;
                    foreach (var x in _array)
                    {
                        sum += x;
                    }
                },
                1000
            );
        }

        [Test]
        public void Test_Contains_Performance()
        {
            int searchValue = _mediumList[500]; // 选择中间的元素
            bool result = false;
            
            _testFramework.RunTest(
                "CollectionUtils.Contains (小型List)",
                () => { result = CollectionUtils.Contains(_smallList, searchValue); },
                "List.Contains (小型List)",
                () => { result = _smallList.Contains(searchValue); },
                1000
            );
            
            _testFramework.RunTest(
                "CollectionUtils.Contains (中型List)",
                () => { result = CollectionUtils.Contains(_mediumList, searchValue); },
                "List.Contains (中型List)",
                () => { result = _mediumList.Contains(searchValue); },
                1000
            );
            
            _testFramework.RunTest(
                "CollectionUtils.Contains (大型List)",
                () => { result = CollectionUtils.Contains(_largeList, searchValue); },
                "List.Contains (大型List)",
                () => { result = _largeList.Contains(searchValue); },
                100
            );
            
            _testFramework.RunTest(
                "CollectionUtils.Contains (Array)",
                () => { result = CollectionUtils.Contains(_array, searchValue); },
                "Array.Contains (Array)",
                () => { result = Array.IndexOf(_array, searchValue) >= 0; },
                1000
            );
        }

        [Test]
        public void Test_GetOrDefault_Performance()
        {
            string existingKey = "key500";
            string nonExistingKey = "nonExistingKey";
            int value = 0;
            
            _testFramework.RunTest(
                "CollectionUtils.GetOrDefault (存在的键)",
                () => { value = CollectionUtils.GetOrDefault(_dictionary, existingKey, -1); },
                "Dictionary TryGetValue (存在的键)",
                () => 
                {
                    if (_dictionary.TryGetValue(existingKey, out int val))
                        value = val;
                    else
                        value = -1;
                },
                10000
            );
            
            _testFramework.RunTest(
                "CollectionUtils.GetOrDefault (不存在的键)",
                () => { value = CollectionUtils.GetOrDefault(_dictionary, nonExistingKey, -1); },
                "Dictionary TryGetValue (不存在的键)",
                () => 
                {
                    if (_dictionary.TryGetValue(nonExistingKey, out int val))
                        value = val;
                    else
                        value = -1;
                },
                10000
            );
        }

        [Test]
        public void Test_Memory_Allocation()
        {
            // 测试内存分配
            _testFramework.RunMemoryTest(
                "CollectionUtils.Shuffle 内存分配",
                () => 
                {
                    List<int> testList = new List<int>(_smallList);
                    for (int i = 0; i < 100; i++)
                    {
                        CollectionUtils.Shuffle(testList);
                    }
                },
                "LINQ Shuffle 内存分配",
                () => 
                {
                    List<int> testList = new List<int>(_smallList);
                    for (int i = 0; i < 100; i++)
                    {
                        testList = testList.OrderBy(x => UnityEngine.Random.value).ToList();
                    }
                }
            );
            
            _testFramework.RunMemoryTest(
                "CollectionUtils.FindAll 内存分配",
                () => 
                {
                    for (int i = 0; i < 100; i++)
                    {
                        var result = CollectionUtils.FindAll(_mediumList, x => x > 5000);
                    }
                },
                "List.FindAll 内存分配",
                () => 
                {
                    for (int i = 0; i < 100; i++)
                    {
                        var result = _mediumList.FindAll(x => x > 5000);
                    }
                }
            );
        }

        [Test]
        public void GeneratePerformanceReport()
        {
            // 生成性能测试报告
            PerformanceTestFramework.GenerateReport("CollectionUtils性能测试报告");
        }
    }
} 