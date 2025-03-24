using System;
using NUnit.Framework;
using TByd.Core.Utils.Runtime;
using TByd.Core.Utils.Tests.Editor.Framework;

// 假设StringUtils位于此命名空间

namespace TByd.Core.Utils.Tests.Editor.Unit
{
    /// <summary>
    /// StringUtils类的单元测试
    /// </summary>
    [TestFixture]
    public class StringUtilsTests : TestBase
    {
        /// <summary>
        /// 测试GenerateRandom方法
        /// </summary>
        [Test]
        public void GenerateRandom_WithSpecifiedLength_ReturnsStringOfCorrectLength()
        {
            // 准备
            int length = 10;
            
            // 执行
            string result = StringUtils.GenerateRandom(length);
            
            // 验证
            Assert.AreEqual(length, result.Length);
        }
        
        /// <summary>
        /// 测试GenerateRandom方法
        /// </summary>
        [Test]
        public void GenerateRandom_CalledMultipleTimes_ReturnsDifferentStrings()
        {
            // 准备
            int length = 20;
            
            // 执行
            string result1 = StringUtils.GenerateRandom(length);
            string result2 = StringUtils.GenerateRandom(length);
            
            // 验证
            Assert.AreNotEqual(result1, result2);
        }
        
        /// <summary>
        /// 测试GenerateRandom方法
        /// </summary>
        [Test]
        public void GenerateRandom_WithInvalidLength_ThrowsArgumentException()
        {
            // 准备
            int length = -1;
            
            // 执行和验证
            Assert.Throws<ArgumentException>(() => StringUtils.GenerateRandom(length));
        }
        
        /// <summary>
        /// 测试GenerateRandom方法，包含特殊字符
        /// </summary>
        [Test]
        public void GenerateRandom_WithSpecialChars_ReturnsStringWithSpecialChars()
        {
            // 准备
            int length = 100;
            string allowedChars = StringUtils.AlphanumericAndSpecialChars;
            
            // 执行
            string result = StringUtils.GenerateRandom(length, allowedChars);
            
            // 验证 - 至少包含一个特殊字符
            Assert.IsTrue(ContainsAnySpecialChar(result));
        }
        
        /// <summary>
        /// 测试ToSlug方法
        /// </summary>
        [Test]
        public void ToSlug_WithNormalString_ReturnsSlugifiedString()
        {
            // 准备
            string input = "Hello World! This is a Test";
            string expected = "hello-world-this-is-a-test";
            
            // 执行
            string result = StringUtils.ToSlug(input);
            
            // 验证
            Assert.AreEqual(expected, result);
        }
        
        /// <summary>
        /// 测试ToSlug方法
        /// </summary>
        [Test]
        public void ToSlug_WithChineseString_ReturnsSlugifiedString()
        {
            // 准备
            string input = "你好，世界！";
            
            // 执行
            string result = StringUtils.ToSlug(input);
            
            // 验证 - 空格转为连字符，移除特殊字符
            Assert.IsFalse(result.Contains(" "));
            Assert.IsFalse(result.Contains("，"));
            Assert.IsFalse(result.Contains("！"));
        }
        
        /// <summary>
        /// 测试ToSlug方法
        /// </summary>
        [Test]
        public void ToSlug_WithNullInput_ThrowsArgumentNullException()
        {
            // 准备
            string input = null;
            
            // 执行和验证
            Assert.Throws<ArgumentNullException>(() => StringUtils.ToSlug(input));
        }
        
        /// <summary>
        /// 测试Truncate方法
        /// </summary>
        [Test]
        public void Truncate_StringLongerThanMaxLength_ReturnsCorrectlyTruncatedString()
        {
            // 准备
            string input = "这是一个很长的字符串，需要被截断";
            int maxLength = 5;
            string expected = "这是一个很...";
            
            // 执行
            string result = StringUtils.Truncate(input, maxLength);
            
            // 验证
            Assert.AreEqual(expected, result);
        }
        
        /// <summary>
        /// 测试Truncate方法
        /// </summary>
        [Test]
        public void Truncate_StringShorterThanMaxLength_ReturnsOriginalString()
        {
            // 准备
            string input = "短字符串";
            int maxLength = 10;
            
            // 执行
            string result = StringUtils.Truncate(input, maxLength);
            
            // 验证
            Assert.AreEqual(input, result);
        }
        
        /// <summary>
        /// 测试Truncate方法
        /// </summary>
        [Test]
        public void Truncate_WithCustomSuffix_ReturnsCorrectlyTruncatedStringWithCustomSuffix()
        {
            // 准备
            string input = "这是一个很长的字符串，需要被截断";
            int maxLength = 5;
            string suffix = "...等等";
            string expected = "这是一个很...等等";
            
            // 执行
            string result = StringUtils.Truncate(input, maxLength, suffix);
            
            // 验证
            Assert.AreEqual(expected, result);
        }
        
        /// <summary>
        /// 测试Truncate方法
        /// </summary>
        [Test]
        public void Truncate_WithNegativeMaxLength_ThrowsArgumentOutOfRangeException()
        {
            // 准备
            string input = "Test";
            int maxLength = -1;
            
            // 执行和验证
            Assert.Throws<ArgumentOutOfRangeException>(() => StringUtils.Truncate(input, maxLength));
        }
        
        /// <summary>
        /// 辅助方法：检查字符串是否包含特殊字符
        /// </summary>
        private bool ContainsAnySpecialChar(string value)
        {
            const string specialChars = "!@#$%^&*()_-+=<>?";
            foreach (char c in value)
            {
                if (specialChars.IndexOf(c) >= 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
} 