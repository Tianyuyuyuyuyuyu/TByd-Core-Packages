using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using TByd.Core.Utils.Tests.Editor.Framework;
using Unity.PerformanceTesting;
using UnityEngine;

namespace TByd.Core.Utils.Tests.Editor.Performance
{
    /// <summary>
    /// IOUtils工具类的性能测试
    /// </summary>
    [TestFixture]
    public class IOUtilsPerformanceTests : PerformanceTestBase
    {
        private const int BenchmarkIterations = 1000;
        private const int FileSize = 1024 * 10; // 10KB的测试文件
        
        private string _testFolderPath;
        private string _testFilePath;
        private string _testContent;
        private byte[] _testBinaryData;
        
        [SetUp]
        public void Setup()
        {
            // 创建测试用的临时文件夹和文件
            _testFolderPath = Path.Combine(Application.temporaryCachePath, "IOUtilsPerformanceTests");
            _testFilePath = Path.Combine(_testFolderPath, "test_file.txt");
            
            // 生成测试内容
            StringBuilder sb = new StringBuilder(FileSize);
            for (int i = 0; i < FileSize / 10; i++)
            {
                sb.AppendLine($"这是测试内容的第{i}行，用于测试IOUtils类的性能。");
            }
            _testContent = sb.ToString();
            _testBinaryData = Encoding.UTF8.GetBytes(_testContent);
            
            // 确保测试目录存在
            if (!Directory.Exists(_testFolderPath))
            {
                Directory.CreateDirectory(_testFolderPath);
            }
            
            // 创建测试文件
            File.WriteAllText(_testFilePath, _testContent);
        }
        
        [TearDown]
        public void TearDown()
        {
            // 清理测试文件和文件夹
            try
            {
                if (Directory.Exists(_testFolderPath))
                {
                    Directory.Delete(_testFolderPath, true);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"清理测试文件时出错: {ex.Message}");
            }
        }
        
        #region Path Operations Performance Tests
        
        /// <summary>
        /// 测试CombinePath方法的性能
        /// </summary>
        /*
        [Test, Performance]
        public void CombinePath_Performance()
        {
            // 准备测试数据
            string part1 = "path1";
            string part2 = "path2";
            string part3 = "path3";
            
            Measure.Method(() =>
            {
                for (int i = 0; i < BenchmarkIterations; i++)
                {
                    string result = IOUtils.CombinePath(part1, part2, part3);
                }
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }
        */
        
        /// <summary>
        /// 测试GetFileName方法的性能
        /// </summary>
        /*
        [Test, Performance]
        public void GetFileName_Performance()
        {
            // 准备测试数据
            string path = Path.Combine("directory", "subdirectory", "file.txt");
            
            Measure.Method(() =>
            {
                for (int i = 0; i < BenchmarkIterations; i++)
                {
                    string result = IOUtils.GetFileName(path);
                }
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }
        */
        
        /// <summary>
        /// 测试GetFileNameWithoutExtension方法的性能
        /// </summary>
        /*
        [Test, Performance]
        public void GetFileNameWithoutExtension_Performance()
        {
            // 准备测试数据
            string path = Path.Combine("directory", "subdirectory", "file.txt");
            
            Measure.Method(() =>
            {
                for (int i = 0; i < BenchmarkIterations; i++)
                {
                    string result = IOUtils.GetFileNameWithoutExtension(path);
                }
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }
        */
        
        /// <summary>
        /// 测试GetFileExtension方法的性能
        /// </summary>
        [Test, Performance]
        public void GetFileExtension_Performance()
        {
            // 准备测试数据
            string path = Path.Combine("directory", "subdirectory", "file.txt");
            
            Measure.Method(() =>
            {
                for (int i = 0; i < BenchmarkIterations; i++)
                {
                    string result = IOUtils.GetFileExtension(path);
                }
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }
        
        /// <summary>
        /// 测试IOUtils.NormalizePath方法的性能
        /// </summary>
        [Test, Performance]
        public void NormalizePath_IOUtils_Performance()
        {
            // 准备测试数据
            string path = Path.Combine("directory", "subdirectory", "file.txt");
            
            Measure.Method(() =>
            {
                for (int i = 0; i < BenchmarkIterations; i++)
                {
                    string result = IOUtils.NormalizePath(path);
                }
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }
        
        /// <summary>
        /// 测试System.IO.Path.GetFileName方法的性能
        /// </summary>
        /*
        [Test, Performance]
        public void GetFileName_SystemIO_Performance()
        {
            // 准备测试数据
            string path = Path.Combine("directory", "subdirectory", "file.txt");
            
            Measure.Method(() =>
            {
                for (int i = 0; i < BenchmarkIterations; i++)
                {
                    string result = Path.GetFileName(path);
                }
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }
        */
        
        #endregion
        
        #region File Operation Performance Tests
        
        /// <summary>
        /// 测试FileExists方法的性能
        /// </summary>
        [Test, Performance]
        public void FileExists_IOUtils_Performance()
        {
            Measure.Method(() =>
            {
                for (int i = 0; i < BenchmarkIterations; i++)
                {
                    bool result = IOUtils.FileExists(_testFilePath);
                }
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }
        
        /// <summary>
        /// 测试System.IO.File.Exists方法的性能
        /// </summary>
        [Test, Performance]
        public void FileExists_SystemIO_Performance()
        {
            Measure.Method(() =>
            {
                for (int i = 0; i < BenchmarkIterations; i++)
                {
                    bool result = File.Exists(_testFilePath);
                }
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }
        
        /// <summary>
        /// 测试DirectoryExists方法的性能
        /// </summary>
        [Test, Performance]
        public void DirectoryExists_Performance()
        {
            Measure.Method(() =>
            {
                for (int i = 0; i < BenchmarkIterations; i++)
                {
                    bool result = IOUtils.DirectoryExists(_testFolderPath);
                }
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }
        
        #endregion
        
        #region ReadWrite Performance Tests
        
        /// <summary>
        /// 测试ReadAllText方法的性能
        /// </summary>
        [Test, Performance]
        public void ReadAllText_Performance()
        {
            // 使用较少的迭代，因为文件I/O是昂贵的操作
            Measure.Method(() =>
            {
                for (int i = 0; i < BenchmarkIterations / 100; i++)
                {
                    string content = IOUtils.ReadAllText(_testFilePath);
                }
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }
        
        /// <summary>
        /// 测试WriteAllText方法的性能
        /// </summary>
        [Test, Performance]
        public void WriteAllText_Performance()
        {
            // 使用更少的迭代，因为写入是昂贵的操作
            string tempFilePath = Path.Combine(_testFolderPath, "write_test.txt");
            
            Measure.Method(() =>
            {
                for (int i = 0; i < BenchmarkIterations / 1000; i++)
                {
                    IOUtils.WriteAllText(tempFilePath, _testContent);
                }
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }
        
        /// <summary>
        /// 测试ReadAllBytes方法的性能
        /// </summary>
        [Test, Performance]
        public void ReadAllBytes_Performance()
        {
            // 使用较少的迭代，因为文件I/O是昂贵的操作
            string binaryFilePath = Path.Combine(_testFolderPath, "binary_test.bin");
            File.WriteAllBytes(binaryFilePath, _testBinaryData);
            
            Measure.Method(() =>
            {
                for (int i = 0; i < BenchmarkIterations / 100; i++)
                {
                    byte[] content = IOUtils.ReadAllBytes(binaryFilePath);
                }
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }
        
        /// <summary>
        /// 测试WriteAllBytes方法的性能
        /// </summary>
        [Test, Performance]
        public void WriteAllBytes_Performance()
        {
            // 使用更少的迭代，因为写入是昂贵的操作
            string tempFilePath = Path.Combine(_testFolderPath, "binary_write_test.bin");
            
            Measure.Method(() =>
            {
                for (int i = 0; i < BenchmarkIterations / 1000; i++)
                {
                    IOUtils.WriteAllBytes(tempFilePath, _testBinaryData);
                }
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }
        
        /// <summary>
        /// 比较不同文件大小下的读写性能
        /// </summary>
        [Test, Performance]
        public void CompareFileSizeReadWrite_Performance()
        {
            // 准备不同大小的文件
            string smallFilePath = Path.Combine(_testFolderPath, "small.txt");
            string mediumFilePath = Path.Combine(_testFolderPath, "medium.txt");
            string largeFilePath = Path.Combine(_testFolderPath, "large.txt");
            
            // 创建1KB的小文件
            StringBuilder sbSmall = new StringBuilder(1024);
            for (int i = 0; i < 100; i++)
            {
                sbSmall.AppendLine($"小文件测试行{i}");
            }
            string smallContent = sbSmall.ToString();
            File.WriteAllText(smallFilePath, smallContent);
            
            // 创建100KB的中等文件（使用测试内容的10倍）
            StringBuilder sbMedium = new StringBuilder(1024 * 100);
            for (int i = 0; i < 1000; i++)
            {
                sbMedium.AppendLine($"中等文件测试行{i}");
            }
            string mediumContent = sbMedium.ToString();
            File.WriteAllText(mediumFilePath, mediumContent);
            
            // 创建1MB的大文件
            StringBuilder sbLarge = new StringBuilder(1024 * 1024);
            for (int i = 0; i < 10000; i++)
            {
                sbLarge.AppendLine($"大文件测试行{i}");
            }
            string largeContent = sbLarge.ToString();
            File.WriteAllText(largeFilePath, largeContent);
            
            // 读取小文件
            Measure.Method(() =>
            {
                for (int i = 0; i < BenchmarkIterations / 100; i++)
                {
                    string content = IOUtils.ReadAllText(smallFilePath);
                }
            })
            .WarmupCount(3)
            .MeasurementCount(5)
            .GC()
            .Run();
            
            // 读取中等文件
            Measure.Method(() =>
            {
                for (int i = 0; i < BenchmarkIterations / 1000; i++)
                {
                    string content = IOUtils.ReadAllText(mediumFilePath);
                }
            })
            .WarmupCount(3)
            .MeasurementCount(5)
            .GC()
            .Run();
            
            // 读取大文件
            Measure.Method(() =>
            {
                for (int i = 0; i < BenchmarkIterations / 10000; i++)
                {
                    string content = IOUtils.ReadAllText(largeFilePath);
                }
            })
            .WarmupCount(3)
            .MeasurementCount(5)
            .GC()
            .Run();
        }
        
        #endregion
        
        #region Delete Operations Performance Tests
        
        /// <summary>
        /// 测试DeleteFile方法的性能
        /// </summary>
        [Test, Performance]
        public void DeleteFile_Performance()
        {
            Measure.Method(() =>
            {
                for (int i = 0; i < BenchmarkIterations / 1000; i++)
                {
                    // 创建临时文件
                    string tempFilePath = Path.Combine(_testFolderPath, $"temp_file_{i}.txt");
                    File.WriteAllText(tempFilePath, "临时文件内容");
                    
                    // 删除文件
                    IOUtils.DeleteFile(tempFilePath);
                }
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }
        
        /// <summary>
        /// 测试DeleteDirectory方法的性能
        /// </summary>
        [Test, Performance]
        public void DeleteDirectory_Performance()
        {
            Measure.Method(() =>
            {
                for (int i = 0; i < BenchmarkIterations / 1000; i++)
                {
                    // 创建临时目录
                    string tempDirPath = Path.Combine(_testFolderPath, $"temp_dir_{i}");
                    Directory.CreateDirectory(tempDirPath);
                    
                    // 创建几个文件
                    for (int j = 0; j < 5; j++)
                    {
                        File.WriteAllText(Path.Combine(tempDirPath, $"file_{j}.txt"), $"文件{j}内容");
                    }
                    
                    // 删除目录
                    IOUtils.DeleteDirectory(tempDirPath);
                }
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }
        
        #endregion
        
        #region Memory Allocation Tests
        
        /// <summary>
        /// 测量路径操作的内存分配
        /// </summary>
        [Test, Performance]
        public void PathOperations_GCAllocation()
        {
            // 准备测试数据
            string path = Path.Combine("directory", "subdirectory", "file.txt");
            
            // GetFileName
            double getFileNameAllocation = MeasureGC.Allocation(() =>
            {
                string result = Path.GetFileName(path);
            });
            
            // GetFileNameWithoutExtension
            double getFileNameWOExtAllocation = MeasureGC.Allocation(() =>
            {
                string result = Path.GetFileNameWithoutExtension(path);
            });
            
            // GetFileExtension
            double getFileExtAllocation = MeasureGC.Allocation(() =>
            {
                string result = IOUtils.GetFileExtension(path);
            });
            
            // 断言
            Assert.That(getFileNameAllocation, Is.LessThan(100), "GetFileName的内存分配应控制在合理范围内");
            Assert.That(getFileNameWOExtAllocation, Is.LessThan(100), "GetFileNameWithoutExtension的内存分配应控制在合理范围内");
            Assert.That(getFileExtAllocation, Is.LessThan(100), "GetFileExtension的内存分配应控制在合理范围内");
        }
        
        /// <summary>
        /// 测量文件存在性检查的内存分配
        /// </summary>
        [Test, Performance]
        public void FileExistsCheck_GCAllocation()
        {
            // FileExists
            double fileExistsAllocation = MeasureGC.Allocation(() =>
            {
                bool result = IOUtils.FileExists(_testFilePath);
            });
            
            // DirectoryExists
            double dirExistsAllocation = MeasureGC.Allocation(() =>
            {
                bool result = IOUtils.DirectoryExists(_testFolderPath);
            });
            
            // 断言
            Assert.That(fileExistsAllocation, Is.LessThan(100), "FileExists的内存分配应控制在合理范围内");
            Assert.That(dirExistsAllocation, Is.LessThan(100), "DirectoryExists的内存分配应控制在合理范围内");
        }
        
        /// <summary>
        /// 测量CreateDirectory方法的内存分配
        /// </summary>
        [Test, Performance]
        public void CreateDirectory_GCAllocation()
        {
            // 准备
            string dirPath = Path.Combine(_testFolderPath, "ensure_exists_test");
            
            // 目录不存在时
            if (Directory.Exists(dirPath))
            {
                Directory.Delete(dirPath);
            }
            double notExistsAllocation = MeasureGC.Allocation(() =>
            {
                IOUtils.CreateDirectory(dirPath);
            });
            
            // 目录已存在时
            double existsAllocation = MeasureGC.Allocation(() =>
            {
                IOUtils.CreateDirectory(dirPath);
            });
            
            // 断言
            // 创建目录时可能会有一些分配，但应该控制在合理范围内
            Assert.That(notExistsAllocation, Is.LessThan(500), "在目录不存在时，CreateDirectory的内存分配应控制在合理范围内");
            // 目录已存在时应该几乎没有分配
            Assert.That(existsAllocation, Is.LessThan(100), "在目录已存在时，CreateDirectory的内存分配应控制在合理范围内");
        }
        
        #endregion
    }
} 