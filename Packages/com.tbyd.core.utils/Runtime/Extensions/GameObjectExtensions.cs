using UnityEngine;

namespace TByd.Core.Utils.Runtime.Extensions
{
    /// <summary>
    /// GameObject组件的扩展方法集合
    /// </summary>
    /// <remarks>
    /// 这个类提供了一系列实用的GameObject扩展方法，简化了常见的GameObject操作，
    /// 如查找或创建子物体、设置层级、激活状态管理等。
    /// 
    /// 所有方法均采用链式设计，允许连续调用多个方法。
    /// </remarks>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// 查找子物体，如果不存在则创建
        /// </summary>
        /// <param name="parent">父物体</param>
        /// <param name="name">子物体名称</param>
        /// <returns>找到或创建的子物体</returns>
        /// <remarks>
        /// 此方法首先尝试在父物体下查找指定名称的子物体，如果找不到则创建一个新的空物体。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// // 查找或创建UI容器
        /// GameObject uiContainer = gameObject.FindOrCreateChild("UI_Container");
        /// </code>
        /// </remarks>
        public static GameObject FindOrCreateChild(this GameObject parent, string name)
        {
            if (parent == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarning("子物体名称不能为空");
                return parent;
            }

            Transform child = parent.transform.Find(name);
            if (child != null)
            {
                return child.gameObject;
            }

            GameObject newChild = new GameObject(name);
            newChild.transform.SetParent(parent.transform, false);
            newChild.transform.localPosition = Vector3.zero;
            newChild.transform.localRotation = Quaternion.identity;
            newChild.transform.localScale = Vector3.one;
            
            return newChild;
        }

        /// <summary>
        /// 递归设置物体及其所有子物体的层级
        /// </summary>
        /// <param name="gameObject">要设置的物体</param>
        /// <param name="layer">目标层级</param>
        /// <returns>设置后的物体引用，用于链式调用</returns>
        /// <remarks>
        /// 此方法会递归设置物体及其所有子物体的层级。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// // 将物体及其所有子物体设置为UI层级
        /// gameObject.SetLayerRecursively(LayerMask.NameToLayer("UI"));
        /// </code>
        /// </remarks>
        public static GameObject SetLayerRecursively(this GameObject gameObject, int layer)
        {
            if (gameObject == null)
            {
                return null;
            }

            gameObject.layer = layer;
            
            foreach (Transform child in gameObject.transform)
            {
                if (child != null)
                {
                    SetLayerRecursively(child.gameObject, layer);
                }
            }
            
            return gameObject;
        }

        /// <summary>
        /// 安全地获取或添加组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="gameObject">目标物体</param>
        /// <returns>获取到的或新添加的组件</returns>
        /// <remarks>
        /// 此方法首先尝试获取指定类型的组件，如果不存在则添加一个新的。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// // 获取或添加Rigidbody组件
        /// Rigidbody rb = gameObject.GetOrAddComponent&lt;Rigidbody&gt;();
        /// </code>
        /// </remarks>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }

        /// <summary>
        /// 设置物体的激活状态并返回物体引用
        /// </summary>
        /// <param name="gameObject">目标物体</param>
        /// <param name="active">是否激活</param>
        /// <returns>物体引用，用于链式调用</returns>
        /// <remarks>
        /// 此方法设置物体的激活状态，并返回物体引用以支持链式调用。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// // 激活物体并添加组件
        /// gameObject.SetActive(true).GetOrAddComponent&lt;BoxCollider&gt;();
        /// </code>
        /// </remarks>
        public static GameObject SetActive(this GameObject gameObject, bool active)
        {
            if (gameObject != null && gameObject.activeSelf != active)
            {
                gameObject.SetActive(active);
            }
            return gameObject;
        }

        /// <summary>
        /// 销毁物体的所有子物体
        /// </summary>
        /// <param name="gameObject">父物体</param>
        /// <param name="immediate">是否立即销毁</param>
        /// <returns>父物体引用，用于链式调用</returns>
        /// <remarks>
        /// 此方法销毁物体的所有子物体，可选择是否立即销毁。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// // 清空容器并创建新子物体
        /// gameObject.DestroyAllChildren().FindOrCreateChild("NewChild");
        /// </code>
        /// </remarks>
        public static GameObject DestroyAllChildren(this GameObject gameObject, bool immediate = false)
        {
            if (gameObject == null)
            {
                return null;
            }

            Transform transform = gameObject.transform;
            while (transform.childCount > 0)
            {
                Transform child = transform.GetChild(0);
                if (immediate)
                {
                    Object.DestroyImmediate(child.gameObject);
                }
                else
                {
                    Object.Destroy(child.gameObject);
                }
            }
            
            return gameObject;
        }
    }
} 