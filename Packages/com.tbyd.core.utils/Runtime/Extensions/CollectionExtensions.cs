using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TByd.Core.Utils.Runtime.Extensions
{
    /// <summary>
    /// 集合类型的扩展方法集合
    /// </summary>
    /// <remarks>
    /// 这个类提供了一系列实用的集合扩展方法，简化了常见的集合操作，
    /// 如随机洗牌、批量处理、安全访问等。
    /// </remarks>
    public static class CollectionExtensions
    {
        /// <summary>
        /// 随机打乱集合中的元素
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="list">要打乱的列表</param>
        /// <returns>打乱后的列表引用，用于链式调用</returns>
        /// <remarks>
        /// 此方法使用Fisher-Yates洗牌算法随机打乱列表中的元素。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// List&lt;int&gt; numbers = new List&lt;int&gt; { 1, 2, 3, 4, 5 };
        /// numbers.Shuffle();
        /// </code>
        /// </remarks>
        public static List<T> Shuffle<T>(this List<T> list)
        {
            if (list == null || list.Count < 2)
            {
                return list;
            }

            System.Random random = new System.Random();
            int n = list.Count;
            
            for (int i = n - 1; i > 0; i--)
            {
                int j = random.Next(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
            
            return list;
        }

        /// <summary>
        /// 将集合分割成指定大小的批次
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="batchSize">每个批次的大小</param>
        /// <returns>批次集合的枚举器</returns>
        /// <remarks>
        /// 此方法将集合分割成多个批次，每个批次包含指定数量的元素。
        /// 最后一个批次可能包含少于指定数量的元素。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// List&lt;int&gt; numbers = new List&lt;int&gt; { 1, 2, 3, 4, 5, 6, 7 };
        /// foreach (var batch in numbers.Batch(3))
        /// {
        ///     // 第一批: [1, 2, 3]
        ///     // 第二批: [4, 5, 6]
        ///     // 第三批: [7]
        /// }
        /// </code>
        /// </remarks>
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (batchSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(batchSize), "批次大小必须大于0");
            }

            List<T> batch = new List<T>(batchSize);
            foreach (var item in source)
            {
                batch.Add(item);
                if (batch.Count == batchSize)
                {
                    yield return batch;
                    batch = new List<T>(batchSize);
                }
            }

            if (batch.Count > 0)
            {
                yield return batch;
            }
        }

        /// <summary>
        /// 安全地获取集合中指定索引的元素，如果索引越界则返回默认值
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="list">源列表</param>
        /// <param name="index">要获取的元素索引</param>
        /// <param name="defaultValue">索引越界时返回的默认值</param>
        /// <returns>指定索引的元素，或默认值</returns>
        /// <remarks>
        /// 此方法安全地访问列表中的元素，避免索引越界异常。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// List&lt;string&gt; names = new List&lt;string&gt; { "Alice", "Bob", "Charlie" };
        /// string name = names.GetSafe(5, "Unknown"); // 返回 "Unknown"
        /// </code>
        /// </remarks>
        public static T GetSafe<T>(this IList<T> list, int index, T defaultValue = default)
        {
            if (list == null || index < 0 || index >= list.Count)
            {
                return defaultValue;
            }
            
            return list[index];
        }

        /// <summary>
        /// 安全地获取字典中指定键的值，如果键不存在则返回默认值
        /// </summary>
        /// <typeparam name="TKey">字典键类型</typeparam>
        /// <typeparam name="TValue">字典值类型</typeparam>
        /// <param name="dictionary">源字典</param>
        /// <param name="key">要获取的值的键</param>
        /// <param name="defaultValue">键不存在时返回的默认值</param>
        /// <returns>指定键的值，或默认值</returns>
        /// <remarks>
        /// 此方法安全地访问字典中的值，避免键不存在异常。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// Dictionary&lt;string, int&gt; scores = new Dictionary&lt;string, int&gt;
        /// {
        ///     { "Alice", 95 },
        ///     { "Bob", 87 }
        /// };
        /// int charlieScore = scores.GetSafe("Charlie", 0); // 返回 0
        /// </code>
        /// </remarks>
        public static TValue GetSafe<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default)
        {
            if (dictionary == null || key == null || !dictionary.ContainsKey(key))
            {
                return defaultValue;
            }
            
            return dictionary[key];
        }

        /// <summary>
        /// 从集合中随机获取一个元素
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="list">源列表</param>
        /// <returns>随机选择的元素</returns>
        /// <remarks>
        /// 此方法从列表中随机选择一个元素。如果列表为空，则抛出异常。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// List&lt;string&gt; fruits = new List&lt;string&gt; { "Apple", "Banana", "Cherry" };
        /// string randomFruit = fruits.GetRandom(); // 随机返回一种水果
        /// </code>
        /// </remarks>
        public static T GetRandom<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0)
            {
                throw new ArgumentException("集合不能为空", nameof(list));
            }
            
            int randomIndex = UnityEngine.Random.Range(0, list.Count);
            return list[randomIndex];
        }

        /// <summary>
        /// 从集合中随机获取指定数量的不重复元素
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="list">源列表</param>
        /// <param name="count">要获取的元素数量</param>
        /// <returns>随机选择的元素集合</returns>
        /// <remarks>
        /// 此方法从列表中随机选择指定数量的不重复元素。
        /// 如果请求的数量大于列表中的元素数量，则返回打乱后的整个列表。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// List&lt;int&gt; numbers = new List&lt;int&gt; { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        /// List&lt;int&gt; selected = numbers.GetRandomSubset(3); // 随机返回3个不同的数字
        /// </code>
        /// </remarks>
        public static List<T> GetRandomSubset<T>(this IList<T> list, int count)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }
            
            if (count <= 0)
            {
                return new List<T>();
            }
            
            if (count >= list.Count)
            {
                List<T> result = new List<T>(list);
                result.Shuffle();
                return result;
            }
            
            // 使用Fisher-Yates算法的变体来选择子集
            List<T> subset = new List<T>(count);
            List<T> tempList = new List<T>(list);
            tempList.Shuffle();
            
            for (int i = 0; i < count; i++)
            {
                subset.Add(tempList[i]);
            }
            
            return subset;
        }
    }
} 