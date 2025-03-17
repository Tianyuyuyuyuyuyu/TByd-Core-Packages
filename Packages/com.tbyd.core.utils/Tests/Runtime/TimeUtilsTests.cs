using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using TByd.Core.Utils.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace TByd.Core.Utils.Tests.Runtime
{
    public class TimeUtilsTests
    {
        [Test]
        public void FormatDateTime_ReturnsCorrectFormat()
        {
            // 准备
            DateTime testDate = new DateTime(2023, 9, 15, 14, 30, 45);
            
            // 测试
            string formatted = TimeUtils.FormatDateTime(testDate, "yyyy-MM-dd HH:mm:ss");
            
            // 验证
            Assert.AreEqual("2023-09-15 14:30:45", formatted);
        }
        
        [Test]
        public void FormatTimeSpan_ReturnsFormattedString()
        {
            // 准备
            TimeSpan span1 = new TimeSpan(1, 2, 30, 45); // 1天2小时30分钟45秒
            TimeSpan span2 = new TimeSpan(0, 2, 30, 45); // 2小时30分钟45秒
            TimeSpan span3 = new TimeSpan(0, 0, 30, 45); // 30分钟45秒
            TimeSpan span4 = new TimeSpan(0, 0, 0, 45, 500); // 45秒500毫秒
            
            // 测试
            string result1 = TimeUtils.FormatTimeSpan(span1);
            string result2 = TimeUtils.FormatTimeSpan(span2);
            string result3 = TimeUtils.FormatTimeSpan(span3);
            string result4 = TimeUtils.FormatTimeSpan(span4);
            string result4WithMs = TimeUtils.FormatTimeSpan(span4, true);
            
            // 验证
            Assert.AreEqual("1天2小时30分钟45秒", result1);
            Assert.AreEqual("2小时30分钟45秒", result2);
            Assert.AreEqual("30分钟45秒", result3);
            Assert.AreEqual("45秒", result4);
            Assert.AreEqual("45秒500毫秒", result4WithMs);
        }
        
        [Test]
        public void FormatTimeSpanCompact_ReturnsCompactFormat()
        {
            // 准备
            TimeSpan span1 = new TimeSpan(1, 2, 30, 45); // 1天2小时30分钟45秒
            TimeSpan span2 = new TimeSpan(0, 2, 30, 45); // 2小时30分钟45秒
            
            // 测试
            string result1 = TimeUtils.FormatTimeSpanCompact(span1);
            string result2 = TimeUtils.FormatTimeSpanCompact(span2);
            
            // 验证
            Assert.AreEqual("1:02:30:45", result1);
            Assert.AreEqual("02:30:45", result2);
        }
        
        [Test]
        public void GetRelativeTimeDescription_ReturnsCorrectDescription()
        {
            // 准备 - 使用相对于当前时间的时间点
            DateTime now = DateTime.Now;
            Debug.Log($"当前时间: {now:yyyy-MM-dd HH:mm:ss.fff}");
            
            DateTime fiveSecondsAgo = now.AddSeconds(-5);
            DateTime threeMinutesAgo = now.AddMinutes(-3);
            DateTime twoHoursAgo = now.AddHours(-2);
            DateTime yesterdayTime = now.AddDays(-1);
            DateTime threeDaysAgo = now.AddDays(-3);
            DateTime twoWeeksAgo = now.AddDays(-14);
            DateTime sixMonthsAgo = now.AddMonths(-6);
            DateTime twoYearsAgo = now.AddYears(-2);
            
            DateTime soon = now.AddSeconds(30);
            DateTime inThreeMinutes = now.AddMinutes(3);
            DateTime inTwoHours = now.AddHours(2);
            DateTime tomorrow = now.AddDays(1);
            Debug.Log($"明天时间: {tomorrow:yyyy-MM-dd HH:mm:ss.fff}");
            Debug.Log($"现在与明天时间差（天）: {(tomorrow - now).TotalDays}");
            
            DateTime inThreeDays = now.AddDays(3);
            DateTime inTwoWeeks = now.AddDays(14);
            DateTime inSixMonths = now.AddMonths(6);
            DateTime inTwoYears = now.AddYears(2);
            
            // 测试和验证 - 过去时间
            string descFiveSecondsAgo = TimeUtils.GetRelativeTimeDescription(fiveSecondsAgo);
            Assert.True(descFiveSecondsAgo == "刚才" || descFiveSecondsAgo.Contains("秒前"));
            
            string descThreeMinutesAgo = TimeUtils.GetRelativeTimeDescription(threeMinutesAgo);
            Assert.True(descThreeMinutesAgo.Contains("分钟前"));
            
            string descTwoHoursAgo = TimeUtils.GetRelativeTimeDescription(twoHoursAgo);
            Assert.True(descTwoHoursAgo.Contains("小时前"));
            
            string descYesterday = TimeUtils.GetRelativeTimeDescription(yesterdayTime);
            Assert.AreEqual("昨天", descYesterday);
            
            string descThreeDaysAgo = TimeUtils.GetRelativeTimeDescription(threeDaysAgo);
            Assert.True(descThreeDaysAgo.Contains("天前"));
            
            string descTwoWeeksAgo = TimeUtils.GetRelativeTimeDescription(twoWeeksAgo);
            Assert.True(descTwoWeeksAgo.Contains("周前"));
            
            string descSixMonthsAgo = TimeUtils.GetRelativeTimeDescription(sixMonthsAgo);
            Assert.True(descSixMonthsAgo.Contains("个月前"));
            
            string descTwoYearsAgo = TimeUtils.GetRelativeTimeDescription(twoYearsAgo);
            Assert.True(descTwoYearsAgo.Contains("年前"));
            
            // 测试和验证 - 未来时间
            string descSoon = TimeUtils.GetRelativeTimeDescription(soon);
            Assert.AreEqual("即将到来", descSoon);
            
            string descInThreeMinutes = TimeUtils.GetRelativeTimeDescription(inThreeMinutes);
            Assert.True(descInThreeMinutes.Contains("分钟后"));
            
            string descInTwoHours = TimeUtils.GetRelativeTimeDescription(inTwoHours);
            Assert.True(descInTwoHours.Contains("小时后"));
            
            string descTomorrow = TimeUtils.GetRelativeTimeDescription(tomorrow);
            Debug.Log($"明天的描述: '{descTomorrow}'");
            Debug.Log($"描述中是否包含'天后': {descTomorrow.Contains("天后")}");
            Assert.True(descTomorrow.Contains("天后"), $"预期描述包含'天后'，实际描述为: '{descTomorrow}'");
            
            string descInThreeDays = TimeUtils.GetRelativeTimeDescription(inThreeDays);
            Assert.True(descInThreeDays.Contains("天后"));
            
            string descInTwoWeeks = TimeUtils.GetRelativeTimeDescription(inTwoWeeks);
            Assert.True(descInTwoWeeks.Contains("周后"));
            
            string descInSixMonths = TimeUtils.GetRelativeTimeDescription(inSixMonths);
            Assert.True(descInSixMonths.Contains("个月后"));
            
            string descInTwoYears = TimeUtils.GetRelativeTimeDescription(inTwoYears);
            Assert.True(descInTwoYears.Contains("年后"));
        }
        
        [Test]
        public void TryParseDateTime_ParsesValidDateTime()
        {
            // 准备
            string validDate = "2023-09-15 14:30:45";
            string invalidDate = "not a date";
            
            // 测试
            bool validResult = TimeUtils.TryParseDateTime(validDate, out DateTime validParsed);
            bool invalidResult = TimeUtils.TryParseDateTime(invalidDate, out DateTime invalidParsed);
            
            // 验证
            Assert.True(validResult);
            Assert.AreEqual(2023, validParsed.Year);
            Assert.AreEqual(9, validParsed.Month);
            Assert.AreEqual(15, validParsed.Day);
            Assert.AreEqual(14, validParsed.Hour);
            Assert.AreEqual(30, validParsed.Minute);
            Assert.AreEqual(45, validParsed.Second);
            
            Assert.False(invalidResult);
            Assert.AreEqual(DateTime.MinValue, invalidParsed);
        }
        
        [Test]
        public void LocalToUtc_ConvertsCorrectly()
        {
            // 注意：此测试依赖于本地时区设置
            // 准备
            DateTime local = new DateTime(2023, 9, 15, 14, 30, 45, DateTimeKind.Local);
            
            // 测试
            DateTime utc = TimeUtils.LocalToUtc(local);
            
            // 验证
            Assert.AreEqual(DateTimeKind.Utc, utc.Kind);
            
            // 确认转换后的UTC时间是正确的（这取决于当地时区，所以在测试中不验证具体时间值）
            DateTime backToLocal = TimeUtils.UtcToLocal(utc);
            Assert.AreEqual(local.Year, backToLocal.Year);
            Assert.AreEqual(local.Month, backToLocal.Month);
            Assert.AreEqual(local.Day, backToLocal.Day);
            Assert.AreEqual(local.Hour, backToLocal.Hour);
            Assert.AreEqual(local.Minute, backToLocal.Minute);
            Assert.AreEqual(local.Second, backToLocal.Second);
        }
        
        [Test]
        public void UtcToLocal_ConvertsCorrectly()
        {
            // 准备
            DateTime utc = new DateTime(2023, 9, 15, 14, 30, 45, DateTimeKind.Utc);
            
            // 测试
            DateTime local = TimeUtils.UtcToLocal(utc);
            
            // 验证
            Assert.AreEqual(DateTimeKind.Local, local.Kind);
            
            // 确认转换后的本地时间是正确的（这取决于当地时区，所以在测试中不验证具体时间值）
            DateTime backToUtc = TimeUtils.LocalToUtc(local);
            Assert.AreEqual(utc.Year, backToUtc.Year);
            Assert.AreEqual(utc.Month, backToUtc.Month);
            Assert.AreEqual(utc.Day, backToUtc.Day);
            Assert.AreEqual(utc.Hour, backToUtc.Hour);
            Assert.AreEqual(utc.Minute, backToUtc.Minute);
            Assert.AreEqual(utc.Second, backToUtc.Second);
        }
        
        [Test]
        public void GameDateTime_ConversionsWork()
        {
            // 准备
            DateTime dateTime = new DateTime(2023, 9, 15, 14, 30, 45);
            
            // 测试
            TimeUtils.GameDateTime gameDateTime = TimeUtils.GameDateTime.FromDateTime(dateTime);
            DateTime convertedBack = gameDateTime.ToDateTime();
            
            // 验证
            Assert.AreEqual(2023, gameDateTime.Year);
            Assert.AreEqual(9, gameDateTime.Month);
            Assert.AreEqual(15, gameDateTime.Day);
            Assert.AreEqual(14, gameDateTime.Hour);
            Assert.AreEqual(30, gameDateTime.Minute);
            Assert.AreEqual(45, gameDateTime.Second);
            
            Assert.AreEqual(dateTime, convertedBack);
        }
        
        [Test]
        public void GameTimeScale_WorksCorrectly()
        {
            // 准备
            TimeUtils.SetGameTimeScale(2.0f);
            
            // 测试
            float scale = TimeUtils.GetGameTimeScale();
            
            // 验证
            Assert.AreEqual(2.0f, scale);
            
            // 测试负值（应该抛出异常）
            Assert.Throws<ArgumentException>(() => TimeUtils.SetGameTimeScale(-1.0f));
        }
        
        [Test]
        public void GameTime_UpdatesCorrectly()
        {
            // 准备
            DateTime startTime = new DateTime(2023, 1, 1);
            TimeUtils.SetGameStartTime(startTime);
            TimeUtils.SetGameTimeScale(2.0f); // 游戏时间流逝速度是现实时间的2倍
            
            // 测试
            DateTime beforeUpdate = TimeUtils.GetCurrentGameTime();
            Thread.Sleep(100); // 等待100毫秒
            TimeUtils.UpdateGameTime();
            DateTime afterUpdate = TimeUtils.GetCurrentGameTime();
            
            // 验证 - 游戏时间应该流逝了大约200毫秒
            TimeSpan delta = afterUpdate - beforeUpdate;
            Assert.Greater(delta.TotalMilliseconds, 150); // 允许一些误差
            
            // 测试暂停功能
            TimeUtils.SetGameTimePaused(true);
            DateTime beforePause = TimeUtils.GetCurrentGameTime();
            Thread.Sleep(50);
            TimeUtils.UpdateGameTime(); // 暂停时更新应该没有效果
            DateTime afterPause = TimeUtils.GetCurrentGameTime();
            
            // 验证 - 游戏时间应该没有变化
            Assert.AreEqual(beforePause, afterPause);
            
            // 恢复正常速度，用于后续测试
            TimeUtils.SetGameTimeScale(1.0f);
            TimeUtils.SetGameTimePaused(false);
        }
        
        [Test]
        public void TimeConversions_WorkCorrectly()
        {
            // 准备
            TimeUtils.SetGameTimeScale(2.0f);
            TimeSpan realTimeSpan = TimeSpan.FromSeconds(10);
            
            // 测试
            TimeSpan gameTimeSpan = TimeUtils.RealTimeToGameTime(realTimeSpan);
            TimeSpan convertedBack = TimeUtils.GameTimeToRealTime(gameTimeSpan);
            
            // 验证
            Assert.AreEqual(20, gameTimeSpan.TotalSeconds);
            Assert.AreEqual(10, convertedBack.TotalSeconds);
            
            // 恢复正常速度
            TimeUtils.SetGameTimeScale(1.0f);
        }
        
        [Test]
        public void Timers_WorkCorrectly()
        {
            // 准备和测试
            TimeUtils.StartTimer("testTimer");
            Thread.Sleep(50);
            long elapsed = TimeUtils.StopTimer("testTimer");
            
            // 验证
            Assert.GreaterOrEqual(elapsed, 40); // 应该至少经过了40毫秒（允许一点误差）
            
            // 测试重置
            bool resetResult = TimeUtils.ResetTimer("testTimer");
            long afterReset = TimeUtils.GetTimerElapsedMilliseconds("testTimer");
            
            // 验证
            Assert.True(resetResult);
            Assert.AreEqual(0, afterReset);
            
            // 测试运行状态
            TimeUtils.StartTimer("runningTimer");
            bool isRunning = TimeUtils.IsTimerRunning("runningTimer");
            
            // 验证
            Assert.True(isRunning);
            
            // 清理
            TimeUtils.StopTimer("runningTimer");
        }
        
        [Test]
        public void MeasureExecutionTime_RecordsTime()
        {
            // 准备和测试
            long time = TimeUtils.MeasureExecutionTime("sleepTest", () => Thread.Sleep(50), false);
            
            // 验证
            Assert.GreaterOrEqual(time, 40); // 应该至少经过了40毫秒（允许一点误差）
        }
        
        [Test]
        public void DateCalculations_WorkCorrectly()
        {
            // 获取指定月份的天数
            int daysInFeb2020 = TimeUtils.GetDaysInMonth(2020, 2);
            int daysInFeb2021 = TimeUtils.GetDaysInMonth(2021, 2);
            
            Assert.AreEqual(29, daysInFeb2020); // 2020年是闰年
            Assert.AreEqual(28, daysInFeb2021); // 2021年不是闰年
            
            // 检查闰年
            Assert.True(TimeUtils.IsLeapYear(2020));
            Assert.False(TimeUtils.IsLeapYear(2021));
            
            // 获取两个日期之间的天数
            DateTime start = new DateTime(2023, 1, 1);
            DateTime end = new DateTime(2023, 1, 10);
            
            int days = TimeUtils.GetDaysBetween(start, end);
            Assert.AreEqual(9, days);
            
            // 获取一周的第一天
            DateTime testDate = new DateTime(2023, 9, 20); // 假设这是周三
            DateTime firstDay = TimeUtils.GetFirstDayOfWeek(testDate);
            
            Assert.AreEqual(DayOfWeek.Monday, firstDay.DayOfWeek);
            
            // 获取月份的第一天和最后一天
            DateTime firstDayOfMonth = TimeUtils.GetFirstDayOfMonth(testDate);
            DateTime lastDayOfMonth = TimeUtils.GetLastDayOfMonth(testDate);
            
            Assert.AreEqual(1, firstDayOfMonth.Day);
            Assert.AreEqual(9, firstDayOfMonth.Month);
            Assert.AreEqual(30, lastDayOfMonth.Day); // 9月有30天
            Assert.AreEqual(9, lastDayOfMonth.Month);
        }
        
        [Test]
        public void WorkDays_CalculatedCorrectly()
        {
            // 准备
            DateTime friday = new DateTime(2023, 9, 15); // 假设这是周五
            
            // 测试 - 添加2个工作日应该是下周二（跳过周末）
            DateTime result = TimeUtils.AddWorkDays(friday, 2);
            
            // 验证
            // 周五+2个工作日=下周二
            Assert.AreEqual(DayOfWeek.Tuesday, result.DayOfWeek);
            
            // 测试 - 自定义节假日
            var holidays = new List<DateTime> { new DateTime(2023, 9, 19) }; // 将周二设为假日
            DateTime resultWithHoliday = TimeUtils.AddWorkDays(friday, 2, holidays);
            
            // 验证 - 应该跳过周末和假日，结果是周三
            Assert.AreEqual(DayOfWeek.Wednesday, resultWithHoliday.DayOfWeek);
            
            // 测试工作日检查
            bool isFridayWorkday = TimeUtils.IsWorkDay(friday);
            bool isSaturdayWorkday = TimeUtils.IsWorkDay(friday.AddDays(1)); // 周六
            bool isHolidayWorkday = TimeUtils.IsWorkDay(new DateTime(2023, 9, 19), holidays); // 假日
            
            // 验证
            Assert.True(isFridayWorkday);
            Assert.False(isSaturdayWorkday);
            Assert.False(isHolidayWorkday);
        }
        
        [Test]
        public void PerformanceMeasurements_WorkCorrectly()
        {
            // 准备
            string statName = "perfTest";
            
            // 测试性能测量
            TimeUtils.StartPerformanceMeasure(statName);
            Thread.Sleep(50);
            double result = TimeUtils.EndPerformanceMeasure(statName);
            
            // 验证
            Assert.GreaterOrEqual(result, 40); // 至少经过了40毫秒
            
            // 获取统计信息
            IReadOnlyList<double> stats = TimeUtils.GetPerformanceStats(statName);
            double avg = TimeUtils.GetPerformanceStatsAverage(statName);
            double min = TimeUtils.GetPerformanceStatsMin(statName);
            double max = TimeUtils.GetPerformanceStatsMax(statName);
            
            // 验证
            Assert.AreEqual(1, stats.Count);
            Assert.GreaterOrEqual(avg, 40);
            Assert.GreaterOrEqual(min, 40);
            Assert.GreaterOrEqual(max, 40);
            
            // 测试报告生成
            string report = TimeUtils.GeneratePerformanceReport();
            Assert.True(report.Contains(statName));
            
            // 清理性能数据
            TimeUtils.ClearPerformanceStats(statName);
            stats = TimeUtils.GetPerformanceStats(statName);
            Assert.AreEqual(0, stats.Count);
        }
    }
} 