using System;
using System.Collections.Generic;
using NUnit.Framework;
using TByd.Core.Utils.Runtime;
using TByd.Core.Utils.Tests.Editor.Framework;
using UnityEngine;

namespace TByd.Core.Utils.Tests.Editor.Unit
{
    /// <summary>
    /// RandomUtils类的单元测试
    /// </summary>
    [TestFixture]
    public class RandomUtilsTests : TestBase
    {
        #region Bool Tests

        /// <summary>
        /// 测试Bool方法，默认概率情况
        /// </summary>
        [Test]
        public void Bool_WithDefaultChance_ReturnsFairDistribution()
        {
            // 准备
            int trueCount = 0;
            int totalTests = 10000;
            
            // 执行
            for (int i = 0; i < totalTests; i++)
            {
                if (RandomUtils.Bool())
                {
                    trueCount++;
                }
            }
            
            // 验证 - 在10000次测试中，true的比例应该在45%-55%之间
            float trueRatio = (float)trueCount / totalTests;
            Assert.That(trueRatio, Is.InRange(0.45f, 0.55f), "默认概率下，true的分布应该接近50%");
        }
        
        /// <summary>
        /// 测试Bool方法，指定概率情况
        /// </summary>
        [Test]
        public void Bool_WithSpecifiedChance_ReturnsDistributionMatchingChance()
        {
            // 准备
            int trueCount = 0;
            int totalTests = 10000;
            float trueChance = 0.3f;
            
            // 执行
            for (int i = 0; i < totalTests; i++)
            {
                if (RandomUtils.Bool(trueChance))
                {
                    trueCount++;
                }
            }
            
            // 验证 - 在10000次测试中，true的比例应该在25%-35%之间
            float trueRatio = (float)trueCount / totalTests;
            Assert.That(trueRatio, Is.InRange(0.25f, 0.35f), $"概率为{trueChance}时，true的分布应该接近{trueChance * 100}%");
        }
        
        /// <summary>
        /// 测试Bool方法，概率为0时
        /// </summary>
        [Test]
        public void Bool_WithZeroChance_AlwaysReturnsFalse()
        {
            // 准备
            int totalTests = 1000;
            
            // 执行
            for (int i = 0; i < totalTests; i++)
            {
                // 验证 - 概率为0时，应该始终返回false
                Assert.IsFalse(RandomUtils.Bool(0f), "概率为0时，应该始终返回false");
            }
        }
        
        /// <summary>
        /// 测试Bool方法，概率为1时
        /// </summary>
        [Test]
        public void Bool_WithOneChance_AlwaysReturnsTrue()
        {
            // 准备
            int totalTests = 1000;
            
            // 执行
            for (int i = 0; i < totalTests; i++)
            {
                // 验证 - 概率为1时，应该始终返回true
                Assert.IsTrue(RandomUtils.Bool(1f), "概率为1时，应该始终返回true");
            }
        }
        
        /// <summary>
        /// 测试Bool方法，无效概率情况（小于0）
        /// </summary>
        [Test]
        public void Bool_WithNegativeChance_ThrowsArgumentException()
        {
            // 验证 - 传入负值应该抛出参数异常
            Assert.Throws<ArgumentException>(() => RandomUtils.Bool(-0.1f), 
                "当概率为负值时，应该抛出ArgumentException");
        }
        
        /// <summary>
        /// 测试Bool方法，无效概率情况（大于1）
        /// </summary>
        [Test]
        public void Bool_WithChanceGreaterThanOne_ThrowsArgumentException()
        {
            // 验证 - 传入大于1的值应该抛出参数异常
            Assert.Throws<ArgumentException>(() => RandomUtils.Bool(1.1f), 
                "当概率大于1时，应该抛出ArgumentException");
        }
        
        #endregion
        
        #region WeightedRandom Tests
        
        /// <summary>
        /// 测试WeightedRandom方法，正常权重情况
        /// </summary>
        [Test]
        public void WeightedRandom_WithValidWeights_ReturnsDistributionMatchingWeights()
        {
            // 准备
            string[] items = { "A", "B", "C" };
            float[] weights = { 1f, 2f, 7f }; // 10%, 20%, 70%的权重
            
            int[] counts = new int[3];
            int totalTests = 10000;
            
            // 执行
            for (int i = 0; i < totalTests; i++)
            {
                string result = RandomUtils.WeightedRandom(items, weights);
                if (result == "A") counts[0]++;
                else if (result == "B") counts[1]++;
                else if (result == "C") counts[2]++;
            }
            
            // 验证 - 结果分布应该与权重接近
            float[] ratios = {
                (float)counts[0] / totalTests,
                (float)counts[1] / totalTests,
                (float)counts[2] / totalTests
            };
            
            Assert.That(ratios[0], Is.InRange(0.08f, 0.12f), "A的选择率应该接近10%");
            Assert.That(ratios[1], Is.InRange(0.17f, 0.23f), "B的选择率应该接近20%");
            Assert.That(ratios[2], Is.InRange(0.65f, 0.75f), "C的选择率应该接近70%");
        }
        
        /// <summary>
        /// 测试WeightedRandom方法，所有权重相等的情况
        /// </summary>
        [Test]
        public void WeightedRandom_WithEqualWeights_ReturnsFairDistribution()
        {
            // 准备
            int[] items = { 1, 2, 3, 4 };
            float[] weights = { 1f, 1f, 1f, 1f }; // 所有权重相等
            
            int[] counts = new int[4];
            int totalTests = 10000;
            
            // 执行
            for (int i = 0; i < totalTests; i++)
            {
                int result = RandomUtils.WeightedRandom(items, weights);
                counts[result - 1]++;
            }
            
            // 验证 - 结果应该大致均匀分布
            for (int i = 0; i < counts.Length; i++)
            {
                float ratio = (float)counts[i] / totalTests;
                Assert.That(ratio, Is.InRange(0.23f, 0.27f), $"元素{i+1}的选择率应该接近25%");
            }
        }
        
        /// <summary>
        /// 测试WeightedRandom方法，单一项的情况
        /// </summary>
        [Test]
        public void WeightedRandom_WithSingleItem_AlwaysReturnsThatItem()
        {
            // 准备
            int[] items = { 42 };
            float[] weights = { 1f };
            
            // 执行
            int result = RandomUtils.WeightedRandom(items, weights);
            
            // 验证 - 只有一个选项时，应该始终返回该选项
            Assert.AreEqual(42, result, "只有一个选项时，应该始终返回该选项");
        }
        
        /// <summary>
        /// 测试WeightedRandom方法，空数组情况
        /// </summary>
        [Test]
        public void WeightedRandom_WithEmptyArray_ThrowsArgumentException()
        {
            // 准备
            string[] items = { };
            float[] weights = { };
            
            // 验证 - 空数组应该抛出异常
            Assert.Throws<ArgumentException>(() => RandomUtils.WeightedRandom(items, weights), 
                "传入空数组时，应该抛出ArgumentException");
        }
        
        /// <summary>
        /// 测试WeightedRandom方法，数组长度不匹配情况
        /// </summary>
        [Test]
        public void WeightedRandom_WithMismatchedArrayLengths_ThrowsArgumentException()
        {
            // 准备
            int[] items = { 1, 2, 3 };
            float[] weights = { 1f, 2f };
            
            // 验证 - 数组长度不匹配应该抛出异常
            Assert.Throws<ArgumentException>(() => RandomUtils.WeightedRandom(items, weights), 
                "当items和weights长度不匹配时，应该抛出ArgumentException");
        }
        
        /// <summary>
        /// 测试WeightedRandom方法，负权重情况
        /// </summary>
        [Test]
        public void WeightedRandom_WithNegativeWeight_ThrowsArgumentException()
        {
            // 准备
            int[] items = { 1, 2, 3 };
            float[] weights = { 1f, -1f, 1f };
            
            // 验证 - 负权重应该抛出异常
            Assert.Throws<ArgumentException>(() => RandomUtils.WeightedRandom(items, weights), 
                "当权重包含负值时，应该抛出ArgumentException");
        }
        
        /// <summary>
        /// 测试WeightedRandom方法，全零权重情况
        /// </summary>
        [Test]
        public void WeightedRandom_WithAllZeroWeights_ThrowsArgumentException()
        {
            // 准备
            int[] items = { 1, 2, 3 };
            float[] weights = { 0f, 0f, 0f };
            
            // 验证 - 全零权重应该抛出异常
            Assert.Throws<ArgumentException>(() => RandomUtils.WeightedRandom(items, weights), 
                "当所有权重都为0时，应该抛出ArgumentException");
        }
        
        #endregion
        
        #region Gaussian Tests
        
        /// <summary>
        /// 测试Gaussian方法，默认参数情况
        /// </summary>
        [Test]
        public void Gaussian_WithDefaultParameters_ReturnsStandardNormalDistribution()
        {
            // 准备
            int totalSamples = 10000;
            float sum = 0;
            float sumOfSquares = 0;
            
            // 执行
            for (int i = 0; i < totalSamples; i++)
            {
                float sample = RandomUtils.Gaussian();
                sum += sample;
                sumOfSquares += sample * sample;
            }
            
            // 验证 - 检查均值和标准差
            float mean = sum / totalSamples;
            float variance = (sumOfSquares / totalSamples) - (mean * mean);
            float stdDev = Mathf.Sqrt(variance);
            
            // 标准正态分布的均值应该接近0，标准差应该接近1
            Assert.That(mean, Is.InRange(-0.1f, 0.1f), "标准正态分布的均值应该接近0");
            Assert.That(stdDev, Is.InRange(0.9f, 1.1f), "标准正态分布的标准差应该接近1");
        }
        
        /// <summary>
        /// 测试Gaussian方法，自定义参数情况
        /// </summary>
        [Test]
        public void Gaussian_WithCustomParameters_ReturnsMatchingDistribution()
        {
            // 准备
            int totalSamples = 10000;
            float expectedMean = 5f;
            float expectedStdDev = 2f;
            float sum = 0;
            float sumOfSquares = 0;
            
            // 执行
            for (int i = 0; i < totalSamples; i++)
            {
                float sample = RandomUtils.Gaussian(expectedMean, expectedStdDev);
                sum += sample;
                sumOfSquares += sample * sample;
            }
            
            // 验证 - 检查均值和标准差
            float mean = sum / totalSamples;
            float variance = (sumOfSquares / totalSamples) - (mean * mean);
            float stdDev = Mathf.Sqrt(variance);
            
            // 正态分布的均值和标准差应该接近指定值
            Assert.That(mean, Is.InRange(expectedMean - 0.2f, expectedMean + 0.2f), 
                $"正态分布的均值应该接近{expectedMean}");
            Assert.That(stdDev, Is.InRange(expectedStdDev - 0.2f, expectedStdDev + 0.2f), 
                $"正态分布的标准差应该接近{expectedStdDev}");
        }
        
        /// <summary>
        /// 测试Gaussian方法，负标准差情况
        /// </summary>
        [Test]
        public void Gaussian_WithNegativeStandardDeviation_ThrowsArgumentException()
        {
            // 验证 - 负标准差应该抛出异常
            Assert.Throws<ArgumentException>(() => RandomUtils.Gaussian(0f, -1f), 
                "当标准差为负值时，应该抛出ArgumentException");
        }
        
        /// <summary>
        /// 测试Gaussian方法，零标准差情况
        /// </summary>
        [Test]
        public void Gaussian_WithZeroStandardDeviation_ReturnsExactlyMean()
        {
            // 准备
            float mean = 3.5f;
            
            // 执行
            float result = RandomUtils.Gaussian(mean, 0f);
            
            // 验证 - 标准差为0时，应该始终返回均值
            Assert.AreEqual(mean, result, "当标准差为0时，应该始终返回均值");
        }
        
        #endregion
        
        #region ColorHSV Tests
        
        /// <summary>
        /// 测试ColorHSV方法，默认参数情况
        /// </summary>
        [Test]
        public void ColorHSV_WithDefaultParameters_ReturnsValidColor()
        {
            // 执行
            Color color = RandomUtils.ColorHSV();
            
            // 验证 - 检查颜色属性在有效范围内
            Assert.That(color.r, Is.InRange(0f, 1f), "红色分量应该在0-1范围内");
            Assert.That(color.g, Is.InRange(0f, 1f), "绿色分量应该在0-1范围内");
            Assert.That(color.b, Is.InRange(0f, 1f), "蓝色分量应该在0-1范围内");
            Assert.That(color.a, Is.EqualTo(1f), "透明度应该为1");
        }
        
        /// <summary>
        /// 测试ColorHSV方法，自定义参数情况
        /// </summary>
        [Test]
        public void ColorHSV_WithCustomParameters_ReturnsColorInRange()
        {
            // 准备 - 参数范围
            float saturationMin = 0.7f;
            float saturationMax = 0.9f;
            float valueMin = 0.5f;
            float valueMax = 0.8f;
            
            // 执行
            Color color = RandomUtils.ColorHSV(saturationMin, saturationMax, valueMin, valueMax);
            
            // 转换回HSV以验证范围
            Color.RGBToHSV(color, out float h, out float s, out float v);
            
            // 验证 - 检查HSV属性在指定范围内
            Assert.That(s, Is.InRange(saturationMin, saturationMax), 
                $"饱和度应该在{saturationMin}-{saturationMax}范围内");
            Assert.That(v, Is.InRange(valueMin, valueMax), 
                $"明度应该在{valueMin}-{valueMax}范围内");
            Assert.That(color.a, Is.EqualTo(1f), "透明度应该为1");
        }
        
        /// <summary>
        /// 测试ColorHSV方法，无效参数范围情况
        /// </summary>
        [Test]
        public void ColorHSV_WithInvalidParameterRange_ThrowsArgumentException()
        {
            // 验证 - 最小值大于最大值应该抛出异常
            Assert.Throws<ArgumentException>(() => RandomUtils.ColorHSV(0.8f, 0.5f), 
                "当saturationMin大于saturationMax时，应该抛出ArgumentException");
            
            Assert.Throws<ArgumentException>(() => RandomUtils.ColorHSV(0f, 1f, 0.8f, 0.5f), 
                "当valueMin大于valueMax时，应该抛出ArgumentException");
        }
        
        /// <summary>
        /// 测试ColorHSV方法，参数超出范围情况
        /// </summary>
        [Test]
        public void ColorHSV_WithParametersOutOfRange_ThrowsArgumentException()
        {
            // 验证 - 参数超出0-1范围应该抛出异常
            Assert.Throws<ArgumentException>(() => RandomUtils.ColorHSV(-0.1f, 1f), 
                "当saturationMin小于0时，应该抛出ArgumentException");
            
            Assert.Throws<ArgumentException>(() => RandomUtils.ColorHSV(0f, 1.1f), 
                "当saturationMax大于1时，应该抛出ArgumentException");
            
            Assert.Throws<ArgumentException>(() => RandomUtils.ColorHSV(0f, 1f, -0.1f, 1f), 
                "当valueMin小于0时，应该抛出ArgumentException");
            
            Assert.Throws<ArgumentException>(() => RandomUtils.ColorHSV(0f, 1f, 0f, 1.1f), 
                "当valueMax大于1时，应该抛出ArgumentException");
        }
        
        #endregion
        
        #region GenerateId Tests
        
        /// <summary>
        /// 测试GenerateId方法，基本功能
        /// </summary>
        [Test]
        public void GenerateId_WithSpecifiedLength_ReturnsIdOfCorrectLength()
        {
            // 准备
            int length = 15;
            
            // 执行
            string id = RandomUtils.GenerateId(length);
            
            // 验证
            Assert.AreEqual(length, id.Length, $"生成的ID长度应该为{length}");
        }
        
        /// <summary>
        /// 测试GenerateId方法，多次调用返回不同结果
        /// </summary>
        [Test]
        public void GenerateId_CalledMultipleTimes_ReturnsDifferentIds()
        {
            // 准备
            int length = 10;
            HashSet<string> generatedIds = new HashSet<string>();
            int generateCount = 100;
            
            // 执行 - 生成多个ID
            for (int i = 0; i < generateCount; i++)
            {
                string id = RandomUtils.GenerateId(length);
                generatedIds.Add(id);
            }
            
            // 验证 - 所有生成的ID都应该是唯一的
            Assert.AreEqual(generateCount, generatedIds.Count, "每次生成的ID应该都是唯一的");
        }
        
        /// <summary>
        /// 测试GenerateId方法，包含特殊字符情况
        /// </summary>
        [Test]
        public void GenerateId_WithSpecialChars_ContainsSpecialCharacters()
        {
            // 准备
            int length = 100;
            bool includeSpecialChars = true;
            
            // 执行
            string id = RandomUtils.GenerateId(length, includeSpecialChars);
            
            // 验证 - 检查是否包含特殊字符
            bool containsSpecialChar = false;
            string specialChars = "!@#$%^&*()_-+=<>?";
            
            foreach (char c in specialChars)
            {
                if (id.Contains(c.ToString()))
                {
                    containsSpecialChar = true;
                    break;
                }
            }
            
            Assert.IsTrue(containsSpecialChar, "启用特殊字符时，生成的ID应该包含特殊字符");
        }
        
        /// <summary>
        /// 测试GenerateId方法，不包含特殊字符情况
        /// </summary>
        [Test]
        public void GenerateId_WithoutSpecialChars_DoesNotContainSpecialCharacters()
        {
            // 准备
            int length = 100;
            bool includeSpecialChars = false;
            
            // 执行
            string id = RandomUtils.GenerateId(length, includeSpecialChars);
            
            // 验证 - 检查是否不包含特殊字符
            bool containsSpecialChar = false;
            string specialChars = "!@#$%^&*()_-+=<>?";
            
            foreach (char c in specialChars)
            {
                if (id.Contains(c.ToString()))
                {
                    containsSpecialChar = true;
                    break;
                }
            }
            
            Assert.IsFalse(containsSpecialChar, "不启用特殊字符时，生成的ID不应该包含特殊字符");
        }
        
        /// <summary>
        /// 测试GenerateId方法，无效长度情况
        /// </summary>
        [Test]
        public void GenerateId_WithNegativeLength_ThrowsArgumentException()
        {
            // 验证 - 负长度应该抛出异常
            Assert.Throws<ArgumentException>(() => RandomUtils.GenerateId(-5), 
                "当长度为负值时，应该抛出ArgumentException");
        }
        
        /// <summary>
        /// 测试GenerateId方法，零长度情况
        /// </summary>
        [Test]
        public void GenerateId_WithZeroLength_ReturnsEmptyString()
        {
            // 执行
            string id = RandomUtils.GenerateId(0);
            
            // 验证 - 长度为0应该返回空字符串
            Assert.AreEqual(string.Empty, id, "当长度为0时，应该返回空字符串");
        }
        
        #endregion
    }
} 