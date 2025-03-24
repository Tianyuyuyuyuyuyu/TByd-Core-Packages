using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using System.Runtime.CompilerServices;
using System.Buffers;
using System.Text;
using System.Reflection;

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
        // 列表对象池，用于临时操作以减少GC
        private static readonly ObjectPool<List<object>> GenericListPool = new ObjectPool<List<object>>(
            createFunc: () => new List<object>(64),
            actionOnGet: list => list.Clear(),
            actionOnRelease: list => list.Clear(),
            actionOnDestroy: null,
            collectionCheck: false,
            defaultCapacity: 16,
            maxSize: 32
        );
        
        // 随机数生成器缓存
        private static readonly System.Random CachedRandom = new System.Random();

        // StringBuilder对象池
        private static readonly ObjectPool<StringBuilder> StringBuilderPool = new ObjectPool<StringBuilder>(
            createFunc: () => new StringBuilder(256),
            actionOnGet: sb => sb.Clear(),
            actionOnRelease: sb => sb.Clear(),
            actionOnDestroy: null,
            collectionCheck: false,
            defaultCapacity: 16,
            maxSize: 32
        );

        // 缓存Count属性信息以加速反射，使用并发字典确保线程安全
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, PropertyInfo> _countPropertyCache 
            = new System.Collections.Concurrent.ConcurrentDictionary<Type, PropertyInfo>();

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
        /// - 针对常见集合类型进行优化，避免装箱操作
        /// - 使用泛型特化避免接口调用的装箱
        /// - 减少方法调用和枚举器分配
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(IEnumerable<T> collection)
        {
            if (collection == null)
                return true;
                
            // 优化1: 最常见的几种集合类型直接检查Count属性
            
            // 对于List<T>（最常见的集合类型）
            if (collection is List<T> list)
                return list.Count == 0;
                
            // 对于数组类型
            if (collection is T[] array)
                return array.Length == 0;
                
            // 对于ICollection<T>类型
            if (collection is ICollection<T> c)
                return c.Count == 0;
                
            // 对于IReadOnlyCollection<T>类型
            if (collection is IReadOnlyCollection<T> readOnlyCollection)
                return readOnlyCollection.Count == 0;
                
            // 优化2: 处理字典类型，避免装箱
            if (collection is System.Collections.IDictionary dict)
            {
                return dict.Count == 0;
            }
            
            // 优化3: 特殊集合类型
            // HashSet<T>
            if (collection is HashSet<T> hashSet)
                return hashSet.Count == 0;
                
            // Queue<T>
            if (collection is Queue<T> queue)
                return queue.Count == 0;
                
            // Stack<T>
            if (collection is Stack<T> stack)
                return stack.Count == 0;
                
            // 优化4: 自定义集合使用缓存的反射检查Count属性
            Type collectionType = collection.GetType();
            PropertyInfo countProp = GetCountProperty(collectionType);
            if (countProp != null)
            {
                return (int)countProp.GetValue(collection) == 0;
            }
            
            // 优化5: 使用快速枚举检查，确保枚举器被正确释放
            try
            {
                var enumerator = collection.GetEnumerator();
                using (enumerator as IDisposable)
                {
                    return !enumerator.MoveNext();
                }
            }
            catch
            {
                // 如果枚举器抛出异常，安全地返回true（视为空集合）
                return true;
            }
        }

        /// <summary>
        /// 获取类型的Count属性信息
        /// </summary>
        /// <param name="type">要检查的类型</param>
        /// <returns>Count属性信息，如果不存在则返回null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static PropertyInfo GetCountProperty(Type type)
        {
            return _countPropertyCache.GetOrAdd(type, t => 
                t.GetProperty("Count", BindingFlags.Public | BindingFlags.Instance));
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
        /// <exception cref="InvalidOperationException">source为空集合时抛出</exception>
        /// <remarks>
        /// 性能优化：
        /// - 针对IList类型进行优化，直接通过索引访问
        /// - 对于其他集合类型，使用对象池避免临时列表分配
        /// - 使用缓存的随机数生成器避免重复创建
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetRandomElement<T>(IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            
            // 快速路径：针对IList类型，包括List<T>和数组
            if (source is IList<T> list)
            {
                int count = list.Count;
                if (count == 0)
                    throw new InvalidOperationException("集合不能为空");
                
                if (count == 1)
                    return list[0]; // 单元素集合直接返回第一个元素
                    
                return list[UnityEngine.Random.Range(0, count)];
            }
            
            // 优化：针对ICollection类型，预先知道大小
            if (source is ICollection<T> collection)
            {
                int count = collection.Count;
                if (count == 0)
                    throw new InvalidOperationException("集合不能为空");
                
                if (count == 1)
                {
                    using (var enumerator = collection.GetEnumerator())
                    {
                        enumerator.MoveNext();
                        return enumerator.Current;
                    }
                }
                
                // 使用共享数组池避免内存分配
                T[] array = ArrayPool<T>.Shared.Rent(count);
                try
                {
                    int index = 0;
                    foreach (var item in collection)
                    {
                        array[index++] = item;
                    }
                    
                    // 使用UnityEngine.Random，更适合游戏场景
                    return array[UnityEngine.Random.Range(0, count)];
                }
                finally
                {
                    ArrayPool<T>.Shared.Return(array, false);
                }
            }
            
            // 对于无法预先知道大小的集合，使用蓄水池抽样算法
            int itemCount = 0;
            T result = default;
            
            foreach (var item in source)
            {
                itemCount++;
                
                // 使用蓄水池抽样，保证均匀分布
                if (itemCount == 1 || UnityEngine.Random.Range(0, itemCount) == 0)
                {
                    result = item;
                }
                
                // 安全限制，防止无限枚举
                if (itemCount > 1000)
                    break;
            }
            
            if (itemCount == 0)
                throw new InvalidOperationException("集合不能为空");
                
            return result;
        }
        
        /// <summary>
        /// 对集合元素进行深度复制
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <typeparam name="TCollection">集合类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="createCollection">创建新集合的委托</param>
        /// <returns>深度复制的集合</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TCollection DeepCopy<T, TCollection>(IEnumerable<T> source, Func<IEnumerable<T>, TCollection> createCollection)
            where TCollection : ICollection<T>
        {
            if (source == null)
                return createCollection(Enumerable.Empty<T>());
            
            // 如果元素类型是值类型或不可变类型，直接复制
            if (typeof(T).IsValueType || typeof(T) == typeof(string) || typeof(T).IsEnum)
            {
                return createCollection(source);
            }
            
            // 对于需要深度复制的类型，复制每个元素
            var result = new List<T>();
            foreach (var item in source)
            {
                result.Add(item == null ? default : CloneItem(item));
            }
            
            return createCollection(result);
        }
        
        /// <summary>
        /// 从集合中获取随机元素，使用指定的随机数生成器
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="random">随机数生成器</param>
        /// <returns>随机选择的元素</returns>
        /// <exception cref="ArgumentNullException">source或random为null时抛出</exception>
        /// <exception cref="InvalidOperationException">source为空集合时抛出</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetRandomElement<T>(IEnumerable<T> source, System.Random random)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (random == null) throw new ArgumentNullException(nameof(random));
            
            // 快速路径：针对IList类型
            if (source is IList<T> list)
            {
                int count = list.Count;
                if (count == 0)
                    throw new InvalidOperationException("集合不能为空");
                
                if (count == 1)
                    return list[0];
                
                return list[random.Next(count)];
            }
            
            // 针对其他集合类型
            int itemCount = 0;
            T result = default;
            
            foreach (var item in source)
            {
                itemCount++;
                
                // 使用蓄水池抽样算法
                if (itemCount == 1 || random.Next(itemCount) == 0)
                {
                    result = item;
                }
                
                // 安全限制
                if (itemCount > 1000)
                    break;
            }
            
            if (itemCount == 0)
                throw new InvalidOperationException("集合不能为空");
            
            return result;
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

        #region 集合合并与转换
        
        /// <summary>
        /// 合并两个集合
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="first">第一个集合</param>
        /// <param name="second">第二个集合</param>
        /// <returns>合并后的新集合</returns>
        /// <exception cref="ArgumentNullException">first或second为null时抛出</exception>
        /// <remarks>
        /// 此方法创建一个新的集合，包含两个输入集合的所有元素。
        /// 元素顺序保持不变，先是第一个集合的所有元素，然后是第二个集合的所有元素。
        /// </remarks>
        public static List<T> Join<T>(IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));
            
            // 优化：如果可以确定大小，预分配空间
            int capacity = 0;
            if (first is ICollection<T> firstCollection)
                capacity += firstCollection.Count;
            if (second is ICollection<T> secondCollection)
                capacity += secondCollection.Count;
            
            var result = capacity > 0 ? new List<T>(capacity) : new List<T>();
            
            // 添加第一个集合的元素
            result.AddRange(first);
            
            // 添加第二个集合的元素
            result.AddRange(second);
            
            return result;
        }
        
        /// <summary>
        /// 将集合中的元素连接为字符串
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="collection">要连接的集合</param>
        /// <param name="delimiter">分隔符</param>
        /// <returns>连接后的字符串</returns>
        /// <remarks>
        /// 性能优化：
        /// - 使用StringBuilderPool减少GC分配
        /// - 对常见情况使用快速路径
        /// - 预分配合适容量避免扩容
        /// - 针对小字符串使用ArrayPool直接构建
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string JoinToString<T>(IEnumerable<T> collection, string delimiter)
        {
            if (IsNullOrEmpty(collection))
            {
                return string.Empty;
            }

            // 确保分隔符不为null
            delimiter = delimiter ?? string.Empty;
            
            // 检查是否为小集合，如果是则使用ArrayPool而非StringBuilder
            if (collection is ICollection<T> sizedCollection && sizedCollection.Count <= 16)
            {
                // 快速路径：单元素集合不需要分隔符
                if (sizedCollection.Count == 1)
                {
                    using (var enumerator = sizedCollection.GetEnumerator())
                    {
                        enumerator.MoveNext();
                        return enumerator.Current?.ToString() ?? string.Empty;
                    }
                }
                
                // 小集合使用ArrayPool减少StringBuilder的开销
                if (sizedCollection.Count <= 16 && delimiter.Length <= 2)
                {
                    // 估计每项平均20个字符，加上分隔符
                    int estimatedLength = sizedCollection.Count * (20 + delimiter.Length);
                    
                    // 使用共享字符数组池
                    char[] buffer = ArrayPool<char>.Shared.Rent(estimatedLength);
                    try
                    {
                        int position = 0;
                        bool isFirst = true;
                        
                        foreach (var item in sizedCollection)
                        {
                            if (!isFirst)
                            {
                                // 添加分隔符
                                for (int i = 0; i < delimiter.Length; i++)
                                {
                                    buffer[position++] = delimiter[i];
                                }
                            }
                            
                            // 转换并添加当前项
                            if (item != null)
                            {
                                string itemStr = item.ToString();
                                for (int i = 0; i < itemStr.Length && position < buffer.Length; i++)
                                {
                                    buffer[position++] = itemStr[i];
                                }
                            }
                            
                            isFirst = false;
                        }
                        
                        // 从字符数组创建字符串
                        return new string(buffer, 0, position);
                    }
                    finally
                    {
                        // 归还数组到池
                        ArrayPool<char>.Shared.Return(buffer, true); // 清理数组避免泄露
                    }
                }
            }
            
            // 对于大集合或复杂情况，使用StringBuilder
            var sb = StringBuilderPool.Get();
            try
            {
                // 优化预分配容量
                if (collection is ICollection<T> countable)
                {
                    int count = countable.Count;
                    // 估计平均每项20个字符，加上分隔符的长度
                    int estimatedCapacity = count * (20 + delimiter.Length);
                    if (estimatedCapacity > sb.Capacity)
                    {
                        sb.Capacity = Math.Min(estimatedCapacity, 1024 * 16); // 限制最大容量
                    }
                }
                
                bool isFirst = true;
                foreach (var item in collection)
                {
                    if (!isFirst)
                    {
                        sb.Append(delimiter);
                    }
                    
                    if (item != null)
                    {
                        sb.Append(item.ToString());
                    }
                    
                    isFirst = false;
                }
                
                return sb.ToString();
            }
            finally
            {
                // 归还StringBuilder到对象池
                StringBuilderPool.Release(sb);
            }
        }
        
        /// <summary>
        /// 将集合元素连接为字符串，允许自定义转换器
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="collection">要连接的集合</param>
        /// <param name="delimiter">分隔符</param>
        /// <param name="converter">元素转换器方法</param>
        /// <returns>连接后的字符串</returns>
        /// <exception cref="ArgumentNullException">collection或converter为null时抛出</exception>
        /// <remarks>
        /// 此方法允许通过转换器来自定义元素如何转换为字符串。
        /// 性能优化：
        /// - 使用对象池减少内存分配
        /// - 针对集合类型优化
        /// </remarks>
        public static string JoinToString<T>(IEnumerable<T> collection, string delimiter, Func<T, string> converter)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            delimiter = delimiter ?? string.Empty;
            
            // 快速路径：空集合
            if (collection is ICollection<T> c && c.Count == 0)
                return string.Empty;
                
            // 快速路径：单元素集合不需要分隔符
            if (collection is IList<T> list && list.Count == 1)
                return converter(list[0]) ?? string.Empty;
            
            // 使用StringBuilder对象池来减少GC
            var sb = StringBuilderPool.Get();
            try
            {
                // 预分配合适的容量
                if (collection is ICollection<T> sizedCollection)
                {
                    int count = sizedCollection.Count;
                    int estimatedLength = count * (20 + delimiter.Length);
                    sb.EnsureCapacity(Math.Min(estimatedLength, 16384));
                }
                
                bool isFirst = true;
                foreach (var item in collection)
                {
                    if (!isFirst)
                        sb.Append(delimiter);
                    else
                        isFirst = false;
                        
                    string value = converter(item);
                    if (value != null)
                        sb.Append(value);
                }
                
                return sb.ToString();
            }
            finally
            {
                StringBuilderPool.Release(sb);
            }
        }
        
        /// <summary>
        /// 创建集合的深度副本
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <returns>集合的深度副本</returns>
        /// <exception cref="ArgumentNullException">source为null时抛出</exception>
        /// <remarks>
        /// 此方法创建集合的深度副本，包括值类型和引用类型。
        /// 针对不同类型采用不同的复制策略，提高性能。
        /// 性能优化：
        /// - 预检测类型特征，为常见类型提供快速路径
        /// - 避免不必要的对象创建和类型判断
        /// - 对引用类型使用合适的克隆策略
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> DeepCopy<T>(IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            
            // 快速路径：检查集合是否为空
            if (source is ICollection<T> c && c.Count == 0)
                return new List<T>(0);
            
            // 快速路径：基本类型和字符串可以直接复制
            bool isSimpleType = typeof(T).IsPrimitive || typeof(T) == typeof(string) || typeof(T).IsEnum;
            
            // 针对简单类型的快速路径
            if (isSimpleType)
            {
                // 如果已知集合大小，直接预分配容量
                if (source is ICollection<T> collection)
                    return new List<T>(collection);
                    
                return new List<T>(source);
            }
            
            // 对于IList类型，使用索引访问提高性能
            if (source is IList<T> list)
            {
                int count = list.Count;
                var result = new List<T>(count);
                
                // 快速路径：如果是简单类型，直接复制
                if (isSimpleType)
                {
                    for (int i = 0; i < count; i++)
                        result.Add(list[i]);
                        
                    return result;
                }
                
                // 对于复杂类型，需要对每个元素进行深复制
                for (int i = 0; i < count; i++)
                {
                    T item = list[i];
                    result.Add(item == null ? default : CloneItem(item));
                }
                
                return result;
            }
            
            // 对于其他集合类型
            var resultList = new List<T>();
            
            // 快速路径：如果是简单类型，直接复制
            if (isSimpleType)
            {
                foreach (var item in source)
                    resultList.Add(item);
                    
                return resultList;
            }
            
            // 复杂引用类型需要深复制
            foreach (var item in source)
            {
                resultList.Add(item == null ? default : CloneItem(item));
            }
            
            return resultList;
        }
        
        /// <summary>
        /// 复制单个元素，根据类型选择不同策略
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T CloneItem<T>(T item)
        {
            // null值直接返回默认值
            if (item == null)
                return default;
                
            // 值类型或字符串可以直接返回
            if (typeof(T).IsPrimitive || typeof(T) == typeof(string) || typeof(T).IsEnum)
                return item;
                
            // 如果类型实现了ICloneable接口
            if (item is ICloneable cloneable)
                return (T)cloneable.Clone();
                
            // 如果是数组，创建新数组并复制元素
            if (typeof(T).IsArray)
            {
                var array = item as Array;
                var elementType = typeof(T).GetElementType();
                var clone = Array.CreateInstance(elementType, array.Length);
                array.CopyTo(clone, 0);
                return (T)(object)clone;
            }
            
            try
            {
                // 使用反射调用MemberwiseClone
                var memberwiseClone = typeof(object).GetMethod("MemberwiseClone", 
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                return (T)memberwiseClone.Invoke(item, null);
            }
            catch
            {
                Debug.LogWarning($"无法深拷贝类型 {typeof(T).Name} 的对象。将返回原始引用。");
                return item; // 无法深拷贝时返回原始引用
            }
        }
        
        #endregion
    }

    /// <summary>
    /// StringBuilder对象池，用于减少StringBuilder的分配和回收
    /// </summary>
    /// <remarks>
    /// 优化策略：
    /// - 使用缓存池避免频繁创建StringBuilder
    /// - 自动清理StringBuilder避免内存泄漏
    /// - 限制池大小以平衡内存使用和性能
    /// </remarks>
    public static class StringBuilderPool
    {
        // 线程静态池，避免线程同步开销
        [ThreadStatic]
        private static StringBuilder _cachedInstance;
        
        // 对象池大小上限
        private const int MaxBuilderSize = 1024 * 16; // 16KB
        
        /// <summary>
        /// 从池中获取StringBuilder实例
        /// </summary>
        /// <returns>清空的StringBuilder实例</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Get()
        {
            StringBuilder sb = _cachedInstance;
            
            // 如果没有缓存实例或已被其他地方使用，创建新实例
            if (sb == null)
            {
                sb = new StringBuilder(256); // 初始容量优化
            }
            else
            {
                // 重置线程静态字段防止重复使用
                _cachedInstance = null;
                
                // 清空缓存的实例以便重用
                sb.Clear();
            }
            
            return sb;
        }
        
        /// <summary>
        /// 将StringBuilder实例归还到池中
        /// </summary>
        /// <param name="sb">要归还的StringBuilder实例</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Release(StringBuilder sb)
        {
            // 安全检查
            if (sb == null) return;
            
            // 仅缓存容量合理的实例，避免占用过多内存
            if (sb.Capacity <= MaxBuilderSize)
            {
                // 清空内容以便下次使用
                sb.Clear();
                
                // 缓存实例
                _cachedInstance = sb;
            }
            // 对于超大容量的StringBuilder，让GC自然回收
        }
    }
}