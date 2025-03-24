using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TByd.Core.Utils.Runtime;

namespace TByd.Core.Utils.Tests.Editor.Framework
{
    /// <summary>
    /// 测试数据生成器，用于生成各种类型的测试数据
    /// </summary>
    public static class TestDataGenerator
    {
        private static readonly System.Random _random = new System.Random();
        private static readonly object _lock = new object();
        
        // 常用字符集
        private static readonly char[] UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private static readonly char[] LowercaseLetters = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        private static readonly char[] Digits = "0123456789".ToCharArray();
        private static readonly char[] SpecialChars = "!@#$%^&*()_+-=[]{}|;:,./<>?".ToCharArray();
        private static readonly char[] ChineseChars = "的一是不了人我在有他这为之大来以个中上们到说国和地也子时道出而要于就下得可你年生自会那后能对着事其里所去行过家十用发天如然作方成者多日都三小军二无同么经法当起与好看学进种将还分此心前面又定见只主没公从问使明力尔把等产新合同工己长提事同受彩电军复".ToCharArray();
        
        // 缓存的单词，用于生成逼真的文本
        private static readonly string[] CommonWords = new string[]
        {
            "the", "be", "to", "of", "and", "a", "in", "that", "have", "I", 
            "it", "for", "not", "on", "with", "he", "as", "you", "do", "at", 
            "this", "but", "his", "by", "from", "they", "we", "say", "her", "she", 
            "or", "an", "will", "my", "one", "all", "would", "there", "their", "what", 
            "so", "up", "out", "if", "about", "who", "get", "which", "go", "me",
            "unity", "game", "develop", "test", "object", "vector", "transform", "component"
        };
        
        /// <summary>
        /// 生成指定长度的随机字符串
        /// </summary>
        /// <param name="length">字符串长度</param>
        /// <param name="includeSpecialChars">是否包含特殊字符</param>
        /// <returns>随机字符串</returns>
        public static string GenerateString(int length, bool includeSpecialChars = false)
        {
            if (length <= 0)
                throw new ArgumentException("长度必须大于0", nameof(length));
                
            var chars = new char[length];
            lock (_lock)
            {
                for (int i = 0; i < length; i++)
                {
                    int charType = _random.Next(0, includeSpecialChars ? 4 : 3);
                    chars[i] = charType switch
                    {
                        0 => UppercaseLetters[_random.Next(UppercaseLetters.Length)],
                        1 => LowercaseLetters[_random.Next(LowercaseLetters.Length)],
                        2 => Digits[_random.Next(Digits.Length)],
                        _ => SpecialChars[_random.Next(SpecialChars.Length)]
                    };
                }
            }
            return new string(chars);
        }
        
        /// <summary>
        /// 生成指定长度的随机中文字符串
        /// </summary>
        /// <param name="length">字符串长度</param>
        /// <returns>随机中文字符串</returns>
        public static string GenerateChineseString(int length)
        {
            if (length <= 0)
                throw new ArgumentException("长度必须大于0", nameof(length));
                
            var chars = new char[length];
            lock (_lock)
            {
                for (int i = 0; i < length; i++)
                {
                    chars[i] = ChineseChars[_random.Next(ChineseChars.Length)];
                }
            }
            return new string(chars);
        }
        
        /// <summary>
        /// 生成大型文本块，包含多个段落和句子
        /// </summary>
        /// <param name="paragraphCount">段落数</param>
        /// <param name="sentencesPerParagraph">每段句子数</param>
        /// <param name="averageWordsPerSentence">平均每句话的单词数</param>
        /// <returns>生成的文本</returns>
        public static string GenerateLargeText(int paragraphCount = 3, int sentencesPerParagraph = 5, int averageWordsPerSentence = 10)
        {
            var sb = new StringBuilder();
            
            for (int p = 0; p < paragraphCount; p++)
            {
                for (int s = 0; s < sentencesPerParagraph; s++)
                {
                    // 每句话的单词数在平均值上下浮动
                    int wordCount;
                    lock (_lock)
                    {
                        wordCount = _random.Next(averageWordsPerSentence - 2, averageWordsPerSentence + 3);
                    }
                    
                    for (int w = 0; w < wordCount; w++)
                    {
                        string word;
                        lock (_lock)
                        {
                            word = CommonWords[_random.Next(CommonWords.Length)];
                            
                            // 第一个单词首字母大写
                            if (w == 0)
                            {
                                word = char.ToUpper(word[0]) + word.Substring(1);
                            }
                        }
                        
                        sb.Append(word);
                        
                        // 单词之间添加空格，最后一个单词不加
                        if (w < wordCount - 1)
                            sb.Append(' ');
                    }
                    
                    // 添加句号和空格，最后一句不加空格
                    sb.Append('.');
                    if (s < sentencesPerParagraph - 1)
                        sb.Append(' ');
                }
                
                // 段落之间添加换行，最后一段不加
                if (p < paragraphCount - 1)
                    sb.AppendLine().AppendLine();
            }
            
            return sb.ToString();
        }
        
        /// <summary>
        /// 生成指定范围内的随机整数
        /// </summary>
        /// <param name="min">最小值（包含）</param>
        /// <param name="max">最大值（不包含）</param>
        /// <returns>随机整数</returns>
        public static int GenerateInt(int min = int.MinValue, int max = int.MaxValue)
        {
            lock (_lock)
            {
                return _random.Next(min, max);
            }
        }
        
        /// <summary>
        /// 生成随机浮点数
        /// </summary>
        /// <param name="min">最小值（包含）</param>
        /// <param name="max">最大值（包含）</param>
        /// <returns>随机浮点数</returns>
        public static float GenerateFloat(float min = 0f, float max = 1f)
        {
            lock (_lock)
            {
                return min + (float)_random.NextDouble() * (max - min);
            }
        }
        
        /// <summary>
        /// 生成随机布尔值
        /// </summary>
        /// <param name="trueChance">返回true的概率，0到1之间</param>
        /// <returns>随机布尔值</returns>
        public static bool GenerateBool(float trueChance = 0.5f)
        {
            if (trueChance < 0f || trueChance > 1f)
                throw new ArgumentOutOfRangeException(nameof(trueChance), "概率必须在0到1之间");
                
            lock (_lock)
            {
                return _random.NextDouble() < trueChance;
            }
        }
        
        /// <summary>
        /// 生成随机Vector2
        /// </summary>
        /// <param name="minValue">最小值</param>
        /// <param name="maxValue">最大值</param>
        /// <returns>随机Vector2</returns>
        public static Vector2 GenerateVector2(float minValue = -10f, float maxValue = 10f)
        {
            return new Vector2(
                GenerateFloat(minValue, maxValue),
                GenerateFloat(minValue, maxValue)
            );
        }
        
        /// <summary>
        /// 生成随机Vector3
        /// </summary>
        /// <param name="minValue">最小值</param>
        /// <param name="maxValue">最大值</param>
        /// <returns>随机Vector3</returns>
        public static Vector3 GenerateVector3(float minValue = -10f, float maxValue = 10f)
        {
            return new Vector3(
                GenerateFloat(minValue, maxValue),
                GenerateFloat(minValue, maxValue),
                GenerateFloat(minValue, maxValue)
            );
        }
        
        /// <summary>
        /// 生成随机Color
        /// </summary>
        /// <param name="includeAlpha">是否包含Alpha通道随机值</param>
        /// <returns>随机Color</returns>
        public static Color GenerateColor(bool includeAlpha = false)
        {
            return new Color(
                GenerateFloat(),
                GenerateFloat(),
                GenerateFloat(),
                includeAlpha ? GenerateFloat() : 1f
            );
        }
        
        /// <summary>
        /// 生成随机单位四元数（表示随机旋转）
        /// </summary>
        /// <returns>随机四元数</returns>
        public static Quaternion GenerateQuaternion()
        {
            return Quaternion.Euler(
                GenerateFloat(0f, 360f),
                GenerateFloat(0f, 360f),
                GenerateFloat(0f, 360f)
            );
        }
        
        /// <summary>
        /// 生成指定大小的随机数组
        /// </summary>
        /// <typeparam name="T">数组元素类型</typeparam>
        /// <param name="size">数组大小</param>
        /// <param name="generator">元素生成器函数</param>
        /// <returns>生成的数组</returns>
        public static T[] GenerateArray<T>(int size, Func<int, T> generator)
        {
            if (size < 0)
                throw new ArgumentException("数组大小不能为负数", nameof(size));
                
            var result = new T[size];
            for (int i = 0; i < size; i++)
            {
                result[i] = generator(i);
            }
            return result;
        }
        
        /// <summary>
        /// 生成指定大小的随机列表
        /// </summary>
        /// <typeparam name="T">列表元素类型</typeparam>
        /// <param name="size">列表大小</param>
        /// <param name="generator">元素生成器函数</param>
        /// <returns>生成的列表</returns>
        public static List<T> GenerateList<T>(int size, Func<int, T> generator)
        {
            if (size < 0)
                throw new ArgumentException("列表大小不能为负数", nameof(size));
                
            var result = new List<T>(size);
            for (int i = 0; i < size; i++)
            {
                result.Add(generator(i));
            }
            return result;
        }
        
        /// <summary>
        /// 生成随机日期时间
        /// </summary>
        /// <param name="minYear">最小年份</param>
        /// <param name="maxYear">最大年份</param>
        /// <returns>随机日期时间</returns>
        public static DateTime GenerateDateTime(int minYear = 2000, int maxYear = 2030)
        {
            int year = GenerateInt(minYear, maxYear + 1);
            int month = GenerateInt(1, 13);
            int day = GenerateInt(1, DateTime.DaysInMonth(year, month) + 1);
            int hour = GenerateInt(0, 24);
            int minute = GenerateInt(0, 60);
            int second = GenerateInt(0, 60);
            
            return new DateTime(year, month, day, hour, minute, second);
        }
        
        /// <summary>
        /// 生成随机TimeSpan
        /// </summary>
        /// <param name="maxDays">最大天数</param>
        /// <returns>随机TimeSpan</returns>
        public static TimeSpan GenerateTimeSpan(int maxDays = 30)
        {
            return TimeSpan.FromSeconds(GenerateInt(0, maxDays * 24 * 60 * 60));
        }
        
        /// <summary>
        /// 从提供的枚举值中随机选择一个
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns>随机枚举值</returns>
        public static T GenerateEnum<T>() where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            lock (_lock)
            {
                return (T)values.GetValue(_random.Next(values.Length));
            }
        }
        
        /// <summary>
        /// 生成随机URL
        /// </summary>
        /// <returns>随机URL</returns>
        public static string GenerateUrl()
        {
            string[] protocols = { "http", "https" };
            string[] domains = { "example.com", "test.org", "demo.net", "sample.io", "unittest.dev" };
            string[] paths = { "", "api", "users", "products", "services", "blog", "docs" };
            
            string protocol;
            string domain;
            string path;
            
            lock (_lock)
            {
                protocol = protocols[_random.Next(protocols.Length)];
                domain = domains[_random.Next(domains.Length)];
                path = _random.Next(3) == 0 ? "" : paths[_random.Next(paths.Length)];
            }
            
            var url = $"{protocol}://{domain}";
            if (!string.IsNullOrEmpty(path))
                url += $"/{path}";
                
            if (GenerateBool(0.3f))
                url += $"/{GenerateString(5, false).ToLower()}";
                
            if (GenerateBool(0.2f))
                url += $"?id={GenerateInt(1, 1000)}";
                
            return url;
        }
        
        /// <summary>
        /// 生成随机电子邮件地址
        /// </summary>
        /// <returns>随机电子邮件地址</returns>
        public static string GenerateEmail()
        {
            string[] domains = { "example.com", "test.org", "gmail.com", "outlook.com", "company.net" };
            
            string username = GenerateString(GenerateInt(5, 10), false).ToLower();
            string domain;
            
            lock (_lock)
            {
                domain = domains[_random.Next(domains.Length)];
            }
            
            return $"{username}@{domain}";
        }
        
        /// <summary>
        /// 生成随机IP地址
        /// </summary>
        /// <returns>随机IP地址</returns>
        public static string GenerateIpAddress()
        {
            return $"{GenerateInt(1, 256)}.{GenerateInt(0, 256)}.{GenerateInt(0, 256)}.{GenerateInt(0, 256)}";
        }
        
        /// <summary>
        /// 生成随机的人名
        /// </summary>
        /// <returns>随机人名</returns>
        public static string GeneratePersonName()
        {
            string[] firstNames = { "John", "Jane", "Michael", "Emily", "David", "Sarah", "Robert", "Lisa", "William", "Jessica" };
            string[] lastNames = { "Smith", "Johnson", "Williams", "Jones", "Brown", "Davis", "Miller", "Wilson", "Moore", "Taylor" };
            
            string firstName;
            string lastName;
            
            lock (_lock)
            {
                firstName = firstNames[_random.Next(firstNames.Length)];
                lastName = lastNames[_random.Next(lastNames.Length)];
            }
            
            return $"{firstName} {lastName}";
        }
        
        /// <summary>
        /// 生成随机中文人名
        /// </summary>
        /// <returns>随机中文人名</returns>
        public static string GenerateChinesePersonName()
        {
            string[] familyNames = { "李", "王", "张", "刘", "陈", "杨", "黄", "赵", "吴", "周", "徐", "孙", "马", "朱", "胡", "郭", "何", "林" };
            
            string familyName;
            
            lock (_lock)
            {
                familyName = familyNames[_random.Next(familyNames.Length)];
            }
            
            int nameLength = GenerateBool(0.7f) ? 2 : 1;
            string givenName = GenerateChineseString(nameLength);
            
            return familyName + givenName;
        }
        
        /// <summary>
        /// 设置随机种子，用于可重现的随机数生成
        /// </summary>
        /// <param name="seed">随机种子</param>
        public static void SetSeed(int seed)
        {
            lock (_lock)
            {
                // 不能直接重新初始化_random，所以使用一个新的随机生成器来模拟设置种子的效果
                var tempRandom = new System.Random(seed);
                // 消耗一些随机数，使状态变化
                for (int i = 0; i < 10; i++)
                {
                    tempRandom.Next();
                }
                
                // 设置Unity的随机种子
                UnityEngine.Random.InitState(seed);
            }
        }
    }
} 