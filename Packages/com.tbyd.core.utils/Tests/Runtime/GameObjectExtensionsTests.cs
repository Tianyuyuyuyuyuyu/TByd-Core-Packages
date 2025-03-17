using NUnit.Framework;
using TByd.Core.Utils.Runtime.Extensions;
using UnityEngine;

namespace TByd.Core.Utils.Tests.Runtime
{
    public class GameObjectExtensionsTests
    {
        private GameObject _testObject;
        
        [SetUp]
        public void SetUp()
        {
            _testObject = new GameObject("TestObject");
        }
        
        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_testObject);
        }
        
        [Test]
        public void FindOrCreateChild_WhenChildDoesNotExist_CreatesNewChild()
        {
            // 查找不存在的子物体
            string childName = "TestChild";
            GameObject child = _testObject.FindOrCreateChild(childName);
            
            // 验证子物体被创建
            Assert.That(child, Is.Not.Null);
            Assert.That(child.name, Is.EqualTo(childName));
            Assert.That(child.transform.parent, Is.EqualTo(_testObject.transform));
            
            // 验证子物体的变换属性
            Assert.That(child.transform.localPosition, Is.EqualTo(Vector3.zero));
            Assert.That(child.transform.localRotation, Is.EqualTo(Quaternion.identity));
            Assert.That(child.transform.localScale, Is.EqualTo(Vector3.one));
        }
        
        [Test]
        public void FindOrCreateChild_WhenChildExists_ReturnsExistingChild()
        {
            // 先创建子物体
            string childName = "TestChild";
            GameObject originalChild = new GameObject(childName);
            originalChild.transform.SetParent(_testObject.transform);
            
            // 设置一个特殊位置以便识别
            originalChild.transform.localPosition = new Vector3(1, 2, 3);
            
            // 查找已存在的子物体
            GameObject foundChild = _testObject.FindOrCreateChild(childName);
            
            // 验证返回的是已存在的子物体
            Assert.That(foundChild, Is.Not.Null);
            Assert.That(foundChild, Is.EqualTo(originalChild));
            Assert.That(foundChild.transform.localPosition, Is.EqualTo(new Vector3(1, 2, 3)));
        }
        
        [Test]
        public void FindOrCreateChild_WithEmptyName_ReturnsParent()
        {
            // 使用空名称查找子物体
            GameObject result = _testObject.FindOrCreateChild("");
            
            // 应该返回父物体自身
            Assert.That(result, Is.EqualTo(_testObject));
        }
        
        [Test]
        public void SetLayerRecursively_SetsLayerForAllChildren()
        {
            // 创建一个层级结构
            GameObject child1 = new GameObject("Child1");
            child1.transform.SetParent(_testObject.transform);
            
            GameObject child2 = new GameObject("Child2");
            child2.transform.SetParent(child1.transform);
            
            GameObject child3 = new GameObject("Child3");
            child3.transform.SetParent(_testObject.transform);
            
            // 设置一个不同的层
            int targetLayer = 8; // UI层
            _testObject.SetLayerRecursively(targetLayer);
            
            // 验证所有物体的层都被设置
            Assert.That(_testObject.layer, Is.EqualTo(targetLayer));
            Assert.That(child1.layer, Is.EqualTo(targetLayer));
            Assert.That(child2.layer, Is.EqualTo(targetLayer));
            Assert.That(child3.layer, Is.EqualTo(targetLayer));
        }
        
        [Test]
        public void GetOrAddComponent_WhenComponentDoesNotExist_AddsComponent()
        {
            // 获取不存在的组件
            Rigidbody rb = _testObject.GetOrAddComponent<Rigidbody>();
            
            // 验证组件被添加
            Assert.That(rb, Is.Not.Null);
            Assert.That(_testObject.GetComponent<Rigidbody>(), Is.EqualTo(rb));
        }
        
        [Test]
        public void GetOrAddComponent_WhenComponentExists_ReturnsExistingComponent()
        {
            // 先添加组件
            BoxCollider originalCollider = _testObject.AddComponent<BoxCollider>();
            
            // 获取已存在的组件
            BoxCollider foundCollider = _testObject.GetOrAddComponent<BoxCollider>();
            
            // 验证返回的是已存在的组件
            Assert.That(foundCollider, Is.Not.Null);
            Assert.That(foundCollider, Is.EqualTo(originalCollider));
        }
        
        [Test]
        public void SetActive_SetsActiveStateAndReturnsGameObject()
        {
            // 设置激活状态
            GameObject result = GameObjectExtensions.SetActive(_testObject, false);
            
            // 验证激活状态被设置
            Assert.That(_testObject.activeSelf, Is.False);
            
            // 验证返回的是同一个物体
            Assert.That(result, Is.EqualTo(_testObject));
            
            // 再次设置激活状态
            result = GameObjectExtensions.SetActive(_testObject, true);
            
            // 验证激活状态被设置
            Assert.That(_testObject.activeSelf, Is.True);
            
            // 验证返回的是同一个物体
            Assert.That(result, Is.EqualTo(_testObject));
        }
        
        [Test]
        public void SetActive_WhenStateIsAlreadySet_DoesNotChangeState()
        {
            // 确保物体是激活的
            _testObject.SetActive(true);
            
            // 再次设置为激活状态
            GameObject result = GameObjectExtensions.SetActive(_testObject, true);
            
            // 验证激活状态没有变化
            Assert.That(_testObject.activeSelf, Is.True);
            
            // 验证返回的是同一个物体
            Assert.That(result, Is.EqualTo(_testObject));
        }
        
        [Test]
        public void DestroyAllChildren_DestroysAllChildren()
        {
            // 创建多个子物体
            for (int i = 0; i < 5; i++)
            {
                GameObject child = new GameObject($"Child{i}");
                child.transform.SetParent(_testObject.transform);
            }
            
            // 验证初始子物体数量
            Assert.That(_testObject.transform.childCount, Is.EqualTo(5));
            
            // 销毁所有子物体
            GameObject result = GameObjectExtensions.DestroyAllChildren(_testObject, true);
            
            // 验证所有子物体都被销毁
            Assert.That(_testObject.transform.childCount, Is.EqualTo(0));
            
            // 验证返回的是同一个物体
            Assert.That(result, Is.EqualTo(_testObject));
        }
        
        [Test]
        public void DestroyAllChildren_WithNoChildren_DoesNothing()
        {
            // 确保没有子物体
            Assert.That(_testObject.transform.childCount, Is.EqualTo(0));
            
            // 尝试销毁子物体
            GameObject result = GameObjectExtensions.DestroyAllChildren(_testObject, true);
            
            // 验证没有变化
            Assert.That(_testObject.transform.childCount, Is.EqualTo(0));
            
            // 验证返回的是同一个物体
            Assert.That(result, Is.EqualTo(_testObject));
        }
        
        [Test]
        public void SetLayerRecursively_WithNullGameObject_ReturnsNull()
        {
            // 尝试对null设置层级
            GameObject result = GameObjectExtensions.SetLayerRecursively(null, 8);
            
            // 验证返回null
            Assert.That(result, Is.Null);
        }
        
        [Test]
        public void FindOrCreateChild_WithNullParent_ReturnsNull()
        {
            // 尝试在null上查找子物体
            GameObject result = GameObjectExtensions.FindOrCreateChild(null, "Child");
            
            // 验证返回null
            Assert.That(result, Is.Null);
        }
    }
} 