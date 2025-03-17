using System.Collections.Generic;
using UnityEngine;

namespace TByd.Core.Utils.Runtime.Extensions
{
    /// <summary>
    /// Component组件的扩展方法集合
    /// </summary>
    /// <remarks>
    /// 这个类提供了一系列实用的Component扩展方法，简化了常见的组件操作，
    /// 如查找组件、获取或添加组件、递归查找等。
    /// </remarks>
    public static class ComponentExtensions
    {
        /// <summary>
        /// 获取组件，如果不存在则添加
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="component">当前组件</param>
        /// <returns>获取到的或新添加的组件</returns>
        /// <remarks>
        /// 此方法首先尝试获取指定类型的组件，如果不存在则添加一个新的。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// // 获取或添加Rigidbody组件
        /// Rigidbody rb = transform.GetOrAddComponent&lt;Rigidbody&gt;();
        /// </code>
        /// </remarks>
        public static T GetOrAddComponent<T>(this Component component) where T : Component
        {
            if (component == null)
            {
                return null;
            }
            
            T comp = component.GetComponent<T>();
            if (comp == null)
            {
                comp = component.gameObject.AddComponent<T>();
            }
            return comp;
        }

        /// <summary>
        /// 查找指定类型的所有子组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="component">当前组件</param>
        /// <param name="includeInactive">是否包含非激活的物体</param>
        /// <returns>找到的组件列表</returns>
        /// <remarks>
        /// 此方法查找当前物体及其所有子物体上的指定类型组件。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// // 查找所有子物体上的Collider组件
        /// List&lt;Collider&gt; colliders = transform.GetComponentsInChildrenList&lt;Collider&gt;(true);
        /// </code>
        /// </remarks>
        public static List<T> GetComponentsInChildrenList<T>(this Component component, bool includeInactive = false) where T : Component
        {
            if (component == null)
            {
                return new List<T>();
            }
            
            T[] components = component.GetComponentsInChildren<T>(includeInactive);
            return new List<T>(components);
        }

        /// <summary>
        /// 查找指定类型的所有父组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="component">当前组件</param>
        /// <param name="includeInactive">是否包含非激活的物体</param>
        /// <returns>找到的组件列表</returns>
        /// <remarks>
        /// 此方法查找当前物体及其所有父物体上的指定类型组件。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// // 查找所有父物体上的Canvas组件
        /// List&lt;Canvas&gt; canvases = transform.GetComponentsInParentList&lt;Canvas&gt;(true);
        /// </code>
        /// </remarks>
        public static List<T> GetComponentsInParentList<T>(this Component component, bool includeInactive = false) where T : Component
        {
            if (component == null)
            {
                return new List<T>();
            }
            
            T[] components = component.GetComponentsInParent<T>(includeInactive);
            return new List<T>(components);
        }

        /// <summary>
        /// 递归查找指定名称的子物体上的组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="component">当前组件</param>
        /// <param name="childName">子物体名称</param>
        /// <returns>找到的组件，如果未找到则返回null</returns>
        /// <remarks>
        /// 此方法递归查找指定名称的子物体，并返回其上的指定类型组件。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// // 查找名为"Weapon"的子物体上的Collider组件
        /// Collider weaponCollider = transform.FindComponentInChildren&lt;Collider&gt;("Weapon");
        /// </code>
        /// </remarks>
        public static T FindComponentInChildren<T>(this Component component, string childName) where T : Component
        {
            if (component == null || string.IsNullOrEmpty(childName))
            {
                return null;
            }
            
            Transform childTransform = component.transform.Find(childName);
            if (childTransform != null)
            {
                return childTransform.GetComponent<T>();
            }
            
            // 递归查找
            foreach (Transform child in component.transform)
            {
                T result = child.FindComponentInChildren<T>(childName);
                if (result != null)
                {
                    return result;
                }
            }
            
            return null;
        }

        /// <summary>
        /// 获取或创建子物体上的组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="component">当前组件</param>
        /// <param name="childName">子物体名称</param>
        /// <returns>获取到的或新创建的组件</returns>
        /// <remarks>
        /// 此方法首先尝试查找指定名称的子物体，如果不存在则创建一个新的。
        /// 然后获取或添加指定类型的组件。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// // 在名为"UI_Panel"的子物体上获取或创建Text组件
        /// Text text = transform.GetOrCreateComponentInChild&lt;Text&gt;("UI_Panel");
        /// </code>
        /// </remarks>
        public static T GetOrCreateComponentInChild<T>(this Component component, string childName) where T : Component
        {
            if (component == null || string.IsNullOrEmpty(childName))
            {
                return null;
            }
            
            Transform childTransform = component.transform.Find(childName);
            if (childTransform == null)
            {
                GameObject newChild = new GameObject(childName);
                newChild.transform.SetParent(component.transform, false);
                childTransform = newChild.transform;
            }
            
            return childTransform.GetOrAddComponent<T>();
        }

        /// <summary>
        /// 安全地销毁组件
        /// </summary>
        /// <param name="component">要销毁的组件</param>
        /// <param name="immediate">是否立即销毁</param>
        /// <remarks>
        /// 此方法安全地销毁组件，避免空引用异常。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// // 安全地销毁Rigidbody组件
        /// rigidbody.SafeDestroy();
        /// </code>
        /// </remarks>
        public static void SafeDestroy(this Component component, bool immediate = false)
        {
            if (component != null)
            {
                if (immediate)
                {
                    Object.DestroyImmediate(component);
                }
                else
                {
                    Object.Destroy(component);
                }
            }
        }
    }
} 