using System;
using NUnit.Framework;
using TByd.Core.Utils.Runtime;
using TByd.Core.Utils.Tests.Editor.Framework;
using Unity.PerformanceTesting;

namespace TByd.Core.Utils.Tests.Editor.Performance
{
    /// <summary>
    /// TimeUtils 性能测试类
    /// </summary>
    public class TimeUtilsPerformanceTests : PerformanceTestBase
    {
        private const int TestIterations = 10000;
        
        #region FormatTimeSpan Performance Tests
        
        [Test, Performance]
        public void FormatTimeSpan_Performance()
        {
            Measure.Method(() =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    string result = TimeUtils.FormatTimeSpan(TimeSpan.FromSeconds(3661));
                }
            })
            .WarmupCount(5)
            .MeasurementCount(20)
            .GC()
            .Run();
        }
        
        [Test]
        [Performance]
        public void FormatTimeSpan_CustomFormat_Performance()
        {
            // 定义测试配置
            var config = new PerformanceTestConfig
            {
                TestName = "FormatTimeSpan_CustomFormat_性能",
                MeasurementCount = 10,
                WarmupCount = 3,
                MeasureGC = true
            };
            
            TimeSpan timeSpan = new TimeSpan(2, 30, 45);
            string format = @"hh\时mm\分ss\秒";
            
            // 定义基准实现（使用标准ToString）
            Action baseline = () =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    string result = timeSpan.ToString(format);
                }
            };
            
            // 定义TimeUtils实现
            Action timeUtils = () =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    // 使用FormatTimeSpan，但由于缺少自定义格式参数，我们使用默认格式
                    string result = TimeUtils.FormatTimeSpan(timeSpan);
                }
            };
            
            // 比较性能 - 分别运行基准和TimeUtils测试
            RunPerformanceTest(baseline, new PerformanceTestConfig 
            {
                TestName = config.TestName + "_Baseline",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = true
            });
            
            RunPerformanceTest(timeUtils, new PerformanceTestConfig
            {
                TestName = config.TestName + "_TimeUtils",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = false
            });
        }
        
        [Test]
        [Performance]
        public void FormatCompactTimeSpan_Performance()
        {
            // 定义测试配置
            var config = new PerformanceTestConfig
            {
                TestName = "FormatCompactTimeSpan_性能",
                MeasurementCount = 10,
                WarmupCount = 3,
                MeasureGC = true
            };
            
            float seconds = 3645.5f; // 1小时0分45.5秒
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
            
            // 定义基准实现（手动时间格式化）
            Action baseline = () =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    int totalSeconds = (int)seconds;
                    int hours = totalSeconds / 3600;
                    int minutes = (totalSeconds % 3600) / 60;
                    int secs = totalSeconds % 60;
                    
                    string result;
                    if (hours > 0)
                    {
                        result = string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, secs);
                    }
                    else
                    {
                        result = string.Format("{0:D2}:{1:D2}", minutes, secs);
                    }
                }
            };
            
            // 定义TimeUtils实现
            Action timeUtils = () =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    string result = TimeUtils.FormatTimeSpanCompact(timeSpan);
                }
            };
            
            // 比较性能 - 分别运行基准和TimeUtils测试
            RunPerformanceTest(baseline, new PerformanceTestConfig 
            {
                TestName = config.TestName + "_Baseline",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = true
            });
            
            RunPerformanceTest(timeUtils, new PerformanceTestConfig
            {
                TestName = config.TestName + "_TimeUtils",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = false
            });
        }
        
        #endregion
        
        #region DateTime Performance Tests
        
        [Test, Performance]
        public void GetUtcNow_Performance()
        {
            Measure.Method(() =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    DateTime result = TimeUtils.GetUtcNow();
                }
            })
            .WarmupCount(5)
            .MeasurementCount(20)
            .GC()
            .Run();
        }
        
        [Test]
        [Performance]
        public void DateTime_Conversion_Performance()
        {
            // 定义测试配置
            var config = new PerformanceTestConfig
            {
                TestName = "DateTime_Conversion_性能",
                MeasurementCount = 10,
                WarmupCount = 3,
                MeasureGC = true
            };
            
            DateTime dateTime = DateTime.UtcNow;
            
            // 定义基准实现（手动计算）
            Action baseline = () =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    DateTime utc = dateTime.ToUniversalTime();
                    DateTime local = utc.ToLocalTime();
                }
            };
            
            // 定义TimeUtils实现
            Action timeUtils = () =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    DateTime utc = TimeUtils.LocalToUtc(dateTime);
                    DateTime local = TimeUtils.UtcToLocal(utc);
                }
            };
            
            // 比较性能 - 分别运行基准和TimeUtils测试
            RunPerformanceTest(baseline, new PerformanceTestConfig 
            {
                TestName = config.TestName + "_Baseline",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = true
            });
            
            RunPerformanceTest(timeUtils, new PerformanceTestConfig
            {
                TestName = config.TestName + "_TimeUtils",
                MeasurementCount = config.MeasurementCount,
                WarmupCount = config.WarmupCount,
                MeasureGC = config.MeasureGC,
                IsBaseline = false
            });
        }
        
        #endregion
        
        #region Time Conversion Performance Tests
        
        [Test, Performance]
        public void TimeConversion_Performance()
        {
            Measure.Method(() =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    DateTime local = DateTime.Now;
                    DateTime utc = TimeUtils.LocalToUtc(local);
                    DateTime backToLocal = TimeUtils.UtcToLocal(utc);
                }
            })
            .WarmupCount(5)
            .MeasurementCount(20)
            .GC()
            .Run();
        }
        
        [Test]
        [Performance]
        public void TimeZoneConversion_Performance()
        {
            DateTime utcNow = DateTime.UtcNow;
            string targetTimeZone = TimeZoneInfo.Local.Id;
            
            Measure.Method(() =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    DateTime converted = TimeUtils.ConvertToTimeZone(utcNow, targetTimeZone);
                }
            })
            .WarmupCount(5)
            .MeasurementCount(20)
            .GC()
            .Run();
        }
        
        #endregion
        
        #region Date Calculation Performance Tests
        
        [Test, Performance]
        public void GetDaysBetween_Performance()
        {
            DateTime start = new DateTime(2023, 1, 1);
            DateTime end = new DateTime(2023, 12, 31);
            
            Measure.Method(() =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    int days = TimeUtils.GetDaysBetween(start, end);
                }
            })
            .WarmupCount(5)
            .MeasurementCount(20)
            .GC()
            .Run();
        }
        
        [Test, Performance]
        public void GetDayOfWeek_Performance()
        {
            DateTime date = new DateTime(2023, 5, 15);
            
            Measure.Method(() =>
            {
                for (int i = 0; i < TestIterations; i++)
                {
                    int dayOfWeek = TimeUtils.GetDayOfWeek(date);
                }
            })
            .WarmupCount(5)
            .MeasurementCount(20)
            .GC()
            .Run();
        }
        
        #endregion
        
        #region Memory Allocation Tests
        
        [Test, Performance]
        public void TimeUtils_GCAllocation()
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(3661);
            DateTime date1 = new DateTime(2023, 5, 15);
            DateTime date2 = new DateTime(2023, 6, 15);
            
            // 测量FormatTimeSpan的内存分配
            double formatTimeSpanAllocation = MeasureGC.Allocation(() => {
                string formatResult = TimeUtils.FormatTimeSpan(timeSpan);
            });
            
            // 测量FormatTimeSpanCompact的内存分配
            double formatCompactTimeSpanAllocation = MeasureGC.Allocation(() => {
                string formatResult = TimeUtils.FormatTimeSpanCompact(timeSpan);
            });
            
            // 测量GetUtcNow的内存分配
            double getUtcNowAllocation = MeasureGC.Allocation(() => {
                DateTime utcNow = TimeUtils.GetUtcNow();
            });
            
            // 测量GetDaysBetween的内存分配
            double getDaysBetweenAllocation = MeasureGC.Allocation(() => {
                int days = TimeUtils.GetDaysBetween(date1, date2);
            });
            
            // 测量GetDayOfWeek的内存分配
            double getDayOfWeekAllocation = MeasureGC.Allocation(() => {
                int dayOfWeek = TimeUtils.GetDayOfWeek(date1);
            });
            
            // 测量时间转换方法的内存分配
            double localToUtcAllocation = MeasureGC.Allocation(() => {
                DateTime utc = TimeUtils.LocalToUtc(date1);
            });
            
            double utcToLocalAllocation = MeasureGC.Allocation(() => {
                DateTime local = TimeUtils.UtcToLocal(date1);
            });
            
            // 验证内存分配
            Assert.Less(formatTimeSpanAllocation, 256, "FormatTimeSpan的内存分配应该小于256字节");
            Assert.Less(formatCompactTimeSpanAllocation, 256, "FormatTimeSpanCompact的内存分配应该小于256字节");
            Assert.Less(getUtcNowAllocation, 32, "GetUtcNow的内存分配应该接近于零");
            Assert.Less(getDaysBetweenAllocation, 32, "GetDaysBetween的内存分配应该接近于零");
            Assert.Less(getDayOfWeekAllocation, 32, "GetDayOfWeek的内存分配应该接近于零");
            Assert.Less(localToUtcAllocation, 32, "LocalToUtc的内存分配应该接近于零");
            Assert.Less(utcToLocalAllocation, 32, "UtcToLocal的内存分配应该接近于零");
        }
        
        #endregion
    }
} 