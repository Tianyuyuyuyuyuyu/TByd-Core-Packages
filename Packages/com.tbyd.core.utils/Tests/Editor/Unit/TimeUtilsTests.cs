using System;
using System.Collections;
using NUnit.Framework;
using TByd.Core.Utils.Runtime;
using TByd.Core.Utils.Tests.Editor.Framework;
using UnityEngine.TestTools;

namespace TByd.Core.Utils.Tests.Editor.Unit
{
    /// <summary>
    /// TimeUtils 工具类的单元测试
    /// </summary>
    public class TimeUtilsTests : TestBase
    {
        #region FormatTimeSpan Tests
        
        [Test]
        public void FormatTimeSpan_Zero_ReturnsCorrectFormat()
        {
            // 准备
            TimeSpan timeSpan = TimeSpan.Zero;
            
            // 执行
            string result = TimeUtils.FormatTimeSpanCompact(timeSpan);
            
            // 验证
            Assert.AreEqual("00:00:00", result);
        }
        
        [Test]
        public void FormatTimeSpan_Hours_ReturnsCorrectFormat()
        {
            // 准备
            TimeSpan timeSpan = new TimeSpan(5, 30, 15);
            
            // 执行
            string result = TimeUtils.FormatTimeSpanCompact(timeSpan);
            
            // 验证
            Assert.AreEqual("05:30:15", result);
        }
        
        [Test]
        public void FormatTimeSpan_Days_ReturnsCorrectFormat()
        {
            // 准备
            TimeSpan timeSpan = new TimeSpan(3, 12, 45, 30);
            
            // 执行
            string result = TimeUtils.FormatTimeSpanCompact(timeSpan);
            
            // 验证
            Assert.AreEqual("3:12:45:30", result);
        }
        
        [Test]
        public void FormatTimeSpan_Descriptive_ReturnsCorrectFormat()
        {
            // 准备
            TimeSpan timeSpan = new TimeSpan(0, 45, 30);
            
            // 执行
            string result = TimeUtils.FormatTimeSpan(timeSpan);
            
            // 验证
            Assert.AreEqual("45分钟30秒", result);
        }
        
        [Test]
        public void FormatTimeSpan_NegativeTime_ReturnsCorrectFormat()
        {
            // 准备 - 由于FormatTimeSpan不支持负值，我们测试FormatTimeSpanCompact
            TimeSpan timeSpan = new TimeSpan(0, -45, -30);
            
            // 执行
            string result = TimeUtils.FormatTimeSpanCompact(timeSpan);
            
            // 验证
            Assert.AreEqual("-00:45:30", result);
        }
        
        [Test]
        public void FormatTimeSpan_WithPositiveSeconds_ReturnsFormattedTime()
        {
            // 安排
            TimeSpan timeSpan = TimeSpan.FromSeconds(125);
            
            // 执行
            string result = TimeUtils.FormatTimeSpan(timeSpan);
            
            // 断言
            Assert.AreEqual("2分钟5秒", result);
        }
        
        [Test]
        public void FormatTimeSpan_WithZeroSeconds_ReturnsZeroTime()
        {
            // 安排
            TimeSpan timeSpan = TimeSpan.Zero;
            
            // 执行
            string result = TimeUtils.FormatTimeSpan(timeSpan);
            
            // 断言
            Assert.AreEqual("0秒", result);
        }
        
        [Test]
        public void FormatTimeSpan_WithLargeValue_ReturnsFormattedTime()
        {
            // 安排
            TimeSpan timeSpan = TimeSpan.FromSeconds(3661); // 1小时1分钟1秒
            
            // 执行
            string result = TimeUtils.FormatTimeSpan(timeSpan);
            
            // 断言
            Assert.AreEqual("1小时1分钟1秒", result);
        }
        
        [Test]
        public void FormatTimeSpan_WithIncludeMilliseconds_IncludesMilliseconds()
        {
            // 安排
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(5500); // 5秒500毫秒
            
            // 执行
            string result = TimeUtils.FormatTimeSpan(timeSpan, true);
            
            // 断言
            Assert.AreEqual("5秒500毫秒", result);
        }
        
        #endregion
        
        #region FormatTimeSpanCompact Tests
        
        [Test]
        public void FormatTimeSpanCompact_Zero_ReturnsCorrectFormat()
        {
            // 准备
            TimeSpan timeSpan = TimeSpan.Zero;
            
            // 执行
            string result = TimeUtils.FormatTimeSpanCompact(timeSpan);
            
            // 验证
            Assert.AreEqual("00:00:00", result);
        }
        
        [Test]
        public void FormatTimeSpanCompact_Minutes_ReturnsCorrectFormat()
        {
            // 准备
            TimeSpan timeSpan = TimeSpan.FromSeconds(185); // 3分5秒
            
            // 执行
            string result = TimeUtils.FormatTimeSpanCompact(timeSpan);
            
            // 验证
            Assert.AreEqual("00:03:05", result);
        }
        
        [Test]
        public void FormatTimeSpanCompact_Hours_ReturnsCorrectFormat()
        {
            // 准备
            TimeSpan timeSpan = TimeSpan.FromSeconds(7230); // 2小时0分30秒
            
            // 执行
            string result = TimeUtils.FormatTimeSpanCompact(timeSpan);
            
            // 验证
            Assert.AreEqual("02:00:30", result);
        }
        
        [Test]
        public void FormatTimeSpanCompact_Days_ReturnsCorrectFormat()
        {
            // 准备
            TimeSpan timeSpan = TimeSpan.FromSeconds(187230); // 2天4小时0分30秒
            
            // 执行
            string result = TimeUtils.FormatTimeSpanCompact(timeSpan);
            
            // 验证
            Assert.AreEqual("2:04:00:30", result);
        }
        
        #endregion
        
        #region DateTime Conversion Tests
        
        [Test]
        public void GetUtcNow_ReturnsCurrentUtcTime()
        {
            // 执行
            DateTime result = TimeUtils.GetUtcNow();
            
            // 断言
            Assert.That(result.Kind, Is.EqualTo(DateTimeKind.Utc));
            Assert.That(result, Is.EqualTo(DateTime.UtcNow).Within(1).Seconds);
        }
        
        [Test]
        public void ConvertToTimeZone_ConvertsTimeCorrectly()
        {
            // 安排
            DateTime utcNow = DateTime.UtcNow;
            
            // 执行
            DateTime result = TimeUtils.ConvertToTimeZone(utcNow, TimeZoneInfo.Local.Id);
            
            // 断言
            Assert.That(result, Is.EqualTo(utcNow.ToLocalTime()).Within(1).Seconds);
        }
        
        #endregion
        
        #region Date Calculation Tests
        
        [Test]
        public void GetDaysBetween_WithSameDate_ReturnsZero()
        {
            // 安排
            DateTime date = new DateTime(2023, 5, 15);
            
            // 执行
            int result = TimeUtils.GetDaysBetween(date, date);
            
            // 断言
            Assert.AreEqual(0, result);
        }
        
        [Test]
        public void GetDaysBetween_WithDifferentDates_ReturnsCorrectDayCount()
        {
            // 安排
            DateTime start = new DateTime(2023, 5, 15);
            DateTime end = new DateTime(2023, 5, 20);
            
            // 执行
            int result = TimeUtils.GetDaysBetween(start, end);
            
            // 断言
            Assert.AreEqual(5, result);
        }
        
        [Test]
        public void GetDaysBetween_WithDatesInDifferentMonths_ReturnsCorrectDayCount()
        {
            // 安排
            DateTime start = new DateTime(2023, 5, 15);
            DateTime end = new DateTime(2023, 6, 15);
            
            // 执行
            int result = TimeUtils.GetDaysBetween(start, end);
            
            // 断言
            Assert.AreEqual(31, result);
        }
        
        [Test]
        public void GetDaysBetween_WithEndDateBeforeStartDate_ReturnsNegativeValue()
        {
            // 安排
            DateTime start = new DateTime(2023, 5, 20);
            DateTime end = new DateTime(2023, 5, 15);
            
            // 执行
            int result = TimeUtils.GetDaysBetween(start, end);
            
            // 断言
            Assert.AreEqual(-5, result);
        }
        
        #endregion
        
        #region Day of Week Tests
        
        [Test]
        public void GetDayOfWeek_ForSameDayInWeek_ReturnsSameValue()
        {
            // 安排
            DateTime date1 = new DateTime(2023, 5, 15); // 假设是星期一
            DateTime date2 = new DateTime(2023, 5, 15);
            
            // 执行
            int day1 = TimeUtils.GetDayOfWeek(date1);
            int day2 = TimeUtils.GetDayOfWeek(date2);
            
            // 断言
            Assert.AreEqual(day1, day2);
        }
        
        [Test]
        public void GetDayOfWeek_ForDifferentDaysInWeek_ReturnsDifferentValues()
        {
            // 安排
            DateTime monday = new DateTime(2023, 5, 15); // 假设是星期一
            DateTime tuesday = new DateTime(2023, 5, 16); // 假设是星期二
            
            // 执行
            int mondayValue = TimeUtils.GetDayOfWeek(monday);
            int tuesdayValue = TimeUtils.GetDayOfWeek(tuesday);
            
            // 断言
            Assert.AreNotEqual(mondayValue, tuesdayValue);
        }
        
        #endregion
        
        #region TimeZone Conversion Tests
        
        [Test]
        public void LocalToUtc_ConvertsTimeCorrectly()
        {
            // 安排
            DateTime localNow = DateTime.Now;
            
            // 执行
            DateTime utcResult = TimeUtils.LocalToUtc(localNow);
            
            // 断言
            Assert.That(utcResult.Kind, Is.EqualTo(DateTimeKind.Utc));
            Assert.That(utcResult, Is.EqualTo(localNow.ToUniversalTime()).Within(1).Seconds);
        }
        
        [Test]
        public void UtcToLocal_ConvertsTimeCorrectly()
        {
            // 安排
            DateTime utcNow = DateTime.UtcNow;
            
            // 执行
            DateTime localResult = TimeUtils.UtcToLocal(utcNow);
            
            // 断言
            Assert.That(localResult.Kind, Is.EqualTo(DateTimeKind.Local));
            Assert.That(localResult, Is.EqualTo(utcNow.ToLocalTime()).Within(1).Seconds);
        }
        
        #endregion
    }
} 