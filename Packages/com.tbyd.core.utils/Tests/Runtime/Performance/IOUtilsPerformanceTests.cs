using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TByd.Core.Utils.Runtime;

namespace TByd.Core.Utils.Tests.Runtime.Performance
{
    /// <summary>
    /// IOUtils专用性能测试类，使用PerformanceTestFramework进行测试
    /// </summary>
    public class IOUtilsPerformanceTests
    {
        private string testDirectory;
        private string testFilePath;
        private string testTextFilePath;
        private string testBinaryFilePath;
        private string testLargeFilePath;
        
        [OneTimeSetUp]
        public void Setup()
        {
            // 初始化测试目录和文件
            testDirectory = Path.Combine(Application.temporaryCachePath, "IOUtilsPerformanceTests");
            if (!Directory.Exists(testDirectory))
            {
                Directory.CreateDirectory(testDirectory);
            }
            
            testFilePath = Path.Combine(testDirectory, "test.txt");
            testTextFilePath = Path.Combine(testDirectory, "test_text.txt");
            testBinaryFilePath = Path.Combine(testDirectory, "test_binary.dat");
            testLargeFilePath = Path.Combine(testDirectory, "test_large.dat");
            
            // 创建测试文件
            File.WriteAllText(testFilePath, "This is a test file for IOUtils performance tests.");
            
            // 创建1MB的文本文件
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 10000; i++)
            {
                sb.AppendLine($"Line {i}: This is a test line with some random content: {Guid.NewGuid()}");
            }
            File.WriteAllText(testTextFilePath, sb.ToString());
            
            // 创建1MB的二进制文件
            byte[] binaryData = new byte[1024 * 1024];
            for (int i = 0; i < binaryData.Length; i++)
            {
                binaryData[i] = (byte)(i % 256);
            }
            File.WriteAllBytes(testBinaryFilePath, binaryData);
            
            // 创建10MB的大文件
            byte[] largeData = new byte[10 * 1024 * 1024];
            for (int i = 0; i < largeData.Length; i++)
            {
                largeData[i] = (byte)(i % 256);
            }
            File.WriteAllBytes(testLargeFilePath, largeData);
            
            // 清除之前的测试结果
            PerformanceTestFramework.ClearResults();
        }
        
        [OneTimeTearDown]
        public void Cleanup()
        {
            // 清理测试文件
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
                
            if (File.Exists(testTextFilePath))
                File.Delete(testTextFilePath);
                
            if (File.Exists(testBinaryFilePath))
                File.Delete(testBinaryFilePath);
                
            if (File.Exists(testLargeFilePath))
                File.Delete(testLargeFilePath);
                
            if (Directory.Exists(testDirectory))
                Directory.Delete(testDirectory, true);
                
            // 生成性能测试报告
            PerformanceTestFramework.GenerateReport("IOUtils性能测试");
        }
        
        [Test]
        public void PathOperations_Performance_Test()
        {
            string testPath = @"C:\Test\Path\To\Some\File.txt";
            
            // 测试GetFileName性能
            PerformanceTestFramework.RunPerformanceTest(
                "IOUtils.GetFileName",
                "路径操作",
                () => IOUtils.GetFileName(testPath),
                null,
                10000
            );
            
            // 测试原生Path.GetFileName性能
            PerformanceTestFramework.RunPerformanceTest(
                "Path.GetFileName",
                "路径操作",
                () => Path.GetFileName(testPath),
                "IOUtils.GetFileName",
                10000
            );
            
            // 测试GetFileExtension性能
            PerformanceTestFramework.RunPerformanceTest(
                "IOUtils.GetFileExtension",
                "路径操作",
                () => IOUtils.GetFileExtension(testPath),
                null,
                10000
            );
            
            // 测试原生Path.GetExtension性能
            PerformanceTestFramework.RunPerformanceTest(
                "Path.GetExtension",
                "路径操作",
                () => Path.GetExtension(testPath),
                "IOUtils.GetFileExtension",
                10000
            );
            
            // 测试GetDirectoryPath性能
            PerformanceTestFramework.RunPerformanceTest(
                "IOUtils.GetDirectoryPath",
                "路径操作",
                () => IOUtils.GetDirectoryPath(testPath),
                null,
                10000
            );
            
            // 测试原生Path.GetDirectoryName性能
            PerformanceTestFramework.RunPerformanceTest(
                "Path.GetDirectoryName",
                "路径操作",
                () => Path.GetDirectoryName(testPath),
                "IOUtils.GetDirectoryPath",
                10000
            );
            
            // 测试NormalizePath性能
            string unnormalizedPath = @"C:\Test\Path\.\To\..\Path\Some//File.txt";
            PerformanceTestFramework.RunPerformanceTest(
                "IOUtils.NormalizePath",
                "路径操作",
                () => IOUtils.NormalizePath(unnormalizedPath),
                null,
                10000
            );
            
            // 测试CombinePath性能
            PerformanceTestFramework.RunPerformanceTest(
                "IOUtils.CombinePath",
                "路径操作",
                () => IOUtils.CombinePath("C:", "Test", "Path", "File.txt"),
                null,
                10000
            );
            
            // 测试原生Path.Combine性能
            PerformanceTestFramework.RunPerformanceTest(
                "Path.Combine",
                "路径操作",
                () => Path.Combine(Path.Combine(Path.Combine("C:", "Test"), "Path"), "File.txt"),
                "IOUtils.CombinePath",
                10000
            );
        }
        
        [Test]
        public void FileOperations_Performance_Test()
        {
            // 测试ReadAllText性能
            PerformanceTestFramework.RunPerformanceTest(
                "IOUtils.ReadAllText",
                "文件操作",
                () => IOUtils.ReadAllText(testTextFilePath),
                null,
                100
            );
            
            // 测试原生File.ReadAllText性能
            PerformanceTestFramework.RunPerformanceTest(
                "File.ReadAllText",
                "文件操作",
                () => File.ReadAllText(testTextFilePath),
                "IOUtils.ReadAllText",
                100
            );
            
            // 测试WriteAllText性能
            string content = File.ReadAllText(testTextFilePath);
            string outputPath = Path.Combine(testDirectory, "output_text.txt");
            
            PerformanceTestFramework.RunPerformanceTest(
                "IOUtils.WriteAllText",
                "文件操作",
                () => IOUtils.WriteAllText(outputPath, content),
                null,
                100
            );
            
            // 测试原生File.WriteAllText性能
            PerformanceTestFramework.RunPerformanceTest(
                "File.WriteAllText",
                "文件操作",
                () => File.WriteAllText(outputPath, content),
                "IOUtils.WriteAllText",
                100
            );
            
            // 测试ReadAllBytes性能
            PerformanceTestFramework.RunPerformanceTest(
                "IOUtils.ReadAllBytes",
                "文件操作",
                () => IOUtils.ReadAllBytes(testBinaryFilePath),
                null,
                100
            );
            
            // 测试原生File.ReadAllBytes性能
            PerformanceTestFramework.RunPerformanceTest(
                "File.ReadAllBytes",
                "文件操作",
                () => File.ReadAllBytes(testBinaryFilePath),
                "IOUtils.ReadAllBytes",
                100
            );
            
            // 测试WriteAllBytes性能
            byte[] binaryContent = File.ReadAllBytes(testBinaryFilePath);
            string outputBinaryPath = Path.Combine(testDirectory, "output_binary.dat");
            
            PerformanceTestFramework.RunPerformanceTest(
                "IOUtils.WriteAllBytes",
                "文件操作",
                () => IOUtils.WriteAllBytes(outputBinaryPath, binaryContent),
                null,
                100
            );
            
            // 测试原生File.WriteAllBytes性能
            PerformanceTestFramework.RunPerformanceTest(
                "File.WriteAllBytes",
                "文件操作",
                () => File.WriteAllBytes(outputBinaryPath, binaryContent),
                "IOUtils.WriteAllBytes",
                100
            );
        }
        
        [Test]
        public void HashCalculation_Performance_Test()
        {
            // 测试CalculateMD5性能
            PerformanceTestFramework.RunPerformanceTest(
                "IOUtils.CalculateMD5",
                "哈希计算",
                () => IOUtils.CalculateMD5(testLargeFilePath),
                null,
                10
            );
            
            // 测试原生MD5计算性能
            PerformanceTestFramework.RunPerformanceTest(
                "Native MD5 Calculation",
                "哈希计算",
                () => {
                    using (var md5 = System.Security.Cryptography.MD5.Create())
                    using (var stream = File.OpenRead(testLargeFilePath))
                    {
                        byte[] hash = md5.ComputeHash(stream);
                        string result = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                        return; // 不返回值，避免与Action委托不匹配
                    }
                },
                "IOUtils.CalculateMD5",
                10
            );
            
            // 测试CalculateSHA1性能
            PerformanceTestFramework.RunPerformanceTest(
                "IOUtils.CalculateSHA1",
                "哈希计算",
                () => IOUtils.CalculateSHA1(testLargeFilePath),
                null,
                10
            );
            
            // 测试原生SHA1计算性能
            PerformanceTestFramework.RunPerformanceTest(
                "Native SHA1 Calculation",
                "哈希计算",
                () => {
                    using (var sha1 = System.Security.Cryptography.SHA1.Create())
                    using (var stream = File.OpenRead(testLargeFilePath))
                    {
                        byte[] hash = sha1.ComputeHash(stream);
                        string result = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                        return; // 不返回值，避免与Action委托不匹配
                    }
                },
                "IOUtils.CalculateSHA1",
                10
            );
            
            // 测试CalculateSHA256性能
            PerformanceTestFramework.RunPerformanceTest(
                "IOUtils.CalculateSHA256",
                "哈希计算",
                () => IOUtils.CalculateSHA256(testLargeFilePath),
                null,
                10
            );
            
            // 测试原生SHA256计算性能
            PerformanceTestFramework.RunPerformanceTest(
                "Native SHA256 Calculation",
                "哈希计算",
                () => {
                    using (var sha256 = System.Security.Cryptography.SHA256.Create())
                    using (var stream = File.OpenRead(testLargeFilePath))
                    {
                        byte[] hash = sha256.ComputeHash(stream);
                        string result = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                        return; // 不返回值，避免与Action委托不匹配
                    }
                },
                "IOUtils.CalculateSHA256",
                10
            );
        }
        
        [Test]
        public void FileMonitoring_Performance_Test()
        {
            // 测试StartWatching性能
            PerformanceTestFramework.RunPerformanceTest(
                "IOUtils.StartWatching",
                "文件监控",
                () => {
                    string watcherId = IOUtils.StartWatching(
                        testDirectory,
                        (e) => { },
                        (e) => { },
                        (e) => { },
                        (e) => { },
                        "*.txt",
                        true,
                        300
                    );
                    IOUtils.StopWatching(watcherId);
                },
                null,
                100
            );
            
            // 测试原生FileSystemWatcher性能
            PerformanceTestFramework.RunPerformanceTest(
                "Native FileSystemWatcher",
                "文件监控",
                () => {
                    using (var watcher = new FileSystemWatcher(testDirectory, "*.txt"))
                    {
                        watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                        watcher.Changed += (sender, e) => { };
                        watcher.Created += (sender, e) => { };
                        watcher.Deleted += (sender, e) => { };
                        watcher.Renamed += (sender, e) => { };
                        watcher.EnableRaisingEvents = true;
                        watcher.IncludeSubdirectories = true;
                    }
                },
                "IOUtils.StartWatching",
                100
            );
        }
        
        [Test]
        public void ScalabilityTest_LargeFiles()
        {
            // 测试大文件读取性能
            PerformanceTestFramework.RunPerformanceTest(
                "IOUtils.ReadAllBytes (10MB)",
                "大文件操作",
                () => IOUtils.ReadAllBytes(testLargeFilePath),
                null,
                10
            );
            
            // 测试原生大文件读取性能
            PerformanceTestFramework.RunPerformanceTest(
                "File.ReadAllBytes (10MB)",
                "大文件操作",
                () => File.ReadAllBytes(testLargeFilePath),
                "IOUtils.ReadAllBytes (10MB)",
                10
            );
            
            // 测试大文件写入性能
            byte[] largeContent = File.ReadAllBytes(testLargeFilePath);
            string outputLargePath = Path.Combine(testDirectory, "output_large.dat");
            
            PerformanceTestFramework.RunPerformanceTest(
                "IOUtils.WriteAllBytes (10MB)",
                "大文件操作",
                () => IOUtils.WriteAllBytes(outputLargePath, largeContent),
                null,
                10
            );
            
            // 测试原生大文件写入性能
            PerformanceTestFramework.RunPerformanceTest(
                "File.WriteAllBytes (10MB)",
                "大文件操作",
                () => File.WriteAllBytes(outputLargePath, largeContent),
                "IOUtils.WriteAllBytes (10MB)",
                10
            );
        }
    }
} 