using System;
using System.Collections.Generic;
using NUnit.Framework;
using TByd.Core.Utils.Runtime;
using UnityEngine;

namespace TByd.Core.Utils.Tests.Runtime
{
    public class RandomUtilsTests
    {
        [Test]
        public void Bool_WithDefaultChance_ReturnsReasonableDistribution()
        {
            // 使用固定种子使测试可重现
            RandomUtils.SetSeed(12345);
            
            // 生成1000个随机布尔值，检查分布是否合理
            int trueCount = 0;
            int totalCount = 1000;
            
            for (int i = 0; i < totalCount; i++)
            {
                if (RandomUtils.Bool())
                {
                    trueCount++;
                }
            }
            
            // 默认概率为0.5，所以trueCount应该接近totalCount的一半
            // 允许10%的误差范围
            float trueRatio = (float)trueCount / totalCount;
            Assert.That(trueRatio, Is.InRange(0.4f, 0.6f));
        }
        
        [Test]
        public void Bool_WithZeroChance_ReturnsFalse()
        {
            // 概率为0时，应该始终返回false
            bool result = RandomUtils.Bool(0f);
            Assert.That(result, Is.False);
        }
        
        [Test]
        public void Bool_WithOneChance_ReturnsTrue()
        {
            // 概率为1时，应该始终返回true
            bool result = RandomUtils.Bool(1f);
            Assert.That(result, Is.True);
        }
        
        [Test]
        public void WeightedRandom_WithEqualWeights_ReturnsReasonableDistribution()
        {
            // 使用固定种子使测试可重现
            RandomUtils.SetSeed(12345);
            
            string[] items = { "A", "B", "C" };
            float[] weights = { 1f, 1f, 1f };
            
            // 生成3000个随机选择，每个选项应该接近1000次
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { "A", 0 },
                { "B", 0 },
                { "C", 0 }
            };
            
            int totalCount = 3000;
            for (int i = 0; i < totalCount; i++)
            {
                string selected = RandomUtils.WeightedRandom(items, weights);
                counts[selected]++;
            }
            
            // 每个选项的选中次数应该接近总次数的1/3
            // 允许15%的误差范围
            int expectedCount = totalCount / 3;
            Assert.That(counts["A"], Is.InRange((int)(expectedCount * 0.85f), (int)(expectedCount * 1.15f)));
            Assert.That(counts["B"], Is.InRange((int)(expectedCount * 0.85f), (int)(expectedCount * 1.15f)));
            Assert.That(counts["C"], Is.InRange((int)(expectedCount * 0.85f), (int)(expectedCount * 1.15f)));
        }
        
        [Test]
        public void WeightedRandom_WithUnequalWeights_ReturnsProportionalDistribution()
        {
            // 使用固定种子使测试可重现
            RandomUtils.SetSeed(12345);
            
            string[] items = { "A", "B", "C" };
            float[] weights = { 1f, 2f, 3f }; // B的概率是A的2倍，C的概率是A的3倍
            
            // 生成6000个随机选择
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { "A", 0 },
                { "B", 0 },
                { "C", 0 }
            };
            
            int totalCount = 6000;
            for (int i = 0; i < totalCount; i++)
            {
                string selected = RandomUtils.WeightedRandom(items, weights);
                counts[selected]++;
            }
            
            // 总权重为6，所以A应该接近1/6，B应该接近2/6，C应该接近3/6
            // 允许15%的误差范围
            Assert.That(counts["A"], Is.InRange((int)(totalCount / 6 * 0.85f), (int)(totalCount / 6 * 1.15f)));
            Assert.That(counts["B"], Is.InRange((int)(totalCount / 3 * 0.85f), (int)(totalCount / 3 * 1.15f)));
            Assert.That(counts["C"], Is.InRange((int)(totalCount / 2 * 0.85f), (int)(totalCount / 2 * 1.15f)));
        }
        
        [Test]
        public void WeightedRandom_WithNullItems_ThrowsArgumentNullException()
        {
            float[] weights = { 1f, 2f, 3f };
            Assert.Throws<ArgumentNullException>(() => RandomUtils.WeightedRandom<string>(null, weights));
        }
        
        [Test]
        public void WeightedRandom_WithNullWeights_ThrowsArgumentNullException()
        {
            string[] items = { "A", "B", "C" };
            Assert.Throws<ArgumentNullException>(() => RandomUtils.WeightedRandom(items, null));
        }
        
        [Test]
        public void WeightedRandom_WithEmptyItems_ThrowsArgumentException()
        {
            string[] items = { };
            float[] weights = { };
            Assert.Throws<ArgumentException>(() => RandomUtils.WeightedRandom(items, weights));
        }
        
        [Test]
        public void WeightedRandom_WithMismatchedArrayLengths_ThrowsArgumentException()
        {
            string[] items = { "A", "B", "C" };
            float[] weights = { 1f, 2f };
            Assert.Throws<ArgumentException>(() => RandomUtils.WeightedRandom(items, weights));
        }
        
        [Test]
        public void Gaussian_ReturnsValuesWithinReasonableRange()
        {
            // 使用固定种子使测试可重现
            RandomUtils.SetSeed(12345);
            
            float mean = 100f;
            float standardDeviation = 15f;
            
            // 生成1000个随机值
            List<float> values = new List<float>();
            for (int i = 0; i < 1000; i++)
            {
                values.Add(RandomUtils.Gaussian(mean, standardDeviation));
            }
            
            // 计算实际均值和标准差
            float sum = 0f;
            foreach (float value in values)
            {
                sum += value;
            }
            float actualMean = sum / values.Count;
            
            float sumSquaredDiff = 0f;
            foreach (float value in values)
            {
                sumSquaredDiff += (value - actualMean) * (value - actualMean);
            }
            float actualStdDev = Mathf.Sqrt(sumSquaredDiff / values.Count);
            
            // 实际均值应该接近预期均值，允许5%的误差
            Assert.That(actualMean, Is.InRange(mean * 0.95f, mean * 1.05f));
            
            // 实际标准差应该接近预期标准差，允许20%的误差
            Assert.That(actualStdDev, Is.InRange(standardDeviation * 0.8f, standardDeviation * 1.2f));
            
            // 检查是否有99%的值在均值±3个标准差范围内
            int withinRangeCount = 0;
            foreach (float value in values)
            {
                if (value >= mean - 3 * standardDeviation && value <= mean + 3 * standardDeviation)
                {
                    withinRangeCount++;
                }
            }
            
            float withinRangeRatio = (float)withinRangeCount / values.Count;
            Assert.That(withinRangeRatio, Is.GreaterThanOrEqualTo(0.99f));
        }
        
        [Test]
        public void ColorHSV_ReturnsValidColor()
        {
            // 使用固定种子使测试可重现
            RandomUtils.SetSeed(12345);
            
            // 生成随机颜色
            Color color = RandomUtils.ColorHSV(0.5f, 0.8f, 0.7f, 1f);
            
            // 检查颜色分量是否在有效范围内
            Assert.That(color.r, Is.InRange(0f, 1f));
            Assert.That(color.g, Is.InRange(0f, 1f));
            Assert.That(color.b, Is.InRange(0f, 1f));
            Assert.That(color.a, Is.EqualTo(1f)); // Alpha应该是1
            
            // 将颜色转换回HSV
            Color.RGBToHSV(color, out float h, out float s, out float v);
            
            // 检查饱和度和明度是否在指定范围内
            Assert.That(s, Is.InRange(0.5f, 0.8f));
            Assert.That(v, Is.InRange(0.7f, 1f));
        }
        
        [Test]
        public void GenerateId_ReturnsStringWithCorrectLength()
        {
            // 生成指定长度的随机ID
            int length = 16;
            string id = RandomUtils.GenerateId(length);
            
            // 检查ID长度是否正确
            Assert.That(id.Length, Is.EqualTo(length));
            
            // 检查ID是否只包含字母和数字
            foreach (char c in id)
            {
                Assert.That(char.IsLetterOrDigit(c), Is.True);
            }
        }
        
        [Test]
        public void GenerateId_WithSpecialChars_ReturnsStringWithSpecialChars()
        {
            // 生成包含特殊字符的随机ID
            int length = 100; // 使用较长的长度增加包含特殊字符的概率
            string id = RandomUtils.GenerateId(length, true);
            
            // 检查ID长度是否正确
            Assert.That(id.Length, Is.EqualTo(length));
            
            // 检查ID是否包含至少一个特殊字符
            bool hasSpecialChar = false;
            foreach (char c in id)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    hasSpecialChar = true;
                    break;
                }
            }
            
            Assert.That(hasSpecialChar, Is.True);
        }
        
        [Test]
        public void Shuffle_ChangesArrayOrder()
        {
            // 创建一个有序数组
            int[] array = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            int[] original = new int[array.Length];
            Array.Copy(array, original, array.Length);
            
            // 使用固定种子使测试可重现
            RandomUtils.SetSeed(12345);
            
            // 打乱数组
            RandomUtils.Shuffle(array);
            
            // 检查数组是否被打乱
            bool isShuffled = false;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] != original[i])
                {
                    isShuffled = true;
                    break;
                }
            }
            
            Assert.That(isShuffled, Is.True);
            
            // 检查打乱后的数组是否包含原数组的所有元素
            Array.Sort(array);
            for (int i = 0; i < array.Length; i++)
            {
                Assert.That(array[i], Is.EqualTo(i + 1));
            }
        }
        
        [Test]
        public void RandomSubset_ReturnsCorrectNumberOfElements()
        {
            // 创建源数组
            string[] array = { "A", "B", "C", "D", "E" };
            
            // 获取随机子集
            int count = 3;
            string[] subset = RandomUtils.RandomSubset(array, count);
            
            // 检查子集长度是否正确
            Assert.That(subset.Length, Is.EqualTo(count));
            
            // 检查子集中的元素是否都来自源数组
            foreach (string item in subset)
            {
                Assert.That(Array.IndexOf(array, item), Is.GreaterThanOrEqualTo(0));
            }
            
            // 检查子集中的元素是否不重复
            HashSet<string> uniqueItems = new HashSet<string>(subset);
            Assert.That(uniqueItems.Count, Is.EqualTo(count));
        }
        
        [Test]
        public void RandomSubset_WithCountGreaterThanArrayLength_ReturnsShuffledArray()
        {
            // 创建源数组
            string[] array = { "A", "B", "C" };
            
            // 获取随机子集，请求的数量大于数组长度
            int count = 5;
            string[] subset = RandomUtils.RandomSubset(array, count);
            
            // 检查子集长度是否等于源数组长度
            Assert.That(subset.Length, Is.EqualTo(array.Length));
            
            // 检查子集中的元素是否都来自源数组
            foreach (string item in subset)
            {
                Assert.That(Array.IndexOf(array, item), Is.GreaterThanOrEqualTo(0));
            }
            
            // 检查子集中的元素是否不重复
            HashSet<string> uniqueItems = new HashSet<string>(subset);
            Assert.That(uniqueItems.Count, Is.EqualTo(array.Length));
        }
        
        [Test]
        public void Range_ReturnsValueWithinSpecifiedRange()
        {
            // 使用固定种子使测试可重现
            RandomUtils.SetSeed(12345);
            
            // 生成1000个随机整数
            int min = 1;
            int max = 7; // 不包含
            
            for (int i = 0; i < 1000; i++)
            {
                int value = RandomUtils.Range(min, max);
                
                // 检查值是否在指定范围内
                Assert.That(value, Is.GreaterThanOrEqualTo(min));
                Assert.That(value, Is.LessThan(max));
            }
        }
        
        [Test]
        public void SetSeed_MakesRandomSequenceReproducible()
        {
            // 使用固定种子
            int seed = 12345;
            RandomUtils.SetSeed(seed);
            
            // 生成第一组随机值
            int[] firstSequence = new int[10];
            for (int i = 0; i < 10; i++)
            {
                firstSequence[i] = RandomUtils.Range(1, 100);
            }
            
            // 重新设置相同的种子
            RandomUtils.SetSeed(seed);
            
            // 生成第二组随机值
            int[] secondSequence = new int[10];
            for (int i = 0; i < 10; i++)
            {
                secondSequence[i] = RandomUtils.Range(1, 100);
            }
            
            // 两组随机值应该相同
            for (int i = 0; i < 10; i++)
            {
                Assert.That(secondSequence[i], Is.EqualTo(firstSequence[i]));
            }
        }
    }
} 