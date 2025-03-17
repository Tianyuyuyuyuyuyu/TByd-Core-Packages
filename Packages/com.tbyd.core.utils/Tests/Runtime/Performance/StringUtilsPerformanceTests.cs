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
    /// StringUtils性能测试类，使用PerformanceTestFramework进行测试
    /// </summary>
    public class StringUtilsPerformanceTests
    {
        private PerformanceTestFramework _testFramework;
        private string _testString;
        private string _longTestString;
        private string _base64String;

        [SetUp]
        public void Setup()
        {
            _testFramework = new PerformanceTestFramework();
            _testString = "这是一个用于性能测试的字符串，包含中文和English以及数字123456789";

            // 创建一个较长的测试字符串，用于测试大字符串处理性能
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 1000; i++)
            {
                sb.Append(_testString);
            }

            _longTestString = sb.ToString();

            // 准备Base64测试数据
            _base64String = StringUtils.EncodeToBase64(_testString);
        }

        [Test]
        public void Test_IsNullOrEmpty_Performance()
        {
            _testFramework.RunTest(
                "StringUtils.IsNullOrEmpty",
                () => StringUtils.IsNullOrEmpty(_testString),
                "string.IsNullOrEmpty",
                () => string.IsNullOrEmpty(_testString),
                10000
            );

            // 测试空字符串情况
            _testFramework.RunTest(
                "StringUtils.IsNullOrEmpty (空字符串)",
                () => StringUtils.IsNullOrEmpty(string.Empty),
                "string.IsNullOrEmpty (空字符串)",
                () => string.IsNullOrEmpty(string.Empty),
                10000
            );

            // 测试null情况
            _testFramework.RunTest(
                "StringUtils.IsNullOrEmpty (null)",
                () => StringUtils.IsNullOrEmpty(null),
                "string.IsNullOrEmpty (null)",
                () => string.IsNullOrEmpty(null),
                10000
            );
        }

        [Test]
        public void Test_IsNullOrWhiteSpace_Performance()
        {
            _testFramework.RunTest(
                "StringUtils.IsNullOrWhiteSpace",
                () => StringUtils.IsNullOrWhiteSpace(_testString),
                "string.IsNullOrWhiteSpace",
                () => string.IsNullOrWhiteSpace(_testString),
                10000
            );

            // 测试空白字符串情况
            _testFramework.RunTest(
                "StringUtils.IsNullOrWhiteSpace (空白字符串)",
                () => StringUtils.IsNullOrWhiteSpace("   "),
                "string.IsNullOrWhiteSpace (空白字符串)",
                () => string.IsNullOrWhiteSpace("   "),
                10000
            );
        }

        [Test]
        public void Test_Truncate_Performance()
        {
            _testFramework.RunTest(
                "StringUtils.Truncate",
                () =>
                {
                    StringUtils.Truncate(_testString, 10);
                    return;
                },
                "string.Substring",
                () =>
                {
                    string result = _testString.Length > 10 ? _testString.Substring(0, 10) : _testString;
                    return;
                },
                10000
            );

            // 测试长字符串截断
            _testFramework.RunTest(
                "StringUtils.Truncate (长字符串)",
                () =>
                {
                    string result = StringUtils.Truncate(_longTestString, 100);
                    return;
                },
                "string.Substring (长字符串)",
                () =>
                {
                    string result = _longTestString.Length > 100 ? _longTestString.Substring(0, 100) : _longTestString;
                    return;
                },
                1000
            );
        }

        [Test]
        public void Test_Base64_Performance()
        {
            _testFramework.RunTest(
                "StringUtils.EncodeToBase64",
                () => StringUtils.EncodeToBase64(_testString),
                "Convert.ToBase64String",
                () => Convert.ToBase64String(Encoding.UTF8.GetBytes(_testString)),
                5000
            );

            _testFramework.RunTest(
                "StringUtils.DecodeFromBase64",
                () => StringUtils.DecodeFromBase64(_base64String),
                "Convert.FromBase64String",
                () => Encoding.UTF8.GetString(Convert.FromBase64String(_base64String)),
                5000
            );

            // 测试长字符串Base64编码
            _testFramework.RunTest(
                "StringUtils.EncodeToBase64 (长字符串)",
                () => StringUtils.EncodeToBase64(_longTestString),
                "Convert.ToBase64String (长字符串)",
                () => Convert.ToBase64String(Encoding.UTF8.GetBytes(_longTestString)),
                100
            );
        }

        [Test]
        public void Test_Format_Performance()
        {
            _testFramework.RunTest(
                "StringUtils.Format",
                () => StringUtils.Format("测试{0}格式化{1}", "字符串", 123),
                "string.Format",
                () => string.Format("测试{0}格式化{1}", "字符串", 123),
                10000
            );

            // 测试多参数格式化
            _testFramework.RunTest(
                "StringUtils.Format (多参数)",
                () => StringUtils.Format("参数1:{0}, 参数2:{1}, 参数3:{2}, 参数4:{3}", "字符串", 123, 45.67f, true),
                "string.Format (多参数)",
                () => string.Format("参数1:{0}, 参数2:{1}, 参数3:{2}, 参数4:{3}", "字符串", 123, 45.67f, true),
                10000
            );
        }

        [Test]
        public void Test_Join_Performance()
        {
            string[] testArray = new string[] { "测试", "字符串", "连接", "性能" };

            _testFramework.RunTest(
                "StringUtils.Join",
                () => StringUtils.Join(",", testArray),
                "string.Join",
                () => string.Join(",", testArray),
                10000
            );

            // 测试大数组连接
            string[] largeArray = new string[1000];
            for (int i = 0; i < largeArray.Length; i++)
            {
                largeArray[i] = "项目" + i;
            }

            _testFramework.RunTest(
                "StringUtils.Join (大数组)",
                () => StringUtils.Join(",", largeArray),
                "string.Join (大数组)",
                () => string.Join(",", largeArray),
                100
            );
        }

        [Test]
        public void Test_Contains_Performance()
        {
            _testFramework.RunTest(
                "StringUtils.Contains",
                () => StringUtils.Contains(_testString, "English"),
                "string.Contains",
                () => _testString.Contains("English"),
                10000
            );

            // 测试不区分大小写的包含
            _testFramework.RunTest(
                "StringUtils.ContainsIgnoreCase",
                () => StringUtils.ContainsIgnoreCase(_testString, "ENGLISH"),
                "string.Contains + ToLower",
                () => _testString.ToLower().Contains("english"),
                10000
            );
        }

        [Test]
        public void Test_Replace_Performance()
        {
            _testFramework.RunTest(
                "StringUtils.Replace",
                () => StringUtils.Replace(_testString, "中文", "汉字"),
                "string.Replace",
                () => _testString.Replace("中文", "汉字"),
                10000
            );

            // 测试长字符串替换
            _testFramework.RunTest(
                "StringUtils.Replace (长字符串)",
                () => StringUtils.Replace(_longTestString, "中文", "汉字"),
                "string.Replace (长字符串)",
                () => _longTestString.Replace("中文", "汉字"),
                100
            );
        }

        [Test]
        public void Test_Memory_Allocation()
        {
            // 测试内存分配
            _testFramework.RunMemoryTest(
                "StringUtils.Truncate 内存分配",
                () =>
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        string result = StringUtils.Truncate(_testString, 10);
                    }
                },
                "string.Substring 内存分配",
                () =>
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        string result = _testString.Length > 10 ? _testString.Substring(0, 10) : _testString;
                    }
                }
            );

            _testFramework.RunMemoryTest(
                "StringUtils.Join 内存分配",
                () =>
                {
                    string[] testArray = new string[] { "测试", "字符串", "连接", "性能" };
                    for (int i = 0; i < 1000; i++)
                    {
                        string result = StringUtils.Join(",", testArray);
                    }
                },
                "string.Join 内存分配",
                () => 
                {
                    string[] testArray = new string[] { "测试", "字符串", "连接", "性能" };
                    for (int i = 0; i < 1000; i++)
                    {
                        string result = string.Join(",", testArray);
                    }
                }
            );
        }
    }
}