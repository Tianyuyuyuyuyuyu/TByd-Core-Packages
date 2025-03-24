using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Pool;

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
            AttributeCache.Clear();
            AttributesCache.Clear();
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
        
        #region 高性能实例创建

        /// <summary>
        /// 高性能实例创建器，使用IL动态生成实例创建代码
        /// </summary>
        private static class FastActivator<T>
        {
            // 无参构造函数委托缓存
            private static readonly Func<T> CreateInstanceDelegate;
            
            static FastActivator()
            {
                try
                {
                    // 使用表达式树创建无参构造函数调用
                    var type = typeof(T);
                    NewExpression newExp = Expression.New(type);
                    Expression<Func<T>> lambda = Expression.Lambda<Func<T>>(newExp);
                    CreateInstanceDelegate = lambda.Compile();
                }
                catch
                {
                    // 如果表达式树创建失败，使用反射作为后备方案
                    CreateInstanceDelegate = () => (T)Activator.CreateInstance(typeof(T));
                }
            }
            
            /// <summary>
            /// 快速创建类型实例
            /// </summary>
            /// <returns>新创建的类型实例</returns>
            public static T CreateInstance()
            {
                return CreateInstanceDelegate();
            }
        }

        /// <summary>
        /// 提供参数类型转换的辅助方法
        /// </summary>
        private static class ParameterConverter
        {
            // 通用数值类型转换
            public static object ConvertNumericParameter(object value, Type targetType)
            {
                if (value == null)
                    return null;
                    
                Type valueType = value.GetType();
                
                // 如果目标类型是可空类型，获取其基础类型
                if (IsNullableType(targetType))
                    targetType = Nullable.GetUnderlyingType(targetType);
                    
                // 已经是目标类型，直接返回
                if (targetType.IsAssignableFrom(valueType))
                    return value;
                    
                // 使用Convert进行数值类型转换
                if (IsNumericType(targetType) && IsNumericType(valueType))
                {
                    return Convert.ChangeType(value, targetType);
                }
                
                // 特殊类型的处理
                if (targetType == typeof(string))
                    return value.ToString();
                    
                if (targetType == typeof(Guid) && valueType == typeof(string))
                    return new Guid((string)value);
                    
                if (targetType == typeof(DateTime) && valueType == typeof(string))
                    return DateTime.Parse((string)value);
                    
                // 尝试通过TypeConverter进行转换
                try
                {
                    var converter = System.ComponentModel.TypeDescriptor.GetConverter(targetType);
                    if (converter.CanConvertFrom(valueType))
                        return converter.ConvertFrom(value);
                        
                    converter = System.ComponentModel.TypeDescriptor.GetConverter(valueType);
                    if (converter.CanConvertTo(targetType))
                        return converter.ConvertTo(value, targetType);
                }
                catch
                {
                    // 转换失败，尝试下一种方法
                }
                
                // 尝试一般的转换
                try
                {
                    return Convert.ChangeType(value, targetType);
                }
                catch
                {
                    // 无法转换，返回原值
                    return value;
                }
            }
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
            // 无参数构造函数使用FastActivator优化
            if (args == null || args.Length == 0)
            {
                return FastActivator<T>.CreateInstance();
            }
            
            // 有参数构造函数使用通用CreateInstance
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
                try 
                {
                    NewExpression newExp = Expression.New(type);
                    Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(newExp);
                    var factory = lambda.Compile();
                    
                    ConstructorDelegateCache[cacheKey] = factory;
                    return factory();
                }
                catch (ArgumentException)
                {
                    // 类型可能没有无参构造函数或者是抽象类型，使用Activator作为后备
                    return Activator.CreateInstance(type);
                }
            }
            
            // 有参数构造函数
            // 避免每次调用时创建新数组，减少GC分配
            Type[] paramTypes = new Type[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                paramTypes[i] = args[i]?.GetType() ?? typeof(object);
            }
            
            string ctorCacheKey = GetConstructorKey(type, paramTypes);
            
            // 检查是否已缓存委托
            if (ConstructorDelegateCache.TryGetValue(ctorCacheKey, out var cachedCtorDelegate))
            {
                try
                {
                    // 根据参数数量动态调用正确的委托类型
                    switch (args.Length)
                    {
                        case 1:
                            var func1 = (Func<object, object>)cachedCtorDelegate;
                            return func1(args[0]);
                        case 2:
                            var func2 = (Func<object, object, object>)cachedCtorDelegate;
                            return func2(args[0], args[1]);
                        case 3:
                            var func3 = (Func<object, object, object, object>)cachedCtorDelegate;
                            return func3(args[0], args[1], args[2]);
                        case 4:
                            var func4 = (Func<object, object, object, object, object>)cachedCtorDelegate;
                            return func4(args[0], args[1], args[2], args[3]);
                        default:
                            // 参数太多，回退到反射方式
                            break;
                    }
                }
                catch
                {
                    // 委托调用失败，回退到反射方式
                }
            }
            
            // 获取构造函数信息
            ConstructorInfo ctorInfo = GetConstructorInfo(type, paramTypes);
            
            // 如果未找到精确匹配的构造函数，尝试查找兼容的构造函数
            if (ctorInfo == null)
            {
                // 尝试查找兼容的构造函数
                var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (constructors.Length == 0)
                {
                    // 没有找到任何构造函数，尝试使用Activator
                    try
                    {
                        return Activator.CreateInstance(type, args);
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException($"无法创建类型实例: {type.FullName}, 错误: {ex.Message}", ex);
                    }
                }
                
                // 查找参数数量匹配的构造函数
                List<ConstructorInfo> candidateCtors = new List<ConstructorInfo>();
                foreach (var ctor in constructors)
                {
                    if (ctor.GetParameters().Length == args.Length)
                    {
                        candidateCtors.Add(ctor);
                    }
                }
                
                // 如果找到多个候选构造函数，尝试找到最匹配的一个
                if (candidateCtors.Count > 0)
                {
                    foreach (var ctor in candidateCtors)
                    {
                        if (IsConstructorCompatible(ctor, args))
                        {
                            ctorInfo = ctor;
                            break;
                        }
                    }
                }
                
                // 仍然没有找到匹配的构造函数
                if (ctorInfo == null)
                {
                    throw new ArgumentException($"未找到匹配的构造函数，类型: {type.FullName}");
                }
            }
            
            // 预处理参数，尝试类型转换
            ParameterInfo[] parameters = ctorInfo.GetParameters();
            object[] convertedArgs = new object[args.Length];
            
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == null)
                {
                    convertedArgs[i] = null;
                    continue;
                }
                
                Type paramType = parameters[i].ParameterType;
                Type argType = args[i].GetType();
                
                // 如果类型已经兼容，直接使用
                if (paramType.IsAssignableFrom(argType))
                {
                    convertedArgs[i] = args[i];
                    continue;
                }
                
                // 尝试转换参数类型
                convertedArgs[i] = ParameterConverter.ConvertNumericParameter(args[i], paramType);
            }
            
            // 如果参数数量较少(1-4)，创建并缓存特定的委托
            if (args.Length <= 4)
            {
                try
                {
                    Delegate ctorDelegate = CreateConstructorDelegate(ctorInfo, args.Length);
                    if (ctorDelegate != null)
                    {
                        ConstructorDelegateCache[ctorCacheKey] = ctorDelegate;
                        
                        // 通过委托调用构造函数
                        switch (args.Length)
                        {
                            case 1:
                                var func1 = (Func<object, object>)ctorDelegate;
                                return func1(convertedArgs[0]);
                            case 2:
                                var func2 = (Func<object, object, object>)ctorDelegate;
                                return func2(convertedArgs[0], convertedArgs[1]);
                            case 3:
                                var func3 = (Func<object, object, object, object>)ctorDelegate;
                                return func3(convertedArgs[0], convertedArgs[1], convertedArgs[2]);
                            case 4:
                                var func4 = (Func<object, object, object, object, object>)ctorDelegate;
                                return func4(convertedArgs[0], convertedArgs[1], convertedArgs[2], convertedArgs[3]);
                        }
                    }
                }
                catch
                {
                    // 创建委托失败，回退到反射方式
                }
            }
            
            // 回退到直接调用，但使用转换后的参数
            return ctorInfo.Invoke(convertedArgs);
        }
        
        // 创建构造函数委托
        private static Delegate CreateConstructorDelegate(ConstructorInfo ctorInfo, int paramCount)
        {
            if (ctorInfo == null)
                return null;
                
            ParameterInfo[] parameters = ctorInfo.GetParameters();
            Type returnType = ctorInfo.DeclaringType;
            
            // 参数表达式列表
            var paramExpressions = new ParameterExpression[paramCount];
            for (int i = 0; i < paramCount; i++)
            {
                // 所有参数都使用object类型，在调用时进行转换
                paramExpressions[i] = Expression.Parameter(typeof(object), $"arg{i}");
            }
            
            // 转换后的参数列表，用于构造函数调用
            var convertedArgs = new Expression[paramCount];
            for (int i = 0; i < paramCount; i++)
            {
                Type paramType = parameters[i].ParameterType;
                // 创建转换表达式，包括处理可空类型
                if (IsNullableType(paramType))
                {
                    Type underlyingType = Nullable.GetUnderlyingType(paramType);
                    var nullCheck = Expression.Equal(paramExpressions[i], Expression.Constant(null));
                    var convertedValue = Expression.Convert(paramExpressions[i], underlyingType);
                    var nullableNew = Expression.New(
                        paramType.GetConstructor(new[] { underlyingType }),
                        convertedValue);
                    convertedArgs[i] = Expression.Condition(
                        nullCheck,
                        Expression.Constant(null, paramType),
                        nullableNew);
                }
                else
                {
                    // 普通类型直接转换
                    convertedArgs[i] = Expression.Convert(paramExpressions[i], paramType);
                }
            }
            
            // 创建构造函数调用表达式
            NewExpression newExp = Expression.New(ctorInfo, convertedArgs);
            // 确保返回类型为object
            UnaryExpression convertResult = Expression.Convert(newExp, typeof(object));
            
            // 根据参数数量创建不同的委托类型
            Type delegateType;
            switch (paramCount)
            {
                case 1:
                    delegateType = typeof(Func<object, object>);
                    break;
                case 2:
                    delegateType = typeof(Func<object, object, object>);
                    break;
                case 3:
                    delegateType = typeof(Func<object, object, object, object>);
                    break;
                case 4:
                    delegateType = typeof(Func<object, object, object, object, object>);
                    break;
                default:
                    return null; // 不支持的参数数量
            }
            
            // 创建Lambda表达式并编译为委托
            LambdaExpression lambda = Expression.Lambda(delegateType, convertResult, paramExpressions);
            return lambda.Compile();
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
                    // 不能将null赋值给非可空值类型
                    if (paramType.IsValueType && !IsNullableType(paramType))
                        return false;
                }
                else
                {
                    Type argType = arg.GetType();
                    // 类型直接兼容
                    if (paramType.IsAssignableFrom(argType))
                        continue;
                        
                    // 检查数值类型之间的转换兼容性
                    if (IsNumericType(paramType) && IsNumericType(argType))
                        continue;
                        
                    // 检查其他转换兼容性
                    if (!CanConvertValue(arg, paramType))
                        return false;
                }
            }
            
            return true;
        }
        
        // 检查是否数值类型
        private static bool IsNumericType(Type type)
        {
            if (type == null)
                return false;
                
            // 处理可空类型
            if (IsNullableType(type))
                type = Nullable.GetUnderlyingType(type);
                
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
                default:
                    return false;
            }
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
            
            // 避免使用LINQ创建数组，减少GC分配
            Type[] paramTypes = null;
            if (args != null && args.Length > 0)
            {
                paramTypes = new Type[args.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    paramTypes[i] = args[i]?.GetType() ?? typeof(object);
                }
            }
            else
            {
                paramTypes = Type.EmptyTypes;
            }
            
            // 方法调用缓存键
            string methodCacheKey = GetMethodKey(type, methodName, paramTypes) + "_invoke";
            
            // 检查是否已缓存委托
            if (MethodDelegateCache.TryGetValue(methodCacheKey, out var cachedDelegate))
            {
                try
                {
                    // 根据参数数量调用对应的委托
                    switch (args?.Length ?? 0)
                    {
                        case 0:
                            return ((Func<object, object>)cachedDelegate)(instance);
                        case 1:
                            return ((Func<object, object, object>)cachedDelegate)(instance, args[0]);
                        case 2:
                            return ((Func<object, object, object, object>)cachedDelegate)(instance, args[0], args[1]);
                        case 3:
                            return ((Func<object, object, object, object, object>)cachedDelegate)(instance, args[0], args[1], args[2]);
                        case 4:
                            return ((Func<object, object, object, object, object, object>)cachedDelegate)(instance, args[0], args[1], args[2], args[3]);
                        default:
                            // 参数过多，使用DynamicMethod
                            break;
                    }
                }
                catch
                {
                    // 委托调用失败，回退到反射
                }
            }
            
            // 获取方法信息
            MethodInfo methodInfo = GetMethodInfo(type, methodName, paramTypes);
            
            if (methodInfo == null)
                throw new ArgumentException($"方法未找到: {methodName}", nameof(methodName));
            
            // 如果是简单调用（参数少于5个），创建并缓存委托
            if ((args?.Length ?? 0) <= 4)
            {
                try
                {
                    // 创建方法调用委托
                    Delegate methodDelegate = CreateInstanceMethodDelegate(methodInfo, args?.Length ?? 0);
                    
                    if (methodDelegate != null)
                    {
                        // 缓存委托
                        MethodDelegateCache[methodCacheKey] = methodDelegate;
                        
                        // 通过委托调用方法
                        switch (args?.Length ?? 0)
                        {
                            case 0:
                                return ((Func<object, object>)methodDelegate)(instance);
                            case 1:
                                return ((Func<object, object, object>)methodDelegate)(instance, args[0]);
                            case 2:
                                return ((Func<object, object, object, object>)methodDelegate)(instance, args[0], args[1]);
                            case 3:
                                return ((Func<object, object, object, object, object>)methodDelegate)(instance, args[0], args[1], args[2]);
                            case 4:
                                return ((Func<object, object, object, object, object, object>)methodDelegate)(instance, args[0], args[1], args[2], args[3]);
                        }
                    }
                }
                catch
                {
                    // 创建委托失败，回退到反射
                }
            }
            else
            {
                // 多参数方法，使用DynamicMethod优化
                try
                {
                    return DynamicMethodExecutor.Execute(methodInfo, instance, args);
                }
                catch
                {
                    // 如果DynamicMethod执行失败，回退到反射方式
                }
            }
            
            // 回退到直接调用
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
            
            // 避免使用LINQ创建数组，减少GC分配
            Type[] paramTypes = null;
            if (args != null && args.Length > 0)
            {
                paramTypes = new Type[args.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    paramTypes[i] = args[i]?.GetType() ?? typeof(object);
                }
            }
            else
            {
                paramTypes = Type.EmptyTypes;
            }
            
            // 方法调用缓存键
            string methodCacheKey = GetMethodKey(type, methodName, paramTypes) + "_static_invoke";
            
            // 检查是否已缓存委托
            if (MethodDelegateCache.TryGetValue(methodCacheKey, out var cachedDelegate))
            {
                try
                {
                    // 根据参数数量调用对应的委托
                    switch (args?.Length ?? 0)
                    {
                        case 0:
                            return ((Func<object>)cachedDelegate)();
                        case 1:
                            return ((Func<object, object>)cachedDelegate)(args[0]);
                        case 2:
                            return ((Func<object, object, object>)cachedDelegate)(args[0], args[1]);
                        case 3:
                            return ((Func<object, object, object, object>)cachedDelegate)(args[0], args[1], args[2]);
                        case 4:
                            return ((Func<object, object, object, object, object>)cachedDelegate)(args[0], args[1], args[2], args[3]);
                        default:
                            // 参数过多，使用DynamicMethod
                            break;
                    }
                }
                catch
                {
                    // 委托调用失败，回退到反射
                }
            }
            
            // 获取方法信息
            MethodInfo methodInfo = GetMethodInfo(type, methodName, paramTypes, BindingFlags.Public | BindingFlags.Static);
            
            if (methodInfo == null)
                throw new ArgumentException($"静态方法未找到: {methodName}", nameof(methodName));
            
            // 如果是简单调用（参数少于5个），创建并缓存委托
            if ((args?.Length ?? 0) <= 4)
            {
                try
                {
                    // 创建静态方法调用委托
                    Delegate methodDelegate = CreateStaticMethodDelegate(methodInfo, args?.Length ?? 0);
                    
                    if (methodDelegate != null)
                    {
                        // 缓存委托
                        MethodDelegateCache[methodCacheKey] = methodDelegate;
                        
                        // 通过委托调用方法
                        switch (args?.Length ?? 0)
                        {
                            case 0:
                                return ((Func<object>)methodDelegate)();
                            case 1:
                                return ((Func<object, object>)methodDelegate)(args[0]);
                            case 2:
                                return ((Func<object, object, object>)methodDelegate)(args[0], args[1]);
                            case 3:
                                return ((Func<object, object, object, object>)methodDelegate)(args[0], args[1], args[2]);
                            case 4:
                                return ((Func<object, object, object, object, object>)methodDelegate)(args[0], args[1], args[2], args[3]);
                        }
                    }
                }
                catch
                {
                    // 创建委托失败，回退到反射
                }
            }
            else
            {
                // 多参数方法，使用DynamicMethod优化
                try
                {
                    return DynamicMethodExecutor.Execute(methodInfo, null, args);
                }
                catch
                {
                    // 如果DynamicMethod执行失败，回退到反射方式
                }
            }
            
            // 回退到直接调用
            return methodInfo.Invoke(null, args);
        }
        
        // 创建实例方法调用委托
        private static Delegate CreateInstanceMethodDelegate(MethodInfo methodInfo, int paramCount)
        {
            if (methodInfo == null)
                return null;
            
            Type instanceType = methodInfo.DeclaringType;
            Type returnType = methodInfo.ReturnType;
            ParameterInfo[] parameters = methodInfo.GetParameters();
            
            // 实例参数
            var instanceParam = Expression.Parameter(typeof(object), "instance");
            // 实例转换为正确类型
            var convertedInstance = Expression.Convert(instanceParam, instanceType);
            
            // 方法参数
            var paramExpressions = new ParameterExpression[paramCount];
            var convertedArgs = new Expression[paramCount];
            
            for (int i = 0; i < paramCount; i++)
            {
                // 所有参数都使用object类型
                paramExpressions[i] = Expression.Parameter(typeof(object), $"arg{i}");
                
                // 将参数转换为方法参数类型
                Type paramType = parameters[i].ParameterType;
                convertedArgs[i] = Expression.Convert(paramExpressions[i], paramType);
            }
            
            // 创建方法调用表达式
            MethodCallExpression callExp = Expression.Call(convertedInstance, methodInfo, convertedArgs);
            
            // 处理返回值
            Expression returnExp;
            if (returnType == typeof(void))
            {
                // void方法返回null
                returnExp = Expression.Block(callExp, Expression.Constant(null, typeof(object)));
            }
            else
            {
                // 将返回值转换为object
                returnExp = Expression.Convert(callExp, typeof(object));
            }
            
            // 创建委托类型
            Type delegateType;
            
            // 根据参数数量选择对应的委托类型
            switch (paramCount)
            {
                case 0:
                    delegateType = typeof(Func<object, object>);
                    break;
                case 1:
                    delegateType = typeof(Func<object, object, object>);
                    break;
                case 2:
                    delegateType = typeof(Func<object, object, object, object>);
                    break;
                case 3:
                    delegateType = typeof(Func<object, object, object, object, object>);
                    break;
                case 4:
                    delegateType = typeof(Func<object, object, object, object, object, object>);
                    break;
                default:
                    return null; // 不支持的参数数量
            }
            
            // 创建并编译表达式树
            var allParams = new ParameterExpression[paramCount + 1];
            allParams[0] = instanceParam;
            Array.Copy(paramExpressions, 0, allParams, 1, paramCount);
            
            LambdaExpression lambda = Expression.Lambda(delegateType, returnExp, allParams);
            return lambda.Compile();
        }
        
        // 创建静态方法调用委托
        private static Delegate CreateStaticMethodDelegate(MethodInfo methodInfo, int paramCount)
        {
            if (methodInfo == null)
                return null;
            
            Type returnType = methodInfo.ReturnType;
            ParameterInfo[] parameters = methodInfo.GetParameters();
            
            // 方法参数
            var paramExpressions = new ParameterExpression[paramCount];
            var convertedArgs = new Expression[paramCount];
            
            for (int i = 0; i < paramCount; i++)
            {
                // 所有参数都使用object类型
                paramExpressions[i] = Expression.Parameter(typeof(object), $"arg{i}");
                
                // 将参数转换为方法参数类型
                Type paramType = parameters[i].ParameterType;
                convertedArgs[i] = Expression.Convert(paramExpressions[i], paramType);
            }
            
            // 创建方法调用表达式
            MethodCallExpression callExp = Expression.Call(methodInfo, convertedArgs);
            
            // 处理返回值
            Expression returnExp;
            if (returnType == typeof(void))
            {
                // void方法返回null
                returnExp = Expression.Block(callExp, Expression.Constant(null, typeof(object)));
            }
            else
            {
                // 将返回值转换为object
                returnExp = Expression.Convert(callExp, typeof(object));
            }
            
            // 创建委托类型
            Type delegateType;
            
            // 根据参数数量选择对应的委托类型
            switch (paramCount)
            {
                case 0:
                    delegateType = typeof(Func<object>);
                    break;
                case 1:
                    delegateType = typeof(Func<object, object>);
                    break;
                case 2:
                    delegateType = typeof(Func<object, object, object>);
                    break;
                case 3:
                    delegateType = typeof(Func<object, object, object, object>);
                    break;
                case 4:
                    delegateType = typeof(Func<object, object, object, object, object>);
                    break;
                default:
                    return null; // 不支持的参数数量
            }
            
            // 创建并编译表达式树
            LambdaExpression lambda = Expression.Lambda(delegateType, returnExp, paramExpressions);
            return lambda.Compile();
        }
        
        #endregion
        
        #region 特性处理
        
        // 特性缓存
        private static readonly Dictionary<string, Attribute> AttributeCache = new Dictionary<string, Attribute>();
        private static readonly Dictionary<string, Attribute[]> AttributesCache = new Dictionary<string, Attribute[]>();

        // 获取特性缓存键
        private static string GetAttributeCacheKey(MemberInfo memberInfo, Type attributeType, bool inherit)
        {
            return $"{memberInfo.DeclaringType?.FullName ?? "global"}.{memberInfo.Name}_{attributeType.FullName}_{inherit}";
        }

        // 获取多特性缓存键
        private static string GetAttributesCacheKey(MemberInfo memberInfo, Type attributeType, bool inherit)
        {
            return $"{memberInfo.DeclaringType?.FullName ?? "global"}.{memberInfo.Name}_{attributeType.FullName}_{inherit}_multiple";
        }

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
                
            // 检查缓存
            string cacheKey = GetAttributeCacheKey(type, typeof(TAttribute), inherit);
            
            if (AttributeCache.TryGetValue(cacheKey, out Attribute cachedAttribute))
            {
                return (TAttribute)cachedAttribute;
            }
            
            // 直接获取特性，避免LINQ
            var attributes = type.GetCustomAttributes(typeof(TAttribute), inherit);
            TAttribute attribute = attributes.Length > 0 ? (TAttribute)attributes[0] : null;
            
            // 缓存结果，即使为null
            AttributeCache[cacheKey] = attribute;
            
            return attribute;
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
                
            // 检查缓存
            string cacheKey = GetAttributeCacheKey(memberInfo, typeof(TAttribute), inherit);
            
            if (AttributeCache.TryGetValue(cacheKey, out Attribute cachedAttribute))
            {
                return (TAttribute)cachedAttribute;
            }
            
            // 直接获取特性，避免LINQ
            var attributes = memberInfo.GetCustomAttributes(typeof(TAttribute), inherit);
            TAttribute attribute = attributes.Length > 0 ? (TAttribute)attributes[0] : null;
            
            // 缓存结果，即使为null
            AttributeCache[cacheKey] = attribute;
            
            return attribute;
        }
        
        /// <summary>
        /// 获取指定类型上的所有特性
        /// </summary>
        /// <typeparam name="TAttribute">特性类型</typeparam>
        /// <param name="type">目标类型</param>
        /// <param name="inherit">是否包含继承的特性</param>
        /// <returns>特性实例集合</returns>
        public static TAttribute[] GetAttributes<TAttribute>(Type type, bool inherit = false) where TAttribute : Attribute
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
                
            // 检查缓存
            string cacheKey = GetAttributesCacheKey(type, typeof(TAttribute), inherit);
            
            if (AttributesCache.TryGetValue(cacheKey, out Attribute[] cachedAttributes))
            {
                return cachedAttributes as TAttribute[];
            }
            
            // 使用自定义转换避免LINQ的Cast<T>()操作
            var attributes = type.GetCustomAttributes(typeof(TAttribute), inherit);
            TAttribute[] typedAttributes = new TAttribute[attributes.Length];
            
            for (int i = 0; i < attributes.Length; i++)
            {
                typedAttributes[i] = (TAttribute)attributes[i];
            }
            
            // 缓存结果
            AttributesCache[cacheKey] = typedAttributes;
            
            return typedAttributes;
        }
        
        /// <summary>
        /// 获取指定成员上的所有特性
        /// </summary>
        /// <typeparam name="TAttribute">特性类型</typeparam>
        /// <param name="memberInfo">成员信息</param>
        /// <param name="inherit">是否包含继承的特性</param>
        /// <returns>特性实例集合</returns>
        public static TAttribute[] GetAttributes<TAttribute>(MemberInfo memberInfo, bool inherit = false) where TAttribute : Attribute
        {
            if (memberInfo == null)
                throw new ArgumentNullException(nameof(memberInfo));
                
            // 检查缓存
            string cacheKey = GetAttributesCacheKey(memberInfo, typeof(TAttribute), inherit);
            
            if (AttributesCache.TryGetValue(cacheKey, out Attribute[] cachedAttributes))
            {
                return cachedAttributes as TAttribute[];
            }
            
            // 使用自定义转换避免LINQ的Cast<T>()操作
            var attributes = memberInfo.GetCustomAttributes(typeof(TAttribute), inherit);
            TAttribute[] typedAttributes = new TAttribute[attributes.Length];
            
            for (int i = 0; i < attributes.Length; i++)
            {
                typedAttributes[i] = (TAttribute)attributes[i];
            }
            
            // 缓存结果
            AttributesCache[cacheKey] = typedAttributes;
            
            return typedAttributes;
        }
        
        /// <summary>
        /// 获取多种类型的特性
        /// </summary>
        /// <param name="memberInfo">成员信息</param>
        /// <param name="inherit">是否包含继承的特性</param>
        /// <returns>特性数组</returns>
        public static Attribute[] GetAttributes(MemberInfo memberInfo, bool inherit = false)
        {
            if (memberInfo == null)
                throw new ArgumentNullException(nameof(memberInfo));
            
            // 缓存键
            string cacheKey = $"{memberInfo.DeclaringType?.FullName ?? "global"}.{memberInfo.Name}_all_{inherit}";
            
            if (AttributesCache.TryGetValue(cacheKey, out Attribute[] cachedAttributes))
            {
                return cachedAttributes;
            }
            
            var attributes = memberInfo.GetCustomAttributes(inherit);
            var attributeArray = new Attribute[attributes.Length];
            
            // 转换为Attribute[]
            for (int i = 0; i < attributes.Length; i++)
            {
                attributeArray[i] = (Attribute)attributes[i];
            }
            
            // 缓存结果
            AttributesCache[cacheKey] = attributeArray;
            
            return attributeArray;
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
            return GetAttribute<TAttribute>(type, inherit) != null;
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
            return GetAttribute<TAttribute>(memberInfo, inherit) != null;
        }
        
        /// <summary>
        /// 清除特性缓存
        /// </summary>
        public static void ClearAttributeCache()
        {
            AttributeCache.Clear();
            AttributesCache.Clear();
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

        #region 预热机制

        /// <summary>
        /// 预热反射工具类，减少首次调用的性能开销
        /// </summary>
        /// <remarks>
        /// 建议在应用启动时调用此方法，以减少首次使用反射API时的延迟
        /// </remarks>
        public static void Warmup()
        {
            // 创建一个简单类型用于预热
            var warmupType = typeof(List<object>);
            var warmupInstance = new List<object>();
            
            try
            {
                // 预热类型查询
                GetType(warmupType.FullName);
                
                // 预热成员查询（修复方法名）
                warmupType.GetFields();  // 代替不存在的GetFieldNames
                GetPropertyNames(warmupType);
                GetMethodNames(warmupType);
                
                // 预热实例创建
                CreateInstance<List<object>>();
                CreateInstance<Dictionary<string, object>>(new object[] { 10 });
                
                // 预热字段和属性访问（修复方法名）
                var countProp = GetPropertyInfo(warmupType, "Count");
                var itemsProp = GetPropertyInfo(warmupType, "Item");
                countProp?.GetValue(warmupInstance);  // 代替不存在的GetPropertyValue
                var capacityProp = GetPropertyInfo(warmupType, "Capacity");
                capacityProp?.SetValue(warmupInstance, 20);  // 代替不存在的SetPropertyValue
                
                // 预热方法调用
                InvokeMethod(warmupInstance, "Add", new object[] { "WarmupItem" });
                InvokeMethod(warmupInstance, "Clear", null);
                InvokeStaticMethod(typeof(Math), "Max", new object[] { 1, 2 });
                
                // 预热特性处理
                GetAttribute<SerializableAttribute>(typeof(List<>));
                HasAttribute<ObsoleteAttribute>(typeof(List<>));
                
                // 预热委托创建
                var method = warmupType.GetMethod("Contains");
                CreateMethodDelegate<Func<List<object>, object, bool>>(warmupInstance, method);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"反射工具预热过程出现异常，这不会影响正常使用: {ex.Message}");
            }
            
            // 指示已完成预热
            Debug.Log("ReflectionUtils预热完成，首次调用性能将得到提升");
        }

        #endregion

        #region 多参数方法调用优化

        /// <summary>
        /// 使用DynamicMethod进行高性能方法调用
        /// </summary>
        private static class DynamicMethodExecutor
        {
            // 缓存大于4个参数的动态方法调用器
            private static readonly Dictionary<string, DynamicMethodDelegate> DynamicMethodCache = 
                new Dictionary<string, DynamicMethodDelegate>();
            
            // 动态方法委托类型
            public delegate object DynamicMethodDelegate(object target, object[] args);
            
            /// <summary>
            /// 创建或获取动态方法调用委托
            /// </summary>
            /// <param name="methodInfo">方法信息</param>
            /// <returns>动态方法委托</returns>
            public static DynamicMethodDelegate CreateExecutor(MethodInfo methodInfo)
            {
                if (methodInfo == null)
                    throw new ArgumentNullException(nameof(methodInfo));
                    
                string cacheKey = GetMethodCacheKey(methodInfo);
                
                if (DynamicMethodCache.TryGetValue(cacheKey, out var executor))
                    return executor;
                    
                lock (DynamicMethodCache)
                {
                    if (DynamicMethodCache.TryGetValue(cacheKey, out executor))
                        return executor;
                        
                    // 使用System.Reflection.Emit.DynamicMethod创建动态方法
                    var dynamicMethod = new System.Reflection.Emit.DynamicMethod(
                        $"Execute_{methodInfo.Name}",
                        typeof(object),
                        new[] { typeof(object), typeof(object[]) },
                        typeof(DynamicMethodExecutor).Module,
                        true);
                        
                    var il = dynamicMethod.GetILGenerator();
                    var parameterInfos = methodInfo.GetParameters();
                    var paramTypes = new Type[parameterInfos.Length];
                    var locals = new System.Reflection.Emit.LocalBuilder[parameterInfos.Length];
                    
                    // 加载参数
                    for (int i = 0; i < parameterInfos.Length; i++)
                    {
                        paramTypes[i] = parameterInfos[i].ParameterType;
                        locals[i] = il.DeclareLocal(paramTypes[i]);
                        
                        il.Emit(System.Reflection.Emit.OpCodes.Ldarg_1); // 加载args数组
                        il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, i); // 加载索引i
                        il.Emit(System.Reflection.Emit.OpCodes.Ldelem_Ref); // 获取args[i]
                        
                        // 转换参数类型
                        EmitCastToReference(il, paramTypes[i]);
                        
                        il.Emit(System.Reflection.Emit.OpCodes.Stloc, locals[i]); // 存储到局部变量
                    }
                    
                    // 处理实例方法和静态方法
                    if (!methodInfo.IsStatic)
                    {
                        il.Emit(System.Reflection.Emit.OpCodes.Ldarg_0); // 加载target实例
                        EmitCastToReference(il, methodInfo.DeclaringType); // 转换类型
                    }
                    
                    // 加载所有参数
                    for (int i = 0; i < parameterInfos.Length; i++)
                    {
                        il.Emit(System.Reflection.Emit.OpCodes.Ldloc, locals[i]); // 加载局部变量
                    }
                    
                    // 调用方法
                    il.EmitCall(
                        methodInfo.IsStatic ? System.Reflection.Emit.OpCodes.Call : System.Reflection.Emit.OpCodes.Callvirt,
                        methodInfo,
                        null);
                        
                    // 处理返回值
                    if (methodInfo.ReturnType == typeof(void))
                    {
                        il.Emit(System.Reflection.Emit.OpCodes.Ldnull); // 返回null
                    }
                    else
                    {
                        // 如果返回类型是值类型，需要装箱
                        if (methodInfo.ReturnType.IsValueType)
                        {
                            il.Emit(System.Reflection.Emit.OpCodes.Box, methodInfo.ReturnType);
                        }
                    }
                    
                    il.Emit(System.Reflection.Emit.OpCodes.Ret);
                    
                    executor = (DynamicMethodDelegate)dynamicMethod.CreateDelegate(typeof(DynamicMethodDelegate));
                    DynamicMethodCache[cacheKey] = executor;
                    
                    return executor;
                }
            }
            
            // 生成IL代码，将堆栈上的对象转换为指定类型
            private static void EmitCastToReference(System.Reflection.Emit.ILGenerator il, Type type)
            {
                if (type.IsValueType)
                {
                    il.Emit(System.Reflection.Emit.OpCodes.Unbox_Any, type);
                }
                else
                {
                    il.Emit(System.Reflection.Emit.OpCodes.Castclass, type);
                }
            }
            
            // 生成方法缓存键
            private static string GetMethodCacheKey(MethodInfo methodInfo)
            {
                var sb = new System.Text.StringBuilder();
                sb.Append(methodInfo.DeclaringType.FullName).Append('.');
                sb.Append(methodInfo.Name);
                
                // 添加参数类型
                sb.Append('(');
                var parameters = methodInfo.GetParameters();
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (i > 0) sb.Append(',');
                    sb.Append(parameters[i].ParameterType.FullName);
                }
                sb.Append(')');
                
                return sb.ToString();
            }
            
            /// <summary>
            /// 执行动态方法
            /// </summary>
            /// <param name="methodInfo">方法信息</param>
            /// <param name="target">目标实例，静态方法为null</param>
            /// <param name="args">方法参数</param>
            /// <returns>方法返回值</returns>
            public static object Execute(MethodInfo methodInfo, object target, params object[] args)
            {
                var executor = CreateExecutor(methodInfo);
                return executor(target, args);
            }
        }

        #endregion
    }
}
