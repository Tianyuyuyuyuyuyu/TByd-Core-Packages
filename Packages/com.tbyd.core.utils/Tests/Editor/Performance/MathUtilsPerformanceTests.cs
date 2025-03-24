using NUnit.Framework;
using TByd.Core.Utils.Runtime;
using TByd.Core.Utils.Tests.Editor.Framework;
using Unity.PerformanceTesting;
using UnityEngine;

namespace TByd.Core.Utils.Tests.Editor.Performance
{
    /// <summary>
    /// MathUtils工具类的性能测试
    /// </summary>
    [TestFixture]
    [Category("Performance")]
    public class MathUtilsPerformanceTests : PerformanceTestBase
    {
        private const int BenchmarkIterations = 10000;
        private Vector2[] _testPolygon;
        private Vector2[] _testPoints;
        
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            // 准备测试数据 - 生成一个复杂多边形
            _testPolygon = new Vector2[8];
            _testPolygon[0] = new Vector2(0, 0);     // 左下
            _testPolygon[1] = new Vector2(100, 0);   // 右下
            _testPolygon[2] = new Vector2(100, 20);  // 右下内
            _testPolygon[3] = new Vector2(20, 20);   // 左下内
            _testPolygon[4] = new Vector2(20, 80);   // 左上内
            _testPolygon[5] = new Vector2(100, 80);  // 右上内
            _testPolygon[6] = new Vector2(100, 100); // 右上
            _testPolygon[7] = new Vector2(0, 100);   // 左上
            
            // 准备测试点 - 一些点在多边形内，一些在外
            _testPoints = new Vector2[1000];
            for (int i = 0; i < _testPoints.Length; i++)
            {
                _testPoints[i] = TestDataGenerator.GenerateVector2(
                    minValue: -50f, 
                    maxValue: 150f
                );
            }
        }
        
        /// <summary>
        /// 测试MathUtils.Remap方法的性能，与手动计算比较
        /// </summary>
        [Test, Performance]
        public void Remap_Performance()
        {
            // 准备测试数据 - 1000个随机值
            float[] values = new float[1000];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = TestDataGenerator.GenerateFloat(-100f, 100f);
            }
            
            // 重映射参数
            float fromMin = -100f;
            float fromMax = 100f;
            float toMin = 0f;
            float toMax = 1f;
            
            ComparePerformance(
                // 基准实现 - 手动计算
                () => {
                    for (int j = 0; j < BenchmarkIterations / 100; j++)
                    {
                        for (int i = 0; i < values.Length; i++)
                        {
                            float normalized = (values[i] - fromMin) / (fromMax - fromMin);
                            float result = toMin + normalized * (toMax - toMin);
                            // 使用结果防止被优化掉
                            _ = result;
                        }
                    }
                },
                
                // 优化实现 - MathUtils.Remap
                () => {
                    for (int j = 0; j < BenchmarkIterations / 100; j++)
                    {
                        for (int i = 0; i < values.Length; i++)
                        {
                            float result = MathUtils.Remap(values[i], fromMin, fromMax, toMin, toMax);
                            // 使用结果防止被优化掉
                            _ = result;
                        }
                    }
                },
                
                "Remap性能比较"
            );
        }
        
        /// <summary>
        /// 测试MathUtils.SmoothDamp方法的性能，与Unity内置Mathf.SmoothDamp比较
        /// </summary>
        [Test, Performance]
        public void SmoothDamp_Performance()
        {
            // 准备测试数据 - 1000组平滑阻尼插值数据
            float[] currentValues = new float[1000];
            float[] targetValues = new float[1000];
            float[] velocities = new float[1000];
            float[] ourVelocities = new float[1000];
            float smoothTime = 0.3f;
            float deltaTime = 0.016f; // 约60FPS
            
            for (int i = 0; i < currentValues.Length; i++)
            {
                currentValues[i] = TestDataGenerator.GenerateFloat(-100f, 100f);
                targetValues[i] = TestDataGenerator.GenerateFloat(-100f, 100f);
                velocities[i] = 0f;
                ourVelocities[i] = 0f;
            }
            
            ComparePerformance(
                // 基准实现 - Unity内置
                () => {
                    for (int j = 0; j < BenchmarkIterations / 1000; j++)
                    {
                        for (int i = 0; i < currentValues.Length; i++)
                        {
                            float result = Mathf.SmoothDamp(
                                currentValues[i], 
                                targetValues[i], 
                                ref velocities[i], 
                                smoothTime, 
                                Mathf.Infinity, 
                                deltaTime
                            );
                            // 使用结果防止被优化掉
                            _ = result;
                        }
                    }
                },
                
                // 优化实现 - MathUtils.SmoothDamp
                () => {
                    for (int j = 0; j < BenchmarkIterations / 1000; j++)
                    {
                        for (int i = 0; i < currentValues.Length; i++)
                        {
                            float result = MathUtils.SmoothDamp(
                                currentValues[i], 
                                targetValues[i], 
                                ref ourVelocities[i], 
                                smoothTime, 
                                float.MaxValue, 
                                deltaTime
                            );
                            // 使用结果防止被优化掉
                            _ = result;
                        }
                    }
                },
                
                "SmoothDamp性能比较"
            );
        }
        
        /// <summary>
        /// 测试MathUtils.DirectionToRotation方法的性能，与Quaternion.LookRotation比较
        /// </summary>
        [Test, Performance]
        public void DirectionToRotation_Performance()
        {
            // 准备测试数据 - 1000个随机方向
            Vector3[] directions = new Vector3[1000];
            for (int i = 0; i < directions.Length; i++)
            {
                directions[i] = TestDataGenerator.GenerateVector3().normalized;
            }
            
            ComparePerformance(
                // 基准实现 - Unity内置
                () => {
                    for (int j = 0; j < BenchmarkIterations / 1000; j++)
                    {
                        for (int i = 0; i < directions.Length; i++)
                        {
                            Quaternion result = Quaternion.LookRotation(directions[i]);
                            // 使用结果防止被优化掉
                            _ = result;
                        }
                    }
                },
                
                // 优化实现 - MathUtils.DirectionToRotation
                () => {
                    for (int j = 0; j < BenchmarkIterations / 1000; j++)
                    {
                        for (int i = 0; i < directions.Length; i++)
                        {
                            Quaternion result = MathUtils.DirectionToRotation(directions[i]);
                            // 使用结果防止被优化掉
                            _ = result;
                        }
                    }
                },
                
                "DirectionToRotation性能比较"
            );
        }
        
        /// <summary>
        /// 测试MathUtils.DirectionToRotation方法的性能（带自定义up参数）
        /// </summary>
        [Test, Performance]
        public void DirectionToRotation_WithCustomUp_Performance()
        {
            // 准备测试数据 - 1000组随机方向和up向量
            Vector3[] directions = new Vector3[1000];
            Vector3[] upVectors = new Vector3[1000];
            
            for (int i = 0; i < directions.Length; i++)
            {
                directions[i] = TestDataGenerator.GenerateVector3().normalized;
                
                // 生成一个与direction不共线的up向量
                Vector3 randomVector;
                do
                {
                    randomVector = TestDataGenerator.GenerateVector3().normalized;
                } while (Mathf.Abs(Vector3.Dot(directions[i], randomVector)) > 0.95f);
                
                upVectors[i] = randomVector;
            }
            
            ComparePerformance(
                // 基准实现 - Unity内置
                () => {
                    for (int j = 0; j < BenchmarkIterations / 1000; j++)
                    {
                        for (int i = 0; i < directions.Length; i++)
                        {
                            Quaternion result = Quaternion.LookRotation(directions[i], upVectors[i]);
                            // 使用结果防止被优化掉
                            _ = result;
                        }
                    }
                },
                
                // 优化实现 - MathUtils.DirectionToRotation
                () => {
                    for (int j = 0; j < BenchmarkIterations / 1000; j++)
                    {
                        for (int i = 0; i < directions.Length; i++)
                        {
                            Quaternion result = MathUtils.DirectionToRotation(directions[i], upVectors[i]);
                            // 使用结果防止被优化掉
                            _ = result;
                        }
                    }
                },
                
                "DirectionToRotation(带自定义up)性能比较"
            );
        }
        
        /// <summary>
        /// 测试MathUtils.IsPointInPolygon方法的性能
        /// </summary>
        [Test, Performance]
        public void IsPointInPolygon_Performance()
        {
            // 测量不同复杂度多边形的性能
            MeasurePolygonPerformance("凹多边形(8顶点)", _testPolygon);
            
            // 创建更复杂的多边形
            Vector2[] complexPolygon = new Vector2[32];
            for (int i = 0; i < 32; i++)
            {
                float angle = (i / 32f) * Mathf.PI * 2f;
                float radius = 50f + Mathf.Sin(angle * 5f) * 30f;
                complexPolygon[i] = new Vector2(
                    Mathf.Cos(angle) * radius + 50f,
                    Mathf.Sin(angle) * radius + 50f
                );
            }
            
            MeasurePolygonPerformance("复杂多边形(32顶点)", complexPolygon);
            
            // 简单三角形
            Vector2[] triangle = new Vector2[3]
            {
                new Vector2(0, 0),
                new Vector2(100, 0),
                new Vector2(50, 100)
            };
            
            MeasurePolygonPerformance("三角形(3顶点)", triangle);
        }
        
        /// <summary>
        /// 测量多边形点包含测试的性能
        /// </summary>
        private void MeasurePolygonPerformance(string name, Vector2[] polygon)
        {
            MeasurePerformance(
                () => {
                    for (int j = 0; j < BenchmarkIterations / 1000; j++)
                    {
                        for (int i = 0; i < _testPoints.Length; i++)
                        {
                            bool result = MathUtils.IsPointInPolygon(_testPoints[i], polygon);
                            // 使用结果防止被优化掉
                            _ = result;
                        }
                    }
                },
                $"IsPointInPolygon({name})"
            );
        }
        
        /// <summary>
        /// 测试MathUtils数学操作的GC分配
        /// </summary>
        [Test]
        [Performance]
        public void MathUtils_GCAllocation()
        {
            // SmoothDamp方法应该有限的GC分配
            MeasureGC.AssertMaxAllocation(() => {
                float velocity = 0;
                for (int i = 0; i < 1000; i++)
                {
                    var result = MathUtils.SmoothDamp(0f, 100f, ref velocity, 0.3f);
                    _ = result;
                }
            }, 10, "SmoothDamp方法应该有限的GC分配");

            // Remap方法应该没有GC分配
            MeasureGC.AssertNoAllocation(() => {
                for (int i = 0; i < 1000; i++)
                {
                    var result = MathUtils.Remap(50.0f, 0.0f, 100.0f, 0.0f, 1.0f);
                    _ = result;
                }
            }, "Remap方法应该没有GC分配");

            // DirectionToRotation方法应该有限的GC分配
            MeasureGC.AssertMaxAllocation(() => {
                for (int i = 0; i < 1000; i++)
                {
                    var result = MathUtils.DirectionToRotation(Vector3.forward);
                    _ = result;
                }
            }, 10, "DirectionToRotation方法应该有限的GC分配");

            // IsPointInPolygon方法应该有限的GC分配
            MeasureGC.AssertMaxAllocation(() => {
                for (int i = 0; i < 100; i++)
                {
                    var result = MathUtils.IsPointInPolygon(new Vector2(50, 50), _testPolygon);
                    _ = result;
                }
            }, 20, "IsPointInPolygon方法应该有限的GC分配");
        }

        [Test]
        [Performance]
        public void Clamp_Int_Performance()
        {
            // ... existing code ...
        }

        [Test]
        [Performance]
        public void Clamp_Float_Performance()
        {
            // ... existing code ...
        }

        [Test]
        [Performance]
        public void Lerp_Performance()
        {
            // ... existing code ...
        }

        [Test]
        [Performance]
        public void InverseLerp_Performance()
        {
            // ... existing code ...
        }

        [Test]
        [Performance]
        public void SmoothStep_Performance()
        {
            // ... existing code ...
        }

        [Test]
        [Performance]
        public void Map_Performance()
        {
            // ... existing code ...
        }

        [Test]
        [Performance]
        public void Approximately_Performance()
        {
            // ... existing code ...
        }

        [Test]
        [Performance]
        public void MathUtils_Clamp_Int_GCAllocation()
        {
            // ... existing code ...
        }

        [Test]
        [Performance]
        public void MathUtils_Clamp_Float_GCAllocation()
        {
            // ... existing code ...
        }

        [Test]
        [Performance]
        public void MathUtils_Lerp_GCAllocation()
        {
            // ... existing code ...
        }

        [Test]
        [Performance]
        public void MathUtils_InverseLerp_GCAllocation()
        {
            // ... existing code ...
        }

        [Test]
        [Performance]
        public void MathUtils_SmoothStep_GCAllocation()
        {
            // ... existing code ...
        }

        [Test]
        [Performance]
        public void MathUtils_Map_GCAllocation()
        {
            // ... existing code ...
        }

        [Test]
        [Performance]
        public void MathUtils_Approximately_GCAllocation()
        {
            // ... existing code ...
        }
    }
} 