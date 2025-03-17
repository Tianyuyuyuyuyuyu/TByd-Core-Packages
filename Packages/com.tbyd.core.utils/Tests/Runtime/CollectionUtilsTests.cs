using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TByd.Core.Utils;
using UnityEngine;
using UnityEngine.TestTools;

namespace TByd.Core.Utils.Tests
{
    public class CollectionUtilsTests
    {
        #region 批量处理测试

        [Test]
        public void BatchProcess_ProcessesItemsInBatches()
        {
            // 准备测试数据
            var source = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var batchSize = 3;
            var processedBatches = new List<List<int>>();

            // 执行测试
            CollectionUtils.BatchProcess(source, batchSize, batch =>
            {
                processedBatches.Add(new List<int>(batch));
            });

            // 验证结果
            Assert.AreEqual(4, processedBatches.Count);
            Assert.AreEqual(3, processedBatches[0].Count);
            Assert.AreEqual(3, processedBatches[1].Count);
            Assert.AreEqual(3, processedBatches[2].Count);
            Assert.AreEqual(1, processedBatches[3].Count);

            Assert.AreEqual(1, processedBatches[0][0]);
            Assert.AreEqual(2, processedBatches[0][1]);
            Assert.AreEqual(3, processedBatches[0][2]);

            Assert.AreEqual(4, processedBatches[1][0]);
            Assert.AreEqual(5, processedBatches[1][1]);
            Assert.AreEqual(6, processedBatches[1][2]);

            Assert.AreEqual(7, processedBatches[2][0]);
            Assert.AreEqual(8, processedBatches[2][1]);
            Assert.AreEqual(9, processedBatches[2][2]);

            Assert.AreEqual(10, processedBatches[3][0]);
        }

        [Test]
        public void BatchProcess_ThrowsException_WhenSourceIsNull()
        {
            // 验证异常
            Assert.Throws<ArgumentNullException>(() =>
            {
                CollectionUtils.BatchProcess<int>(null, 3, batch => { });
            });
        }

        [Test]
        public void BatchProcess_ThrowsException_WhenActionIsNull()
        {
            // 准备测试数据
            var source = new List<int> { 1, 2, 3 };

            // 验证异常
            Assert.Throws<ArgumentNullException>(() =>
            {
                CollectionUtils.BatchProcess(source, 3, null);
            });
        }

        [Test]
        public void BatchProcess_ThrowsException_WhenBatchSizeIsLessThanOne()
        {
            // 准备测试数据
            var source = new List<int> { 1, 2, 3 };

            // 验证异常
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                CollectionUtils.BatchProcess(source, 0, batch => { });
            });
        }

        [Test]
        public async void BatchProcessAsync_ProcessesItemsInBatches()
        {
            // 准备测试数据
            var source = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var batchSize = 3;
            var processedBatches = new List<List<int>>();

            // 执行测试
            await CollectionUtils.BatchProcessAsync(source, batchSize, batch =>
            {
                processedBatches.Add(new List<int>(batch));
                return System.Threading.Tasks.Task.CompletedTask;
            });

            // 验证结果
            Assert.AreEqual(4, processedBatches.Count);
            Assert.AreEqual(3, processedBatches[0].Count);
            Assert.AreEqual(3, processedBatches[1].Count);
            Assert.AreEqual(3, processedBatches[2].Count);
            Assert.AreEqual(1, processedBatches[3].Count);
        }

        [Test]
        public void ForEach_ProcessesAllItems()
        {
            // 准备测试数据
            var source = new List<int> { 1, 2, 3, 4, 5 };
            var result = new List<int>();

            // 执行测试
            CollectionUtils.ForEach(source, item => result.Add(item * 2));

            // 验证结果
            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(2, result[0]);
            Assert.AreEqual(4, result[1]);
            Assert.AreEqual(6, result[2]);
            Assert.AreEqual(8, result[3]);
            Assert.AreEqual(10, result[4]);
        }

        [Test]
        public void ForEach_WithIndex_ProcessesAllItems()
        {
            // 准备测试数据
            var source = new List<string> { "a", "b", "c" };
            var result = new List<string>();

            // 执行测试
            CollectionUtils.ForEach(source, (item, index) => result.Add($"{index}:{item}"));

            // 验证结果
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("0:a", result[0]);
            Assert.AreEqual("1:b", result[1]);
            Assert.AreEqual("2:c", result[2]);
        }

        #endregion

        #region 集合比较与差异计算测试

        [Test]
        public void Compare_ReturnsTrueForEqualCollections()
        {
            // 准备测试数据
            var first = new List<int> { 1, 2, 3, 4, 5 };
            var second = new List<int> { 5, 4, 3, 2, 1 };

            // 执行测试并验证结果
            Assert.IsTrue(CollectionUtils.Compare(first, second));
        }

        [Test]
        public void Compare_ReturnsFalseForDifferentCollections()
        {
            // 准备测试数据
            var first = new List<int> { 1, 2, 3, 4, 5 };
            var second = new List<int> { 1, 2, 3, 4, 6 };

            // 执行测试并验证结果
            Assert.IsFalse(CollectionUtils.Compare(first, second));
        }

        [Test]
        public void Compare_ReturnsTrueForEqualCollectionsWithDuplicates()
        {
            // 准备测试数据
            var first = new List<int> { 1, 2, 2, 3, 4, 5 };
            var second = new List<int> { 5, 4, 3, 2, 2, 1 };

            // 执行测试并验证结果
            Assert.IsTrue(CollectionUtils.Compare(first, second));
        }

        [Test]
        public void Compare_ReturnsFalseForCollectionsWithDifferentCounts()
        {
            // 准备测试数据
            var first = new List<int> { 1, 2, 3, 4, 5 };
            var second = new List<int> { 1, 2, 3, 4 };

            // 执行测试并验证结果
            Assert.IsFalse(CollectionUtils.Compare(first, second));
        }

        [Test]
        public void FindDifferences_ReturnsCorrectDifferences()
        {
            // 准备测试数据
            var first = new List<int> { 1, 2, 3, 4, 5 };
            var second = new List<int> { 3, 4, 5, 6, 7 };

            // 执行测试
            var result = CollectionUtils.FindDifferences(first, second);

            // 验证结果
            Assert.AreEqual(2, result.OnlyInFirst.Count());
            Assert.AreEqual(2, result.OnlyInSecond.Count());
            Assert.AreEqual(3, result.InBoth.Count());

            Assert.IsTrue(result.OnlyInFirst.Contains(1));
            Assert.IsTrue(result.OnlyInFirst.Contains(2));

            Assert.IsTrue(result.OnlyInSecond.Contains(6));
            Assert.IsTrue(result.OnlyInSecond.Contains(7));

            Assert.IsTrue(result.InBoth.Contains(3));
            Assert.IsTrue(result.InBoth.Contains(4));
            Assert.IsTrue(result.InBoth.Contains(5));
        }

        #endregion

        #region 集合转换与映射测试

        [Test]
        public void Map_ReturnsCorrectlyMappedCollection()
        {
            // 准备测试数据
            var source = new List<int> { 1, 2, 3, 4, 5 };

            // 执行测试
            var result = CollectionUtils.Map(source, x => x * 2).ToList();

            // 验证结果
            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(2, result[0]);
            Assert.AreEqual(4, result[1]);
            Assert.AreEqual(6, result[2]);
            Assert.AreEqual(8, result[3]);
            Assert.AreEqual(10, result[4]);
        }

        [Test]
        public void MapWithIndex_ReturnsCorrectlyMappedCollection()
        {
            // 准备测试数据
            var source = new List<string> { "a", "b", "c" };

            // 执行测试
            var result = CollectionUtils.Map(source, (x, i) => $"{i}:{x}").ToList();

            // 验证结果
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("0:a", result[0]);
            Assert.AreEqual("1:b", result[1]);
            Assert.AreEqual("2:c", result[2]);
        }

        #endregion

        #region 集合过滤测试

        [Test]
        public void Filter_ReturnsCorrectlyFilteredCollection()
        {
            // 准备测试数据
            var source = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            // 执行测试
            var result = CollectionUtils.Filter(source, x => x % 2 == 0).ToList();

            // 验证结果
            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(2, result[0]);
            Assert.AreEqual(4, result[1]);
            Assert.AreEqual(6, result[2]);
            Assert.AreEqual(8, result[3]);
            Assert.AreEqual(10, result[4]);
        }

        [Test]
        public void FilterWithIndex_ReturnsCorrectlyFilteredCollection()
        {
            // 准备测试数据
            var source = new List<string> { "a", "b", "c", "d", "e" };

            // 执行测试
            var result = CollectionUtils.Filter(source, (x, i) => i % 2 == 0).ToList();

            // 验证结果
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("a", result[0]);
            Assert.AreEqual("c", result[1]);
            Assert.AreEqual("e", result[2]);
        }

        #endregion

        #region 集合分页测试

        [Test]
        public void Paginate_ReturnsCorrectPage()
        {
            // 准备测试数据
            var source = Enumerable.Range(1, 100).ToList();

            // 执行测试 - 第2页，每页10条
            var result = CollectionUtils.Paginate(source, 2, 10).ToList();

            // 验证结果
            Assert.AreEqual(10, result.Count);
            Assert.AreEqual(11, result[0]);
            Assert.AreEqual(20, result[9]);
        }

        [Test]
        public void Paginate_ReturnsEmptyCollection_WhenPageIsOutOfRange()
        {
            // 准备测试数据
            var source = Enumerable.Range(1, 10).ToList();

            // 执行测试 - 第3页，每页5条（超出范围）
            var result = CollectionUtils.Paginate(source, 3, 5).ToList();

            // 验证结果
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void Paginate_ReturnsAllItems_WhenPageSizeIsLargerThanCollection()
        {
            // 准备测试数据
            var source = Enumerable.Range(1, 5).ToList();

            // 执行测试 - 第1页，每页10条
            var result = CollectionUtils.Paginate(source, 1, 10).ToList();

            // 验证结果
            Assert.AreEqual(5, result.Count);
        }

        #endregion

        #region 集合排序测试

        [Test]
        public void OrderBy_SortsCollectionCorrectly()
        {
            // 准备测试数据
            var source = new List<string> { "banana", "apple", "cherry", "date" };

            // 执行测试
            var result = CollectionUtils.OrderBy(source, x => x).ToList();

            // 验证结果
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual("apple", result[0]);
            Assert.AreEqual("banana", result[1]);
            Assert.AreEqual("cherry", result[2]);
            Assert.AreEqual("date", result[3]);
        }

        [Test]
        public void OrderByDescending_SortsCollectionCorrectly()
        {
            // 准备测试数据
            var source = new List<int> { 3, 1, 4, 2 };

            // 执行测试
            var result = CollectionUtils.OrderByDescending(source, x => x).ToList();

            // 验证结果
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual(4, result[0]);
            Assert.AreEqual(3, result[1]);
            Assert.AreEqual(2, result[2]);
            Assert.AreEqual(1, result[3]);
        }

        #endregion

        #region 集合分组测试

        [Test]
        public void GroupBy_GroupsCollectionCorrectly()
        {
            // 准备测试数据
            var source = new List<int> { 1, 2, 3, 4, 5, 6 };

            // 执行测试 - 按奇偶分组
            var result = CollectionUtils.GroupBy(source, x => x % 2 == 0 ? "even" : "odd")
                                        .ToDictionary(g => g.Key, g => g.ToList());

            // 验证结果
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.ContainsKey("odd"));
            Assert.IsTrue(result.ContainsKey("even"));
            
            Assert.AreEqual(3, result["odd"].Count);
            Assert.AreEqual(3, result["even"].Count);
            
            CollectionAssert.Contains(result["odd"], 1);
            CollectionAssert.Contains(result["odd"], 3);
            CollectionAssert.Contains(result["odd"], 5);
            
            CollectionAssert.Contains(result["even"], 2);
            CollectionAssert.Contains(result["even"], 4);
            CollectionAssert.Contains(result["even"], 6);
        }

        #endregion

        #region 集合聚合测试

        [Test]
        public void Aggregate_AggregatesCollectionCorrectly()
        {
            // 准备测试数据
            var source = new List<int> { 1, 2, 3, 4, 5 };

            // 执行测试 - 计算总和
            var sum = CollectionUtils.Aggregate(source, 0, (acc, x) => acc + x);

            // 验证结果
            Assert.AreEqual(15, sum);
        }

        [Test]
        public void Aggregate_WithSeed_AggregatesCollectionCorrectly()
        {
            // 准备测试数据
            var source = new List<string> { "a", "b", "c" };

            // 执行测试 - 连接字符串
            var joined = CollectionUtils.Aggregate(source, "start:", (acc, x) => acc + x);

            // 验证结果
            Assert.AreEqual("start:abc", joined);
        }

        #endregion

        #region 集合批处理测试

        [Test]
        public void Batch_CreatesCorrectBatches()
        {
            // 准备测试数据
            var source = Enumerable.Range(1, 10).ToList();

            // 执行测试 - 每批3个
            var batches = CollectionUtils.Batch(source, 3).ToList();

            // 验证结果
            Assert.AreEqual(4, batches.Count);
            
            Assert.AreEqual(3, batches[0].Count());
            Assert.AreEqual(3, batches[1].Count());
            Assert.AreEqual(3, batches[2].Count());
            Assert.AreEqual(1, batches[3].Count());
            
            Assert.AreEqual(1, batches[0].ElementAt(0));
            Assert.AreEqual(2, batches[0].ElementAt(1));
            Assert.AreEqual(3, batches[0].ElementAt(2));
            
            Assert.AreEqual(4, batches[1].ElementAt(0));
            Assert.AreEqual(5, batches[1].ElementAt(1));
            Assert.AreEqual(6, batches[1].ElementAt(2));
            
            Assert.AreEqual(7, batches[2].ElementAt(0));
            Assert.AreEqual(8, batches[2].ElementAt(1));
            Assert.AreEqual(9, batches[2].ElementAt(2));
            
            Assert.AreEqual(10, batches[3].ElementAt(0));
        }

        [Test]
        public void Batch_EmptyCollection_ReturnsEmptyBatches()
        {
            // 准备测试数据
            var source = new List<int>();

            // 执行测试
            var batches = CollectionUtils.Batch(source, 3).ToList();

            // 验证结果
            Assert.AreEqual(0, batches.Count);
        }

        #endregion

        #region 集合洗牌测试

        [Test]
        public void Shuffle_ShufflesCollection()
        {
            // 准备测试数据
            var source = Enumerable.Range(1, 100).ToList();
            var original = new List<int>(source);

            // 执行测试
            CollectionUtils.Shuffle(source);

            // 验证结果 - 内容相同但顺序不同
            Assert.AreEqual(original.Count, source.Count);
            CollectionAssert.AreEquivalent(original, source);
            
            // 很小的概率会洗牌后顺序不变，但概率非常低
            // 如果集合足够大，这个测试几乎总是能通过
            Assert.IsFalse(original.SequenceEqual(source), "洗牌操作没有改变集合顺序");
        }

        [Test]
        public void Shuffle_EmptyCollection_DoesNothing()
        {
            // 准备测试数据
            var source = new List<int>();

            // 执行测试 - 不应抛出异常
            Assert.DoesNotThrow(() => CollectionUtils.Shuffle(source));
        }

        #endregion
    }
} 