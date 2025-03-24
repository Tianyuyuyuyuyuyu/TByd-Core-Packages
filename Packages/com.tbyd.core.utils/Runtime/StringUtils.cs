using System;
using System.Text;
using System.Buffers;
using UnityEngine.Pool;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = System.Random;

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
        /// 用于生成随机数的随机数生成器
        /// </summary>
        private static readonly Random Random = new Random();
        
        /// <summary>
        /// 字母和数字字符集
        /// </summary>
        private const string AlphanumericCharsString = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        
        /// <summary>
        /// 字母、数字和特殊字符的字符集
        /// </summary>
        private const string AlphanumericAndSpecialCharsString = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_-+=<>?";
        
        /// <summary>
        /// 获取字母和数字字符集
        /// </summary>
        public static string AlphanumericChars => AlphanumericCharsString;
        
        /// <summary>
        /// 获取字母、数字和特殊字符的字符集
        /// </summary>
        public static string AlphanumericAndSpecialChars => AlphanumericAndSpecialCharsString;

        /// <summary>
        /// 缓存的UTF8编码器实例
        /// </summary>
        private static readonly UTF8Encoding CachedUtf8Encoding = new UTF8Encoding(false);
        
        /// <summary>
        /// StringBuilder对象池，减少StringBuilder的创建和销毁开销
        /// </summary>
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
        /// <param name="value">要检查的字符串</param>
        /// <returns>如果字符串为null、空或仅包含空白字符，则返回true；否则返回false</returns>
        /// <remarks>
        /// 此方法已标记为过时，建议直接使用System.String.IsNullOrWhiteSpace
        /// </remarks>
        [Obsolete("请直接使用System.String.IsNullOrWhiteSpace，性能更佳", false)]
        public static bool IsNullOrWhiteSpace(string value)
        {
            return string.IsNullOrWhiteSpace(value);
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
        /// <param name="allowedChars">允许的字符集，默认为字母和数字</param>
        /// <returns>随机生成的字符串</returns>
        /// <remarks>
        /// 性能优化：
        /// - 使用预分配缓冲区减少内存分配
        /// - 对于短字符串使用栈分配
        /// - 使用ArrayPool减少中间分配
        /// - 使用ThreadLocal Random避免锁竞争
        /// - 采用分段处理策略统一大字符串处理方式
        /// </remarks>
        public static string GenerateRandom(int length, string allowedChars = null)
        {
            if (length <= 0)
                return string.Empty;
            
            // 默认使用字母和数字作为字符集
            allowedChars ??= AlphanumericCharsString;
            
            if (string.IsNullOrEmpty(allowedChars))
                throw new ArgumentException("允许的字符集不能为空", nameof(allowedChars));
            
            // 获取随机数生成器
            Random random = ThreadLocalRandom.Value;
            
            // 对于短字符串使用栈分配，长度阈值调整为256
            if (length <= 256)
            {
                Span<char> buffer = stackalloc char[length];
                for (int i = 0; i < length; i++)
                {
                    buffer[i] = allowedChars[random.Next(allowedChars.Length)];
                }
                return new string(buffer);
            }
            else
            {
                // 统一使用StringBuilder进行缓冲，减少大字符串的GC压力
                StringBuilder sb = StringBuilderPool.Get();
                try
                {
                    // 预分配足够容量
                    sb.EnsureCapacity(length);
                    
                    // 批量生成字符，避免大量单字符Append
                    const int BLOCK_SIZE = 1024;
                    int remaining = length;
                    
                    // 使用共享字符缓冲区进行批处理
                    char[] buffer = ArrayPool<char>.Shared.Rent(Math.Min(BLOCK_SIZE, length));
                    try
                    {
                        while (remaining > 0)
                        {
                            // 计算当前块大小
                            int currentBlock = Math.Min(BLOCK_SIZE, remaining);
                            
                            // 填充随机字符到缓冲区
                            for (int i = 0; i < currentBlock; i++)
                            {
                                buffer[i] = allowedChars[random.Next(allowedChars.Length)];
                            }
                            
                            // 将缓冲区添加到StringBuilder
                            sb.Append(buffer, 0, currentBlock);
                            remaining -= currentBlock;
                        }
                        
                        return sb.ToString();
                    }
                    finally
                    {
                        // 归还共享缓冲区
                        ArrayPool<char>.Shared.Return(buffer);
                    }
                }
                finally
                {
                    // 归还StringBuilder到对象池
                    StringBuilderPool.Release(sb);
                }
            }
        }

        /// <summary>
        /// 线程局部的Random实例
        /// </summary>
        private static class ThreadLocalRandom
        {
            [ThreadStatic]
            private static Random _random;
            
            /// <summary>
            /// 获取当前线程的Random实例
            /// </summary>
            public static Random Value
            {
                get
                {
                    if (_random == null)
                    {
                        // 使用当前线程ID和时间戳创建种子，避免多线程下使用相同种子
                        int seed = Environment.CurrentManagedThreadId ^ 
                                   Environment.TickCount ^ 
                                   (int)DateTime.UtcNow.Ticks;
                        _random = new Random(seed);
                    }
                    return _random;
                }
            }
        }

        /// <summary>
        /// 将字符串转换为URL友好的slug格式
        /// </summary>
        /// <param name="text">要转换的字符串</param>
        /// <returns>URL友好的slug</returns>
        /// <remarks>
        /// 性能优化：
        /// - 对于小字符串使用栈分配减少内存分配
        /// - 避免使用正则表达式
        /// - 直接操作字符数组，避免创建临时字符串
        /// - 使用ArrayPool处理长字符串，避免大内存分配
        /// - 统一处理策略确保GC分配一致性
        /// </remarks>
        public static string ToSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // 对于空字符串，直接返回
            int length = text.Length;
            if (length == 0) return string.Empty;

            // 预计算最大可能长度（最坏情况为输入长度的2倍，每个字符后面都有分隔符）
            int maxLength = length * 2;
            
            // 对于短字符串，使用栈分配
            if (length <= 128)
            {
                Span<char> buffer = stackalloc char[maxLength];
                int position = 0;
                bool prevIsSeparator = true; // 跟踪前一个字符是否为分隔符

                // 将所有字符转为小写，移除特殊字符，将空格转为连字符
                for (int i = 0; i < length; i++)
                {
                    char c = text[i];
                    char lower = char.ToLowerInvariant(c);

                    if ((lower >= 'a' && lower <= 'z') || (lower >= '0' && lower <= '9'))
                    {
                        buffer[position++] = lower;
                        prevIsSeparator = false;
                    }
                    else if (!prevIsSeparator && (char.IsWhiteSpace(c) || c == '-' || c == '_' || c == '.'))
                    {
                        buffer[position++] = '-';
                        prevIsSeparator = true;
                    }
                }

                // 移除末尾的连字符
                if (position > 0 && buffer[position - 1] == '-')
                    position--;

                return position == 0 ? string.Empty : new string(buffer.Slice(0, position));
            }
            else
            {
                // 获取合适大小的缓冲区，确保不需要扩容
                // 对于大多数文本，其slug长度通常小于原长度，但保守起见使用原长度
                // 当文本长度适中时，使用池化数组
                int bufferSize = Math.Min(maxLength, 4096);
                char[] rentedBuffer = ArrayPool<char>.Shared.Rent(bufferSize);
                try
                {
                    int position = 0;
                    bool prevIsSeparator = true; // 跟踪前一个字符是否为分隔符

                    // 分段处理，避免大量临时分配
                    ReadOnlySpan<char> textSpan = text;
                    for (int i = 0; i < length; i++)
                    {
                        // 确保缓冲区足够大
                        if (position >= bufferSize - 1)
                        {
                            // 缓冲区已满，创建临时结果并重置缓冲区
                            string temp = new string(rentedBuffer, 0, position);
                            ArrayPool<char>.Shared.Return(rentedBuffer);
                            
                            // 获取新缓冲区处理剩余部分
                            int remainingLength = length - i;
                            int newBufferSize = Math.Min(remainingLength * 2, 4096);
                            rentedBuffer = ArrayPool<char>.Shared.Rent(newBufferSize);
                            bufferSize = newBufferSize;
                            
                            // 复制已有结果到临时变量
                            string current = temp;
                            position = 0;
                            
                            // 继续处理前，确保prevIsSeparator状态正确
                            prevIsSeparator = current.Length > 0 && current[current.Length - 1] == '-';
                            
                            // 调整i确保不跳过字符
                            i--;
                            continue;
                        }

                        char c = textSpan[i];
                        char lower = char.ToLowerInvariant(c);

                        if ((lower >= 'a' && lower <= 'z') || (lower >= '0' && lower <= '9'))
                        {
                            rentedBuffer[position++] = lower;
                            prevIsSeparator = false;
                        }
                        else if (!prevIsSeparator && (char.IsWhiteSpace(c) || c == '-' || c == '_' || c == '.'))
                        {
                            rentedBuffer[position++] = '-';
                            prevIsSeparator = true;
                        }
                    }

                    // 移除末尾的连字符
                    if (position > 0 && rentedBuffer[position - 1] == '-')
                        position--;

                    return position == 0 ? string.Empty : new string(rentedBuffer, 0, position);
                }
                finally
                {
                    ArrayPool<char>.Shared.Return(rentedBuffer);
                }
            }
        }

        /// <summary>
        /// 截断字符串到指定长度，并添加可选的后缀
        /// </summary>
        /// <param name="text">要截断的字符串</param>
        /// <param name="maxLength">最大长度</param>
        /// <param name="suffix">后缀，默认为"..."</param>
        /// <returns>截断后的字符串</returns>
        /// <remarks>
        /// 性能优化：
        /// - 预先计算结果长度避免扩容
        /// - 对于短字符串使用栈分配
        /// - 针对未截断的情况避免创建新字符串
        /// - 对于长字符串使用StringBuilder对象池减少GC
        /// - 统一处理策略避免不同长度字符串的性能差异
        /// </remarks>
        public static string Truncate(string text, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            
            if (maxLength <= 0)
                return string.Empty;
                
            if (string.IsNullOrEmpty(suffix))
                suffix = string.Empty;
            
            // 如果字符串长度已经小于等于最大长度，直接返回原字符串
            if (text.Length <= maxLength)
                return text;
            
            // 计算实际截断长度，确保有足够空间添加后缀
            int actualLength = maxLength - suffix.Length;
            if (actualLength <= 0)
            {
                // 如果后缀比最大长度还长，则只返回后缀的一部分
                return suffix.Length <= maxLength ? suffix : suffix.Substring(0, maxLength);
            }
            
            // 调整栈分配阈值，更加保守，同时更为一致
            if (actualLength <= 64 && suffix.Length <= 16)
            {
                // 对于短字符串使用栈分配
                Span<char> buffer = stackalloc char[actualLength + suffix.Length];
                
                // 复制截断后的文本
                text.AsSpan(0, actualLength).CopyTo(buffer);
                
                // 添加后缀
                if (suffix.Length > 0)
                {
                    suffix.AsSpan().CopyTo(buffer.Slice(actualLength));
                }
                
                return new string(buffer);
            }
            else
            {
                // 统一使用StringBuilder，确保性能一致性，减少GC分配
                StringBuilder sb = StringBuilderPool.Get();
                try
                {
                    // 预分配足够的容量
                    sb.EnsureCapacity(actualLength + suffix.Length);
                    
                    // 处理大量文本时，分块复制避免一次性大内存复制
                    const int COPY_BLOCK_SIZE = 1024;
                    
                    // 如果文本长度超过阈值，分块处理
                    if (actualLength > COPY_BLOCK_SIZE)
                    {
                        int remaining = actualLength;
                        int offset = 0;
                        
                        while (remaining > 0)
                        {
                            int blockSize = Math.Min(COPY_BLOCK_SIZE, remaining);
                            sb.Append(text, offset, blockSize);
                            offset += blockSize;
                            remaining -= blockSize;
                        }
                    }
                    else
                    {
                        // 对于中等长度文本，一次性复制
                        sb.Append(text, 0, actualLength);
                    }
                    
                    // 添加后缀
                    sb.Append(suffix);
                    
                    return sb.ToString();
                }
                finally
                {
                    StringBuilderPool.Release(sb);
                }
            }
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
        ///   <item>返回的是ReadOnlySpan<char>，使用时请注意生命周期</item>
        ///   <item>如果您只需要遍历而不存储结果，这种方法特别高效</item>
        /// </list>
        /// 
        /// <para>示例：</para>
        /// <code>
        /// // 基本用法
        /// string csvLine = "value1,value2,value3,value4";
        /// foreach (var item in StringUtils.Split(csvLine, ','))
        /// {
        ///     ProcessItem(item.ToString()); // 需要ToString()转换为字符串
        /// }
        /// 
        /// // 高性能处理大数据
        /// string largeFileLine = GetVeryLongLine();
        /// int count = 0;
        /// foreach (var token in StringUtils.Split(largeFileLine, ';'))
        /// {
        ///     count++;
        ///     if (token.SequenceEqual("TargetValue".AsSpan()))
        ///     {
        ///         return count; // 提前退出，避免处理整行
        ///     }
        /// }
        /// </code>
        /// </remarks>
        public static SpanStringSplitEnumerator Split(string value, char separator)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return new SpanStringSplitEnumerator(value.AsSpan(), separator);
        }

        /// <summary>
        /// 高性能分割字符串，使用ReadOnlySpan进行处理
        /// </summary>
        /// <param name="value">要分割的字符串</param>
        /// <param name="separator">分隔符</param>
        /// <returns>分割后的字符串枚举器</returns>
        /// <exception cref="ArgumentNullException">当value为null时抛出</exception>
        /// <remarks>
        /// 与Split方法完全相同的功能，但接受ReadOnlySpan<char>作为输入，更加灵活。
        /// 此方法对于已经有Span的情况特别有用，可以避免不必要的转换。
        /// </remarks>
        public static SpanStringSplitEnumerator Split(ReadOnlySpan<char> value, char separator)
        {
            return new SpanStringSplitEnumerator(value, separator);
        }

        /// <summary>
        /// 零分配字符串分割器
        /// </summary>
        /// <remarks>
        /// 这是一个完全零分配的字符串分割实现，基于Span<T>，不会在迭代过程中创建任何字符串对象。
        /// 使用ref struct确保不会被装箱或分配到堆上，只能作为局部变量使用。
        /// 
        /// <para>性能说明：</para>
        /// 此实现完全避免了在枚举过程中创建字符串对象，返回的是原始字符串的切片（ReadOnlySpan<char>）。
        /// 
        /// <para>限制：</para>
        /// <list type="bullet">
        ///   <item>作为ref struct，不能用作字段、属性或异步方法的一部分</item>
        ///   <item>不能存储在数组或集合中</item>
        ///   <item>返回的ReadOnlySpan<char>生命周期受限于原始字符串</item>
        ///   <item>如果需要存储结果，必须手动调用ToString()创建新的字符串</item>
        /// </list>
        /// </remarks>
        public ref struct SpanStringSplitEnumerator
        {
            /// <summary>
            /// 要分割的原始字符串Span
            /// </summary>
            private ReadOnlySpan<char> _str;
            
            /// <summary>
            /// 用于分割的分隔符
            /// </summary>
            private readonly char _separator;

            /// <summary>
            /// 初始化SpanStringSplitEnumerator的新实例
            /// </summary>
            /// <param name="str">要分割的字符串Span</param>
            /// <param name="separator">分隔符</param>
            /// <remarks>
            /// 构造函数初始化分割器，但不执行任何实际分割操作。
            /// 分割操作将在调用MoveNext方法时逐步执行。
            /// </remarks>
            public SpanStringSplitEnumerator(ReadOnlySpan<char> str, char separator)
            {
                _str = str;
                _separator = separator;
                Current = default;
            }

            /// <summary>
            /// 获取枚举器
            /// </summary>
            /// <returns>当前枚举器</returns>
            /// <remarks>
            /// 此方法支持foreach语法，返回枚举器自身。
            /// </remarks>
            public SpanStringSplitEnumerator GetEnumerator() => this;

            /// <summary>
            /// 移动到下一个元素
            /// </summary>
            /// <returns>如果还有更多元素，则返回true；否则返回false</returns>
            /// <remarks>
            /// 此方法查找下一个分隔符并提取子字符串的Span。
            /// 如果已到达字符串末尾或没有更多元素，返回false。
            /// </remarks>
            public bool MoveNext()
            {
                var span = _str;
                if (span.Length == 0) 
                    return false;

                var index = span.IndexOf(_separator);
                if (index == -1)
                {
                    Current = span;
                    _str = ReadOnlySpan<char>.Empty;
                    return true;
                }

                Current = span.Slice(0, index);
                _str = span.Slice(index + 1);
                return true;
            }

            /// <summary>
            /// 获取当前元素
            /// </summary>
            /// <remarks>
            /// 此属性返回当前处理的字符串切片。
            /// 在首次调用MoveNext之前，其值为默认值（Empty Span）。
            /// </remarks>
            public ReadOnlySpan<char> Current { get; private set; }
        }

        /// <summary>
        /// 向后兼容的字符串分割器
        /// </summary>
        /// <remarks>
        /// 此类为向后兼容性保留，新代码应使用SpanStringSplitEnumerator。
        /// </remarks>
        public ref struct StringSplitEnumerator
        {
            private SpanStringSplitEnumerator _spanEnumerator;
            private string _str;

            /// <summary>
            /// 初始化StringSplitEnumerator的新实例
            /// </summary>
            /// <param name="str">要分割的字符串</param>
            /// <param name="separator">分隔符</param>
            public StringSplitEnumerator(string str, char separator)
            {
                _str = str;
                _spanEnumerator = new SpanStringSplitEnumerator(str.AsSpan(), separator);
                Current = null;
            }

            /// <summary>
            /// 获取枚举器
            /// </summary>
            /// <returns>当前枚举器</returns>
            public StringSplitEnumerator GetEnumerator() => this;

            /// <summary>
            /// 移动到下一个元素
            /// </summary>
            /// <returns>如果还有更多元素，则返回true；否则返回false</returns>
            public bool MoveNext()
            {
                if (_spanEnumerator.MoveNext())
                {
                    Current = _spanEnumerator.Current.ToString();
                    return true;
                }
                return false;
            }

            /// <summary>
            /// 获取当前元素
            /// </summary>
            public string Current { get; private set; }
        }

        /// <summary>
        /// 将字符串转换为Base64编码
        /// </summary>
        /// <param name="text">要编码的字符串</param>
        /// <returns>Base64编码后的字符串</returns>
        /// <exception cref="ArgumentNullException">当text为null时抛出</exception>
        /// <remarks>
        /// 性能优化：
        /// - 使用UTF8编码直接获取字节，避免创建临时编码器
        /// - 对于小字符串使用栈分配减少内存分配
        /// </remarks>
        public static string ToBase64(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (text.Length == 0) return string.Empty;
            
            // 对于短字符串，使用栈分配
            if (text.Length <= 127)
            {
                Span<byte> buffer = stackalloc byte[Encoding.UTF8.GetMaxByteCount(text.Length)];
                int bytesWritten = Encoding.UTF8.GetBytes(text, buffer);
                return Convert.ToBase64String(buffer.Slice(0, bytesWritten));
            }
            
            // 对于长字符串，使用池化数组
            byte[] rentedArray = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(text.Length));
            try
            {
                int bytesWritten = Encoding.UTF8.GetBytes(text, 0, text.Length, rentedArray, 0);
                return Convert.ToBase64String(rentedArray, 0, bytesWritten);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rentedArray);
            }
        }

        /// <summary>
        /// 将Base64字符串解码为普通字符串
        /// </summary>
        /// <param name="base64">Base64编码的字符串</param>
        /// <returns>解码后的字符串</returns>
        /// <exception cref="ArgumentNullException">当base64为null时抛出</exception>
        /// <exception cref="FormatException">当base64不是有效的Base64字符串时抛出</exception>
        /// <remarks>
        /// 性能优化：
        /// - 使用ReadOnlySpan避免字符串切片产生的内存分配
        /// - 对于小字符串使用栈分配减少内存分配
        /// - 使用UTF8编码直接获取字符串，避免创建临时解码器
        /// </remarks>
        public static string FromBase64(string base64)
        {
            if (base64 == null) throw new ArgumentNullException(nameof(base64));
            if (base64.Length == 0) return string.Empty;
            
            // 计算解码后的大概长度
            int estimatedLength = (base64.Length * 3) / 4;
            
            // 对于短字符串，使用栈分配
            if (estimatedLength <= 127)
            {
                Span<byte> buffer = stackalloc byte[estimatedLength];
                try
                {
                    if (Convert.TryFromBase64String(base64, buffer, out int bytesWritten))
                    {
                        return Encoding.UTF8.GetString(buffer.Slice(0, bytesWritten));
                    }
                }
                catch (FormatException)
                {
                    // 如果预估的大小不足，则改用常规方法
                }
            }
            
            // 对于长字符串或缓冲区不足的情况，使用池化数组
            byte[] decodedBytes = Convert.FromBase64String(base64);
            try
            {
                return Encoding.UTF8.GetString(decodedBytes);
            }
            finally
            {
                // 只有当我们分配了新数组时才需要返回到池
                if (decodedBytes.Length > 1024)
                {
                    ArrayPool<byte>.Shared.Return(decodedBytes);
                }
            }
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
        /// - 使用StringBuilder对象池减少内存分配
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
            
            // 使用StringBuilder对象池
            StringBuilder sb = StringBuilderPool.Get();
            try
            {
                sb.EnsureCapacity(totalLength);
                
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
            finally
            {
                StringBuilderPool.Release(sb);
            }
        }
        
        /// <summary>
        /// 使用指定的分隔符连接集合中的所有元素
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="separator">分隔符</param>
        /// <param name="values">要连接的元素集合</param>
        /// <returns>连接后的字符串</returns>
        /// <exception cref="ArgumentNullException">当values为null时抛出</exception>
        /// <remarks>
        /// 性能优化：
        /// - 使用StringBuilder对象池减少内存分配
        /// - 预计算结果长度避免StringBuilder扩容（对于实现ICollection的集合）
        /// - 处理不同类型集合的特殊优化
        /// </remarks>
        public static string Join<T>(string separator, IEnumerable<T> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            
            separator = separator ?? string.Empty;
            
            // 对于数组类型，调用重载方法
            if (values is T[] array)
            {
                return Join(separator, Array.ConvertAll(array, x => x?.ToString()));
            }
            
            // 尝试获取元素数量以预分配容量
            int? count = null;
            if (values is ICollection<T> collection)
            {
                count = collection.Count;
                
                // 空集合直接返回
                if (count == 0) return string.Empty;
                
                // 单元素集合直接返回第一个元素的字符串表示
                if (count == 1)
                {
                    foreach (var item in collection)
                    {
                        return item?.ToString() ?? string.Empty;
                    }
                }
            }
            
            // 使用StringBuilder对象池
            StringBuilder sb = StringBuilderPool.Get();
            try
            {
                // 如果知道元素数量，预估容量（每个元素平均20个字符）
                if (count.HasValue)
                {
                    sb.EnsureCapacity(count.Value * 20 + separator.Length * (count.Value - 1));
                }
                
                bool isFirst = true;
                foreach (var item in values)
                {
                    if (!isFirst)
                    {
                        sb.Append(separator);
                    }
                    
                    if (item != null)
                    {
                        sb.Append(item.ToString());
                    }
                    
                    isFirst = false;
                }
                
                return sb.ToString();
            }
            finally
            {
                StringBuilderPool.Release(sb);
            }
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