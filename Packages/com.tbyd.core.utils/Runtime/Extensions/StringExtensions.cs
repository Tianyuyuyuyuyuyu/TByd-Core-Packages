using System;
using System.Text;
using UnityEngine;
using System.Buffers;
using UnityEngine.Pool;

namespace TByd.Core.Utils.Runtime.Extensions
{
    /// <summary>
    /// 字符串的扩展方法集合
    /// </summary>
    /// <remarks>
    /// 这个类提供了一系列实用的字符串扩展方法，简化了常见的字符串操作，
    /// 如格式化、转换、验证等。
    /// </remarks>
    public static class StringExtensions
    {
        // StringBuilder对象池，使用Unity原生对象池
        private static readonly ObjectPool<StringBuilder> StringBuilderPool = new ObjectPool<StringBuilder>(
            createFunc: () => new StringBuilder(256),
            actionOnGet: sb => sb.Clear(),
            actionOnRelease: sb => sb.Clear(),
            actionOnDestroy: null,
            collectionCheck: false,
            defaultCapacity: 16,
            maxSize: 32
        );

        /// <summary>
        /// 检查字符串是否为空或仅包含空白字符
        /// </summary>
        /// <param name="str">要检查的字符串</param>
        /// <returns>如果字符串为null、空或仅包含空白字符，则返回true；否则返回false</returns>
        /// <remarks>
        /// 此方法是对StringUtils.IsNullOrWhiteSpace的扩展方法包装。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// string input = "   ";
        /// bool isEmpty = input.IsNullOrWhiteSpace(); // 返回 true
        /// </code>
        /// </remarks>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return StringUtils.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// 将字符串转换为URL友好的slug格式
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <returns>转换后的slug字符串</returns>
        /// <remarks>
        /// 此方法是对StringUtils.ToSlug的扩展方法包装。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// string title = "Hello World 2023!";
        /// string slug = title.ToSlug(); // 返回 "hello-world-2023"
        /// </code>
        /// </remarks>
        public static string ToSlug(this string str)
        {
            return StringUtils.ToSlug(str);
        }

        /// <summary>
        /// 截断字符串到指定长度，添加省略号
        /// </summary>
        /// <param name="str">要截断的字符串</param>
        /// <param name="maxLength">最大长度</param>
        /// <param name="suffix">省略号后缀，默认为"..."</param>
        /// <returns>截断后的字符串</returns>
        /// <remarks>
        /// 此方法是对StringUtils.Truncate的扩展方法包装。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// string description = "This is a very long description that needs to be truncated";
        /// string truncated = description.Truncate(20); // 返回 "This is a very long..."
        /// </code>
        /// </remarks>
        public static string Truncate(this string str, int maxLength, string suffix = "...")
        {
            return StringUtils.Truncate(str, maxLength, suffix);
        }

        /// <summary>
        /// 将字符串转换为指定类型的值
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="str">要转换的字符串</param>
        /// <param name="defaultValue">转换失败时返回的默认值</param>
        /// <returns>转换后的值，或默认值</returns>
        /// <remarks>
        /// 此方法尝试将字符串转换为指定类型的值，如果转换失败则返回默认值。
        /// 支持基本类型（int、float、bool等）和Unity类型（Vector2、Vector3、Color等）。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// string numberStr = "42";
        /// int number = numberStr.To&lt;int&gt;(); // 返回 42
        /// 
        /// string vectorStr = "(1, 2, 3)";
        /// Vector3 vector = vectorStr.To&lt;Vector3&gt;(); // 返回 Vector3(1, 2, 3)
        /// </code>
        /// </remarks>
        public static T To<T>(this string str, T defaultValue = default)
        {
            if (string.IsNullOrEmpty(str))
            {
                return defaultValue;
            }

            try
            {
                Type type = typeof(T);
                
                // 处理基本类型
                if (type == typeof(int))
                {
                    return (T)(object)int.Parse(str);
                }
                if (type == typeof(float))
                {
                    return (T)(object)float.Parse(str);
                }
                if (type == typeof(double))
                {
                    return (T)(object)double.Parse(str);
                }
                if (type == typeof(bool))
                {
                    return (T)(object)bool.Parse(str);
                }
                
                // 处理Unity类型
                if (type == typeof(Vector2))
                {
                    ReadOnlySpan<char> span = str.AsSpan();
                    if (span.Length > 0 && span[0] == '(' && span[span.Length - 1] == ')')
                    {
                        span = span.Slice(1, span.Length - 2);
                    }
                    
                    int commaIndex = span.IndexOf(',');
                    if (commaIndex > 0)
                    {
                        ReadOnlySpan<char> xSpan = span.Slice(0, commaIndex).Trim();
                        ReadOnlySpan<char> ySpan = span.Slice(commaIndex + 1).Trim();
                        
                        if (float.TryParse(xSpan, out float x) && float.TryParse(ySpan, out float y))
                        {
                            return (T)(object)new Vector2(x, y);
                        }
                    }
                }
                if (type == typeof(Vector3))
                {
                    ReadOnlySpan<char> span = str.AsSpan();
                    if (span.Length > 0 && span[0] == '(' && span[span.Length - 1] == ')')
                    {
                        span = span.Slice(1, span.Length - 2);
                    }
                    
                    int firstCommaIndex = span.IndexOf(',');
                    if (firstCommaIndex > 0)
                    {
                        ReadOnlySpan<char> xSpan = span.Slice(0, firstCommaIndex).Trim();
                        ReadOnlySpan<char> restSpan = span.Slice(firstCommaIndex + 1);
                        
                        int secondCommaIndex = restSpan.IndexOf(',');
                        if (secondCommaIndex > 0)
                        {
                            ReadOnlySpan<char> ySpan = restSpan.Slice(0, secondCommaIndex).Trim();
                            ReadOnlySpan<char> zSpan = restSpan.Slice(secondCommaIndex + 1).Trim();
                            
                            if (float.TryParse(xSpan, out float x) && 
                                float.TryParse(ySpan, out float y) && 
                                float.TryParse(zSpan, out float z))
                            {
                                return (T)(object)new Vector3(x, y, z);
                            }
                        }
                    }
                }
                if (type == typeof(Color))
                {
                    if (str.StartsWith("#"))
                    {
                        Color color;
                        if (ColorUtility.TryParseHtmlString(str, out color))
                        {
                            return (T)(object)color;
                        }
                    }
                    else if (str.StartsWith("RGBA(") && str.EndsWith(")"))
                    {
                        ReadOnlySpan<char> span = str.AsSpan().Slice(5, str.Length - 6);
                        
                        int commaCount = 0;
                        int lastCommaIndex = -1;
                        int[] commaIndices = new int[3]; // 最多3个逗号
                        
                        for (int i = 0; i < span.Length && commaCount < 3; i++)
                        {
                            if (span[i] == ',')
                            {
                                commaIndices[commaCount++] = i;
                                lastCommaIndex = i;
                            }
                        }
                        
                        if (commaCount == 3 && lastCommaIndex > 0)
                        {
                            ReadOnlySpan<char> rSpan = span.Slice(0, commaIndices[0]).Trim();
                            ReadOnlySpan<char> gSpan = span.Slice(commaIndices[0] + 1, commaIndices[1] - commaIndices[0] - 1).Trim();
                            ReadOnlySpan<char> bSpan = span.Slice(commaIndices[1] + 1, commaIndices[2] - commaIndices[1] - 1).Trim();
                            ReadOnlySpan<char> aSpan = span.Slice(commaIndices[2] + 1).Trim();
                            
                            if (float.TryParse(rSpan, out float r) && 
                                float.TryParse(gSpan, out float g) && 
                                float.TryParse(bSpan, out float b) && 
                                float.TryParse(aSpan, out float a))
                            {
                                return (T)(object)new Color(r / 255f, g / 255f, b / 255f, a);
                            }
                        }
                    }
                }
                
                // 尝试使用Convert类
                return (T)Convert.ChangeType(str, type);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 将字符串重复指定次数
        /// </summary>
        /// <param name="str">要重复的字符串</param>
        /// <param name="count">重复次数</param>
        /// <returns>重复后的字符串</returns>
        /// <remarks>
        /// 此方法将字符串重复指定次数，并返回连接后的结果。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// string star = "*";
        /// string stars = star.Repeat(5); // 返回 "*****"
        /// </code>
        /// </remarks>
        public static string Repeat(this string str, int count)
        {
            if (string.IsNullOrEmpty(str) || count <= 0)
            {
                return string.Empty;
            }
            
            if (count == 1)
            {
                return str;
            }
            
            // 从对象池获取StringBuilder
            StringBuilder sb = StringBuilderPool.Get();
            try
            {
                sb.EnsureCapacity(str.Length * count);
                for (int i = 0; i < count; i++)
                {
                    sb.Append(str);
                }
                
                return sb.ToString();
            }
            finally
            {
                // 返回StringBuilder到对象池
                StringBuilderPool.Release(sb);
            }
        }

        /// <summary>
        /// 反转字符串
        /// </summary>
        /// <param name="str">要反转的字符串</param>
        /// <returns>反转后的字符串</returns>
        /// <remarks>
        /// 此方法将字符串中的字符顺序反转。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// string text = "Hello";
        /// string reversed = text.Reverse(); // 返回 "olleH"
        /// </code>
        /// </remarks>
        public static string Reverse(this string str)
        {
            if (string.IsNullOrEmpty(str) || str.Length == 1)
            {
                return str;
            }
            
            // 对于短字符串，使用栈分配以避免堆分配
            if (str.Length <= 128)
            {
                Span<char> chars = stackalloc char[str.Length];
                str.AsSpan().CopyTo(chars);
                chars.Reverse();
                return new string(chars);
            }
            
            // 对于长字符串，使用ArrayPool
            char[] rentedArray = ArrayPool<char>.Shared.Rent(str.Length);
            try
            {
                str.CopyTo(0, rentedArray, 0, str.Length);
                Array.Reverse(rentedArray, 0, str.Length);
                return new string(rentedArray, 0, str.Length);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(rentedArray);
            }
        }

        /// <summary>
        /// 检查字符串是否包含另一个字符串，忽略大小写
        /// </summary>
        /// <param name="str">源字符串</param>
        /// <param name="value">要查找的子字符串</param>
        /// <returns>如果包含则返回true，否则返回false</returns>
        /// <remarks>
        /// 此方法是对StringUtils.ContainsIgnoreCase的扩展方法包装。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// string text = "Hello World";
        /// bool contains = text.ContainsIgnoreCase("hello"); // 返回 true
        /// </code>
        /// </remarks>
        public static bool ContainsIgnoreCase(this string str, string value)
        {
            return StringUtils.ContainsIgnoreCase(str, value);
        }

        /// <summary>
        /// 将字符串转换为驼峰命名法(camelCase)
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <returns>转换后的驼峰命名法字符串</returns>
        /// <remarks>
        /// 此方法将字符串转换为驼峰命名法，即第一个单词首字母小写，其余单词首字母大写。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// string text = "Hello World";
        /// string camelCase = text.ToCamelCase(); // 返回 "helloWorld"
        /// </code>
        /// </remarks>
        public static string ToCamelCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }
            
            if (str.Length == 1)
            {
                return str.ToLowerInvariant();
            }
            
            // 从对象池获取StringBuilder
            StringBuilder sb = StringBuilderPool.Get();
            try
            {
                sb.EnsureCapacity(str.Length);
                
                bool capitalizeNext = false;
                bool isFirstChar = true;
                
                foreach (char c in str)
                {
                    if (char.IsWhiteSpace(c) || c == '_' || c == '-')
                    {
                        capitalizeNext = true;
                        continue;
                    }
                    
                    if (capitalizeNext)
                    {
                        sb.Append(char.ToUpperInvariant(c));
                        capitalizeNext = false;
                    }
                    else if (isFirstChar)
                    {
                        sb.Append(char.ToLowerInvariant(c));
                        isFirstChar = false;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                
                return sb.ToString();
            }
            finally
            {
                // 返回StringBuilder到对象池
                StringBuilderPool.Release(sb);
            }
        }
    }
} 