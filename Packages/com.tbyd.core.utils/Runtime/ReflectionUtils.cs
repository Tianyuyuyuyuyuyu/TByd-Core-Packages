using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace TByd.Core.Utils.Runtime
{
    /// <summary>
    /// 反射工具类
    /// </summary>
    /// <remarks>
    /// 提供缓存和高性能的反射操作，包括类型信息获取、属性和字段访问、方法调用等。
    /// 主要特点是使用缓存和委托/表达式树来提高反射性能。
    /// </remarks>
    public static class ReflectionUtils
    {
        #region 类型缓存
        
        // 类型信息缓存
        private static readonly Dictionary<string, Type> TypeCache = new Dictionary<string, Type>();
        
        /// <summary>
        /// 根据类型名称获取类型对象，使用缓存提高性能
        /// </summary>
        /// <param name="typeName">类型的完全限定名</param>
        /// <returns>类型对象，如果未找到则返回null</returns>
        public static Type GetType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;
                
            if (TypeCache.TryGetValue(typeName, out Type type))
                return type;
                
            type = Type.GetType(typeName);
            if (type == null)
            {
                // 在所有已加载的程序集中查找类型
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    type = assembly.GetType(typeName);
                    if (type != null)
                        break;
                }
            }
            
            if (type != null)
                TypeCache[typeName] = type;
                
            return type;
        }
        
        /// <summary>
        /// 获取程序集中所有符合条件的类型
        /// </summary>
        /// <param name="assembly">程序集对象</param>
        /// <param name="predicate">筛选条件</param>
        /// <returns>符合条件的类型集合</returns>
        public static IEnumerable<Type> GetTypes(Assembly assembly, Func<Type, bool> predicate = null)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));
                
            try
            {
                Type[] types = assembly.GetTypes();
                return predicate == null ? types : types.Where(predicate);
            }
            catch (ReflectionTypeLoadException ex)
            {
                Debug.LogError($"反射加载类型时出错: {ex.Message}");
                return ex.Types.Where(t => t != null && (predicate == null || predicate(t)));
            }
        }
        
        /// <summary>
        /// 获取所有已加载程序集中符合条件的类型
        /// </summary>
        /// <param name="predicate">筛选条件</param>
        /// <returns>符合条件的类型集合</returns>
        public static IEnumerable<Type> GetAllTypes(Func<Type, bool> predicate = null)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in GetTypes(assembly, predicate))
                {
                    yield return type;
                }
            }
        }
        
        /// <summary>
        /// 获取指定类型的所有公共静态字段名称
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <returns>静态字段名称列表</returns>
        public static List<string> GetStaticFieldNames(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
                
            return type.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(f => f.Name)
                .ToList();
        }
        
        /// <summary>
        /// 获取指定类型的所有公共属性名称
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <returns>属性名称列表</returns>
        public static List<string> GetPropertyNames(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
                
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.Name)
                .ToList();
        }
        
        /// <summary>
        /// 获取指定类型的所有公共方法名称
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <returns>方法名称列表</returns>
        public static List<string> GetMethodNames(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
                
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => !m.IsSpecialName) // 排除属性的get/set方法
                .Select(m => m.Name)
                .Distinct()
                .ToList();
        }
        
        #endregion
        
        #region 反射缓存
        
        // 成员信息缓存
        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> FieldInfoCache = 
            new Dictionary<Type, Dictionary<string, FieldInfo>>();
        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> PropertyInfoCache = 
            new Dictionary<Type, Dictionary<string, PropertyInfo>>();
        private static readonly Dictionary<string, MethodInfo> MethodInfoCache = 
            new Dictionary<string, MethodInfo>();
        private static readonly Dictionary<string, ConstructorInfo> ConstructorInfoCache = 
            new Dictionary<string, ConstructorInfo>();
            
        // 方法参数类型缓存键生成
        private static string GetMethodKey(Type type, string methodName, Type[] parameterTypes)
        {
            if (parameterTypes == null || parameterTypes.Length == 0)
                return $"{type.FullName}.{methodName}";
                
            return $"{type.FullName}.{methodName}({string.Join(",", parameterTypes.Select(t => t.FullName))})";
        }
        
        // 构造函数缓存键生成
        private static string GetConstructorKey(Type type, Type[] parameterTypes)
        {
            if (parameterTypes == null || parameterTypes.Length == 0)
                return $"{type.FullName}..ctor";
                
            return $"{type.FullName}..ctor({string.Join(",", parameterTypes.Select(t => t.FullName))})";
        }
        
        /// <summary>
        /// 获取字段信息，使用缓存提高性能
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <param name="fieldName">字段名称</param>
        /// <param name="bindingFlags">绑定标志，默认查找所有字段</param>
        /// <returns>字段信息对象，如果未找到则返回null</returns>
        public static FieldInfo GetFieldInfo(Type type, string fieldName, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
                
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException(nameof(fieldName));
                
            // 检查缓存
            if (!FieldInfoCache.TryGetValue(type, out var fieldCache))
            {
                fieldCache = new Dictionary<string, FieldInfo>();
                FieldInfoCache[type] = fieldCache;
            }
            
            string cacheKey = $"{fieldName}_{(int)bindingFlags}";
            if (fieldCache.TryGetValue(cacheKey, out var fieldInfo))
                return fieldInfo;
                
            // 缓存未命中，通过反射获取
            fieldInfo = type.GetField(fieldName, bindingFlags);
            fieldCache[cacheKey] = fieldInfo; // 即使为null也缓存，避免重复查找
            
            return fieldInfo;
        }
        
        /// <summary>
        /// 获取属性信息，使用缓存提高性能
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="bindingFlags">绑定标志，默认查找所有属性</param>
        /// <returns>属性信息对象，如果未找到则返回null</returns>
        public static PropertyInfo GetPropertyInfo(Type type, string propertyName, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
                
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException(nameof(propertyName));
                
            // 检查缓存
            if (!PropertyInfoCache.TryGetValue(type, out var propertyCache))
            {
                propertyCache = new Dictionary<string, PropertyInfo>();
                PropertyInfoCache[type] = propertyCache;
            }
            
            string cacheKey = $"{propertyName}_{(int)bindingFlags}";
            if (propertyCache.TryGetValue(cacheKey, out var propertyInfo))
                return propertyInfo;
                
            // 缓存未命中，通过反射获取
            propertyInfo = type.GetProperty(propertyName, bindingFlags);
            propertyCache[cacheKey] = propertyInfo; // 即使为null也缓存，避免重复查找
            
            return propertyInfo;
        }
        
        /// <summary>
        /// 获取方法信息，使用缓存提高性能
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <param name="methodName">方法名称</param>
        /// <param name="parameterTypes">参数类型数组，用于区分重载</param>
        /// <param name="bindingFlags">绑定标志，默认查找所有方法</param>
        /// <returns>方法信息对象，如果未找到则返回null</returns>
        public static MethodInfo GetMethodInfo(Type type, string methodName, Type[] parameterTypes = null, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
                
            if (string.IsNullOrEmpty(methodName))
                throw new ArgumentNullException(nameof(methodName));
                
            // 生成缓存键
            string cacheKey = GetMethodKey(type, methodName, parameterTypes) + $"_{(int)bindingFlags}";
            
            // 检查缓存
            if (MethodInfoCache.TryGetValue(cacheKey, out var methodInfo))
                return methodInfo;
                
            // 缓存未命中，通过反射获取
            if (parameterTypes == null || parameterTypes.Length == 0)
            {
                methodInfo = type.GetMethod(methodName, bindingFlags);
            }
            else
            {
                methodInfo = type.GetMethod(methodName, bindingFlags, null, parameterTypes, null);
            }
            
            MethodInfoCache[cacheKey] = methodInfo; // 即使为null也缓存，避免重复查找
            
            return methodInfo;
        }
        
        /// <summary>
        /// 获取构造函数信息，使用缓存提高性能
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <param name="parameterTypes">参数类型数组，用于区分重载</param>
        /// <param name="bindingFlags">绑定标志，默认查找所有构造函数</param>
        /// <returns>构造函数信息对象，如果未找到则返回null</returns>
        public static ConstructorInfo GetConstructorInfo(Type type, Type[] parameterTypes = null, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
                
            // 生成缓存键
            string cacheKey = GetConstructorKey(type, parameterTypes) + $"_{(int)bindingFlags}";
            
            // 检查缓存
            if (ConstructorInfoCache.TryGetValue(cacheKey, out var constructorInfo))
                return constructorInfo;
                
            // 缓存未命中，通过反射获取
            if (parameterTypes == null || parameterTypes.Length == 0)
            {
                constructorInfo = type.GetConstructor(bindingFlags, null, Type.EmptyTypes, null);
            }
            else
            {
                constructorInfo = type.GetConstructor(bindingFlags, null, parameterTypes, null);
            }
            
            ConstructorInfoCache[cacheKey] = constructorInfo; // 即使为null也缓存，避免重复查找
            
            return constructorInfo;
        }
        
        /// <summary>
        /// 清除所有反射缓存
        /// </summary>
        public static void ClearAllCaches()
        {
            TypeCache.Clear();
            FieldInfoCache.Clear();
            PropertyInfoCache.Clear();
            MethodInfoCache.Clear();
            ConstructorInfoCache.Clear();
            GetterDelegateCache.Clear();
            SetterDelegateCache.Clear();
            MethodDelegateCache.Clear();
            ConstructorDelegateCache.Clear();
        }
        
        #endregion
        
        #region 高性能访问器
        
        // 委托缓存
        private static readonly Dictionary<string, Delegate> GetterDelegateCache = new Dictionary<string, Delegate>();
        private static readonly Dictionary<string, Delegate> SetterDelegateCache = new Dictionary<string, Delegate>();
        private static readonly Dictionary<string, Delegate> MethodDelegateCache = new Dictionary<string, Delegate>();
        private static readonly Dictionary<string, Delegate> ConstructorDelegateCache = new Dictionary<string, Delegate>();
        
        /// <summary>
        /// 创建用于获取属性或字段值的委托
        /// </summary>
        /// <typeparam name="TTarget">目标对象类型</typeparam>
        /// <typeparam name="TResult">属性或字段类型</typeparam>
        /// <param name="memberName">成员名称（属性或字段）</param>
        /// <returns>获取器委托</returns>
        public static Func<TTarget, TResult> CreateGetter<TTarget, TResult>(string memberName)
        {
            Type targetType = typeof(TTarget);
            string cacheKey = $"{targetType.FullName}.{memberName}.getter<{typeof(TResult).FullName}>";
            
            // 检查缓存
            if (GetterDelegateCache.TryGetValue(cacheKey, out var cachedDelegate))
                return (Func<TTarget, TResult>)cachedDelegate;
                
            // 尝试作为属性处理
            PropertyInfo propertyInfo = GetPropertyInfo(targetType, memberName);
            if (propertyInfo != null && propertyInfo.CanRead)
            {
                var getter = CreatePropertyGetter<TTarget, TResult>(propertyInfo);
                GetterDelegateCache[cacheKey] = getter;
                return getter;
            }
            
            // 尝试作为字段处理
            FieldInfo fieldInfo = GetFieldInfo(targetType, memberName);
            if (fieldInfo != null)
            {
                var getter = CreateFieldGetter<TTarget, TResult>(fieldInfo);
                GetterDelegateCache[cacheKey] = getter;
                return getter;
            }
            
            throw new ArgumentException($"未找到属性或字段: {memberName}，或类型不匹配", nameof(memberName));
        }
        
        /// <summary>
        /// 创建用于设置属性或字段值的委托
        /// </summary>
        /// <typeparam name="TTarget">目标对象类型</typeparam>
        /// <typeparam name="TValue">属性或字段类型</typeparam>
        /// <param name="memberName">成员名称（属性或字段）</param>
        /// <returns>设置器委托</returns>
        public static Action<TTarget, TValue> CreateSetter<TTarget, TValue>(string memberName)
        {
            Type targetType = typeof(TTarget);
            string cacheKey = $"{targetType.FullName}.{memberName}.setter<{typeof(TValue).FullName}>";
            
            // 检查缓存
            if (SetterDelegateCache.TryGetValue(cacheKey, out var cachedDelegate))
                return (Action<TTarget, TValue>)cachedDelegate;
                
            // 尝试作为属性处理
            PropertyInfo propertyInfo = GetPropertyInfo(targetType, memberName);
            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                var setter = CreatePropertySetter<TTarget, TValue>(propertyInfo);
                SetterDelegateCache[cacheKey] = setter;
                return setter;
            }
            
            // 尝试作为字段处理
            FieldInfo fieldInfo = GetFieldInfo(targetType, memberName);
            if (fieldInfo != null)
            {
                var setter = CreateFieldSetter<TTarget, TValue>(fieldInfo);
                SetterDelegateCache[cacheKey] = setter;
                return setter;
            }
            
            throw new ArgumentException($"未找到属性或字段: {memberName}，或类型不匹配", nameof(memberName));
        }
        
        /// <summary>
        /// 使用表达式树创建属性获取器
        /// </summary>
        private static Func<TTarget, TResult> CreatePropertyGetter<TTarget, TResult>(PropertyInfo propertyInfo)
        {
            // 参数表达式 target
            ParameterExpression targetExp = Expression.Parameter(typeof(TTarget), "target");
            
            // target.Property 表达式
            MemberExpression property = Expression.Property(targetExp, propertyInfo);
            
            // 构建 lambda: target => (TResult)target.Property
            UnaryExpression castExp = Expression.Convert(property, typeof(TResult));
            Expression<Func<TTarget, TResult>> lambda = Expression.Lambda<Func<TTarget, TResult>>(castExp, targetExp);
            
            // 编译为委托
            return lambda.Compile();
        }
        
        /// <summary>
        /// 使用表达式树创建字段获取器
        /// </summary>
        private static Func<TTarget, TResult> CreateFieldGetter<TTarget, TResult>(FieldInfo fieldInfo)
        {
            // 参数表达式 target
            ParameterExpression targetExp = Expression.Parameter(typeof(TTarget), "target");
            
            // target.Field 表达式
            MemberExpression field = Expression.Field(targetExp, fieldInfo);
            
            // 构建 lambda: target => (TResult)target.Field
            UnaryExpression castExp = Expression.Convert(field, typeof(TResult));
            Expression<Func<TTarget, TResult>> lambda = Expression.Lambda<Func<TTarget, TResult>>(castExp, targetExp);
            
            // 编译为委托
            return lambda.Compile();
        }
        
        /// <summary>
        /// 使用表达式树创建属性设置器
        /// </summary>
        private static Action<TTarget, TValue> CreatePropertySetter<TTarget, TValue>(PropertyInfo propertyInfo)
        {
            // 参数表达式 target, value
            ParameterExpression targetExp = Expression.Parameter(typeof(TTarget), "target");
            ParameterExpression valueExp = Expression.Parameter(typeof(TValue), "value");
            
            // target.Property = value 表达式
            MemberExpression property = Expression.Property(targetExp, propertyInfo);
            UnaryExpression castValueExp = Expression.Convert(valueExp, propertyInfo.PropertyType);
            BinaryExpression assignExp = Expression.Assign(property, castValueExp);
            
            // 构建 lambda: (target, value) => target.Property = (PropertyType)value
            Expression<Action<TTarget, TValue>> lambda = Expression.Lambda<Action<TTarget, TValue>>(assignExp, targetExp, valueExp);
            
            // 编译为委托
            return lambda.Compile();
        }
        
        /// <summary>
        /// 使用表达式树创建字段设置器
        /// </summary>
        private static Action<TTarget, TValue> CreateFieldSetter<TTarget, TValue>(FieldInfo fieldInfo)
        {
            // 参数表达式 target, value
            ParameterExpression targetExp = Expression.Parameter(typeof(TTarget), "target");
            ParameterExpression valueExp = Expression.Parameter(typeof(TValue), "value");
            
            // target.Field = value 表达式
            MemberExpression field = Expression.Field(targetExp, fieldInfo);
            UnaryExpression castValueExp = Expression.Convert(valueExp, fieldInfo.FieldType);
            BinaryExpression assignExp = Expression.Assign(field, castValueExp);
            
            // 构建 lambda: (target, value) => target.Field = (FieldType)value
            Expression<Action<TTarget, TValue>> lambda = Expression.Lambda<Action<TTarget, TValue>>(assignExp, targetExp, valueExp);
            
            // 编译为委托
            return lambda.Compile();
        }
        
        #endregion
        
        #region 实例创建与方法调用
        
        /// <summary>
        /// 动态创建类型实例
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="args">构造函数参数</param>
        /// <returns>新创建的实例</returns>
        public static T CreateInstance<T>(params object[] args)
        {
            return (T)CreateInstance(typeof(T), args);
        }
        
        /// <summary>
        /// 动态创建类型实例
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <param name="args">构造函数参数</param>
        /// <returns>新创建的实例</returns>
        public static object CreateInstance(Type type, params object[] args)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
                
            // 无参构造函数的特殊处理
            if (args == null || args.Length == 0)
            {
                string cacheKey = GetConstructorKey(type, Type.EmptyTypes);
                
                if (ConstructorDelegateCache.TryGetValue(cacheKey, out var cachedDelegate))
                {
                    var factoryFunc = (Func<object>)cachedDelegate;
                    return factoryFunc();
                }
                
                // 使用表达式树创建无参构造函数调用
                NewExpression newExp = Expression.New(type);
                Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(newExp);
                var factory = lambda.Compile();
                
                ConstructorDelegateCache[cacheKey] = factory;
                return factory();
            }
            
            // 有参数构造函数
            Type[] paramTypes = args.Select(arg => arg?.GetType() ?? typeof(object)).ToArray();
            ConstructorInfo ctorInfo = GetConstructorInfo(type, paramTypes);
            
            if (ctorInfo == null)
            {
                // 尝试查找兼容的构造函数
                ctorInfo = type.GetConstructors()
                    .FirstOrDefault(ctor => IsConstructorCompatible(ctor, args));
                    
                if (ctorInfo == null)
                    throw new ArgumentException($"未找到匹配的构造函数，类型: {type.FullName}");
            }
            
            return ctorInfo.Invoke(args);
        }
        
        // 检查构造函数是否与参数兼容
        private static bool IsConstructorCompatible(ConstructorInfo ctor, object[] args)
        {
            if (ctor == null || args == null)
                return false;
                
            ParameterInfo[] parameters = ctor.GetParameters();
            
            if (parameters.Length != args.Length)
                return false;
                
            for (int i = 0; i < parameters.Length; i++)
            {
                Type paramType = parameters[i].ParameterType;
                object arg = args[i];
                
                if (arg == null)
                {
                    if (paramType.IsValueType && !IsNullableType(paramType))
                        return false; // 不能将null赋值给非可空值类型
                }
                else if (!paramType.IsAssignableFrom(arg.GetType()) && !CanConvertValue(arg, paramType))
                {
                    return false; // 参数类型不兼容
                }
            }
            
            return true;
        }
        
        // 检查是否可空类型
        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        
        // 检查值是否可以转换为目标类型
        private static bool CanConvertValue(object value, Type targetType)
        {
            try
            {
                var convertedValue = System.Convert.ChangeType(value, targetType);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// 创建方法调用委托
        /// </summary>
        /// <typeparam name="TDelegate">委托类型</typeparam>
        /// <param name="instance">目标实例（静态方法传null）</param>
        /// <param name="methodInfo">方法信息</param>
        /// <returns>方法调用委托</returns>
        public static TDelegate CreateMethodDelegate<TDelegate>(object instance, MethodInfo methodInfo) where TDelegate : class
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));
                
            if (!typeof(Delegate).IsAssignableFrom(typeof(TDelegate)))
                throw new ArgumentException("TDelegate必须是委托类型", nameof(TDelegate));
                
            string cacheKey = $"{methodInfo.DeclaringType.FullName}.{methodInfo.Name}.{typeof(TDelegate).FullName}.{instance?.GetHashCode() ?? 0}";
            
            // 检查缓存
            if (MethodDelegateCache.TryGetValue(cacheKey, out var cachedDelegate))
                return cachedDelegate as TDelegate;
                
            // 创建委托
            Delegate methodDelegate;
            if (methodInfo.IsStatic)
                methodDelegate = Delegate.CreateDelegate(typeof(TDelegate), methodInfo);
            else
                methodDelegate = Delegate.CreateDelegate(typeof(TDelegate), instance, methodInfo);
                
            MethodDelegateCache[cacheKey] = methodDelegate;
            return methodDelegate as TDelegate;
        }
        
        /// <summary>
        /// 安全调用方法
        /// </summary>
        /// <param name="instance">目标实例</param>
        /// <param name="methodName">方法名称</param>
        /// <param name="args">方法参数</param>
        /// <returns>方法返回值</returns>
        public static object InvokeMethod(object instance, string methodName, params object[] args)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
                
            Type type = instance.GetType();
            Type[] paramTypes = args?.Select(a => a?.GetType() ?? typeof(object)).ToArray() ?? Type.EmptyTypes;
            
            MethodInfo methodInfo = GetMethodInfo(type, methodName, paramTypes);
            
            if (methodInfo == null)
                throw new ArgumentException($"方法未找到: {methodName}", nameof(methodName));
                
            return methodInfo.Invoke(instance, args);
        }
        
        /// <summary>
        /// 安全调用静态方法
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <param name="methodName">方法名称</param>
        /// <param name="args">方法参数</param>
        /// <returns>方法返回值</returns>
        public static object InvokeStaticMethod(Type type, string methodName, params object[] args)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
                
            Type[] paramTypes = args?.Select(a => a?.GetType() ?? typeof(object)).ToArray() ?? Type.EmptyTypes;
            
            MethodInfo methodInfo = GetMethodInfo(type, methodName, paramTypes, BindingFlags.Public | BindingFlags.Static);
            
            if (methodInfo == null)
                throw new ArgumentException($"静态方法未找到: {methodName}", nameof(methodName));
                
            return methodInfo.Invoke(null, args);
        }
        
        #endregion
        
        #region 特性处理
        
        /// <summary>
        /// 获取指定类型上的特性
        /// </summary>
        /// <typeparam name="TAttribute">特性类型</typeparam>
        /// <param name="type">目标类型</param>
        /// <param name="inherit">是否包含继承的特性</param>
        /// <returns>特性实例，如果未找到则返回null</returns>
        public static TAttribute GetAttribute<TAttribute>(Type type, bool inherit = false) where TAttribute : Attribute
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
                
            return (TAttribute)type.GetCustomAttributes(typeof(TAttribute), inherit).FirstOrDefault();
        }
        
        /// <summary>
        /// 获取指定成员上的特性
        /// </summary>
        /// <typeparam name="TAttribute">特性类型</typeparam>
        /// <param name="memberInfo">成员信息</param>
        /// <param name="inherit">是否包含继承的特性</param>
        /// <returns>特性实例，如果未找到则返回null</returns>
        public static TAttribute GetAttribute<TAttribute>(MemberInfo memberInfo, bool inherit = false) where TAttribute : Attribute
        {
            if (memberInfo == null)
                throw new ArgumentNullException(nameof(memberInfo));
                
            return (TAttribute)memberInfo.GetCustomAttributes(typeof(TAttribute), inherit).FirstOrDefault();
        }
        
        /// <summary>
        /// 获取指定类型上的所有特性
        /// </summary>
        /// <typeparam name="TAttribute">特性类型</typeparam>
        /// <param name="type">目标类型</param>
        /// <param name="inherit">是否包含继承的特性</param>
        /// <returns>特性实例集合</returns>
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(Type type, bool inherit = false) where TAttribute : Attribute
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
                
            return type.GetCustomAttributes(typeof(TAttribute), inherit).Cast<TAttribute>();
        }
        
        /// <summary>
        /// 获取指定成员上的所有特性
        /// </summary>
        /// <typeparam name="TAttribute">特性类型</typeparam>
        /// <param name="memberInfo">成员信息</param>
        /// <param name="inherit">是否包含继承的特性</param>
        /// <returns>特性实例集合</returns>
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(MemberInfo memberInfo, bool inherit = false) where TAttribute : Attribute
        {
            if (memberInfo == null)
                throw new ArgumentNullException(nameof(memberInfo));
                
            return memberInfo.GetCustomAttributes(typeof(TAttribute), inherit).Cast<TAttribute>();
        }
        
        /// <summary>
        /// 检查类型是否具有指定特性
        /// </summary>
        /// <typeparam name="TAttribute">特性类型</typeparam>
        /// <param name="type">目标类型</param>
        /// <param name="inherit">是否包含继承的特性</param>
        /// <returns>如果具有特性则返回true，否则返回false</returns>
        public static bool HasAttribute<TAttribute>(Type type, bool inherit = false) where TAttribute : Attribute
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
                
            return type.GetCustomAttributes(typeof(TAttribute), inherit).Any();
        }
        
        /// <summary>
        /// 检查成员是否具有指定特性
        /// </summary>
        /// <typeparam name="TAttribute">特性类型</typeparam>
        /// <param name="memberInfo">成员信息</param>
        /// <param name="inherit">是否包含继承的特性</param>
        /// <returns>如果具有特性则返回true，否则返回false</returns>
        public static bool HasAttribute<TAttribute>(MemberInfo memberInfo, bool inherit = false) where TAttribute : Attribute
        {
            if (memberInfo == null)
                throw new ArgumentNullException(nameof(memberInfo));
                
            return memberInfo.GetCustomAttributes(typeof(TAttribute), inherit).Any();
        }
        
        #endregion
        
        #region 类型转换
        
        /// <summary>
        /// 尝试将值转换为指定类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="value">要转换的值</param>
        /// <param name="result">转换结果</param>
        /// <returns>如果转换成功则返回true，否则返回false</returns>
        public static bool TryConvert<T>(object value, out T result)
        {
            result = default;
            
            if (value == null)
            {
                // 如果目标类型是引用类型或可空值类型，可以接受null
                if (!typeof(T).IsValueType || IsNullableType(typeof(T)))
                {
                    result = default;
                    return true;
                }
                return false;
            }
            
            try
            {
                // 如果值已经是目标类型
                if (value is T typedValue)
                {
                    result = typedValue;
                    return true;
                }
                
                // 如果目标类型是枚举
                if (typeof(T).IsEnum)
                {
                    if (value is string stringValue)
                    {
                        result = (T)Enum.Parse(typeof(T), stringValue, true);
                        return true;
                    }
                    else if (value is IConvertible)
                    {
                        result = (T)Enum.ToObject(typeof(T), value);
                        return true;
                    }
                }
                
                // 使用Convert类进行转换
                if (value is IConvertible)
                {
                    result = (T)System.Convert.ChangeType(value, Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T));
                    return true;
                }
            }
            catch
            {
                return false;
            }
            
            return false;
        }
        
        /// <summary>
        /// 将值转换为指定类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="value">要转换的值</param>
        /// <returns>转换后的值</returns>
        /// <exception cref="InvalidCastException">如果转换失败</exception>
        public static T ConvertTo<T>(object value)
        {
            if (TryConvert<T>(value, out T result))
                return result;
                
            throw new InvalidCastException($"无法将 {value?.GetType().FullName ?? "null"} 转换为 {typeof(T).FullName}");
        }
        
        #endregion
    }
}
