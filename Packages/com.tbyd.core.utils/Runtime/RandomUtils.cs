using System;
using System.Collections.Generic;
using UnityEngine;

namespace TByd.Core.Utils.Runtime
{
    /// <summary>
    /// 提供增强的随机功能
    /// </summary>
    /// <remarks>
    /// RandomUtils类包含一系列增强的随机数生成方法，扩展了Unity内置的Random类功能。
    /// 主要功能包括生成随机布尔值、根据权重随机选择、生成符合特定分布的随机值等。
    /// 
    /// 所有方法均经过优化，适合在游戏开发中使用。
    /// </remarks>
    public static class RandomUtils
    {
        /// <summary>
        /// 用于生成随机数的随机数生成器
        /// </summary>
        private static System.Random _random = new System.Random();
        
        /// <summary>
        /// 用于线程安全操作的锁对象
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// 获取随机布尔值，可指定true的概率
        /// </summary>
        /// <param name="trueChance">返回true的概率，范围[0,1]，默认为0.5</param>
        /// <returns>随机布尔值</returns>
        /// <remarks>
        /// 此方法生成一个随机布尔值，可以指定返回true的概率。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// // 20%的概率返回true
        /// bool result = RandomUtils.Bool(0.2f);
        /// </code>
        /// </remarks>
        public static bool Bool(float trueChance = 0.5f)
        {
            if (trueChance < 0f)
                throw new ArgumentException("概率值不能小于0", nameof(trueChance));
            
            if (trueChance > 1f)
                throw new ArgumentException("概率值不能大于1", nameof(trueChance));
            
            if (trueChance <= 0f)
                return false;
            
            if (trueChance >= 1f)
                return true;
            
            return UnityEngine.Random.value < trueChance;
        }

        /// <summary>
        /// 根据权重随机选择数组中的一个元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="items">元素数组</param>
        /// <param name="weights">权重数组，与items数组长度相同</param>
        /// <returns>随机选择的元素</returns>
        /// <exception cref="ArgumentNullException">items或weights为null时抛出</exception>
        /// <exception cref="ArgumentException">items为空或weights长度与items不匹配时抛出</exception>
        /// <remarks>
        /// 此方法根据指定的权重随机选择数组中的一个元素。权重值越大，被选中的概率越高。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// string[] fruits = { "Apple", "Banana", "Cherry" };
        /// float[] weights = { 1f, 2f, 3f }; // Cherry的选中概率是Apple的3倍
        /// string selectedFruit = RandomUtils.WeightedRandom(fruits, weights);
        /// </code>
        /// </remarks>
        public static T WeightedRandom<T>(T[] items, float[] weights)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            
            if (weights == null)
                throw new ArgumentNullException(nameof(weights));
            
            if (items.Length == 0)
                throw new ArgumentException("Items array cannot be empty", nameof(items));
            
            if (items.Length != weights.Length)
                throw new ArgumentException("Weights array length must match items array length", nameof(weights));
            
            // 计算权重总和
            float totalWeight = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                if (weights[i] < 0f)
                    throw new ArgumentException("Weights cannot be negative", nameof(weights));
                
                totalWeight += weights[i];
            }
            
            if (totalWeight <= 0f)
                throw new ArgumentException("Total weight must be positive", nameof(weights));
            
            // 生成随机值
            float randomValue = UnityEngine.Random.value * totalWeight;
            
            // 查找对应的元素
            float currentWeight = 0f;
            for (int i = 0; i < items.Length; i++)
            {
                currentWeight += weights[i];
                if (randomValue < currentWeight)
                {
                    return items[i];
                }
            }
            
            // 浮点数精度问题可能导致未找到元素，返回最后一个
            return items[items.Length - 1];
        }

        /// <summary>
        /// 生成符合正态分布的随机值
        /// </summary>
        /// <param name="mean">均值，默认为0</param>
        /// <param name="standardDeviation">标准差，默认为1</param>
        /// <returns>符合正态分布的随机值</returns>
        /// <exception cref="ArgumentException">当standardDeviation为负值时抛出</exception>
        /// <remarks>
        /// 此方法使用Box-Muller变换生成符合正态分布的随机值。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// // 生成均值为100，标准差为15的随机值（类似IQ分布）
        /// float randomIQ = RandomUtils.Gaussian(100f, 15f);
        /// </code>
        /// </remarks>
        public static float Gaussian(float mean = 0f, float standardDeviation = 1f)
        {
            if (standardDeviation < 0f)
                throw new ArgumentException("标准差不能为负值", nameof(standardDeviation));
                
            lock (_lock)
            {
                float u1 = 1.0f - (float)_random.NextDouble();
                float u2 = 1.0f - (float)_random.NextDouble();
                
                float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * 
                                      Mathf.Sin(2.0f * Mathf.PI * u2);
                
                return mean + standardDeviation * randStdNormal;
            }
        }

        /// <summary>
        /// 生成随机HSV颜色
        /// </summary>
        /// <param name="saturationMin">最小饱和度，范围[0,1]</param>
        /// <param name="saturationMax">最大饱和度，范围[0,1]</param>
        /// <param name="valueMin">最小明度，范围[0,1]</param>
        /// <param name="valueMax">最大明度，范围[0,1]</param>
        /// <returns>随机HSV颜色</returns>
        /// <remarks>
        /// 此方法生成随机的HSV颜色，可以指定饱和度和明度的范围。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// // 生成鲜艳的随机颜色
        /// Color randomColor = RandomUtils.ColorHSV(0.7f, 1f, 0.7f, 1f);
        /// </code>
        /// </remarks>
        public static Color ColorHSV(float saturationMin = 0f, float saturationMax = 1f,
                                    float valueMin = 0f, float valueMax = 1f)
        {
            // 验证参数范围
            if (saturationMin < 0f)
                throw new ArgumentException("最小饱和度不能小于0", nameof(saturationMin));
            
            if (saturationMax > 1f)
                throw new ArgumentException("最大饱和度不能大于1", nameof(saturationMax));
            
            if (valueMin < 0f)
                throw new ArgumentException("最小明度不能小于0", nameof(valueMin));
            
            if (valueMax > 1f)
                throw new ArgumentException("最大明度不能大于1", nameof(valueMax));
            
            // 验证最小值不大于最大值
            if (saturationMin > saturationMax)
                throw new ArgumentException("最小饱和度不能大于最大饱和度", nameof(saturationMin));
            
            if (valueMin > valueMax)
                throw new ArgumentException("最小明度不能大于最大明度", nameof(valueMin));
            
            float h = UnityEngine.Random.value;
            float s = UnityEngine.Random.Range(saturationMin, saturationMax);
            float v = UnityEngine.Random.Range(valueMin, valueMax);
            
            return Color.HSVToRGB(h, s, v);
        }

        /// <summary>
        /// 生成指定长度的随机ID
        /// </summary>
        /// <param name="length">ID长度</param>
        /// <param name="includeSpecialChars">是否包含特殊字符</param>
        /// <returns>随机生成的ID</returns>
        /// <exception cref="ArgumentException">当length为负值时抛出</exception>
        /// <remarks>
        /// 此方法生成指定长度的随机字符串ID，可以选择是否包含特殊字符。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// // 生成16位随机ID，不包含特殊字符
        /// string id = RandomUtils.GenerateId(16);
        /// </code>
        /// </remarks>
        public static string GenerateId(int length, bool includeSpecialChars = false)
        {
            if (length < 0)
                throw new ArgumentException("ID长度不能为负值", nameof(length));
                
            // 根据includeSpecialChars参数选择合适的字符集
            string allowedChars = includeSpecialChars
                ? StringUtils.AlphanumericAndSpecialChars
                : StringUtils.AlphanumericChars;
                
            return StringUtils.GenerateRandom(length, allowedChars);
        }

        /// <summary>
        /// 随机打乱数组元素顺序
        /// </summary>
        /// <typeparam name="T">数组元素类型</typeparam>
        /// <param name="array">要打乱的数组</param>
        /// <remarks>
        /// 此方法使用Fisher-Yates洗牌算法随机打乱数组中的元素。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// int[] numbers = { 1, 2, 3, 4, 5 };
        /// RandomUtils.Shuffle(numbers);
        /// </code>
        /// </remarks>
        public static void Shuffle<T>(T[] array)
        {
            if (array == null || array.Length < 2)
                return;
            
            int n = array.Length;
            for (int i = n - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                T temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }

        /// <summary>
        /// 从数组中随机选择指定数量的不重复元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">源数组</param>
        /// <param name="count">要选择的元素数量</param>
        /// <returns>随机选择的元素数组</returns>
        /// <remarks>
        /// 此方法从数组中随机选择指定数量的不重复元素。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// string[] names = { "Alice", "Bob", "Charlie", "David", "Eve" };
        /// string[] selected = RandomUtils.RandomSubset(names, 3);
        /// </code>
        /// </remarks>
        public static T[] RandomSubset<T>(T[] array, int count)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            
            if (count <= 0)
                return new T[0];
            
            if (count >= array.Length)
            {
                T[] result = new T[array.Length];
                Array.Copy(array, result, array.Length);
                Shuffle(result);
                return result;
            }
            
            // 创建索引数组
            int[] indices = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                indices[i] = i;
            }
            
            // 打乱索引
            Shuffle(indices);
            
            // 选择前count个元素
            T[] subset = new T[count];
            for (int i = 0; i < count; i++)
            {
                subset[i] = array[indices[i]];
            }
            
            return subset;
        }

        /// <summary>
        /// 生成指定范围内的随机整数，包含最小值，不包含最大值
        /// </summary>
        /// <param name="minInclusive">最小值（包含）</param>
        /// <param name="maxExclusive">最大值（不包含）</param>
        /// <returns>随机整数</returns>
        /// <remarks>
        /// 此方法是线程安全的，可以在多线程环境中使用。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// // 生成1到6之间的随机整数（模拟骰子）
        /// int diceRoll = RandomUtils.Range(1, 7);
        /// </code>
        /// </remarks>
        public static int Range(int minInclusive, int maxExclusive)
        {
            lock (_lock)
            {
                return _random.Next(minInclusive, maxExclusive);
            }
        }

        /// <summary>
        /// 设置随机种子
        /// </summary>
        /// <param name="seed">种子值</param>
        /// <remarks>
        /// 此方法设置随机数生成器的种子，使随机序列可重现。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// // 设置固定种子，使随机序列可重现
        /// RandomUtils.SetSeed(12345);
        /// </code>
        /// </remarks>
        public static void SetSeed(int seed)
        {
            lock (_lock)
            {
                _random = new System.Random(seed);
                UnityEngine.Random.InitState(seed);
            }
        }
        
        /// <summary>
        /// 从数组中随机获取一个元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">源数组</param>
        /// <returns>随机选择的元素</returns>
        /// <exception cref="ArgumentNullException">当array为null时抛出</exception>
        /// <exception cref="ArgumentException">当array为空时抛出</exception>
        /// <remarks>
        /// 此方法从数组中随机选择一个元素。
        /// </remarks>
        [Obsolete("此方法将在1.0.0版本中移除，请使用RandomSubset方法并指定count为1替代", false)]
        public static T GetRandom<T>(T[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            
            if (array.Length == 0)
                throw new ArgumentException("数组不能为空", nameof(array));
            
            int index = UnityEngine.Random.Range(0, array.Length);
            return array[index];
        }
    }
} 