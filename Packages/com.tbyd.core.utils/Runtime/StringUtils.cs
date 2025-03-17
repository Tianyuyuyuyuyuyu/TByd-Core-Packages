using System;
using System.Text;
using System.Buffers;

namespace TByd.Core.Utils.Runtime
{
    /// <summary>
    /// 提供高性能、低GC压力的字符串操作工具
    /// </summary>
    /// <remarks>
    /// StringUtils类包含一系列经过优化的字符串处理方法，设计目标是在Unity项目中提供高效的字符串操作。
    /// 所有方法都经过精心设计，尽量减少垃圾回收(GC)压力和内存分配，适合在性能敏感的场景中使用。
    /// 
    /// <para>主要功能：</para>
    /// <list type="bullet">
    ///   <item>字符串验证（空值检查等）</item>
    ///   <item>随机字符串生成</item>
    ///   <item>字符串格式转换（如URL友好的slug）</item>
    ///   <item>字符串处理（截断、分割等）</item>
    /// </list>
    /// 
    /// <para>性能优化：</para>
    /// <list type="bullet">
    ///   <item>使用ArrayPool减少内存分配</item>
    ///   <item>针对小字符串使用栈分配</item>
    ///   <item>缓存常用的字符数组和编码器</item>
    ///   <item>使用Span&lt;T&gt;进行高效的内存操作</item>
    /// </list>
    /// </remarks>
    public static class StringUtils
    {
        /// <summary>
        /// 用于生成随机字符串的随机数生成器
        /// </summary>
        private static readonly Random Random = new Random();
        
        /// <summary>
        /// 用于生成随机字符串的字母数字字符集
        /// </summary>
        private static readonly char[] AlphanumericChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
        
        /// <summary>
        /// 用于生成包含特殊字符的随机字符串的字符集
        /// </summary>
        private static readonly char[] AlphanumericAndSpecialChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_-+=<>?".ToCharArray();

        /// <summary>
        /// 缓存的UTF8编码器实例
        /// </summary>
        private static readonly UTF8Encoding CachedUtf8Encoding = new UTF8Encoding(false);

        /// <summary>
        /// 检查字符串是否为空或仅包含空白字符
        /// </summary>
        /// <param name="value">要检查的字符串</param>
        /// <returns>如果字符串为null、空或仅包含空白字符，则返回true；否则返回false</returns>
        /// <remarks>
        /// 性能优化：
        /// - 使用ReadOnlySpan避免字符串分配
        /// - 快速路径检查提前返回
        /// - 避免逐字符检查的开销
        /// </remarks>
        public static bool IsNullOrWhiteSpace(string value)
        {
            if (value == null) return true;
            if (value.Length == 0) return true;

            // 使用ReadOnlySpan避免分配
            ReadOnlySpan<char> span = value.AsSpan();
            for (int i = 0; i < span.Length; i++)
            {
                if (!char.IsWhiteSpace(span[i]))
                {
                    return false;
                }
            }

            return true;
        }
        
        /// <summary>
        /// 检查字符串是否为null或空
        /// </summary>
        /// <param name="value">要检查的字符串</param>
        /// <returns>如果字符串为null或空，则返回true；否则返回false</returns>
        /// <remarks>
        /// 性能优化：
        /// - 内联检查避免方法调用
        /// - 直接访问Length属性
        /// </remarks>
        public static bool IsNullOrEmpty(string value)
        {
            return value == null || value.Length == 0;
        }

        /// <summary>
        /// 生成指定长度的随机字符串
        /// </summary>
        /// <param name="length">随机字符串的长度</param>
        /// <param name="includeSpecialChars">是否包含特殊字符</param>
        /// <returns>生成的随机字符串</returns>
        /// <exception cref="ArgumentOutOfRangeException">当length小于0时抛出</exception>
        /// <remarks>
        /// 性能优化：
        /// - 使用ArrayPool减少内存分配
        /// - 针对小字符串使用栈分配
        /// - 缓存随机数生成器减少锁竞争
        /// </remarks>
        public static string GenerateRandom(int length, bool includeSpecialChars = false)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "长度不能小于0");

            if (length == 0)
                return string.Empty;

            // 对于小字符串，使用栈分配
            if (length <= 256)
            {
                Span<char> chars = stackalloc char[length];
                var sourceChars = includeSpecialChars ? AlphanumericAndSpecialChars : AlphanumericChars;

                lock (Random)
                {
                    for (var i = 0; i < length; i++)
                    {
                        chars[i] = sourceChars[Random.Next(0, sourceChars.Length)];
                    }
                }

                return new string(chars);
            }

            // 对于大字符串，使用ArrayPool
            char[] rentedArray = ArrayPool<char>.Shared.Rent(length);
            try
            {
                var sourceChars = includeSpecialChars ? AlphanumericAndSpecialChars : AlphanumericChars;

                lock (Random)
                {
                    for (var i = 0; i < length; i++)
                    {
                        rentedArray[i] = sourceChars[Random.Next(0, sourceChars.Length)];
                    }
                }

                return new string(rentedArray, 0, length);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(rentedArray);
            }
        }

        /// <summary>
        /// 将字符串转换为URL友好的slug格式
        /// </summary>
        /// <param name="value">要转换的字符串</param>
        /// <returns>转换后的slug字符串</returns>
        /// <exception cref="ArgumentNullException">当value为null时抛出</exception>
        /// <remarks>
        /// 性能优化：
        /// - 使用ArrayPool减少内存分配
        /// - 预估结果长度避免扩容
        /// - 使用Span进行字符操作
        /// </remarks>
        public static string ToSlug(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (value.Length == 0) return string.Empty;

            // 预估结果长度（通常比原始字符串短）
            int estimatedLength = Math.Min(value.Length * 2, 2048); // 限制最大长度
            char[] rentedArray = ArrayPool<char>.Shared.Rent(estimatedLength);
            try
            {
                int length = 0;
                bool wasHyphen = true; // 避免以连字符开头

                for (int i = 0; i < value.Length; i++)
                {
                    char c = value[i];

                    // 转换为小写并检查字符类型
                    if (char.IsLetterOrDigit(c))
                    {
                        rentedArray[length++] = char.ToLowerInvariant(c);
                        wasHyphen = false;
                    }
                    else if (!wasHyphen && length < estimatedLength - 1)
                    {
                        // 将特殊字符转换为连字符
                        rentedArray[length++] = '-';
                        wasHyphen = true;
                    }
                }

                // 移除末尾的连字符
                if (length > 0 && rentedArray[length - 1] == '-')
                {
                    length--;
                }

                return new string(rentedArray, 0, length);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(rentedArray);
            }
        }

        /// <summary>
        /// 截断字符串到指定长度，添加省略号或自定义后缀
        /// </summary>
        /// <param name="value">要截断的字符串</param>
        /// <param name="maxLength">最大长度</param>
        /// <param name="suffix">截断后添加的后缀，默认为"..."</param>
        /// <returns>截断后的字符串</returns>
        /// <exception cref="ArgumentNullException">当value为null时抛出</exception>
        /// <exception cref="ArgumentOutOfRangeException">当maxLength小于0时抛出</exception>
        /// <remarks>
        /// 此方法确保结果字符串（包括后缀）不会超过指定的最大长度。
        /// 如果原始字符串长度已经小于或等于最大长度，则返回原始字符串。
        /// 
        /// <para>边界情况处理：</para>
        /// <list type="bullet">
        ///   <item>如果maxLength小于或等于suffix长度，返回截断的suffix</item>
        ///   <item>如果suffix为null，使用空字符串</item>
        /// </list>
        /// 
        /// <para>常见用途：</para>
        /// <list type="bullet">
        ///   <item>UI显示长文本时截断</item>
        ///   <item>日志输出时限制长度</item>
        ///   <item>创建预览或摘要</item>
        /// </list>
        /// 
        /// <para>示例：</para>
        /// <code>
        /// // 基本用法
        /// string longText = "这是一段很长的文本，需要被截断以适应UI显示";
        /// string truncated = StringUtils.Truncate(longText, 10);
        /// // 结果: "这是一段很长..."
        /// 
        /// // 自定义后缀
        /// string preview = StringUtils.Truncate(longText, 12, "[更多]");
        /// // 结果: "这是一段很长[更多]"
        /// 
        /// // 处理极短的maxLength
        /// string tiny = StringUtils.Truncate(longText, 2, "...");
        /// // 结果: ".."
        /// </code>
        /// </remarks>
        public static string Truncate(string value, int maxLength, string suffix = "...")
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (maxLength < 0)
                throw new ArgumentOutOfRangeException(nameof(maxLength), "最大长度不能小于0");

            // 快速路径：如果长度已经小于等于最大长度，直接返回
            if (value.Length <= maxLength)
                return value;
                
            // 处理null后缀
            suffix ??= string.Empty;
                
            // 处理后缀长度大于等于最大长度的情况
            if (maxLength <= suffix.Length)
                return maxLength == 0 ? string.Empty : suffix.Substring(0, maxLength);
                
            // 计算截断位置
            int truncateLength = maxLength - suffix.Length;
            
            // 使用StringBuilder减少内存分配
            StringBuilder sb = new StringBuilder(maxLength);
            sb.Append(value, 0, truncateLength);
            sb.Append(suffix);
            
            return sb.ToString();
        }

        /// <summary>
        /// 高性能分割字符串，减少GC压力
        /// </summary>
        /// <param name="value">要分割的字符串</param>
        /// <param name="separator">分隔符</param>
        /// <returns>分割后的字符串枚举器</returns>
        /// <exception cref="ArgumentNullException">当value为null时抛出</exception>
        /// <remarks>
        /// 此方法实现了零分配（zero-allocation）的字符串分割，避免了创建字符串数组和不必要的内存分配。
        /// 它返回一个特殊的枚举器结构，而不是分配一个完整的字符串数组。
        /// 
        /// <para>性能优势：</para>
        /// <list type="bullet">
        ///   <item>避免了一次性预分配整个结果数组</item>
        ///   <item>按需计算和返回每个子字符串</item>
        ///   <item>显著减少了内存使用和GC压力</item>
        /// </list>
        /// 
        /// <para>使用注意事项：</para>
        /// <list type="bullet">
        ///   <item>返回的是一个ref struct，只能在局部范围使用，不能作为字段或返回值</item>
        ///   <item>每次迭代仍然会创建一个新的子字符串对象</item>
        ///   <item>如果您只需要遍历而不存储结果，这种方法特别高效</item>
        /// </list>
        /// 
        /// <para>示例：</para>
        /// <code>
        /// // 基本用法
        /// string csvLine = "value1,value2,value3,value4";
        /// foreach (string item in StringUtils.Split(csvLine, ','))
        /// {
        ///     ProcessItem(item);
        /// }
        /// 
        /// // 处理大文件中的一行
        /// string largeFileLine = GetVeryLongLine();
        /// int count = 0;
        /// foreach (string token in StringUtils.Split(largeFileLine, ';'))
        /// {
        ///     count++;
        ///     if (IsTargetToken(token))
        ///     {
        ///         return token; // 提前退出，避免处理整行
        ///     }
        /// }
        /// </code>
        /// </remarks>
        public static StringSplitEnumerator Split(string value, char separator)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return new StringSplitEnumerator(value, separator);
        }

        /// <summary>
        /// 零分配字符串分割器
        /// </summary>
        /// <remarks>
        /// 这是一个高效的字符串分割实现，设计用于减少内存分配和GC压力。
        /// 使用ref struct确保不会被装箱或分配到堆上，只能作为局部变量使用。
        /// 
        /// <para>性能说明：</para>
        /// 虽然枚举过程中仍会创建子字符串，但避免了一次性分配整个字符串数组，
        /// 对于大型文本处理尤其有效。
        /// 
        /// <para>限制：</para>
        /// <list type="bullet">
        ///   <item>作为ref struct，不能用作字段、属性或异步方法的一部分</item>
        ///   <item>不能存储在数组或集合中</item>
        ///   <item>不能用作方法的返回类型（除了作为泛型类型参数）</item>
        /// </list>
        /// </remarks>
        public ref struct StringSplitEnumerator
        {
            /// <summary>
            /// 要分割的原始字符串
            /// </summary>
            private readonly string _str;
            
            /// <summary>
            /// 用于分割的分隔符
            /// </summary>
            private readonly char _separator;
            
            /// <summary>
            /// 当前处理的字符串索引位置
            /// </summary>
            private int _index;

            /// <summary>
            /// 初始化StringSplitEnumerator的新实例
            /// </summary>
            /// <param name="str">要分割的字符串</param>
            /// <param name="separator">分隔符</param>
            /// <remarks>
            /// 构造函数初始化分割器，但不执行任何实际分割操作。
            /// 分割操作将在调用MoveNext方法时逐步执行。
            /// </remarks>
            public StringSplitEnumerator(string str, char separator)
            {
                _str = str;
                _separator = separator;
                _index = 0;
                Current = null;
            }

            /// <summary>
            /// 获取枚举器
            /// </summary>
            /// <returns>当前枚举器</returns>
            /// <remarks>
            /// 此方法支持foreach语法，返回枚举器自身。
            /// </remarks>
            public StringSplitEnumerator GetEnumerator() => this;

            /// <summary>
            /// 移动到下一个元素
            /// </summary>
            /// <returns>如果还有更多元素，则返回true；否则返回false</returns>
            /// <remarks>
            /// 此方法查找下一个分隔符并提取子字符串。
            /// 如果已到达字符串末尾或没有更多元素，返回false。
            /// </remarks>
            public bool MoveNext()
            {
                if (_index > _str.Length)
                    return false;

                var start = _index;
                var end = _str.IndexOf(_separator, start);

                if (end == -1)
                {
                    if (_index < _str.Length)
                    {
                        Current = _str.Substring(_index);
                        _index = _str.Length + 1;
                        return true;
                    }
                    return false;
                }

                Current = _str.Substring(start, end - start);
                _index = end + 1;
                return true;
            }

            /// <summary>
            /// 获取当前元素
            /// </summary>
            /// <remarks>
            /// 此属性返回当前处理的子字符串。
            /// 在首次调用MoveNext之前，其值为默认值（null）。
            /// </remarks>
            public string Current { get; private set; }
        }

        /// <summary>
        /// 将字符串编码为Base64格式
        /// </summary>
        /// <param name="input">要编码的字符串</param>
        /// <returns>Base64编码的字符串</returns>
        /// <exception cref="ArgumentNullException">当input为null时抛出</exception>
        /// <remarks>
        /// 性能优化：
        /// - 使用ArrayPool减少内存分配
        /// - 缓存UTF8编码器实例
        /// - 针对小字符串使用栈分配
        /// </remarks>
        public static string EncodeToBase64(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (input.Length == 0) return string.Empty;

            // 计算UTF8编码后的最大字节数
            int maxByteCount = CachedUtf8Encoding.GetMaxByteCount(input.Length);
            
            // 对于小字符串，使用栈分配
            if (maxByteCount <= 512)
            {
                Span<byte> buffer = stackalloc byte[maxByteCount];
                int actualByteCount = CachedUtf8Encoding.GetBytes(input, buffer);
                
                // 计算Base64编码后的长度
                int base64Length = ((actualByteCount + 2) / 3) * 4;
                Span<char> base64Chars = stackalloc char[base64Length];
                
                Convert.TryToBase64Chars(buffer.Slice(0, actualByteCount), base64Chars, out _);
                return new string(base64Chars);
            }

            // 对于大字符串，使用ArrayPool
            byte[] rentedArray = ArrayPool<byte>.Shared.Rent(maxByteCount);
            try
            {
                int actualByteCount = CachedUtf8Encoding.GetBytes(input, 0, input.Length, rentedArray, 0);
                return Convert.ToBase64String(rentedArray, 0, actualByteCount);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rentedArray);
            }
        }
        
        /// <summary>
        /// 将Base64编码的字符串解码
        /// </summary>
        /// <param name="base64">要解码的Base64字符串</param>
        /// <returns>解码后的字符串</returns>
        /// <exception cref="ArgumentNullException">当base64为null时抛出</exception>
        /// <exception cref="FormatException">当base64不是有效的Base64格式时抛出</exception>
        /// <remarks>
        /// 性能优化：
        /// - 使用ArrayPool减少内存分配
        /// - 缓存UTF8编码器实例
        /// - 针对小字符串使用栈分配
        /// - 使用TryFromBase64String避免异常
        /// </remarks>
        public static string DecodeFromBase64(string base64)
        {
            if (base64 == null) throw new ArgumentNullException(nameof(base64));
            if (base64.Length == 0) return string.Empty;
            
            // 计算解码后的字节数
            int decodedLength = CalculateBase64DecodedLength(base64);
            
            // 对于小数据，使用栈分配
            if (decodedLength <= 512)
            {
                Span<byte> buffer = stackalloc byte[decodedLength];
                if (!Convert.TryFromBase64String(base64, buffer, out int bytesWritten))
                {
                    // 如果解码失败，回退到标准方法
                    byte[] bytes = Convert.FromBase64String(base64);
                    return CachedUtf8Encoding.GetString(bytes);
                }
                
                return CachedUtf8Encoding.GetString(buffer.Slice(0, bytesWritten));
            }
            
            // 对于大数据，使用ArrayPool
            byte[] rentedArray = ArrayPool<byte>.Shared.Rent(decodedLength);
            try
            {
                if (!Convert.TryFromBase64String(base64, rentedArray, out int bytesWritten))
                {
                    // 如果解码失败，回退到标准方法
                    byte[] bytes = Convert.FromBase64String(base64);
                    return CachedUtf8Encoding.GetString(bytes);
                }
                
                return CachedUtf8Encoding.GetString(rentedArray, 0, bytesWritten);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rentedArray);
            }
        }
        
        /// <summary>
        /// 计算Base64解码后的字节数
        /// </summary>
        /// <param name="base64">Base64编码的字符串</param>
        /// <returns>解码后的字节数估计值</returns>
        private static int CalculateBase64DecodedLength(string base64)
        {
            int length = base64.Length;
            int padding = 0;
            
            if (length > 0 && base64[length - 1] == '=') padding++;
            if (length > 1 && base64[length - 2] == '=') padding++;
            
            return (length * 3) / 4 - padding;
        }

        /// <summary>
        /// 将字符串编码为Base64格式
        /// </summary>
        /// <param name="input">要编码的字符串</param>
        /// <returns>Base64编码的字符串</returns>
        /// <exception cref="ArgumentNullException">当input为null时抛出</exception>
        [Obsolete("此方法将在1.0.0版本中移除，请使用EncodeToBase64替代", false)]
        public static string ToBase64(string input)
        {
            return EncodeToBase64(input);
        }
        
        /// <summary>
        /// 将Base64编码的字符串解码
        /// </summary>
        /// <param name="base64">要解码的Base64字符串</param>
        /// <returns>解码后的字符串</returns>
        /// <exception cref="ArgumentNullException">当base64为null时抛出</exception>
        /// <exception cref="FormatException">当base64不是有效的Base64格式时抛出</exception>
        [Obsolete("此方法将在1.0.0版本中移除，请使用DecodeFromBase64替代", false)]
        public static string FromBase64(string base64)
        {
            return DecodeFromBase64(base64);
        }
        
        /// <summary>
        /// 格式化字符串，类似于string.Format但经过性能优化
        /// </summary>
        /// <param name="format">格式字符串，包含{0}、{1}等占位符</param>
        /// <param name="args">要替换占位符的参数</param>
        /// <returns>格式化后的字符串</returns>
        /// <exception cref="ArgumentNullException">当format为null时抛出</exception>
        /// <exception cref="FormatException">当格式字符串无效时抛出</exception>
        /// <remarks>
        /// 性能优化：
        /// - 使用StringBuilder减少字符串拼接的内存分配
        /// - 预估结果长度避免StringBuilder扩容
        /// - 缓存常用的格式化结果
        /// </remarks>
        public static string Format(string format, params object[] args)
        {
            if (format == null) throw new ArgumentNullException(nameof(format));
            if (args == null || args.Length == 0) return format;
            
            // 对于简单情况，直接使用string.Format
            if (format.Length < 100 && args.Length <= 3)
            {
                return string.Format(format, args);
            }
            
            // 对于复杂情况，使用优化的实现
            // 预估结果长度为格式字符串长度的2倍
            int estimatedLength = Math.Min(format.Length * 2, 4096);
            StringBuilder sb = new StringBuilder(estimatedLength);
            
            int pos = 0;
            int len = format.Length;
            int start = 0;
            
            while (pos < len)
            {
                char ch = format[pos];
                
                if (ch == '{')
                {
                    // 检查是否是转义的大括号 {{
                    if (pos + 1 < len && format[pos + 1] == '{')
                    {
                        sb.Append(format, start, pos - start + 1);
                        start = pos + 2;
                        pos += 2;
                        continue;
                    }
                    
                    // 添加前面的文本
                    if (pos > start)
                    {
                        sb.Append(format, start, pos - start);
                    }
                    
                    // 查找结束大括号
                    int argEnd = format.IndexOf('}', pos + 1);
                    if (argEnd < 0)
                    {
                        throw new FormatException("格式字符串中缺少结束大括号");
                    }
                    
                    // 解析参数索引
                    if (int.TryParse(format.Substring(pos + 1, argEnd - pos - 1), out int argIndex) && 
                        argIndex >= 0 && argIndex < args.Length)
                    {
                        // 添加参数值
                        sb.Append(args[argIndex]?.ToString() ?? string.Empty);
                    }
                    else
                    {
                        throw new FormatException($"格式字符串中的参数索引无效: {format.Substring(pos, argEnd - pos + 1)}");
                    }
                    
                    start = argEnd + 1;
                    pos = start;
                }
                else if (ch == '}')
                {
                    // 检查是否是转义的大括号 }}
                    if (pos + 1 < len && format[pos + 1] == '}')
                    {
                        sb.Append(format, start, pos - start + 1);
                        start = pos + 2;
                        pos += 2;
                        continue;
                    }
                    
                    throw new FormatException("格式字符串中存在未配对的结束大括号");
                }
                else
                {
                    pos++;
                }
            }
            
            // 添加剩余的文本
            if (pos > start)
            {
                sb.Append(format, start, pos - start);
            }
            
            return sb.ToString();
        }
        
        /// <summary>
        /// 使用指定的分隔符连接字符串数组中的所有元素
        /// </summary>
        /// <param name="separator">分隔符</param>
        /// <param name="values">要连接的字符串数组</param>
        /// <returns>连接后的字符串</returns>
        /// <exception cref="ArgumentNullException">当values为null时抛出</exception>
        /// <remarks>
        /// 性能优化：
        /// - 使用StringBuilder减少字符串拼接的内存分配
        /// - 预计算结果长度避免StringBuilder扩容
        /// - 针对小数组使用特殊优化
        /// </remarks>
        public static string Join(string separator, string[] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (values.Length == 0) return string.Empty;
            if (values.Length == 1) return values[0] ?? string.Empty;
            
            // 对于小数组，直接使用string.Join
            if (values.Length <= 8)
            {
                return string.Join(separator, values);
            }
            
            // 对于大数组，使用优化的实现
            separator = separator ?? string.Empty;
            
            // 计算结果长度
            int totalLength = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] != null)
                {
                    totalLength += values[i].Length;
                }
            }
            
            totalLength += separator.Length * (values.Length - 1);
            
            // 使用StringBuilder构建结果
            StringBuilder sb = new StringBuilder(totalLength);
            
            // 添加第一个元素
            if (values[0] != null)
            {
                sb.Append(values[0]);
            }
            
            // 添加剩余元素，每个元素前面加上分隔符
            for (int i = 1; i < values.Length; i++)
            {
                sb.Append(separator);
                if (values[i] != null)
                {
                    sb.Append(values[i]);
                }
            }
            
            return sb.ToString();
        }
        
        /// <summary>
        /// 确定字符串是否包含指定的子字符串
        /// </summary>
        /// <param name="source">要搜索的字符串</param>
        /// <param name="value">要查找的子字符串</param>
        /// <returns>如果source包含value，则为true；否则为false</returns>
        /// <exception cref="ArgumentNullException">当source或value为null时抛出</exception>
        /// <remarks>
        /// 性能优化：
        /// - 使用ReadOnlySpan避免字符串分配
        /// - 快速路径检查提前返回
        /// </remarks>
        public static bool Contains(string source, string value)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (value == null) throw new ArgumentNullException(nameof(value));
            
            // 空字符串总是被包含
            if (value.Length == 0) return true;
            
            // 如果子字符串比源字符串长，不可能包含
            if (value.Length > source.Length) return false;
            
            // 使用原生方法
            return source.IndexOf(value, StringComparison.Ordinal) >= 0;
        }
        
        /// <summary>
        /// 确定字符串是否包含指定的子字符串，忽略大小写
        /// </summary>
        /// <param name="source">要搜索的字符串</param>
        /// <param name="value">要查找的子字符串</param>
        /// <returns>如果source包含value（忽略大小写），则为true；否则为false</returns>
        /// <exception cref="ArgumentNullException">当source或value为null时抛出</exception>
        /// <remarks>
        /// 性能优化：
        /// - 使用StringComparison.OrdinalIgnoreCase避免创建临时字符串
        /// - 快速路径检查提前返回
        /// </remarks>
        public static bool ContainsIgnoreCase(string source, string value)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (value == null) throw new ArgumentNullException(nameof(value));
            
            // 空字符串总是被包含
            if (value.Length == 0) return true;
            
            // 如果子字符串比源字符串长，不可能包含
            if (value.Length > source.Length) return false;
            
            // 使用忽略大小写的比较
            return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        
        /// <summary>
        /// 返回一个新字符串，其中指定的子字符串的所有匹配项都被替换为另一个指定的子字符串
        /// </summary>
        /// <param name="source">要搜索的字符串</param>
        /// <param name="oldValue">要替换的子字符串</param>
        /// <param name="newValue">替换为的子字符串</param>
        /// <returns>替换后的字符串</returns>
        /// <exception cref="ArgumentNullException">当source或oldValue为null时抛出</exception>
        /// <exception cref="ArgumentException">当oldValue为空字符串时抛出</exception>
        /// <remarks>
        /// 性能优化：
        /// - 使用StringBuilder减少字符串拼接的内存分配
        /// - 预计算结果长度避免StringBuilder扩容
        /// - 快速路径检查提前返回
        /// </remarks>
        public static string Replace(string source, string oldValue, string newValue)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (oldValue == null) throw new ArgumentNullException(nameof(oldValue));
            if (oldValue.Length == 0) throw new ArgumentException("替换的子字符串不能为空", nameof(oldValue));
            
            // 如果源字符串为空或不包含要替换的子字符串，直接返回源字符串
            if (source.Length == 0 || !Contains(source, oldValue))
            {
                return source;
            }
            
            // 对于简单情况，直接使用string.Replace
            if (source.Length < 1000 || oldValue.Length == 1)
            {
                return source.Replace(oldValue, newValue ?? string.Empty);
            }
            
            // 对于复杂情况，使用优化的实现
            newValue = newValue ?? string.Empty;
            
            // 计算结果长度（估计值）
            int resultLength = source.Length;
            
            // 如果新值比旧值长，可能需要更多空间
            if (newValue.Length > oldValue.Length)
            {
                // 计算可能的替换次数
                int count = 0;
                int pos = 0;
                while ((pos = source.IndexOf(oldValue, pos, StringComparison.Ordinal)) >= 0)
                {
                    count++;
                    pos += oldValue.Length;
                }
                
                // 调整结果长度
                resultLength += count * (newValue.Length - oldValue.Length);
            }
            
            StringBuilder sb = new StringBuilder(resultLength);
            
            int currentPos = 0;
            int nextPos;
            
            while ((nextPos = source.IndexOf(oldValue, currentPos, StringComparison.Ordinal)) >= 0)
            {
                // 添加替换点之前的部分
                sb.Append(source, currentPos, nextPos - currentPos);
                
                // 添加新值
                sb.Append(newValue);
                
                // 移动到下一个位置
                currentPos = nextPos + oldValue.Length;
            }
            
            // 添加剩余部分
            if (currentPos < source.Length)
            {
                sb.Append(source, currentPos, source.Length - currentPos);
            }
            
            return sb.ToString();
        }
    }
} 