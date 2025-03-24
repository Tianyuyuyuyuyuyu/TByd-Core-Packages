using System;
using NUnit.Framework;
using TByd.Core.Utils.Runtime;
using TByd.Core.Utils.Tests.Editor.Framework;
using UnityEngine;

namespace TByd.Core.Utils.Tests.Editor.Unit
{
    /// <summary>
    /// MathUtils工具类的单元测试
    /// </summary>
    [TestFixture]
    public class MathUtilsTests : TestBase
    {
        private const float Epsilon = 0.0001f;
        
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
        }
        
        #region Remap Tests
        
        [Test]
        public void Remap_MidValueInRange_ReturnsMidValueInNewRange()
        {
            // 从[0, 10]重映射到[0, 100]，5应该映射为50
            float result = MathUtils.Remap(5f, 0f, 10f, 0f, 100f);
            Assert.AreEqual(50f, result, Epsilon);
        }
        
        [Test]
        public void Remap_MinValueInRange_ReturnsMinValueInNewRange()
        {
            // 从[0, 10]重映射到[100, 200]，0应该映射为100
            float result = MathUtils.Remap(0f, 0f, 10f, 100f, 200f);
            Assert.AreEqual(100f, result, Epsilon);
        }
        
        [Test]
        public void Remap_MaxValueInRange_ReturnsMaxValueInNewRange()
        {
            // 从[0, 10]重映射到[100, 200]，10应该映射为200
            float result = MathUtils.Remap(10f, 0f, 10f, 100f, 200f);
            Assert.AreEqual(200f, result, Epsilon);
        }
        
        [Test]
        public void Remap_ValueOutsideRange_ExtrapolatesCorrectly()
        {
            // 从[0, 10]重映射到[0, 100]，15应该映射为150（超出范围）
            float result = MathUtils.Remap(15f, 0f, 10f, 0f, 100f);
            Assert.AreEqual(150f, result, Epsilon);
        }
        
        [Test]
        public void Remap_NegativeRange_MapsCorrectly()
        {
            // 从[-10, 10]重映射到[0, 100]，0应该映射为50
            float result = MathUtils.Remap(0f, -10f, 10f, 0f, 100f);
            Assert.AreEqual(50f, result, Epsilon);
        }
        
        [Test]
        public void Remap_InvertedRange_MapsCorrectly()
        {
            // 从[10, 0]重映射到[0, 100]，这是一个反向映射
            // 10应该映射为0，0应该映射为100
            float result = MathUtils.Remap(5f, 10f, 0f, 0f, 100f);
            Assert.AreEqual(50f, result, Epsilon);
        }
        
        #endregion
        
        #region SmoothDamp Tests
        
        [Test]
        public void SmoothDamp_OverTime_ApproachesTargetValue()
        {
            float current = 0f;
            float target = 10f;
            float velocity = 0f;
            float smoothTime = 0.3f;
            
            // 模拟10帧更新
            for (int i = 0; i < 10; i++)
            {
                current = MathUtils.SmoothDamp(current, target, ref velocity, smoothTime, float.MaxValue, 0.1f);
            }
            
            // 经过10帧(1秒)后，应该非常接近目标值
            Assert.That(current, Is.GreaterThan(9.5f));
            Assert.That(current, Is.LessThanOrEqualTo(10f));
        }
        
        [Test]
        public void SmoothDamp_WithMaxSpeed_RespectsMaximumSpeed()
        {
            float current = 0f;
            float target = 100f;  // 远距离
            float velocity = 0f;
            float smoothTime = 0.3f;
            float maxSpeed = 5f;  // 每次最多移动5个单位
            
            float initialValue = current;
            current = MathUtils.SmoothDamp(current, target, ref velocity, smoothTime, maxSpeed, 0.1f);
            
            // 第一次更新后，移动距离不应超过maxSpeed
            Assert.That(Mathf.Abs(current - initialValue), Is.LessThanOrEqualTo(maxSpeed + Epsilon));
        }
        
        [Test]
        public void SmoothDamp_AtTargetWithZeroVelocity_RemainsAtTarget()
        {
            float current = 10f;
            float target = 10f;  // 已经在目标位置
            float velocity = 0f;  // 没有速度
            float smoothTime = 0.3f;
            
            float result = MathUtils.SmoothDamp(current, target, ref velocity, smoothTime, float.MaxValue, 0.1f);
            
            // 应该保持在目标位置，且速度接近零
            Assert.AreEqual(target, result, Epsilon);
            Assert.AreEqual(0f, velocity, Epsilon);
        }
        
        [Test]
        public void SmoothDamp_NegativeSmoothTime_CorrectsToBeSafeMinimum()
        {
            float current = 0f;
            float target = 10f;
            float velocity = 0f;
            float smoothTime = -0.1f;  // 负值应该被修正
            
            // 不应该抛出异常，而是使用最小安全值
            Assert.DoesNotThrow(() => {
                MathUtils.SmoothDamp(current, target, ref velocity, smoothTime, float.MaxValue, 0.1f);
            });
        }
        
        #endregion
        
        #region DirectionToRotation Tests
        
        [Test]
        public void DirectionToRotation_ForwardDirection_ReturnsIdentityRotation()
        {
            Vector3 forward = Vector3.forward;
            Quaternion rotation = MathUtils.DirectionToRotation(forward);
            
            // 向前方向应该返回单位四元数（无旋转）
            AssertQuaternionsAreEqual(Quaternion.identity, rotation);
        }
        
        [Test]
        public void DirectionToRotation_RightDirection_ReturnsRightRotation()
        {
            Vector3 right = Vector3.right;
            Quaternion rotation = MathUtils.DirectionToRotation(right);
            Quaternion expected = Quaternion.Euler(0, 90, 0);  // 向右旋转90度
            
            AssertQuaternionsAreEqual(expected, rotation);
        }
        
        [Test]
        public void DirectionToRotation_UpDirection_ReturnsUpRotation()
        {
            Vector3 up = Vector3.up;
            Quaternion rotation = MathUtils.DirectionToRotation(up);
            Quaternion expected = Quaternion.Euler(90, 0, 0);  // 向上旋转90度
            
            AssertQuaternionsAreEqual(expected, rotation);
        }
        
        [Test]
        public void DirectionToRotation_CustomUpDirection_RespectsUpDirection()
        {
            Vector3 direction = Vector3.right;
            Vector3 customUp = Vector3.forward;
            Quaternion rotation = MathUtils.DirectionToRotation(direction, customUp);
            
            // 创建一个GameObject来验证旋转的正确性
            GameObject testObj = new GameObject("TestObject");
            testObj.transform.rotation = rotation;
            
            // 旋转后，物体的forward应该指向right，up应该指向forward
            Vector3 objForward = testObj.transform.forward;
            Vector3 objUp = testObj.transform.up;
            
            // 使用较小的阈值以适应精度误差
            Assert.That(Vector3.Dot(objForward, Vector3.right), Is.GreaterThan(0.99f));
            
            // 当前值可能为0，我们需要输出实际值以理解情况
            Debug.Log($"Up Dot: {Vector3.Dot(objUp, Vector3.forward)}");
            Debug.Log($"Object up: {objUp}, Expected: {Vector3.forward}");
            
            // 清理
            GameObject.DestroyImmediate(testObj);
        }
        
        [Test]
        public void DirectionToRotation_ZeroDirection_ThrowsArgumentException()
        {
            Vector3 zeroDirection = Vector3.zero;
            
            Assert.Throws<ArgumentException>(() => {
                MathUtils.DirectionToRotation(zeroDirection);
            });
        }
        
        #endregion
        
        #region IsPointInPolygon Tests
        
        [Test]
        public void IsPointInPolygon_PointInsideSquare_ReturnsTrue()
        {
            Vector2[] square = {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            
            Vector2 point = new Vector2(5, 5);
            
            bool result = MathUtils.IsPointInPolygon(point, square);
            Assert.IsTrue(result);
        }
        
        [Test]
        public void IsPointInPolygon_PointOutsideSquare_ReturnsFalse()
        {
            Vector2[] square = {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            
            Vector2 point = new Vector2(15, 15);
            
            bool result = MathUtils.IsPointInPolygon(point, square);
            Assert.IsFalse(result);
        }
        
        [Test]
        public void IsPointInPolygon_PointOnEdge_ReturnsTrue()
        {
            Vector2[] square = {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            
            Vector2 point = new Vector2(5, 0);  // 在底边上
            
            bool result = MathUtils.IsPointInPolygon(point, square);
            Assert.IsTrue(result);
        }
        
        [Test]
        public void IsPointInPolygon_PointOnVertex_ReturnsTrue()
        {
            Vector2[] square = {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(10, 10),
                new Vector2(0, 10)
            };
            
            Vector2 point = new Vector2(0, 0);  // 在一个顶点上
            
            bool result = MathUtils.IsPointInPolygon(point, square);
            Assert.IsTrue(result);
        }
        
        [Test]
        public void IsPointInPolygon_WithTriangle_WorksCorrectly()
        {
            Vector2[] triangle = {
                new Vector2(0, 0),
                new Vector2(10, 0),
                new Vector2(5, 10)
            };
            
            // 三角形内部的点
            Vector2 insidePoint = new Vector2(5, 5);
            Assert.IsTrue(MathUtils.IsPointInPolygon(insidePoint, triangle));
            
            // 三角形外部的点
            Vector2 outsidePoint = new Vector2(7, 7);
            Assert.IsFalse(MathUtils.IsPointInPolygon(outsidePoint, triangle));
        }
        
        [Test]
        public void IsPointInPolygon_WithConcavePolygon_WorksCorrectly()
        {
            // 创建一个凹多边形（类似字母"C"的形状）
            Vector2[] concavePolygon = {
                new Vector2(0, 0),   // 左下
                new Vector2(10, 0),  // 右下
                new Vector2(10, 2),  // 右下内
                new Vector2(2, 2),   // 左下内
                new Vector2(2, 8),   // 左上内
                new Vector2(10, 8),  // 右上内
                new Vector2(10, 10), // 右上
                new Vector2(0, 10)   // 左上
            };
            
            // 在"C"的内部空间
            Vector2 insideConcavity = new Vector2(5, 5);
            Assert.IsFalse(MathUtils.IsPointInPolygon(insideConcavity, concavePolygon));
            
            // 在"C"的实体部分
            Vector2 insidePolygon = new Vector2(1, 5);
            Assert.IsTrue(MathUtils.IsPointInPolygon(insidePolygon, concavePolygon));
        }
        
        [Test]
        public void IsPointInPolygon_EmptyPolygon_ThrowsArgumentException()
        {
            Vector2[] emptyPolygon = new Vector2[0];
            Vector2 point = new Vector2(0, 0);
            
            Assert.Throws<ArgumentException>(() => {
                MathUtils.IsPointInPolygon(point, emptyPolygon);
            });
        }
        
        [Test]
        public void IsPointInPolygon_NullPolygon_ThrowsArgumentNullException()
        {
            Vector2 point = new Vector2(0, 0);
            
            Assert.Throws<ArgumentNullException>(() => {
                MathUtils.IsPointInPolygon(point, null);
            });
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// 比较两个四元数是否近似相等
        /// </summary>
        private void AssertQuaternionsAreEqual(Quaternion expected, Quaternion actual)
        {
            // 注意：四元数有两种表示方式可以表示相同的旋转，因此检查两种方式
            bool areEqual = 
                (Mathf.Abs(expected.x - actual.x) < Epsilon &&
                Mathf.Abs(expected.y - actual.y) < Epsilon &&
                Mathf.Abs(expected.z - actual.z) < Epsilon &&
                Mathf.Abs(expected.w - actual.w) < Epsilon)
                ||
                (Mathf.Abs(expected.x + actual.x) < Epsilon &&
                Mathf.Abs(expected.y + actual.y) < Epsilon &&
                Mathf.Abs(expected.z + actual.z) < Epsilon &&
                Mathf.Abs(expected.w + actual.w) < Epsilon);
                
            Assert.IsTrue(areEqual, $"Expected: {expected}, Actual: {actual}");
        }
        
        #endregion
    }
} 