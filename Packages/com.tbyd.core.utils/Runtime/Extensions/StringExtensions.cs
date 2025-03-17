using System;
using System.Text;
using UnityEngine;

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
                    if (str.StartsWith("(") && str.EndsWith(")"))
                    {
                        str = str.Substring(1, str.Length - 2);
                    }
                    
                    string[] parts = str.Split(',');
                    if (parts.Length >= 2)
                    {
                        float x = float.Parse(parts[0].Trim());
                        float y = float.Parse(parts[1].Trim());
                        return (T)(object)new Vector2(x, y);
                    }
                }
                if (type == typeof(Vector3))
                {
                    if (str.StartsWith("(") && str.EndsWith(")"))
                    {
                        str = str.Substring(1, str.Length - 2);
                    }
                    
                    string[] parts = str.Split(',');
                    if (parts.Length >= 3)
                    {
                        float x = float.Parse(parts[0].Trim());
                        float y = float.Parse(parts[1].Trim());
                        float z = float.Parse(parts[2].Trim());
                        return (T)(object)new Vector3(x, y, z);
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
                        str = str.Substring(5, str.Length - 6);
                        string[] parts = str.Split(',');
                        if (parts.Length >= 4)
                        {
                            float r = float.Parse(parts[0].Trim()) / 255f;
                            float g = float.Parse(parts[1].Trim()) / 255f;
                            float b = float.Parse(parts[2].Trim()) / 255f;
                            float a = float.Parse(parts[3].Trim());
                            return (T)(object)new Color(r, g, b, a);
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
            
            StringBuilder sb = new StringBuilder(str.Length * count);
            for (int i = 0; i < count; i++)
            {
                sb.Append(str);
            }
            
            return sb.ToString();
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
            
            char[] chars = str.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        /// <summary>
        /// 检查字符串是否包含另一个字符串（忽略大小写）
        /// </summary>
        /// <param name="str">源字符串</param>
        /// <param name="value">要查找的字符串</param>
        /// <returns>如果包含则返回true，否则返回false</returns>
        /// <remarks>
        /// 此方法检查字符串是否包含另一个字符串，忽略大小写。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// string text = "Hello World";
        /// bool contains = text.ContainsIgnoreCase("world"); // 返回 true
        /// </code>
        /// </remarks>
        public static bool ContainsIgnoreCase(this string str, string value)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(value))
            {
                return false;
            }
            
            return str.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// 将字符串转换为驼峰命名法
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <returns>驼峰命名法格式的字符串</returns>
        /// <remarks>
        /// 此方法将字符串转换为驼峰命名法格式（首字母小写，后续单词首字母大写）。
        /// 
        /// <para>示例：</para>
        /// <code>
        /// string text = "hello world";
        /// string camelCase = text.ToCamelCase(); // 返回 "helloWorld"
        /// </code>
        /// </remarks>
        public static string ToCamelCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }
            
            string[] words = str.Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
            {
                return string.Empty;
            }
            
            StringBuilder sb = new StringBuilder(str.Length);
            
            // 第一个单词首字母小写
            sb.Append(char.ToLowerInvariant(words[0][0]));
            if (words[0].Length > 1)
            {
                sb.Append(words[0].Substring(1));
            }
            
            // 后续单词首字母大写
            for (int i = 1; i < words.Length; i++)
            {
                if (!string.IsNullOrEmpty(words[i]))
                {
                    sb.Append(char.ToUpperInvariant(words[i][0]));
                    if (words[i].Length > 1)
                    {
                        sb.Append(words[i].Substring(1));
                    }
                }
            }
            
            return sb.ToString();
        }
    }
} 