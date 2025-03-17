using UnityEngine;

namespace TByd.Core.Utils.Runtime.Extensions
{
    /// <summary>
    /// Vector2和Vector3的扩展方法集合
    /// </summary>
    /// <remarks>
    /// 这个类提供了一系列实用的向量扩展方法，简化了常见的向量操作，
    /// 如分量修改、转换、插值等。
    /// </remarks>
    public static class VectorExtensions
    {
        #region Vector2扩展

        /// <summary>
        /// 设置Vector2的x分量，保持y不变
        /// </summary>
        /// <param name="vector">原始向量</param>
        /// <param name="x">新的x值</param>
        /// <returns>修改后的新向量</returns>
        /// <remarks>
        /// 此方法创建一个新的Vector2，只修改x分量。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// Vector2 position = new Vector2(3f, 4f);
        /// position = position.WithX(5f); // 结果: (5, 4)
        /// </code>
        /// </remarks>
        public static Vector2 WithX(this Vector2 vector, float x)
        {
            return new Vector2(x, vector.y);
        }

        /// <summary>
        /// 设置Vector2的y分量，保持x不变
        /// </summary>
        /// <param name="vector">原始向量</param>
        /// <param name="y">新的y值</param>
        /// <returns>修改后的新向量</returns>
        /// <remarks>
        /// 此方法创建一个新的Vector2，只修改y分量。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// Vector2 position = new Vector2(3f, 4f);
        /// position = position.WithY(5f); // 结果: (3, 5)
        /// </code>
        /// </remarks>
        public static Vector2 WithY(this Vector2 vector, float y)
        {
            return new Vector2(vector.x, y);
        }

        /// <summary>
        /// 将Vector2转换为Vector3，指定z值
        /// </summary>
        /// <param name="vector">原始二维向量</param>
        /// <param name="z">z分量的值，默认为0</param>
        /// <returns>转换后的三维向量</returns>
        /// <remarks>
        /// 此方法将Vector2转换为Vector3，可以指定z分量的值。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// Vector2 position2D = new Vector2(3f, 4f);
        /// Vector3 position3D = position2D.ToVector3(1f); // 结果: (3, 4, 1)
        /// </code>
        /// </remarks>
        public static Vector3 ToVector3(this Vector2 vector, float z = 0f)
        {
            return new Vector3(vector.x, vector.y, z);
        }

        /// <summary>
        /// 计算Vector2的垂直向量（顺时针旋转90度）
        /// </summary>
        /// <param name="vector">原始向量</param>
        /// <returns>垂直向量</returns>
        /// <remarks>
        /// 此方法返回原始向量顺时针旋转90度的垂直向量。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// Vector2 direction = new Vector2(1f, 0f);
        /// Vector2 perpendicular = direction.Perpendicular(); // 结果: (0, -1)
        /// </code>
        /// </remarks>
        public static Vector2 Perpendicular(this Vector2 vector)
        {
            return new Vector2(vector.y, -vector.x);
        }

        #endregion

        #region Vector3扩展

        /// <summary>
        /// 设置Vector3的x分量，保持y和z不变
        /// </summary>
        /// <param name="vector">原始向量</param>
        /// <param name="x">新的x值</param>
        /// <returns>修改后的新向量</returns>
        /// <remarks>
        /// 此方法创建一个新的Vector3，只修改x分量。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// Vector3 position = new Vector3(3f, 4f, 5f);
        /// position = position.WithX(6f); // 结果: (6, 4, 5)
        /// </code>
        /// </remarks>
        public static Vector3 WithX(this Vector3 vector, float x)
        {
            return new Vector3(x, vector.y, vector.z);
        }

        /// <summary>
        /// 设置Vector3的y分量，保持x和z不变
        /// </summary>
        /// <param name="vector">原始向量</param>
        /// <param name="y">新的y值</param>
        /// <returns>修改后的新向量</returns>
        /// <remarks>
        /// 此方法创建一个新的Vector3，只修改y分量。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// Vector3 position = new Vector3(3f, 4f, 5f);
        /// position = position.WithY(6f); // 结果: (3, 6, 5)
        /// </code>
        /// </remarks>
        public static Vector3 WithY(this Vector3 vector, float y)
        {
            return new Vector3(vector.x, y, vector.z);
        }

        /// <summary>
        /// 设置Vector3的z分量，保持x和y不变
        /// </summary>
        /// <param name="vector">原始向量</param>
        /// <param name="z">新的z值</param>
        /// <returns>修改后的新向量</returns>
        /// <remarks>
        /// 此方法创建一个新的Vector3，只修改z分量。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// Vector3 position = new Vector3(3f, 4f, 5f);
        /// position = position.WithZ(6f); // 结果: (3, 4, 6)
        /// </code>
        /// </remarks>
        public static Vector3 WithZ(this Vector3 vector, float z)
        {
            return new Vector3(vector.x, vector.y, z);
        }

        /// <summary>
        /// 将Vector3转换为Vector2，丢弃z分量
        /// </summary>
        /// <param name="vector">原始三维向量</param>
        /// <returns>转换后的二维向量</returns>
        /// <remarks>
        /// 此方法将Vector3转换为Vector2，丢弃z分量。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// Vector3 position3D = new Vector3(3f, 4f, 5f);
        /// Vector2 position2D = position3D.ToVector2(); // 结果: (3, 4)
        /// </code>
        /// </remarks>
        public static Vector2 ToVector2(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.y);
        }

        /// <summary>
        /// 将Vector3的各分量限制在指定的最小值和最大值之间
        /// </summary>
        /// <param name="vector">原始向量</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>限制后的新向量</returns>
        /// <remarks>
        /// 此方法创建一个新的Vector3，将各分量限制在指定范围内。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// Vector3 position = new Vector3(10f, -5f, 3f);
        /// position = position.Clamp(0f, 5f); // 结果: (5, 0, 3)
        /// </code>
        /// </remarks>
        public static Vector3 Clamp(this Vector3 vector, float min, float max)
        {
            return new Vector3(
                Mathf.Clamp(vector.x, min, max),
                Mathf.Clamp(vector.y, min, max),
                Mathf.Clamp(vector.z, min, max)
            );
        }

        /// <summary>
        /// 将Vector3的各分量限制在指定的最小向量和最大向量之间
        /// </summary>
        /// <param name="vector">原始向量</param>
        /// <param name="min">最小向量</param>
        /// <param name="max">最大向量</param>
        /// <returns>限制后的新向量</returns>
        /// <remarks>
        /// 此方法创建一个新的Vector3，将各分量限制在对应的最小和最大向量分量之间。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// Vector3 position = new Vector3(10f, -5f, 3f);
        /// Vector3 min = new Vector3(0f, 0f, 0f);
        /// Vector3 max = new Vector3(5f, 5f, 5f);
        /// position = position.Clamp(min, max); // 结果: (5, 0, 3)
        /// </code>
        /// </remarks>
        public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max)
        {
            return new Vector3(
                Mathf.Clamp(vector.x, min.x, max.x),
                Mathf.Clamp(vector.y, min.y, max.y),
                Mathf.Clamp(vector.z, min.z, max.z)
            );
        }

        #endregion
    }
} 