using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TByd.Core.Utils.Tests.Editor.Framework;

namespace TByd.Core.Utils.Tests.Editor.Unit
{
    /// <summary>
    /// CollectionUtils类的单元测试
    /// </summary>
    [TestFixture]
    public class CollectionUtilsTests : TestBase
    {
        #region IsNullOrEmpty Tests

        /// <summary>
        /// 测试IsNullOrEmpty方法，null集合情况
        /// </summary>
        [Test]
        public void IsNullOrEmpty_WithNullCollection_ReturnsTrue()
        {
            // 准备
            List<string> collection = null;
            
            // 执行
            bool result = CollectionUtils.IsNullOrEmpty(collection);
            
            // 验证
            Assert.IsTrue(result);
        }

        /// <summary>
        /// 测试IsNullOrEmpty方法，空集合情况
        /// </summary>
        [Test]
        public void IsNullOrEmpty_WithEmptyCollection_ReturnsTrue()
        {
            // 准备
            var collection = new List<string>();
            
            // 执行
            bool result = CollectionUtils.IsNullOrEmpty(collection);
            
            // 验证
            Assert.IsTrue(result);
        }

        /// <summary>
        /// 测试IsNullOrEmpty方法，非空集合情况
        /// </summary>
        [Test]
        public void IsNullOrEmpty_WithNonEmptyCollection_ReturnsFalse()
        {
            // 准备
            var collection = new List<string> { "Item1" };
            
            // 执行
            bool result = CollectionUtils.IsNullOrEmpty(collection);
            
            // 验证
            Assert.IsFalse(result);
        }

        /// <summary>
        /// 测试IsNullOrEmpty方法，数组版本
        /// </summary>
        [Test]
        public void IsNullOrEmpty_WithEmptyArray_ReturnsTrue()
        {
            // 准备
            string[] array = new string[0];
            
            // 执行
            bool result = CollectionUtils.IsNullOrEmpty(array);
            
            // 验证
            Assert.IsTrue(result);
        }

        /// <summary>
        /// 测试IsNullOrEmpty方法，空字典版本
        /// </summary>
        [Test]
        public void IsNullOrEmpty_WithEmptyDictionary_ReturnsTrue()
        {
            // 准备
            var dictionary = new Dictionary<string, string>();
            
            // 执行
            bool result = CollectionUtils.IsNullOrEmpty(dictionary);
            
            // 验证
            Assert.IsTrue(result);
        }

        #endregion

        #region ForEach Tests

        /// <summary>
        /// 测试ForEach方法，正常集合情况
        /// </summary>
        [Test]
        public void ForEach_WithCollection_ExecutesActionForEachItem()
        {
            // 准备
            var collection = new List<int> { 1, 2, 3, 4, 5 };
            var sum = 0;
            
            // 执行
            CollectionUtils.ForEach(collection, item => sum += item);
            
            // 验证
            Assert.AreEqual(15, sum, "应该对每个元素执行操作");
        }

        /// <summary>
        /// 测试ForEach方法，带索引版本
        /// </summary>
        [Test]
        public void ForEach_WithIndex_ExecutesActionWithIndex()
        {
            // 准备
            var collection = new List<string> { "A", "B", "C" };
            var result = new Dictionary<int, string>();
            
            // 执行
            CollectionUtils.ForEach(collection, (item, index) => result[index] = item);
            
            // 验证
            Assert.AreEqual(3, result.Count, "应该有3个元素");
            Assert.AreEqual("A", result[0], "索引0应该是A");
            Assert.AreEqual("B", result[1], "索引1应该是B");
            Assert.AreEqual("C", result[2], "索引2应该是C");
        }

        /// <summary>
        /// 测试ForEach方法，空集合情况
        /// </summary>
        [Test]
        public void ForEach_WithEmptyCollection_DoesNotExecuteAction()
        {
            // 准备
            var collection = new List<int>();
            var wasCalled = false;
            
            // 执行
            CollectionUtils.ForEach(collection, item => wasCalled = true);
            
            // 验证
            Assert.IsFalse(wasCalled, "空集合不应该执行操作");
        }

        /// <summary>
        /// 测试ForEach方法，null集合情况
        /// </summary>
        [Test]
        public void ForEach_WithNullCollection_ThrowsArgumentNullException()
        {
            // 准备
            IEnumerable<int> collection = null;
            
            // 执行和验证
            Assert.Throws<ArgumentNullException>(() => 
                CollectionUtils.ForEach(collection, item => { }),
                "传入null集合应该抛出ArgumentNullException");
        }

        /// <summary>
        /// 测试ForEach方法，null操作情况
        /// </summary>
        [Test]
        public void ForEach_WithNullAction_ThrowsArgumentNullException()
        {
            // 准备
            var collection = new List<int> { 1, 2, 3 };
            Action<int> action = null;
            
            // 执行和验证
            Assert.Throws<ArgumentNullException>(() => 
                CollectionUtils.ForEach(collection, action),
                "传入null操作应该抛出ArgumentNullException");
        }

        #endregion

        #region Shuffle Tests

        /// <summary>
        /// 测试Shuffle方法，正常集合情况
        /// </summary>
        [Test]
        public void Shuffle_WithCollection_ChangesOrder()
        {
            // 准备 - 创建一个大集合来增加排序变化的概率
            var original = Enumerable.Range(1, 100).ToList();
            var shuffled = new List<int>(original);
            
            // 执行
            CollectionUtils.Shuffle(shuffled);
            
            // 验证 - 元素数量应该保持不变
            Assert.AreEqual(original.Count, shuffled.Count, "洗牌后元素数量应该保持不变");
            
            // 验证 - 至少有一个元素的位置发生了变化
            // 注意：这个测试有极小概率会失败，因为理论上洗牌可能不会改变任何元素的位置
            bool anyDifferent = false;
            for (int i = 0; i < original.Count; i++)
            {
                if (original[i] != shuffled[i])
                {
                    anyDifferent = true;
                    break;
                }
            }
            
            Assert.IsTrue(anyDifferent, "洗牌后至少有一个元素的位置应该发生变化");
            
            // 验证 - 元素集合保持不变（只是顺序变了）
            CollectionAssert.AreEquivalent(original, shuffled, "洗牌后元素集合应该保持不变");
        }

        /// <summary>
        /// 测试Shuffle方法，空集合情况
        /// </summary>
        [Test]
        public void Shuffle_WithEmptyCollection_DoesNotThrowException()
        {
            // 准备
            var collection = new List<int>();
            
            // 执行和验证
            Assert.DoesNotThrow(() => CollectionUtils.Shuffle(collection),
                "空集合不应该抛出异常");
        }

        /// <summary>
        /// 测试Shuffle方法，单元素集合情况
        /// </summary>
        [Test]
        public void Shuffle_WithSingleElementCollection_DoesNotChangeOrder()
        {
            // 准备
            var collection = new List<int> { 42 };
            
            // 执行
            CollectionUtils.Shuffle(collection);
            
            // 验证
            Assert.AreEqual(1, collection.Count, "元素数量应该保持不变");
            Assert.AreEqual(42, collection[0], "单元素集合的元素值应该保持不变");
        }

        /// <summary>
        /// 测试Shuffle方法，null集合情况
        /// </summary>
        [Test]
        public void Shuffle_WithNullCollection_ThrowsArgumentNullException()
        {
            // 准备
            List<int> collection = null;
            
            // 执行和验证
            Assert.Throws<ArgumentNullException>(() => CollectionUtils.Shuffle(collection),
                "传入null集合应该抛出ArgumentNullException");
        }

        #endregion

        #region GetRandomElement Tests

        /// <summary>
        /// 测试GetRandomElement方法，正常集合情况
        /// </summary>
        [Test]
        public void GetRandomElement_WithCollection_ReturnsItemFromCollection()
        {
            // 准备
            var collection = new List<int> { 1, 2, 3, 4, 5 };
            
            // 执行
            int result = CollectionUtils.GetRandomElement(collection);
            
            // 验证 - 结果应该在集合中
            CollectionAssert.Contains(collection, result, "返回的元素应该在集合中");
        }

        /// <summary>
        /// 测试GetRandomElement方法，空集合情况
        /// </summary>
        [Test]
        public void GetRandomElement_WithEmptyCollection_ThrowsInvalidOperationException()
        {
            // 准备
            var collection = new List<int>();
            
            // 执行和验证
            // 允许抛出ArgumentException或InvalidOperationException
            try
            {
                CollectionUtils.GetRandomElement(collection);
                
                // 如果没抛出异常则失败
                Assert.Fail("空集合应该抛出异常");
            }
            catch (ArgumentException ex)
            {
                // 确保异常信息包含"集合不能为空"
                StringAssert.Contains("集合不能为空", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                // 确保异常信息包含"集合不能为空"
                StringAssert.Contains("集合不能为空", ex.Message);
            }
        }

        /// <summary>
        /// 测试GetRandomElement方法，null集合情况
        /// </summary>
        [Test]
        public void GetRandomElement_WithNullCollection_ThrowsArgumentNullException()
        {
            // 准备
            List<int> collection = null;
            
            // 执行和验证
            Assert.Throws<ArgumentNullException>(() => CollectionUtils.GetRandomElement(collection),
                "传入null集合应该抛出ArgumentNullException");
        }

        /// <summary>
        /// 测试GetRandomElement方法，单元素集合情况
        /// </summary>
        [Test]
        public void GetRandomElement_WithSingleElementCollection_ReturnsThatElement()
        {
            // 准备
            var collection = new List<string> { "唯一元素" };
            
            // 执行
            string result = CollectionUtils.GetRandomElement(collection);
            
            // 验证
            Assert.AreEqual("唯一元素", result, "单元素集合应该返回唯一的元素");
        }

        /// <summary>
        /// 测试GetRandomElement方法的随机性
        /// </summary>
        [Test]
        public void GetRandomElement_WithMultipleCalls_ReturnsVariousElements()
        {
            // 准备
            var collection = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var results = new HashSet<int>();
            
            // 执行 - 多次调用GetRandomElement
            for (int i = 0; i < 100; i++)
            {
                results.Add(CollectionUtils.GetRandomElement(collection));
                
                // 如果已经获取了至少3个不同的元素，则通过测试
                if (results.Count >= 3)
                    break;
            }
            
            // 验证 - 应该至少有3个不同的结果（表明有一定的随机性）
            Assert.GreaterOrEqual(results.Count, 3, "多次调用应该返回不同的元素，表明有随机性");
        }

        #endregion

        #region Join Tests

        /// <summary>
        /// 测试Join方法，正常集合情况
        /// </summary>
        [Test]
        public void Join_WithValidCollections_CombinesLists()
        {
            // 准备
            var target = new List<int> { 1, 2, 3 };
            var source = new List<int> { 4, 5, 6 };
            var expected = new List<int> { 1, 2, 3, 4, 5, 6 };
            
            // 执行
            var result = CollectionUtils.Join(target, source);
            
            // 验证
            CollectionAssert.AreEqual(expected, result, "所有源元素应该添加到结果集合");
        }

        /// <summary>
        /// 测试Join方法，源集合为空情况
        /// </summary>
        [Test]
        public void Join_WithEmptySource_ReturnsTargetCopy()
        {
            // 准备
            var target = new List<string> { "项目1", "项目2" };
            var source = new List<string>();
            var expected = new List<string> { "项目1", "项目2" };
            
            // 执行
            var result = CollectionUtils.Join(target, source);
            
            // 验证
            CollectionAssert.AreEqual(expected, result, "空源集合应该返回目标集合的副本");
            Assert.AreNotSame(target, result, "应返回新的集合实例，而不是原始集合");
        }

        /// <summary>
        /// 测试Join方法，目标集合为空情况
        /// </summary>
        [Test]
        public void Join_WithEmptyTarget_ReturnsSourceCopy()
        {
            // 准备
            var target = new List<int>();
            var source = new List<int> { 1, 2, 3 };
            
            // 执行
            var result = CollectionUtils.Join(target, source);
            
            // 验证
            CollectionAssert.AreEqual(source, result, "所有源元素应该在结果集合中");
            Assert.AreNotSame(source, result, "应返回新的集合实例，而不是原始集合");
        }

        /// <summary>
        /// 测试Join方法，源集合为null情况
        /// </summary>
        [Test]
        public void Join_WithNullSource_ThrowsArgumentNullException()
        {
            // 准备
            var target = new List<int> { 1, 2, 3 };
            List<int> source = null;
            
            // 执行和验证
            Assert.Throws<ArgumentNullException>(() => CollectionUtils.Join(target, source),
                "源集合为null应该抛出ArgumentNullException");
        }

        /// <summary>
        /// 测试Join方法，目标集合为null情况
        /// </summary>
        [Test]
        public void Join_WithNullTarget_ThrowsArgumentNullException()
        {
            // 准备
            List<int> target = null;
            var source = new List<int> { 1, 2, 3 };
            
            // 执行和验证
            Assert.Throws<ArgumentNullException>(() => CollectionUtils.Join(target, source),
                "目标集合为null应该抛出ArgumentNullException");
        }

        /// <summary>
        /// 测试Join方法，数组作为源集合
        /// </summary>
        [Test]
        public void Join_WithArraySource_CombinesItemsToNewList()
        {
            // 准备
            var target = new List<int> { 1, 2, 3 };
            int[] source = { 4, 5, 6 };
            var expected = new List<int> { 1, 2, 3, 4, 5, 6 };
            
            // 执行
            var result = CollectionUtils.Join(target, source);
            
            // 验证
            CollectionAssert.AreEqual(expected, result, "数组元素应该和目标集合合并到新集合");
        }

        #endregion

        #region DeepCopy Tests

        /// <summary>
        /// 测试DeepCopy方法，基本类型情况
        /// </summary>
        [Test]
        public void DeepCopy_WithPrimitiveTypes_CreatesNewCollectionWithSameValues()
        {
            // 准备
            var original = new List<int> { 1, 2, 3, 4, 5 };
            
            // 执行
            var copied = CollectionUtils.DeepCopy(original);
            
            // 验证 - 值相同
            CollectionAssert.AreEqual(original, copied, "复制后的值应该相同");
            
            // 验证 - 对象不同
            Assert.AreNotSame(original, copied, "复制后应该是不同的对象");
        }

        /// <summary>
        /// 测试DeepCopy方法，引用类型情况
        /// </summary>
        [Test]
        public void DeepCopy_WithReferenceTypes_CreatesCopy()
        {
            // 准备
            var original = new List<TestClass>
            {
                new TestClass { Id = 1, Name = "Test1" },
                new TestClass { Id = 2, Name = "Test2" }
            };
            
            // 执行
            var copied = CollectionUtils.DeepCopy(original);
            
            // 验证 - 集合不同
            Assert.AreNotSame(original, copied, "复制后应该是不同的集合");
            
            // 验证 - 元素值相同但是引用不同
            Assert.AreEqual(original[0].Id, copied[0].Id);
            Assert.AreEqual(original[0].Name, copied[0].Name);
            Assert.AreEqual(original[1].Id, copied[1].Id);
            Assert.AreEqual(original[1].Name, copied[1].Name);
            
            // 修改原始对象不应影响复制对象
            original[0].Name = "Modified";
            Assert.AreNotEqual(original[0].Name, copied[0].Name);
        }

        /// <summary>
        /// 测试DeepCopy方法，空集合情况
        /// </summary>
        [Test]
        public void DeepCopy_WithEmptyCollection_ReturnsEmptyCollection()
        {
            // 准备
            var original = new List<string>();
            
            // 执行
            var copied = CollectionUtils.DeepCopy(original);
            
            // 验证
            Assert.IsNotNull(copied, "返回的集合不应该为null");
            Assert.AreEqual(0, copied.Count, "复制的空集合应该还是空的");
            Assert.AreNotSame(original, copied, "复制后应该是不同的对象");
        }

        /// <summary>
        /// 测试DeepCopy方法，null集合情况
        /// </summary>
        [Test]
        public void DeepCopy_WithNullCollection_ThrowsArgumentNullException()
        {
            // 准备
            List<int> original = null;
            
            // 执行和验证
            Assert.Throws<ArgumentNullException>(() => CollectionUtils.DeepCopy(original),
                "null集合应该抛出ArgumentNullException");
        }

        #endregion

        #region Collection Utility Tests

        /// <summary>
        /// 测试Batch方法，处理项目批次
        /// </summary>
        [Test]
        public void Batch_ProcessesItemsInBatches()
        {
            // 准备
            var items = Enumerable.Range(1, 10).ToList();
            var results = new List<int[]>();
            
            // 执行
            foreach (var batch in CollectionUtils.Batch(items, 3))
            {
                results.Add(batch.ToArray());
            }
            
            // 验证
            Assert.AreEqual(4, results.Count); // 3 + 3 + 3 + 1
            Assert.AreEqual(3, results[0].Length);
            Assert.AreEqual(3, results[1].Length);
            Assert.AreEqual(3, results[2].Length);
            Assert.AreEqual(1, results[3].Length);
        }

        /// <summary>
        /// 测试ForEach方法，处理所有项目
        /// </summary>
        [Test]
        public void ForEach_ProcessesAllItems()
        {
            // 准备
            var items = new[] { 1, 2, 3, 4, 5 };
            var sum = 0;
            
            // 执行
            CollectionUtils.ForEach(items, item => sum += item);
            
            // 验证
            Assert.AreEqual(15, sum);
        }

        /// <summary>
        /// 测试Join方法，组合集合
        /// </summary>
        [Test]
        public void Join_CombinesCollections()
        {
            // 准备
            var list1 = new List<int> { 1, 2, 3 };
            var list2 = new List<int> { 4, 5, 6 };
            
            // 执行
            var joined = CollectionUtils.Join(list1, list2);
            
            // 验证
            Assert.AreEqual(6, joined.Count);
            for (int i = 0; i < 6; i++)
            {
                Assert.AreEqual(i + 1, joined[i]);
            }
        }

        /// <summary>
        /// 测试GetRandomElement方法，返回集合中的元素
        /// </summary>
        [Test]
        public void GetRandomElement_ReturnsElementFromCollection()
        {
            // 准备
            var list = new List<int> { 1, 2, 3, 4, 5 };
            
            // 执行
            int result = CollectionUtils.GetRandomElement(list);
            
            // 验证
            Assert.IsTrue(list.Contains(result));
        }

        #endregion

        #region String Collection Tests

        /// <summary>
        /// 测试JoinToString方法，连接集合元素
        /// </summary>
        [Test]
        public void JoinToString_JoinsCollectionElementsWithDelimiter()
        {
            // 准备
            var collection = new[] { "One", "Two", "Three" };
            
            // 执行
            string result = CollectionUtils.JoinToString(collection, ", ");
            
            // 验证
            Assert.AreEqual("One, Two, Three", result);
        }

        /// <summary>
        /// 测试JoinToString方法，空集合情况
        /// </summary>
        [Test]
        public void JoinToString_WithEmptyCollection_ReturnsEmptyString()
        {
            // 准备
            var collection = new string[0];
            
            // 执行
            string result = CollectionUtils.JoinToString(collection, ", ");
            
            // 验证
            Assert.AreEqual("", result);
        }

        #endregion

        #region Collection Transformation Tests

        /// <summary>
        /// 测试DeepCopy方法，创建独立副本
        /// </summary>
        [Test]
        public void DeepCopy_CreatesIndependentCopy()
        {
            // 准备
            var original = new List<TestClass>
            {
                new TestClass { Id = 1, Name = "Test1" },
                new TestClass { Id = 2, Name = "Test2" }
            };
            
            // 执行
            var copy = CollectionUtils.DeepCopy(original);
            
            // 修改原始集合中的对象
            original[0].Name = "Modified";
            
            // 验证 - 副本中的对象不应受影响
            Assert.AreEqual("Test1", copy[0].Name);
            Assert.AreEqual(1, copy[0].Id);
            Assert.AreEqual("Test2", copy[1].Name);
            Assert.AreEqual(2, copy[1].Id);
        }

        /// <summary>
        /// 测试TestClass类，序列化
        /// </summary>
        [Serializable]
        private class TestClass
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        #endregion
    }
} 