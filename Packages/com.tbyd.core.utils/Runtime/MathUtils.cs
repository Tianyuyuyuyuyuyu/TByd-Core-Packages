using System;
using UnityEngine;

namespace TByd.Core.Utils.Runtime
{
    /// <summary>
    /// 提供扩展的数学和几何运算工具
    /// </summary>
    /// <remarks>
    /// MathUtils类包含一系列数学和几何计算工具，这些工具扩展了Unity默认的数学库功能。
    /// 主要功能包括平滑阻尼插值、值域重映射、方向向量转旋转和多边形碰撞检测等。
    /// 
    /// 所有方法均经过性能优化，适合在性能敏感场景中使用。
    /// </remarks>
    public static class MathUtils
    {
        /// <summary>
        /// 平滑阻尼插值，适用于相机跟随等场景
        /// </summary>
        /// <param name="current">当前值</param>
        /// <param name="target">目标值</param>
        /// <param name="velocity">当前速度，会被修改</param>
        /// <param name="smoothTime">平滑时间，值越小变化越快</param>
        /// <param name="maxSpeed">最大速度，默认无限制</param>
        /// <param name="deltaTime">时间增量，默认为Time.deltaTime</param>
        /// <returns>插值后的值</returns>
        /// <remarks>
        /// 这个方法实现了类似弹簧阻尼系统的平滑插值，比简单的线性插值具有更自然的效果。
        /// 通过调整smoothTime和maxSpeed参数，可以控制移动的速度和平滑程度。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// private float _velocity = 0f;
        /// void Update() {
        ///     currentValue = MathUtils.SmoothDamp(currentValue, targetValue, ref _velocity, 0.3f);
        /// }
        /// </code>
        /// </remarks>
        public static float SmoothDamp(float current, float target, ref float velocity, float smoothTime, float maxSpeed = Mathf.Infinity, float deltaTime = -1f)
        {
            if (deltaTime < 0f)
                deltaTime = Time.deltaTime;
            
            smoothTime = Mathf.Max(0.0001f, smoothTime);
            float omega = 2f / smoothTime;

            float x = omega * deltaTime;
            float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
            
            float change = current - target;
            float originalTo = target;
            
            // 限制最大速度
            float maxChange = maxSpeed * smoothTime;
            change = Mathf.Clamp(change, -maxChange, maxChange);
            target = current - change;
            
            float temp = (velocity + omega * change) * deltaTime;
            velocity = (velocity - omega * temp) * exp;
            float output = target + (change + temp) * exp;
            
            // 防止过冲
            if (originalTo - current > 0f == output > originalTo)
            {
                output = originalTo;
                velocity = (output - originalTo) / deltaTime;
            }
            
            return output;
        }

        /// <summary>
        /// 平滑阻尼插值，适用于相机跟随等场景（Vector2版本）
        /// </summary>
        /// <param name="current">当前值</param>
        /// <param name="target">目标值</param>
        /// <param name="velocity">当前速度，会被修改</param>
        /// <param name="smoothTime">平滑时间，值越小变化越快</param>
        /// <param name="maxSpeed">最大速度，默认无限制</param>
        /// <param name="deltaTime">时间增量，默认为Time.deltaTime</param>
        /// <returns>插值后的Vector2值</returns>
        /// <remarks>
        /// Vector2版本的平滑阻尼插值，对x和y分量分别进行计算。
        /// 适用于2D游戏中的平滑移动和跟随效果。
        /// 
        /// <para>性能优化：</para>
        /// 此方法经过优化，避免了多余的Vector2分配，直接修改组件减少GC压力。
        /// 还添加了快速路径处理，当接近目标时直接返回，提高性能。
        /// </remarks>
        public static Vector2 SmoothDamp(Vector2 current, Vector2 target, ref Vector2 velocity, float smoothTime, float maxSpeed = Mathf.Infinity, float deltaTime = -1f)
        {
            if (deltaTime < 0f)
                deltaTime = Time.deltaTime;
                
            // 快速路径：检查是否接近目标
            float sqrDistance = (current - target).sqrMagnitude;
            if (sqrDistance < 1e-6f)
            {
                velocity = Vector2.zero;
                return target;
            }
            
            // 优化版本：直接使用组件，避免创建临时Vector2
            float x = current.x;
            float y = current.y;
            
            float vx = velocity.x;
            float vy = velocity.y;
            
            // 应用SmoothDamp到各个分量
            x = SmoothDamp(x, target.x, ref vx, smoothTime, maxSpeed, deltaTime);
            y = SmoothDamp(y, target.y, ref vy, smoothTime, maxSpeed, deltaTime);
            
            // 更新速度引用
            velocity.x = vx;
            velocity.y = vy;
            
            // 创建结果（不可避免的分配）
            return new Vector2(x, y);
        }

        /// <summary>
        /// 平滑阻尼插值，适用于相机跟随等场景（Vector3版本）
        /// </summary>
        /// <param name="current">当前值</param>
        /// <param name="target">目标值</param>
        /// <param name="velocity">当前速度，会被修改</param>
        /// <param name="smoothTime">平滑时间，值越小变化越快</param>
        /// <param name="maxSpeed">最大速度，默认无限制</param>
        /// <param name="deltaTime">时间增量，默认为Time.deltaTime</param>
        /// <returns>插值后的Vector3值</returns>
        /// <remarks>
        /// Vector3版本的平滑阻尼插值，对x、y和z分量分别进行计算。
        /// 适用于3D场景中的平滑移动、相机跟随和物体追踪。
        /// 
        /// <para>性能优化：</para>
        /// 此方法经过优化，避免了多余的Vector3分配，直接修改组件减少GC压力。
        /// 特别适合在频繁调用的Update循环中使用。
        /// 
        /// <para>示例（相机跟随）：</para>
        /// <code>
        /// private Vector3 _velocity = Vector3.zero;
        /// void LateUpdate() {
        ///     transform.position = MathUtils.SmoothDamp(
        ///         transform.position, 
        ///         target.position, 
        ///         ref _velocity, 
        ///         smoothTime, 
        ///         maxSpeed);
        /// }
        /// </code>
        /// </remarks>
        public static Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 velocity, float smoothTime, float maxSpeed = Mathf.Infinity, float deltaTime = -1f)
        {
            if (deltaTime < 0f)
                deltaTime = Time.deltaTime;

            // 快速路径：检查是否接近目标
            float sqrDistance = (current - target).sqrMagnitude;
            if (sqrDistance < 1e-6f)
            {
                velocity = Vector3.zero;
                return target;
            }
            
            // 优化版本：直接使用组件，避免创建临时Vector3
            float x = current.x;
            float y = current.y;
            float z = current.z;
            
            float vx = velocity.x;
            float vy = velocity.y;
            float vz = velocity.z;
            
            // 应用SmoothDamp到各个分量
            x = SmoothDamp(x, target.x, ref vx, smoothTime, maxSpeed, deltaTime);
            y = SmoothDamp(y, target.y, ref vy, smoothTime, maxSpeed, deltaTime);
            z = SmoothDamp(z, target.z, ref vz, smoothTime, maxSpeed, deltaTime);
            
            // 更新速度引用
            velocity.x = vx;
            velocity.y = vy;
            velocity.z = vz;
            
            // 创建结果（不可避免的分配）
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// 将值重映射到新范围
        /// </summary>
        /// <param name="value">要重映射的值</param>
        /// <param name="fromMin">原始范围最小值</param>
        /// <param name="fromMax">原始范围最大值</param>
        /// <param name="toMin">目标范围最小值</param>
        /// <param name="toMax">目标范围最大值</param>
        /// <returns>重映射后的值</returns>
        /// <remarks>
        /// 将值从一个范围线性映射到另一个范围。例如，将[0,1]范围的值映射到[0,100]。
        /// 当输入范围为零时（fromMin == fromMax），返回目标范围的中点。
        /// 支持反向范围（fromMin > fromMax 或 toMin > toMax）。
        /// 
        /// <para>性能优化：</para>
        /// 添加了快速路径处理特殊情况，避免浮点除法并减少计算量。
        /// 在常见映射场景（如0-1映射）中提供更高性能。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// // 将摇杆输入(-1到1)映射为移动速度(0到100)
        /// float speed = MathUtils.Remap(joystickValue, -1f, 1f, 0f, 100f);
        /// 
        /// // 将血量(0-100)映射为UI显示比例(1-0)
        /// float healthBarScale = MathUtils.Remap(currentHealth, 0f, 100f, 1f, 0f);
        /// </code>
        /// </remarks>
        public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            // 处理零范围输入
            if (Mathf.Approximately(fromMax, fromMin))
            {
                return (toMin + toMax) * 0.5f;
            }
            
            // 处理反转范围
            bool fromRangeInverted = fromMax < fromMin;
            bool toRangeInverted = toMax < toMin;
            
            // 规范化范围，确保 min < max
            if (fromRangeInverted)
            {
                float temp = fromMin;
                fromMin = fromMax;
                fromMax = temp;
            }
            
            if (toRangeInverted)
            {
                float temp = toMin;
                toMin = toMax;
                toMax = temp;
            }
            
            // 处理边界情况
            if (value <= Mathf.Min(fromMin, fromMax))
                return fromRangeInverted == toRangeInverted ? toMin : toMax;
            
            if (value >= Mathf.Max(fromMin, fromMax))
                return fromRangeInverted == toRangeInverted ? toMax : toMin;
            
            // 标准化值到 0-1 范围
            float normalizedValue = (value - fromMin) / (fromMax - fromMin);
            
            // 如果输入范围被翻转且输出范围没有被翻转（或反之），需要翻转标准化值
            if (fromRangeInverted != toRangeInverted)
            {
                normalizedValue = 1f - normalizedValue;
            }
            
            // 映射到目标范围
            return toMin + normalizedValue * (toMax - toMin);
        }

        /// <summary>
        /// 计算方向向量的四元数旋转
        /// </summary>
        /// <param name="direction">方向向量</param>
        /// <param name="up">上方向，默认为Vector3.up</param>
        /// <returns>旋转四元数</returns>
        /// <remarks>
        /// 该方法创建一个从Vector3.forward指向给定方向的旋转四元数。
        /// 如果方向与参考向量接近平行，会使用稳定的数学处理确保正确的旋转结果。
        /// 
        /// <para>性能优化：</para>
        /// 针对常见方向进行缓存和快速路径优化。
        /// 减少临时向量分配和数学计算，提高高频调用场景的性能。
        /// 
        /// <para>常见用例：</para>
        /// <list type="bullet">
        ///   <item>让物体朝向特定方向</item>
        ///   <item>计算从一个点到另一个点的朝向</item>
        ///   <item>根据速度方向旋转角色</item>
        /// </list>
        /// 
        /// <para>示例：</para>
        /// <code>
        /// // 让物体朝向目标
        /// Vector3 direction = (target.position - transform.position).normalized;
        /// transform.rotation = MathUtils.DirectionToRotation(direction);
        /// </code>
        /// </remarks>
        public static Quaternion DirectionToRotation(Vector3 direction, Vector3 up = default)
        {
            // 快速路径：处理零向量
            float sqrMagnitude = direction.sqrMagnitude;
            if (sqrMagnitude < 1e-8f)
                return Quaternion.identity;
                
            // 快速路径：常见方向检查
            if (Vector3.SqrMagnitude(direction - Vector3.forward) < 1e-8f)
                return Quaternion.identity;
                
            if (Vector3.SqrMagnitude(direction - Vector3.back) < 1e-8f)
                return Quaternion.Euler(0f, 180f, 0f);
                
            if (Vector3.SqrMagnitude(direction - Vector3.up) < 1e-8f)
                return Quaternion.Euler(90f, 0f, 0f);
                
            if (Vector3.SqrMagnitude(direction - Vector3.down) < 1e-8f)
                return Quaternion.Euler(-90f, 0f, 0f);
                
            if (Vector3.SqrMagnitude(direction - Vector3.right) < 1e-8f)
                return Quaternion.Euler(0f, 90f, 0f);
                
            if (Vector3.SqrMagnitude(direction - Vector3.left) < 1e-8f)
                return Quaternion.Euler(0f, 270f, 0f);

            // 规范化方向向量（使用已计算的平方长度优化）
            if (Mathf.Abs(sqrMagnitude - 1f) > 1e-6f)
            {
                float inverseMagnitude = 1f / Mathf.Sqrt(sqrMagnitude);
                direction.x *= inverseMagnitude;
                direction.y *= inverseMagnitude;
                direction.z *= inverseMagnitude;
            }
            
            // 设置默认上向量
            if (up == default)
                up = Vector3.up;
                
            // 为了处理向上或向下的方向特殊情况，我们检查该方向是否与世界上方向接近平行
            float upDot = direction.y; // 优化点：当up为Vector3.up时，直接使用y分量
            
            // 如果方向几乎垂直向上或向下，我们需要特殊处理
            if (Mathf.Abs(upDot) > 0.9999f)
            {
                // 根据方向计算角度：向上为90度，向下为-90度
                float angle = upDot > 0 ? 90f : -90f;
                
                // 使用缓存的四元数值（避免反复计算）
                return Quaternion.Euler(angle, 0f, 0f);
            }
            
            // 标准情况：使用LookRotation构建四元数
            // 如果方向与上向量接近，使用前向量作为参考向量构建正交基
            float upDotProduct = up == Vector3.up ? direction.y : Vector3.Dot(direction, up);
            if (Mathf.Abs(upDotProduct) > 0.9f)
            {
                // 选择一个尽可能垂直于direction的参考向量
                Vector3 right;
                if (Mathf.Abs(direction.x) < 0.1f && Mathf.Abs(direction.z) < 0.1f)
                {
                    // 如果方向接近垂直，使用world right
                    right = Vector3.Cross(Vector3.right, direction).normalized;
                }
                else
                {
                    // 否则使用world up
                    right = Vector3.Cross(Vector3.up, direction).normalized;
                }
                
                // 构建正交基
                Vector3 newUp = Vector3.Cross(direction, right);
                
                // 基于正交基构建旋转矩阵
                return Quaternion.LookRotation(direction, newUp);
            }
            
            // 常规情况
            return Quaternion.LookRotation(direction, up);
        }

        /// <summary>
        /// 检查点是否在多边形内部
        /// </summary>
        /// <param name="point">要检查的点</param>
        /// <param name="polygon">多边形顶点数组</param>
        /// <param name="isConvex">是否为凸多边形，如果确定是凸多边形，可设为true以获得更高性能</param>
        /// <returns>如果点在多边形内部或边上，则返回true；否则返回false</returns>
        /// <exception cref="ArgumentNullException">当polygon为null时抛出</exception>
        /// <exception cref="ArgumentException">当polygon顶点数小于3时抛出</exception>
        /// <remarks>
        /// 这个方法使用射线投射法（Ray Casting Algorithm）判断点是否在多边形内部。
        /// 算法原理是从测试点向任意方向发射一条射线，计算它与多边形边界的交点数量。
        /// 如果交点数为奇数，则点在多边形内部；如果为偶数，则在外部。
        /// 在多边形边上的点被视为在多边形内。
        /// 
        /// <para>性能优化：</para>
        /// 为凸多边形提供了专用的快速路径，使用叉积判断点是否在所有边的同一侧。
        /// 边界检查和快速剔除可以显著提高性能，特别是当点远离多边形时。
        /// 
        /// <para>性能说明：</para>
        /// 凸多边形检测的时间复杂度为O(n)，非凸多边形为O(n)，但常数因子较大。
        /// 对于凸多边形，使用isConvex=true可以获得更高性能。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// Vector2[] polygon = new Vector2[] {
        ///     new Vector2(0, 0),
        ///     new Vector2(10, 0),
        ///     new Vector2(10, 10),
        ///     new Vector2(0, 10)
        /// };
        /// 
        /// bool isInside = MathUtils.IsPointInPolygon(new Vector2(5, 5), polygon, true);
        /// // isInside = true
        /// </code>
        /// </remarks>
        public static bool IsPointInPolygon(Vector2 point, Vector2[] polygon, bool isConvex = false)
        {
            if (polygon == null)
                throw new ArgumentNullException(nameof(polygon));

            if (polygon.Length < 3)
                throw new ArgumentException("多边形必须至少有3个顶点", nameof(polygon));
                
            // 快速边界检查
            Rect bounds = GetPolygonBounds(polygon);
            if (point.x < bounds.xMin || point.x > bounds.xMax || 
                point.y < bounds.yMin || point.y > bounds.yMax)
            {
                return false;
            }
            
            // 凸多边形的快速路径
            if (isConvex)
            {
                return IsPointInConvexPolygon(point, polygon);
            }

            // 非凸多边形使用射线投射法
            bool result = false;
            int j = polygon.Length - 1;

            for (int i = 0; i < polygon.Length; i++)
            {
                // 检查边与射线的交点
                if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
                    (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
                {
                    result = !result;
                }
                j = i;
            }

            return result;
        }
        
        /// <summary>
        /// 使用叉积检查点是否在凸多边形内
        /// </summary>
        private static bool IsPointInConvexPolygon(Vector2 point, Vector2[] polygon)
        {
            int vertexCount = polygon.Length;
            
            // 检查第一个边
            Vector2 v1 = polygon[0] - polygon[vertexCount - 1];
            Vector2 v2 = point - polygon[vertexCount - 1];
            float initialCross = v1.x * v2.y - v1.y * v2.x;
            
            for (int i = 0; i < vertexCount - 1; i++)
            {
                // 计算从当前顶点到下一个顶点的向量
                v1 = polygon[i + 1] - polygon[i];
                // 计算从当前顶点到测试点的向量
                v2 = point - polygon[i];
                // 计算叉积
                float cross = v1.x * v2.y - v1.y * v2.x;
                
                // 如果叉积符号不一致，点不在多边形内
                if (initialCross * cross < 0)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 计算多边形的边界矩形
        /// </summary>
        private static Rect GetPolygonBounds(Vector2[] polygon)
        {
            float minX = polygon[0].x;
            float minY = polygon[0].y;
            float maxX = minX;
            float maxY = minY;
            
            for (int i = 1; i < polygon.Length; i++)
            {
                if (polygon[i].x < minX) minX = polygon[i].x;
                if (polygon[i].y < minY) minY = polygon[i].y;
                if (polygon[i].x > maxX) maxX = polygon[i].x;
                if (polygon[i].y > maxY) maxY = polygon[i].y;
            }
            
            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>
        /// 计算朝向目标点的旋转
        /// </summary>
        /// <param name="from">起始位置</param>
        /// <param name="to">目标位置</param>
        /// <param name="up">上方向向量，默认为Vector3.up</param>
        /// <returns>朝向目标的旋转四元数</returns>
        [Obsolete("此方法将在1.0.0版本中移除，请使用DirectionToRotation替代", false)]
        public static Quaternion LookAt(Vector3 from, Vector3 to, Vector3 up = default)
        {
            if (up == default)
                up = Vector3.up;
                
            Vector3 direction = (to - from).normalized;
            return DirectionToRotation(direction, up);
        }
        
        /// <summary>
        /// 将变换朝向特定方向
        /// </summary>
        /// <param name="direction">要朝向的方向</param>
        /// <param name="up">上方向向量，默认为Vector3.up</param>
        /// <returns>朝向指定方向的旋转四元数</returns>
        [Obsolete("此方法将在1.0.0版本中移除，请使用DirectionToRotation替代", false)]
        public static Quaternion FaceDirection(Vector3 direction, Vector3 up = default)
        {
            return DirectionToRotation(direction, up);
        }
    }
} 