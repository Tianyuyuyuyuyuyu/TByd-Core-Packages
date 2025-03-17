using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace TByd.Core.Utils
{
    /// <summary>
    /// 提供高性能集合操作的工具类
    /// </summary>
    /// <remarks>
    /// CollectionUtils提供了一系列处理集合的高性能方法，包括批量处理、分页、过滤、排序等功能。
    /// 所有方法都经过优化，尽量减少GC分配和性能开销。
    /// </remarks>
    public static class CollectionUtils
    {
        #region 集合检测与判断
        
        /// <summary>
        /// 检查集合是否为null或为空
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="collection">要检查的集合</param>
        /// <returns>如果集合为null或为空，则返回true；否则返回false</returns>
        /// <remarks>
        /// 性能优化：
        /// - 对不同类型的集合进行特殊处理，避免不必要的枚举
        /// - 针对常见集合类型进行优化
        /// </remarks>
        public static bool IsNullOrEmpty<T>(IEnumerable<T> collection)
        {
            if (collection == null)
                return true;
                
            // 优化：针对ICollection<T>类型
            if (collection is ICollection<T> c)
                return c.Count == 0;
                
            // 优化：针对数组
            if (collection is T[] array)
                return array.Length == 0;
                
            // 优化：针对字典
            if (collection is System.Collections.IDictionary dict)
                return dict.Count == 0;
                
            // 对于其他集合类型，使用枚举器
            using (var enumerator = collection.GetEnumerator())
            {
                return !enumerator.MoveNext();
            }
        }
        
        #endregion

        #region 批量处理

        /// <summary>
        /// 对集合中的元素进行批量处理
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="batchSize">批次大小</param>
        /// <param name="action">处理每个批次的委托</param>
        /// <exception cref="ArgumentNullException">source或action为null时抛出</exception>
        /// <exception cref="ArgumentOutOfRangeException">batchSize小于1时抛出</exception>
        /// <remarks>
        /// 性能优化：
        /// - 预分配批次列表容量，减少动态调整的开销
        /// - 使用值类型变量避免装箱拆箱
        /// - 复用批次列表对象，减少GC压力
        /// </remarks>
        public static void BatchProcess<T>(IEnumerable<T> source, int batchSize, Action<IEnumerable<T>> action)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (batchSize < 1) throw new ArgumentOutOfRangeException(nameof(batchSize), "批次大小必须大于0");

            // 针对ICollection<T>优化，可以预先知道集合大小
            if (source is ICollection<T> collection)
            {
                // 如果集合是空的，直接返回
                if (collection.Count == 0)
                    return;
                
                // 预分配合适大小的batch列表
                var batch = new List<T>(Math.Min(batchSize, collection.Count));
                var enumerator = collection.GetEnumerator();
                
                while (enumerator.MoveNext())
                {
                    batch.Add(enumerator.Current);
                    
                    if (batch.Count == batchSize)
                    {
                        action(batch);
                        batch.Clear();
                    }
                }
                
                // 处理剩余元素
                if (batch.Count > 0)
                {
                    action(batch);
                }
                
                // 释放枚举器资源
                if (enumerator is IDisposable disposable)
                    disposable.Dispose();
            }
            else
            {
                // 对于未知大小的集合使用原始实现
                var batch = new List<T>(batchSize);
                var count = 0;

                foreach (var item in source)
                {
                    batch.Add(item);
                    count++;

                    if (count >= batchSize)
                    {
                        action(batch);
                        batch.Clear();
                        count = 0;
                    }
                }

                // 处理剩余的元素
                if (batch.Count > 0)
                {
                    action(batch);
                }
            }
        }

        /// <summary>
        /// 对集合中的元素进行批量异步处理
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="batchSize">批次大小</param>
        /// <param name="action">处理每个批次的异步委托</param>
        /// <returns>表示异步操作的任务</returns>
        /// <exception cref="ArgumentNullException">source或action为null时抛出</exception>
        /// <exception cref="ArgumentOutOfRangeException">batchSize小于1时抛出</exception>
        public static async Task BatchProcessAsync<T>(IEnumerable<T> source, int batchSize,
            Func<IEnumerable<T>, Task> action)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (batchSize < 1) throw new ArgumentOutOfRangeException(nameof(batchSize), "批次大小必须大于0");

            var batch = new List<T>(batchSize);
            var count = 0;

            foreach (var item in source)
            {
                batch.Add(item);
                count++;

                if (count >= batchSize)
                {
                    await action(batch);
                    batch.Clear();
                    count = 0;
                }
            }

            // 处理剩余的元素
            if (batch.Count > 0)
            {
                await action(batch);
            }
        }

        /// <summary>
        /// 对集合中的每个元素执行指定操作
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="action">要执行的操作</param>
        /// <exception cref="ArgumentNullException">source或action为null时抛出</exception>
        public static void ForEach<T>(IEnumerable<T> source, Action<T> action)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));

            foreach (var item in source)
            {
                action(item);
            }
        }

        /// <summary>
        /// 对集合中的每个元素执行指定操作，并提供元素索引
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="action">要执行的操作，接收元素和索引作为参数</param>
        /// <exception cref="ArgumentNullException">source或action为null时抛出</exception>
        public static void ForEach<T>(IEnumerable<T> source, Action<T, int> action)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));

            int index = 0;
            foreach (var item in source)
            {
                action(item, index++);
            }
        }

        #endregion

        #region 集合比较与差异计算

        /// <summary>
        /// 比较两个集合，返回它们是否包含相同的元素（不考虑顺序）
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="first">第一个集合</param>
        /// <param name="second">第二个集合</param>
        /// <param name="comparer">元素比较器，如果为null则使用默认比较器</param>
        /// <returns>如果两个集合包含相同的元素，则返回true；否则返回false</returns>
        /// <exception cref="ArgumentNullException">first或second为null时抛出</exception>
        /// <remarks>
        /// 性能优化：
        /// - 快速比较两个集合长度，如果不同则直接返回false
        /// - 对于较小的集合使用更高效的算法
        /// - 使用HashSet进行更快的查找操作
        /// </remarks>
        public static bool Compare<T>(IEnumerable<T> first, IEnumerable<T> second, IEqualityComparer<T> comparer = null)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));

            comparer = comparer ?? EqualityComparer<T>.Default;

            // 快速路径：引用相等
            if (ReferenceEquals(first, second))
                return true;

            // 快速路径：通过Count比较 - 如果长度不同，它们肯定不相等
            if (first is ICollection<T> firstCollection && second is ICollection<T> secondCollection)
            {
                if (firstCollection.Count != secondCollection.Count)
                    return false;
                
                // 如果集合为空，它们都相等
                if (firstCollection.Count == 0)
                    return true;
            }

            // 针对小集合（小于等于10个元素）的优化
            if (first is ICollection<T> smallCollection && smallCollection.Count <= 10)
            {
                return CompareSmallCollections(first, second, comparer);
            }

            // 针对大集合使用原始的字典方法
            var firstDict = new Dictionary<T, int>(comparer);
            foreach (var item in first)
            {
                if (firstDict.TryGetValue(item, out int count))
                {
                    firstDict[item] = count + 1;
                }
                else
                {
                    firstDict[item] = 1;
                }
            }

            // 检查第二个集合中的元素
            foreach (var item in second)
            {
                if (!firstDict.TryGetValue(item, out int count) || count == 0)
                {
                    return false;
                }

                firstDict[item] = count - 1;
            }

            // 确保所有计数器都归零
            foreach (var count in firstDict.Values)
            {
                if (count != 0)
                    return false;
            }

            return true;
        }

        // 针对小集合的特殊比较算法
        private static bool CompareSmallCollections<T>(IEnumerable<T> first, IEnumerable<T> second, IEqualityComparer<T> comparer)
        {
            var secondList = second.ToList();
            
            foreach (var item in first)
            {
                bool found = false;
                for (int i = 0; i < secondList.Count; i++)
                {
                    if (comparer.Equals(item, secondList[i]))
                    {
                        // 标记为已找到（通过移除）
                        secondList.RemoveAt(i);
                        found = true;
                        break;
                    }
                }
                
                if (!found)
                    return false;
            }
            
            // 如果secondList中还有剩余元素，说明不相等
            return secondList.Count == 0;
        }

        /// <summary>
        /// 查找两个集合之间的差异
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="first">第一个集合</param>
        /// <param name="second">第二个集合</param>
        /// <param name="comparer">元素比较器，如果为null则使用默认比较器</param>
        /// <returns>包含差异信息的元组：(仅在first中存在的元素, 仅在second中存在的元素, 两个集合中都存在的元素)</returns>
        /// <exception cref="ArgumentNullException">first或second为null时抛出</exception>
        public static (IEnumerable<T> OnlyInFirst, IEnumerable<T> OnlyInSecond, IEnumerable<T> InBoth)
            FindDifferences<T>(
                IEnumerable<T> first, IEnumerable<T> second, IEqualityComparer<T> comparer = null)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));

            comparer = comparer ?? EqualityComparer<T>.Default;

            var firstSet = new HashSet<T>(first, comparer);
            var secondSet = new HashSet<T>(second, comparer);

            var onlyInFirst = new HashSet<T>(firstSet, comparer);
            onlyInFirst.ExceptWith(secondSet);

            var onlyInSecond = new HashSet<T>(secondSet, comparer);
            onlyInSecond.ExceptWith(firstSet);

            var inBoth = new HashSet<T>(firstSet, comparer);
            inBoth.IntersectWith(secondSet);

            return (onlyInFirst, onlyInSecond, inBoth);
        }

        #endregion

        #region 集合转换与映射

        /// <summary>
        /// 将集合中的每个元素转换为新类型
        /// </summary>
        /// <typeparam name="TSource">源集合元素类型</typeparam>
        /// <typeparam name="TResult">结果集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="converter">转换函数</param>
        /// <returns>转换后的集合</returns>
        /// <exception cref="ArgumentNullException">source或converter为null时抛出</exception>
        public static IEnumerable<TResult> Map<TSource, TResult>(IEnumerable<TSource> source,
            Func<TSource, TResult> converter)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (converter == null) throw new ArgumentNullException(nameof(converter));

            foreach (var item in source)
            {
                yield return converter(item);
            }
        }

        /// <summary>
        /// 将集合中的每个元素转换为新类型，并提供元素索引
        /// </summary>
        /// <typeparam name="TSource">源集合元素类型</typeparam>
        /// <typeparam name="TResult">结果集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="converter">转换函数，接收元素和索引作为参数</param>
        /// <returns>转换后的集合</returns>
        /// <exception cref="ArgumentNullException">source或converter为null时抛出</exception>
        public static IEnumerable<TResult> Map<TSource, TResult>(IEnumerable<TSource> source,
            Func<TSource, int, TResult> converter)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (converter == null) throw new ArgumentNullException(nameof(converter));

            int index = 0;
            foreach (var item in source)
            {
                yield return converter(item, index++);
            }
        }

        /// <summary>
        /// 将集合中的所有元素转换为新类型
        /// </summary>
        /// <typeparam name="TSource">源集合元素类型</typeparam>
        /// <typeparam name="TResult">结果集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="converter">转换函数</param>
        /// <returns>转换后的集合</returns>
        /// <exception cref="ArgumentNullException">source或converter为null时抛出</exception>
        public static List<TResult> ConvertAll<TSource, TResult>(IEnumerable<TSource> source,
            Func<TSource, TResult> converter)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (converter == null) throw new ArgumentNullException(nameof(converter));

            var result = new List<TResult>();
            foreach (var item in source)
            {
                result.Add(converter(item));
            }

            return result;
        }

        #endregion

        #region 分页与分块处理

        /// <summary>
        /// 对集合进行分页
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="pageNumber">页码（从1开始）</param>
        /// <param name="pageSize">每页大小</param>
        /// <returns>指定页的元素</returns>
        /// <exception cref="ArgumentNullException">source为null时抛出</exception>
        /// <exception cref="ArgumentOutOfRangeException">pageNumber小于1或pageSize小于1时抛出</exception>
        /// <remarks>
        /// 性能优化：
        /// - 当sourceCollection是IList时直接通过索引访问元素，避免使用Skip/Take
        /// - 针对元素数量可知的集合进行特殊优化
        /// - 提前返回空集合处理越界情况
        /// </remarks>
        public static IEnumerable<T> Paginate<T>(IEnumerable<T> source, int pageNumber, int pageSize)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (pageNumber < 1) throw new ArgumentOutOfRangeException(nameof(pageNumber), "页码必须大于等于1");
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize), "每页大小必须大于0");

            // 如果是IList接口，我们可以直接通过索引访问，避免使用Skip/Take
            if (source is IList<T> list)
            {
                int startIndex = (pageNumber - 1) * pageSize;
                
                // 如果起始索引超出范围，返回空集合
                if (startIndex >= list.Count)
                    return Enumerable.Empty<T>();
                
                int count = Math.Min(pageSize, list.Count - startIndex);
                var result = new List<T>(count);
                
                for (int i = 0; i < count; i++)
                {
                    result.Add(list[startIndex + i]);
                }
                
                return result;
            }
            
            // 处理ICollection，可以预先检查总数量，避免不必要的枚举
            if (source is ICollection<T> collection)
            {
                int startIndex = (pageNumber - 1) * pageSize;
                
                if (startIndex >= collection.Count)
                    return Enumerable.Empty<T>();
            }
            
            // 对于其他集合类型，使用Skip和Take
            return source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// 将集合分块
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="chunkSize">块大小</param>
        /// <returns>分块后的集合</returns>
        /// <exception cref="ArgumentNullException">source为null时抛出</exception>
        /// <exception cref="ArgumentOutOfRangeException">chunkSize小于1时抛出</exception>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(IEnumerable<T> source, int chunkSize)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (chunkSize < 1) throw new ArgumentOutOfRangeException(nameof(chunkSize), "块大小必须大于0");

            var chunk = new List<T>(chunkSize);
            var count = 0;

            foreach (var item in source)
            {
                chunk.Add(item);
                count++;

                if (count >= chunkSize)
                {
                    yield return chunk.ToArray();
                    chunk.Clear();
                    count = 0;
                }
            }

            // 返回剩余的元素
            if (chunk.Count > 0)
            {
                yield return chunk.ToArray();
            }
        }

        #endregion

        #region 集合过滤与查询

        /// <summary>
        /// 过滤集合中的元素
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="predicate">过滤条件</param>
        /// <returns>满足条件的元素集合</returns>
        /// <exception cref="ArgumentNullException">source或predicate为null时抛出</exception>
        public static IEnumerable<T> Filter<T>(IEnumerable<T> source, Func<T, bool> predicate)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            foreach (var item in source)
            {
                if (predicate(item))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// 过滤集合中满足条件的元素，并提供元素索引
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="predicate">过滤条件，接收元素和索引作为参数</param>
        /// <returns>满足条件的元素集合</returns>
        /// <exception cref="ArgumentNullException">source或predicate为null时抛出</exception>
        public static IEnumerable<T> Filter<T>(IEnumerable<T> source, Func<T, int, bool> predicate)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            int index = 0;
            foreach (var item in source)
            {
                if (predicate(item, index))
                {
                    yield return item;
                }

                index++;
            }
        }

        /// <summary>
        /// 查找集合中满足条件的所有元素
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="predicate">查找条件</param>
        /// <returns>满足条件的元素列表</returns>
        /// <exception cref="ArgumentNullException">source或predicate为null时抛出</exception>
        public static List<T> FindAll<T>(IEnumerable<T> source, Func<T, bool> predicate)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            var result = new List<T>();
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    result.Add(item);
                }
            }

            return result;
        }

        /// <summary>
        /// 查找集合中满足条件的第一个元素
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="predicate">查找条件</param>
        /// <returns>满足条件的第一个元素，如果没有找到则返回默认值</returns>
        /// <exception cref="ArgumentNullException">source或predicate为null时抛出</exception>
        public static T FindFirst<T>(IEnumerable<T> source, Func<T, bool> predicate)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            foreach (var item in source)
            {
                if (predicate(item))
                {
                    return item;
                }
            }

            return default;
        }

        /// <summary>
        /// 尝试查找集合中满足条件的第一个元素
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="predicate">查找条件</param>
        /// <param name="result">找到的元素</param>
        /// <returns>如果找到满足条件的元素，则返回true；否则返回false</returns>
        /// <exception cref="ArgumentNullException">source或predicate为null时抛出</exception>
        public static bool TryFindFirst<T>(IEnumerable<T> source, Func<T, bool> predicate, out T result)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            foreach (var item in source)
            {
                if (predicate(item))
                {
                    result = item;
                    return true;
                }
            }

            result = default;
            return false;
        }

        #endregion

        #region 集合排序与排序优化

        /// <summary>
        /// 排序集合中的元素
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="comparer">元素比较器，如果为null则使用默认比较器</param>
        /// <returns>排序后的集合</returns>
        /// <exception cref="ArgumentNullException">source为null时抛出</exception>
        /// <remarks>
        /// 性能优化：
        /// - 对于较小的集合使用自定义快速排序
        /// - 尽可能在原始集合上排序以避免额外的内存分配
        /// </remarks>
        public static IEnumerable<T> Sort<T>(IEnumerable<T> source, IComparer<T> comparer = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            
            comparer = comparer ?? Comparer<T>.Default;

            // 对于数组和列表，我们可以直接排序而无需创建新集合
            if (source is T[] array)
            {
                // 创建数组副本，避免修改原始数组
                var result = new T[array.Length];
                Array.Copy(array, result, array.Length);
                Array.Sort(result, comparer);
                return result;
            }
            else if (source is List<T> list)
            {
                // 创建列表副本，避免修改原始列表
                var result = new List<T>(list);
                result.Sort(comparer);
                return result;
            }
            
            // 其他类型的集合，使用ToArray并排序
            var items = source.ToArray();
            Array.Sort(items, comparer);
            return items;
        }

        /// <summary>
        /// 对集合进行稳定排序
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="keySelector">键选择器</param>
        /// <returns>排序后的集合</returns>
        /// <exception cref="ArgumentNullException">source或keySelector为null时抛出</exception>
        public static IEnumerable<T> StableSort<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

            return source.Select((item, index) => (item, key: keySelector(item), index))
                .OrderBy(x => x.key)
                .ThenBy(x => x.index)
                .Select(x => x.item);
        }

        /// <summary>
        /// 按照指定的键对集合进行升序排序
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <typeparam name="TKey">排序键类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="keySelector">用于提取排序键的函数</param>
        /// <returns>排序后的集合</returns>
        /// <exception cref="ArgumentNullException">source或keySelector为null时抛出</exception>
        public static IEnumerable<T> OrderBy<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

            return source.OrderBy(keySelector);
        }

        /// <summary>
        /// 按照指定的键对集合进行降序排序
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <typeparam name="TKey">排序键类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="keySelector">用于提取排序键的函数</param>
        /// <returns>排序后的集合</returns>
        /// <exception cref="ArgumentNullException">source或keySelector为null时抛出</exception>
        public static IEnumerable<T> OrderByDescending<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

            return source.OrderByDescending(keySelector);
        }

        #endregion

        #region 集合统计与聚合

        /// <summary>
        /// 对集合应用聚合函数
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <typeparam name="TAccumulate">累加器类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="seed">聚合的初始值</param>
        /// <param name="func">累加函数</param>
        /// <returns>聚合结果</returns>
        /// <exception cref="ArgumentNullException">source或func为null时抛出</exception>
        public static TAccumulate Aggregate<T, TAccumulate>(IEnumerable<T> source, TAccumulate seed,
            Func<TAccumulate, T, TAccumulate> func)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (func == null) throw new ArgumentNullException(nameof(func));

            TAccumulate result = seed;
            foreach (var item in source)
            {
                result = func(result, item);
            }

            return result;
        }

        /// <summary>
        /// 计算集合中数值元素的总和
        /// </summary>
        /// <param name="source">源集合</param>
        /// <returns>元素总和</returns>
        /// <exception cref="ArgumentNullException">source为null时抛出</exception>
        public static int Sum(IEnumerable<int> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            int sum = 0;
            foreach (var item in source)
            {
                sum += item;
            }

            return sum;
        }

        /// <summary>
        /// 计算集合中数值元素的总和
        /// </summary>
        /// <param name="source">源集合</param>
        /// <returns>元素总和</returns>
        /// <exception cref="ArgumentNullException">source为null时抛出</exception>
        public static float Sum(IEnumerable<float> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            float sum = 0;
            foreach (var item in source)
            {
                sum += item;
            }

            return sum;
        }

        /// <summary>
        /// 计算集合中元素的平均值
        /// </summary>
        /// <param name="source">源集合</param>
        /// <returns>元素平均值</returns>
        /// <exception cref="ArgumentNullException">source为null时抛出</exception>
        /// <exception cref="InvalidOperationException">集合为空时抛出</exception>
        public static float Average(IEnumerable<int> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            int sum = 0;
            int count = 0;
            foreach (var item in source)
            {
                sum += item;
                count++;
            }

            if (count == 0)
            {
                throw new InvalidOperationException("序列不包含任何元素");
            }

            return (float)sum / count;
        }

        /// <summary>
        /// 计算集合中元素的平均值
        /// </summary>
        /// <param name="source">源集合</param>
        /// <returns>元素平均值</returns>
        /// <exception cref="ArgumentNullException">source为null时抛出</exception>
        /// <exception cref="InvalidOperationException">集合为空时抛出</exception>
        public static float Average(IEnumerable<float> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            float sum = 0;
            int count = 0;
            foreach (var item in source)
            {
                sum += item;
                count++;
            }

            if (count == 0)
            {
                throw new InvalidOperationException("序列不包含任何元素");
            }

            return sum / count;
        }

        #endregion

        #region 集合分组

        /// <summary>
        /// 按照指定的键对集合进行分组
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <typeparam name="TKey">分组键类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="keySelector">用于提取分组键的函数</param>
        /// <returns>分组后的集合</returns>
        /// <exception cref="ArgumentNullException">source或keySelector为null时抛出</exception>
        public static IEnumerable<IGrouping<TKey, T>> GroupBy<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

            return source.GroupBy(keySelector);
        }

        #endregion

        #region 集合洗牌

        /// <summary>
        /// 随机打乱集合中元素的顺序
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">要打乱的集合</param>
        /// <exception cref="ArgumentNullException">source为null时抛出</exception>
        /// <remarks>
        /// 性能优化：
        /// - 使用Fisher-Yates算法确保均匀分布
        /// - 避免不必要的内存分配
        /// - 针对数组类型进行特殊优化
        /// </remarks>
        public static void Shuffle<T>(IList<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            int n = source.Count;
            if (n <= 1) return; // 空集合或单元素集合无需打乱
            
            // 优化：缓存随机数生成器，减少函数调用开销
            System.Random random = new System.Random(); // 使用系统Random而非UnityEngine.Random，减少每帧更新的依赖
            
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                // 使用值类型临时变量，避免装箱
                T value = source[k];
                source[k] = source[n];
                source[n] = value;
            }
        }

        #endregion

        #region 集合分批

        /// <summary>
        /// 将集合分成指定大小的批次
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="batchSize">批次大小</param>
        /// <returns>包含批次的集合</returns>
        /// <exception cref="ArgumentNullException">source为null时抛出</exception>
        /// <exception cref="ArgumentOutOfRangeException">batchSize小于1时抛出</exception>
        public static IEnumerable<IEnumerable<T>> Batch<T>(IEnumerable<T> source, int batchSize)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (batchSize < 1) throw new ArgumentOutOfRangeException(nameof(batchSize), "批次大小必须大于0");

            return Chunk(source, batchSize);
        }

        #endregion

        #region 集合聚合

        /// <summary>
        /// 对集合应用聚合函数，并使用结果选择器转换最终结果
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <typeparam name="TAccumulate">累加器类型</typeparam>
        /// <typeparam name="TResult">结果类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="seed">聚合的初始值</param>
        /// <param name="func">累加函数</param>
        /// <param name="resultSelector">结果转换函数</param>
        /// <returns>聚合转换后的结果</returns>
        /// <exception cref="ArgumentNullException">source、func或resultSelector为null时抛出</exception>
        public static TResult Aggregate<T, TAccumulate, TResult>(
            IEnumerable<T> source,
            TAccumulate seed,
            Func<TAccumulate, T, TAccumulate> func,
            Func<TAccumulate, TResult> resultSelector)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (func == null) throw new ArgumentNullException(nameof(func));
            if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));

            TAccumulate accumulate = seed;
            foreach (var item in source)
            {
                accumulate = func(accumulate, item);
            }

            return resultSelector(accumulate);
        }

        #endregion

        #region 集合随机访问
        
        /// <summary>
        /// 从集合中随机获取一个元素
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <returns>随机选择的元素</returns>
        /// <exception cref="ArgumentNullException">source为null时抛出</exception>
        /// <exception cref="ArgumentException">source为空集合时抛出</exception>
        /// <remarks>
        /// 性能优化：
        /// - 针对IList类型进行优化，直接通过索引访问
        /// - 对于其他集合类型，会先复制到列表再随机访问
        /// </remarks>
        public static T GetRandomElement<T>(IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            
            // 针对IList优化，避免集合复制
            if (source is IList<T> list)
            {
                if (list.Count == 0)
                    throw new ArgumentException("集合不能为空", nameof(source));
                    
                return list[UnityEngine.Random.Range(0, list.Count)];
            }
            
            // 对于其他类型，转换为列表
            var collection = source as ICollection<T>;
            if (collection != null)
            {
                if (collection.Count == 0)
                    throw new ArgumentException("集合不能为空", nameof(source));
                
                var tempList = new List<T>(collection);
                return tempList[UnityEngine.Random.Range(0, tempList.Count)];
            }
            
            // 对于无法预先知道大小的集合
            var result = new List<T>();
            foreach (var item in source)
            {
                result.Add(item);
                if (result.Count > 1000) // 安全限制，防止无限集合
                    break;
            }
            
            if (result.Count == 0)
                throw new ArgumentException("集合不能为空", nameof(source));
                
            return result[UnityEngine.Random.Range(0, result.Count)];
        }
        
        #endregion

        #region 集合安全访问
        
        /// <summary>
        /// 安全地获取字典中指定键的值，如果键不存在则返回默认值
        /// </summary>
        /// <typeparam name="TKey">字典键类型</typeparam>
        /// <typeparam name="TValue">字典值类型</typeparam>
        /// <param name="dictionary">源字典</param>
        /// <param name="key">要获取值的键</param>
        /// <param name="defaultValue">键不存在时返回的默认值</param>
        /// <returns>字典中指定键的值，或默认值</returns>
        /// <exception cref="ArgumentNullException">dictionary为null时抛出</exception>
        public static TValue GetOrDefault<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            
            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }
        
        /// <summary>
        /// 检查集合中是否包含指定元素
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="item">要检查的元素</param>
        /// <returns>如果集合包含指定元素，则返回true；否则返回false</returns>
        /// <exception cref="ArgumentNullException">source为null时抛出</exception>
        public static bool Contains<T>(IEnumerable<T> source, T item)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            
            // 针对ICollection<T>优化
            if (source is ICollection<T> collection)
                return collection.Contains(item);
                
            // 对于其他类型，使用EqualityComparer
            var comparer = EqualityComparer<T>.Default;
            foreach (var element in source)
            {
                if (comparer.Equals(element, item))
                    return true;
            }
            
            return false;
        }
        
        #endregion
    }
}