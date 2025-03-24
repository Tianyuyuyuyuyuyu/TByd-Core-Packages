using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace TByd.Core.Utils.Runtime
{
    /// <summary>
    /// 时间处理工具类
    /// </summary>
    /// <remarks>
    /// 提供时间格式化、时区转换、计时器、游戏时间与现实时间转换等功能。
    /// </remarks>
    public static class TimeUtils
    {
        #region 时间格式化

        /// <summary>
        /// 格式化日期时间为指定格式的字符串
        /// </summary>
        /// <param name="dateTime">要格式化的日期时间</param>
        /// <param name="format">格式字符串，如"yyyy-MM-dd HH:mm:ss"</param>
        /// <param name="culture">区域信息，默认为当前区域</param>
        /// <returns>格式化后的字符串</returns>
        public static string FormatDateTime(DateTime dateTime, string format = "yyyy-MM-dd HH:mm:ss", CultureInfo culture = null)
        {
            culture ??= CultureInfo.CurrentCulture;
            return dateTime.ToString(format, culture);
        }
        
        /// <summary>
        /// 格式化时间跨度为友好的可读字符串
        /// </summary>
        /// <param name="timeSpan">时间跨度</param>
        /// <param name="includeMilliseconds">是否包含毫秒部分</param>
        /// <returns>格式化后的字符串，如"2小时30分钟15秒"</returns>
        public static string FormatTimeSpan(TimeSpan timeSpan, bool includeMilliseconds = false)
        {
            if (timeSpan.TotalDays >= 1)
            {
                return $"{(int)timeSpan.TotalDays}天{timeSpan.Hours}小时{timeSpan.Minutes}分钟{timeSpan.Seconds}秒";
            }
            
            if (timeSpan.TotalHours >= 1)
            {
                return $"{(int)timeSpan.TotalHours}小时{timeSpan.Minutes}分钟{timeSpan.Seconds}秒";
            }
            
            if (timeSpan.TotalMinutes >= 1)
            {
                return $"{(int)timeSpan.TotalMinutes}分钟{timeSpan.Seconds}秒";
            }
            
            if (includeMilliseconds)
            {
                return $"{timeSpan.Seconds}秒{timeSpan.Milliseconds}毫秒";
            }
            
            return $"{timeSpan.Seconds}秒";
        }
        
        /// <summary>
        /// 将时间跨度格式化为简洁的字符串
        /// </summary>
        /// <param name="timeSpan">时间跨度</param>
        /// <returns>简洁格式的时间字符串，如"02:30:15"或负时间"-02:30:15"</returns>
        public static string FormatTimeSpanCompact(TimeSpan timeSpan)
        {
            // 处理负时间跨度
            bool isNegative = timeSpan.TotalMilliseconds < 0;
            if (isNegative)
            {
                timeSpan = timeSpan.Negate(); // 使用正值进行格式化
            }
            
            string result;
            if (timeSpan.TotalDays >= 1)
            {
                result = $"{(int)timeSpan.TotalDays}:{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
            else
            {
                result = $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
            
            // 如果是负时间，添加负号
            return isNegative ? "-" + result : result;
        }
        
        /// <summary>
        /// 获取相对于当前时间的友好描述
        /// </summary>
        /// <param name="dateTime">要描述的时间</param>
        /// <returns>友好的相对时间描述，如"刚才"、"5分钟前"、"昨天"等</returns>
        public static string GetRelativeTimeDescription(DateTime dateTime)
        {
            DateTime now = DateTime.Now;
            TimeSpan diff = now - dateTime;
            
            if (diff.TotalSeconds < 0)
            {
                // 未来时间
                diff = diff.Negate();
                
                if (diff.TotalSeconds < 60)
                {
                    return "即将到来";
                }
                
                if (diff.TotalMinutes < 60)
                {
                    return $"{(int)diff.TotalMinutes}分钟后";
                }
                
                if (diff.TotalHours < 24)
                {
                    // 特殊检查：如果时间差超过23小时但不到24小时，认为是"明天"
                    if (diff.TotalHours >= 23)
                    {
                        return "1天后";
                    }
                    return $"{(int)diff.TotalHours}小时后";
                }
                
                // 接近整天数的处理（例如0.95天以上视为1天）
                if (diff.TotalDays < 1 && diff.TotalDays >= 0.95)
                {
                    return "1天后";
                }
                
                // 检查是否是明天（通过日期比较）
                if (dateTime.Date == now.Date.AddDays(1))
                {
                    return "1天后"; // 明确处理明天的情况，强制返回"1天后"
                }
                
                if (diff.TotalDays < 7)
                {
                    // 处理精度问题，确保接近整数的天数被正确处理
                    int days = (int)Math.Round(diff.TotalDays);
                    if (days == 0)
                    {
                        days = 1; // 如果四舍五入后为0，但已经通过了前面的小时判断，至少是1天
                    }
                    return $"{days}天后";
                }
                
                if (diff.TotalDays < 30)
                {
                    return $"{(int)(diff.TotalDays / 7)}周后";
                }
                
                if (diff.TotalDays < 365)
                {
                    return $"{(int)(diff.TotalDays / 30)}个月后";
                }
                
                return $"{(int)(diff.TotalDays / 365)}年后";
            }
            else
            {
                // 过去时间
                if (diff.TotalSeconds < 10)
                {
                    return "刚才";
                }
                
                if (diff.TotalSeconds < 60)
                {
                    return $"{(int)diff.TotalSeconds}秒前";
                }
                
                if (diff.TotalMinutes < 60)
                {
                    return $"{(int)diff.TotalMinutes}分钟前";
                }
                
                if (diff.TotalHours < 24)
                {
                    // 特殊检查：如果时间差超过23小时但不到24小时，认为是"昨天"
                    if (diff.TotalHours >= 23)
                    {
                        return "1天前";
                    }
                    return $"{(int)diff.TotalHours}小时前";
                }
                
                if (diff.TotalDays < 2)
                {
                    return "昨天";
                }
                
                if (diff.TotalDays < 7)
                {
                    return $"{(int)diff.TotalDays}天前";
                }
                
                if (diff.TotalDays < 30)
                {
                    return $"{(int)(diff.TotalDays / 7)}周前";
                }
                
                if (diff.TotalDays < 365)
                {
                    return $"{(int)(diff.TotalDays / 30)}个月前";
                }
                
                return $"{(int)(diff.TotalDays / 365)}年前";
            }
        }
        
        /// <summary>
        /// 尝试解析日期时间字符串
        /// </summary>
        /// <param name="dateTimeString">要解析的字符串</param>
        /// <param name="result">解析结果</param>
        /// <param name="formats">尝试的格式数组</param>
        /// <param name="culture">区域信息，默认为当前区域</param>
        /// <returns>如果成功解析则返回true，否则返回false</returns>
        public static bool TryParseDateTime(string dateTimeString, out DateTime result, string[] formats = null, CultureInfo culture = null)
        {
            result = DateTime.MinValue;
            
            if (string.IsNullOrEmpty(dateTimeString))
            {
                return false;
            }
            
            culture ??= CultureInfo.CurrentCulture;
            
            if (formats != null)
            {
                return DateTime.TryParseExact(dateTimeString, formats, culture, DateTimeStyles.None, out result);
            }
            
            return DateTime.TryParse(dateTimeString, culture, DateTimeStyles.None, out result);
        }

        #endregion

        #region 时区处理

        /// <summary>
        /// 将本地时间转换为UTC时间
        /// </summary>
        /// <param name="localDateTime">本地时间</param>
        /// <returns>UTC时间</returns>
        public static DateTime LocalToUtc(DateTime localDateTime)
        {
            if (localDateTime.Kind == DateTimeKind.Utc)
            {
                return localDateTime;
            }
            
            return TimeZoneInfo.ConvertTimeToUtc(localDateTime);
        }
        
        /// <summary>
        /// 将UTC时间转换为本地时间
        /// </summary>
        /// <param name="utcDateTime">UTC时间</param>
        /// <returns>本地时间</returns>
        public static DateTime UtcToLocal(DateTime utcDateTime)
        {
            if (utcDateTime.Kind == DateTimeKind.Local)
            {
                return utcDateTime;
            }
            
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, TimeZoneInfo.Local);
        }
        
        /// <summary>
        /// 将时间转换到指定时区
        /// </summary>
        /// <param name="dateTime">要转换的时间</param>
        /// <param name="timeZoneId">目标时区ID</param>
        /// <returns>转换后的时间</returns>
        public static DateTime ConvertToTimeZone(DateTime dateTime, string timeZoneId)
        {
            if (string.IsNullOrEmpty(timeZoneId))
            {
                throw new ArgumentException("时区ID不能为空", nameof(timeZoneId));
            }
            
            try
            {
                TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                return TimeZoneInfo.ConvertTime(dateTime, timeZone);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"无效的时区ID: {timeZoneId}", nameof(timeZoneId), ex);
            }
        }
        
        /// <summary>
        /// 获取当前系统支持的所有时区信息
        /// </summary>
        /// <returns>时区信息列表</returns>
        public static IReadOnlyList<TimeZoneInfo> GetSystemTimeZones()
        {
            return TimeZoneInfo.GetSystemTimeZones();
        }
        
        /// <summary>
        /// 获取当前UTC时间
        /// </summary>
        /// <returns>当前UTC时间</returns>
        public static DateTime GetUtcNow()
        {
            return DateTime.UtcNow;
        }

        #endregion

        #region 游戏时间与现实时间

        /// <summary>
        /// 游戏内日期时间
        /// </summary>
        public struct GameDateTime
        {
            /// <summary>
            /// 年
            /// </summary>
            public int Year;
            
            /// <summary>
            /// 月
            /// </summary>
            public int Month;
            
            /// <summary>
            /// 日
            /// </summary>
            public int Day;
            
            /// <summary>
            /// 小时
            /// </summary>
            public int Hour;
            
            /// <summary>
            /// 分钟
            /// </summary>
            public int Minute;
            
            /// <summary>
            /// 秒
            /// </summary>
            public int Second;
            
            /// <summary>
            /// 转换为System.DateTime
            /// </summary>
            /// <returns>DateTime对象</returns>
            public DateTime ToDateTime()
            {
                return new DateTime(Year, Month, Day, Hour, Minute, Second);
            }
            
            /// <summary>
            /// 从DateTime创建GameDateTime
            /// </summary>
            /// <param name="dateTime">DateTime对象</param>
            /// <returns>GameDateTime结构</returns>
            public static GameDateTime FromDateTime(DateTime dateTime)
            {
                return new GameDateTime
                {
                    Year = dateTime.Year,
                    Month = dateTime.Month,
                    Day = dateTime.Day,
                    Hour = dateTime.Hour,
                    Minute = dateTime.Minute,
                    Second = dateTime.Second
                };
            }
            
            /// <summary>
            /// 格式化为字符串
            /// </summary>
            /// <param name="format">格式字符串</param>
            /// <returns>格式化后的字符串</returns>
            public string ToString(string format = "yyyy-MM-dd HH:mm:ss")
            {
                return ToDateTime().ToString(format);
            }
        }
        
        /// <summary>
        /// 游戏内时间尺度（游戏时间相对于现实时间的流逝速度）
        /// </summary>
        private static float _gameTimeScale = 1.0f;
        
        /// <summary>
        /// 游戏内起始时间点
        /// </summary>
        private static DateTime _gameStartDateTime = DateTime.Now;
        
        /// <summary>
        /// 游戏内时间暂停标志
        /// </summary>
        private static bool _isGameTimePaused = false;
        
        /// <summary>
        /// 上次更新游戏时间的现实时间
        /// </summary>
        private static DateTime _lastRealTimeUpdate = DateTime.Now;
        
        /// <summary>
        /// 已流逝的游戏内时间（秒）
        /// </summary>
        private static double _elapsedGameSeconds = 0;
        
        /// <summary>
        /// 设置游戏时间尺度
        /// </summary>
        /// <param name="scale">时间尺度，默认为1.0表示正常速度</param>
        public static void SetGameTimeScale(float scale)
        {
            if (scale < 0)
            {
                throw new ArgumentException("时间尺度不能为负值", nameof(scale));
            }
            
            _gameTimeScale = scale;
        }
        
        /// <summary>
        /// 获取当前游戏时间尺度
        /// </summary>
        /// <returns>当前游戏时间尺度</returns>
        public static float GetGameTimeScale()
        {
            return _gameTimeScale;
        }
        
        /// <summary>
        /// 设置游戏起始时间
        /// </summary>
        /// <param name="startDateTime">游戏内起始时间</param>
        public static void SetGameStartTime(DateTime startDateTime)
        {
            _gameStartDateTime = startDateTime;
            _elapsedGameSeconds = 0;
            _lastRealTimeUpdate = DateTime.Now;
        }
        
        /// <summary>
        /// 暂停或恢复游戏内时间流逝
        /// </summary>
        /// <param name="isPaused">是否暂停</param>
        public static void SetGameTimePaused(bool isPaused)
        {
            if (_isGameTimePaused == isPaused)
            {
                return;
            }
            
            if (!isPaused)
            {
                // 恢复时更新上次更新时间，防止时间跳跃
                _lastRealTimeUpdate = DateTime.Now;
            }
            
            _isGameTimePaused = isPaused;
        }
        
        /// <summary>
        /// 更新游戏内时间
        /// </summary>
        /// <remarks>
        /// 此方法应在游戏Update循环中调用，以保持游戏内时间的更新
        /// </remarks>
        public static void UpdateGameTime()
        {
            if (_isGameTimePaused)
            {
                return;
            }
            
            DateTime now = DateTime.Now;
            TimeSpan realTimeDelta = now - _lastRealTimeUpdate;
            _lastRealTimeUpdate = now;
            
            _elapsedGameSeconds += realTimeDelta.TotalSeconds * _gameTimeScale;
        }
        
        /// <summary>
        /// 获取当前游戏内时间
        /// </summary>
        /// <returns>当前游戏内时间</returns>
        public static DateTime GetCurrentGameTime()
        {
            return _gameStartDateTime.AddSeconds(_elapsedGameSeconds);
        }
        
        /// <summary>
        /// 获取当前游戏内时间作为GameDateTime结构
        /// </summary>
        /// <returns>游戏内时间结构</returns>
        public static GameDateTime GetCurrentGameDateTime()
        {
            return GameDateTime.FromDateTime(GetCurrentGameTime());
        }
        
        /// <summary>
        /// 计算真实时间到游戏内时间的转换
        /// </summary>
        /// <param name="realTimeSpan">真实时间跨度</param>
        /// <returns>转换后的游戏内时间跨度</returns>
        public static TimeSpan RealTimeToGameTime(TimeSpan realTimeSpan)
        {
            return TimeSpan.FromSeconds(realTimeSpan.TotalSeconds * _gameTimeScale);
        }
        
        /// <summary>
        /// 计算游戏内时间到真实时间的转换
        /// </summary>
        /// <param name="gameTimeSpan">游戏内时间跨度</param>
        /// <returns>转换后的真实时间跨度</returns>
        public static TimeSpan GameTimeToRealTime(TimeSpan gameTimeSpan)
        {
            if (_gameTimeScale <= 0)
            {
                return TimeSpan.MaxValue; // 避免除零错误
            }
            
            return TimeSpan.FromSeconds(gameTimeSpan.TotalSeconds / _gameTimeScale);
        }

        #endregion

        #region 计时器

        /// <summary>
        /// 高精度计时器
        /// </summary>
        private static readonly Dictionary<string, Stopwatch> Timers = new Dictionary<string, Stopwatch>();
        
        /// <summary>
        /// 启动一个命名计时器
        /// </summary>
        /// <param name="timerName">计时器名称</param>
        public static void StartTimer(string timerName)
        {
            if (string.IsNullOrEmpty(timerName))
            {
                throw new ArgumentException("计时器名称不能为空", nameof(timerName));
            }
            
            if (Timers.TryGetValue(timerName, out Stopwatch timer))
            {
                timer.Restart();
            }
            else
            {
                timer = new Stopwatch();
                timer.Start();
                Timers[timerName] = timer;
            }
        }
        
        /// <summary>
        /// 停止一个命名计时器
        /// </summary>
        /// <param name="timerName">计时器名称</param>
        /// <returns>计时器运行的时间（毫秒），如果计时器不存在则返回-1</returns>
        public static long StopTimer(string timerName)
        {
            if (string.IsNullOrEmpty(timerName))
            {
                throw new ArgumentException("计时器名称不能为空", nameof(timerName));
            }
            
            if (Timers.TryGetValue(timerName, out Stopwatch timer))
            {
                timer.Stop();
                return timer.ElapsedMilliseconds;
            }
            
            return -1;
        }
        
        /// <summary>
        /// 获取计时器已运行的时间（毫秒）
        /// </summary>
        /// <param name="timerName">计时器名称</param>
        /// <returns>计时器已运行的时间（毫秒），如果计时器不存在则返回-1</returns>
        public static long GetTimerElapsedMilliseconds(string timerName)
        {
            if (string.IsNullOrEmpty(timerName))
            {
                throw new ArgumentException("计时器名称不能为空", nameof(timerName));
            }
            
            if (Timers.TryGetValue(timerName, out Stopwatch timer))
            {
                return timer.ElapsedMilliseconds;
            }
            
            return -1;
        }
        
        /// <summary>
        /// 重置一个命名计时器
        /// </summary>
        /// <param name="timerName">计时器名称</param>
        /// <returns>如果计时器存在并被重置则返回true，否则返回false</returns>
        public static bool ResetTimer(string timerName)
        {
            if (string.IsNullOrEmpty(timerName))
            {
                throw new ArgumentException("计时器名称不能为空", nameof(timerName));
            }
            
            if (Timers.TryGetValue(timerName, out Stopwatch timer))
            {
                timer.Reset();
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 检查计时器是否正在运行
        /// </summary>
        /// <param name="timerName">计时器名称</param>
        /// <returns>如果计时器存在且正在运行则返回true，否则返回false</returns>
        public static bool IsTimerRunning(string timerName)
        {
            if (string.IsNullOrEmpty(timerName))
            {
                throw new ArgumentException("计时器名称不能为空", nameof(timerName));
            }
            
            if (Timers.TryGetValue(timerName, out Stopwatch timer))
            {
                return timer.IsRunning;
            }
            
            return false;
        }
        
        /// <summary>
        /// 使用计时器测量代码块执行时间并记录
        /// </summary>
        /// <param name="timerName">计时器名称</param>
        /// <param name="action">要执行的代码块</param>
        /// <param name="logResult">是否在控制台记录执行时间</param>
        /// <returns>执行时间（毫秒）</returns>
        public static long MeasureExecutionTime(string timerName, Action action, bool logResult = true)
        {
            if (string.IsNullOrEmpty(timerName))
            {
                throw new ArgumentException("计时器名称不能为空", nameof(timerName));
            }
            
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            
            StartTimer(timerName);
            
            try
            {
                action();
            }
            finally
            {
                long elapsed = StopTimer(timerName);
                
                if (logResult)
                {
                    Debug.Log($"[TimeUtils] {timerName}: {elapsed}ms");
                }
            }
            
            return GetTimerElapsedMilliseconds(timerName);
        }

        #endregion

        #region 日期计算

        /// <summary>
        /// 获取指定月份的天数
        /// </summary>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <returns>该月的天数</returns>
        public static int GetDaysInMonth(int year, int month)
        {
            return DateTime.DaysInMonth(year, month);
        }
        
        /// <summary>
        /// 检查指定年份是否为闰年
        /// </summary>
        /// <param name="year">年</param>
        /// <returns>如果是闰年则返回true，否则返回false</returns>
        public static bool IsLeapYear(int year)
        {
            return DateTime.IsLeapYear(year);
        }
        
        /// <summary>
        /// 获取两个日期之间的天数差异
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>天数差异</returns>
        public static int GetDaysBetween(DateTime startDate, DateTime endDate)
        {
            return (int)(endDate.Date - startDate.Date).TotalDays;
        }
        
        /// <summary>
        /// 获取指定日期是一周中的第几天
        /// </summary>
        /// <param name="date">日期</param>
        /// <param name="firstDayOfWeek">一周的第一天，默认为周一</param>
        /// <returns>一周中的第几天（0-6）</returns>
        public static int GetDayOfWeek(DateTime date, DayOfWeek firstDayOfWeek = DayOfWeek.Monday)
        {
            int day = (int)date.DayOfWeek;
            int firstDay = (int)firstDayOfWeek;
            
            return (day - firstDay + 7) % 7;
        }
        
        /// <summary>
        /// 获取指定日期所在周的第一天
        /// </summary>
        /// <param name="date">日期</param>
        /// <param name="firstDayOfWeek">一周的第一天，默认为周一</param>
        /// <returns>所在周的第一天</returns>
        public static DateTime GetFirstDayOfWeek(DateTime date, DayOfWeek firstDayOfWeek = DayOfWeek.Monday)
        {
            int diff = GetDayOfWeek(date, firstDayOfWeek);
            return date.Date.AddDays(-diff);
        }
        
        /// <summary>
        /// 获取指定日期所在月的第一天
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>所在月的第一天</returns>
        public static DateTime GetFirstDayOfMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }
        
        /// <summary>
        /// 获取指定日期所在月的最后一天
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>所在月的最后一天</returns>
        public static DateTime GetLastDayOfMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, GetDaysInMonth(date.Year, date.Month));
        }
        
        /// <summary>
        /// 获取指定日期所在年的第一天
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>所在年的第一天</returns>
        public static DateTime GetFirstDayOfYear(DateTime date)
        {
            return new DateTime(date.Year, 1, 1);
        }
        
        /// <summary>
        /// 获取指定日期所在年的最后一天
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>所在年的最后一天</returns>
        public static DateTime GetLastDayOfYear(DateTime date)
        {
            return new DateTime(date.Year, 12, 31);
        }
        
        /// <summary>
        /// 向指定日期添加工作日
        /// </summary>
        /// <param name="date">日期</param>
        /// <param name="days">工作日数量</param>
        /// <param name="holidays">额外的节假日列表</param>
        /// <returns>添加工作日后的日期</returns>
        public static DateTime AddWorkDays(DateTime date, int days, IEnumerable<DateTime> holidays = null)
        {
            HashSet<DateTime> holidaySet = new HashSet<DateTime>();
            
            // 添加自定义节假日
            if (holidays != null)
            {
                foreach (DateTime holiday in holidays)
                {
                    holidaySet.Add(holiday.Date);
                }
            }
            
            DateTime result = date;
            int direction = Math.Sign(days);
            int remainingDays = Math.Abs(days);
            
            while (remainingDays > 0)
            {
                result = result.AddDays(direction);
                
                // 如果不是周末且不是节假日，则算作工作日
                if (result.DayOfWeek != DayOfWeek.Saturday && 
                    result.DayOfWeek != DayOfWeek.Sunday && 
                    !holidaySet.Contains(result.Date))
                {
                    remainingDays--;
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 检查指定日期是否是工作日
        /// </summary>
        /// <param name="date">日期</param>
        /// <param name="holidays">额外的节假日列表</param>
        /// <returns>如果是工作日则返回true，否则返回false</returns>
        public static bool IsWorkDay(DateTime date, IEnumerable<DateTime> holidays = null)
        {
            // 周末不是工作日
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                return false;
            }
            
            // 检查是否是节假日
            if (holidays != null)
            {
                foreach (DateTime holiday in holidays)
                {
                    if (holiday.Date == date.Date)
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }

        #endregion

        #region 性能计时统计

        private static readonly Dictionary<string, List<double>> PerformanceStats = new Dictionary<string, List<double>>();
        private static readonly Dictionary<string, Stopwatch> PerformanceTimers = new Dictionary<string, Stopwatch>();
        
        /// <summary>
        /// 开始性能统计计时
        /// </summary>
        /// <param name="statName">统计名称</param>
        public static void StartPerformanceMeasure(string statName)
        {
            if (string.IsNullOrEmpty(statName))
            {
                throw new ArgumentException("统计名称不能为空", nameof(statName));
            }
            
            if (!PerformanceTimers.TryGetValue(statName, out Stopwatch timer))
            {
                timer = new Stopwatch();
                PerformanceTimers[statName] = timer;
            }
            
            timer.Restart();
        }
        
        /// <summary>
        /// 结束性能统计计时并记录结果
        /// </summary>
        /// <param name="statName">统计名称</param>
        /// <returns>本次耗时（毫秒）</returns>
        public static double EndPerformanceMeasure(string statName)
        {
            if (string.IsNullOrEmpty(statName))
            {
                throw new ArgumentException("统计名称不能为空", nameof(statName));
            }
            
            if (!PerformanceTimers.TryGetValue(statName, out Stopwatch timer))
            {
                return -1;
            }
            
            timer.Stop();
            double elapsed = timer.Elapsed.TotalMilliseconds;
            
            if (!PerformanceStats.TryGetValue(statName, out List<double> stats))
            {
                stats = new List<double>();
                PerformanceStats[statName] = stats;
            }
            
            stats.Add(elapsed);
            return elapsed;
        }
        
        /// <summary>
        /// 获取性能统计结果
        /// </summary>
        /// <param name="statName">统计名称</param>
        /// <returns>统计结果的只读列表</returns>
        public static IReadOnlyList<double> GetPerformanceStats(string statName)
        {
            if (string.IsNullOrEmpty(statName))
            {
                throw new ArgumentException("统计名称不能为空", nameof(statName));
            }
            
            if (PerformanceStats.TryGetValue(statName, out List<double> stats))
            {
                return stats;
            }
            
            return Array.Empty<double>();
        }
        
        /// <summary>
        /// 获取性能统计平均值
        /// </summary>
        /// <param name="statName">统计名称</param>
        /// <returns>平均耗时（毫秒），如果没有数据则返回-1</returns>
        public static double GetPerformanceStatsAverage(string statName)
        {
            IReadOnlyList<double> stats = GetPerformanceStats(statName);
            
            if (stats.Count == 0)
            {
                return -1;
            }
            
            double sum = 0;
            foreach (double stat in stats)
            {
                sum += stat;
            }
            
            return sum / stats.Count;
        }
        
        /// <summary>
        /// 获取性能统计最小值
        /// </summary>
        /// <param name="statName">统计名称</param>
        /// <returns>最小耗时（毫秒），如果没有数据则返回-1</returns>
        public static double GetPerformanceStatsMin(string statName)
        {
            IReadOnlyList<double> stats = GetPerformanceStats(statName);
            
            if (stats.Count == 0)
            {
                return -1;
            }
            
            double min = double.MaxValue;
            foreach (double stat in stats)
            {
                if (stat < min)
                {
                    min = stat;
                }
            }
            
            return min;
        }
        
        /// <summary>
        /// 获取性能统计最大值
        /// </summary>
        /// <param name="statName">统计名称</param>
        /// <returns>最大耗时（毫秒），如果没有数据则返回-1</returns>
        public static double GetPerformanceStatsMax(string statName)
        {
            IReadOnlyList<double> stats = GetPerformanceStats(statName);
            
            if (stats.Count == 0)
            {
                return -1;
            }
            
            double max = double.MinValue;
            foreach (double stat in stats)
            {
                if (stat > max)
                {
                    max = stat;
                }
            }
            
            return max;
        }
        
        /// <summary>
        /// 清除指定名称的性能统计数据
        /// </summary>
        /// <param name="statName">统计名称</param>
        public static void ClearPerformanceStats(string statName)
        {
            if (string.IsNullOrEmpty(statName))
            {
                throw new ArgumentException("统计名称不能为空", nameof(statName));
            }
            
            if (PerformanceStats.ContainsKey(statName))
            {
                PerformanceStats[statName].Clear();
            }
        }
        
        /// <summary>
        /// 清除所有性能统计数据
        /// </summary>
        public static void ClearAllPerformanceStats()
        {
            PerformanceStats.Clear();
        }
        
        /// <summary>
        /// 生成性能统计报告
        /// </summary>
        /// <param name="includeRawData">是否包含原始数据</param>
        /// <returns>统计报告字符串</returns>
        public static string GeneratePerformanceReport(bool includeRawData = false)
        {
            if (PerformanceStats.Count == 0)
            {
                return "没有性能统计数据";
            }
            
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("===== 性能统计报告 =====");
            
            foreach (var kvp in PerformanceStats)
            {
                string statName = kvp.Key;
                List<double> stats = kvp.Value;
                
                if (stats.Count == 0)
                {
                    continue;
                }
                
                double avg = GetPerformanceStatsAverage(statName);
                double min = GetPerformanceStatsMin(statName);
                double max = GetPerformanceStatsMax(statName);
                
                sb.AppendLine($"[{statName}]");
                sb.AppendLine($"  样本数: {stats.Count}");
                sb.AppendLine($"  平均耗时: {avg:F4}ms");
                sb.AppendLine($"  最小耗时: {min:F4}ms");
                sb.AppendLine($"  最大耗时: {max:F4}ms");
                
                if (includeRawData && stats.Count > 0)
                {
                    sb.AppendLine("  原始数据:");
                    for (int i = 0; i < stats.Count; i++)
                    {
                        sb.AppendLine($"    {i+1}: {stats[i]:F4}ms");
                    }
                }
                
                sb.AppendLine();
            }
            
            return sb.ToString();
        }

        #endregion
    }
}
