using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;

namespace TByd.Core.Utils.Tests.Editor.Unit
{
    /// <summary>
    /// IOUtils工具类的单元测试
    /// </summary>
    [TestFixture]
    public class IOUtilsTests
    {
        private string _testFolderPath;
        private string _testFilePath;
        private string _testContent;
        private byte[] _testBinaryData;
        
        [SetUp]
        public void Setup()
        {
            // 创建测试用的临时文件夹和文件
            _testFolderPath = Path.Combine(Application.temporaryCachePath, "IOUtilsTests");
            _testFilePath = Path.Combine(_testFolderPath, "test_file.txt");
            _testContent = "这是测试内容，用于测试IOUtils类的功能。";
            _testBinaryData = Encoding.UTF8.GetBytes(_testContent);
            
            // 确保测试目录存在
            if (!Directory.Exists(_testFolderPath))
            {
                Directory.CreateDirectory(_testFolderPath);
            }
        }
        
        [TearDown]
        public void TearDown()
        {
            // 清理测试文件和文件夹
            try
            {
                if (File.Exists(_testFilePath))
                {
                    File.Delete(_testFilePath);
                }
                
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
        
        #region CreateDirectory Tests
        
        /// <summary>
        /// 测试CreateDirectory在目录不存在时创建目录
        /// </summary>
        [Test]
        public void CreateDirectory_WhenDirectoryDoesNotExist_CreatesDirectory()
        {
            // 安排
            string directoryPath = Path.Combine(_testFolderPath, "new_directory");
            
            // 确保目录不存在
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
            
            // 执行
            IOUtils.CreateDirectory(directoryPath);
            
            // 断言
            Assert.IsTrue(Directory.Exists(directoryPath), "目录应该被创建");
        }
        
        /// <summary>
        /// 测试CreateDirectory在目录已存在时不报错
        /// </summary>
        [Test]
        public void CreateDirectory_WhenDirectoryExists_DoesNotThrow()
        {
            // 安排
            string directoryPath = Path.Combine(_testFolderPath, "existing_directory");
            
            // 确保目录存在
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            
            // 执行和断言
            Assert.DoesNotThrow(() => IOUtils.CreateDirectory(directoryPath), "对已存在的目录调用不应抛出异常");
        }
        
        #endregion
        
        #region FileExists Tests
        
        /// <summary>
        /// 测试FileExists在文件存在时返回true
        /// </summary>
        [Test]
        public void FileExists_WhenFileExists_ReturnsTrue()
        {
            // 安排
            File.WriteAllText(_testFilePath, _testContent);
            
            // 执行
            bool result = IOUtils.FileExists(_testFilePath);
            
            // 断言
            Assert.IsTrue(result, "文件存在应返回true");
        }
        
        /// <summary>
        /// 测试FileExists在文件不存在时返回false
        /// </summary>
        [Test]
        public void FileExists_WhenFileDoesNotExist_ReturnsFalse()
        {
            // 安排
            string nonExistentFile = Path.Combine(_testFolderPath, "non_existent_file.txt");
            
            // 确保文件不存在
            if (File.Exists(nonExistentFile))
            {
                File.Delete(nonExistentFile);
            }
            
            // 执行
            bool result = IOUtils.FileExists(nonExistentFile);
            
            // 断言
            Assert.IsFalse(result, "文件不存在应返回false");
        }
        
        #endregion
        
        #region DirectoryExists Tests
        
        /// <summary>
        /// 测试DirectoryExists在目录存在时返回true
        /// </summary>
        [Test]
        public void DirectoryExists_WhenDirectoryExists_ReturnsTrue()
        {
            // 安排 - 使用已经在Setup中创建的目录
            
            // 执行
            bool result = IOUtils.DirectoryExists(_testFolderPath);
            
            // 断言
            Assert.IsTrue(result, "目录存在应返回true");
        }
        
        /// <summary>
        /// 测试DirectoryExists在目录不存在时返回false
        /// </summary>
        [Test]
        public void DirectoryExists_WhenDirectoryDoesNotExist_ReturnsFalse()
        {
            // 安排
            string nonExistentDirectory = Path.Combine(_testFolderPath, "non_existent_directory");
            
            // 确保目录不存在
            if (Directory.Exists(nonExistentDirectory))
            {
                Directory.Delete(nonExistentDirectory, true);
            }
            
            // 执行
            bool result = IOUtils.DirectoryExists(nonExistentDirectory);
            
            // 断言
            Assert.IsFalse(result, "目录不存在应返回false");
        }
        
        #endregion
        
        #region ReadAllText Tests
        
        /// <summary>
        /// 测试ReadAllText在文件存在时正确读取内容
        /// </summary>
        [Test]
        public void ReadAllText_WhenFileExists_ReturnsCorrectContent()
        {
            // 安排
            File.WriteAllText(_testFilePath, _testContent);
            
            // 执行
            string result = IOUtils.ReadAllText(_testFilePath);
            
            // 断言
            Assert.AreEqual(_testContent, result, "应返回正确的文件内容");
        }
        
        /// <summary>
        /// 测试ReadAllText在文件不存在时抛出异常
        /// </summary>
        [Test]
        public void ReadAllText_WhenFileDoesNotExist_ThrowsException()
        {
            // 安排
            string nonExistentFile = Path.Combine(_testFolderPath, "non_existent_file.txt");
            
            // 确保文件不存在
            if (File.Exists(nonExistentFile))
            {
                File.Delete(nonExistentFile);
            }
            
            // 执行和断言
            Assert.Throws<FileNotFoundException>(() => IOUtils.ReadAllText(nonExistentFile), "读取不存在的文件应抛出异常");
        }
        
        #endregion
        
        #region WriteAllText Tests
        
        /// <summary>
        /// 测试WriteAllText创建并写入文件
        /// </summary>
        [Test]
        public void WriteAllText_CreatesAndWritesToFile()
        {
            // 安排
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
            
            // 执行
            IOUtils.WriteAllText(_testFilePath, _testContent);
            
            // 断言
            Assert.IsTrue(File.Exists(_testFilePath), "文件应该被创建");
            string content = File.ReadAllText(_testFilePath);
            Assert.AreEqual(_testContent, content, "文件内容应与写入内容一致");
        }
        
        /// <summary>
        /// 测试WriteAllText在目录不存在时创建目录
        /// </summary>
        [Test]
        public void WriteAllText_WhenDirectoryDoesNotExist_CreatesDirectory()
        {
            // 安排
            string newDirectory = Path.Combine(_testFolderPath, "new_directory");
            string newFilePath = Path.Combine(newDirectory, "test_file.txt");
            
            // 确保目录不存在
            if (Directory.Exists(newDirectory))
            {
                Directory.Delete(newDirectory, true);
            }
            
            // 执行
            IOUtils.WriteAllText(newFilePath, _testContent);
            
            // 断言
            Assert.IsTrue(Directory.Exists(newDirectory), "目录应该被创建");
            Assert.IsTrue(File.Exists(newFilePath), "文件应该被创建");
            string content = File.ReadAllText(newFilePath);
            Assert.AreEqual(_testContent, content, "文件内容应与写入内容一致");
        }
        
        #endregion
        
        #region ReadAllBytes Tests
        
        /// <summary>
        /// 测试ReadAllBytes在文件存在时正确读取内容
        /// </summary>
        [Test]
        public void ReadAllBytes_WhenFileExists_ReturnsCorrectContent()
        {
            // 安排
            File.WriteAllBytes(_testFilePath, _testBinaryData);
            
            // 执行
            byte[] result = IOUtils.ReadAllBytes(_testFilePath);
            
            // 断言
            CollectionAssert.AreEqual(_testBinaryData, result, "应返回正确的文件内容");
        }
        
        /// <summary>
        /// 测试ReadAllBytes在文件不存在时抛出异常
        /// </summary>
        [Test]
        public void ReadAllBytes_WhenFileDoesNotExist_ThrowsException()
        {
            // 安排
            string nonExistentFile = Path.Combine(_testFolderPath, "non_existent_file.bin");
            
            // 确保文件不存在
            if (File.Exists(nonExistentFile))
            {
                File.Delete(nonExistentFile);
            }
            
            // 执行和断言
            Assert.Throws<FileNotFoundException>(() => IOUtils.ReadAllBytes(nonExistentFile), "读取不存在的文件应抛出异常");
        }
        
        #endregion
        
        #region WriteAllBytes Tests
        
        /// <summary>
        /// 测试WriteAllBytes创建并写入文件
        /// </summary>
        [Test]
        public void WriteAllBytes_CreatesAndWritesToFile()
        {
            // 安排
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
            
            // 执行
            IOUtils.WriteAllBytes(_testFilePath, _testBinaryData);
            
            // 断言
            Assert.IsTrue(File.Exists(_testFilePath), "文件应该被创建");
            byte[] content = File.ReadAllBytes(_testFilePath);
            CollectionAssert.AreEqual(_testBinaryData, content, "文件内容应与写入内容一致");
        }
        
        /// <summary>
        /// 测试WriteAllBytes在目录不存在时创建目录
        /// </summary>
        [Test]
        public void WriteAllBytes_WhenDirectoryDoesNotExist_CreatesDirectory()
        {
            // 安排
            string newDirectory = Path.Combine(_testFolderPath, "new_directory");
            string newFilePath = Path.Combine(newDirectory, "test_file.bin");
            
            // 确保目录不存在
            if (Directory.Exists(newDirectory))
            {
                Directory.Delete(newDirectory, true);
            }
            
            // 执行
            IOUtils.WriteAllBytes(newFilePath, _testBinaryData);
            
            // 断言
            Assert.IsTrue(Directory.Exists(newDirectory), "目录应该被创建");
            Assert.IsTrue(File.Exists(newFilePath), "文件应该被创建");
            byte[] content = File.ReadAllBytes(newFilePath);
            CollectionAssert.AreEqual(_testBinaryData, content, "文件内容应与写入内容一致");
        }
        
        #endregion
        
        #region DeleteFile Tests
        
        /// <summary>
        /// 测试DeleteFile在文件存在时删除文件
        /// </summary>
        [Test]
        public void DeleteFile_WhenFileExists_DeletesFile()
        {
            // 安排
            File.WriteAllText(_testFilePath, _testContent);
            Assert.IsTrue(File.Exists(_testFilePath), "测试准备：文件应存在");
            
            // 执行
            IOUtils.DeleteFile(_testFilePath);
            
            // 断言
            Assert.IsFalse(File.Exists(_testFilePath), "文件应该被删除");
        }
        
        /// <summary>
        /// 测试DeleteFile在文件不存在时不抛出异常
        /// </summary>
        [Test]
        public void DeleteFile_WhenFileDoesNotExist_DoesNotThrow()
        {
            // 安排
            string nonExistentFile = Path.Combine(_testFolderPath, "non_existent_file.txt");
            
            // 确保文件不存在
            if (File.Exists(nonExistentFile))
            {
                File.Delete(nonExistentFile);
            }
            
            // 执行和断言
            Assert.DoesNotThrow(() => IOUtils.DeleteFile(nonExistentFile), "删除不存在的文件不应抛出异常");
        }
        
        #endregion
        
        #region DeleteDirectory Tests
        
        /// <summary>
        /// 测试DeleteDirectory在目录存在时删除目录
        /// </summary>
        [Test]
        public void DeleteDirectory_WhenDirectoryExists_DeletesDirectory()
        {
            // 安排
            string directoryToDelete = Path.Combine(_testFolderPath, "directory_to_delete");
            Directory.CreateDirectory(directoryToDelete);
            Assert.IsTrue(Directory.Exists(directoryToDelete), "测试准备：目录应存在");
            
            // 执行
            IOUtils.DeleteDirectory(directoryToDelete);
            
            // 断言
            Assert.IsFalse(Directory.Exists(directoryToDelete), "目录应该被删除");
        }
        
        /// <summary>
        /// 测试DeleteDirectory在目录包含文件时仍能删除
        /// </summary>
        [Test]
        public void DeleteDirectory_WhenDirectoryContainsFiles_DeletesDirectoryAndContents()
        {
            // 安排
            string directoryToDelete = Path.Combine(_testFolderPath, "directory_with_files");
            Directory.CreateDirectory(directoryToDelete);
            File.WriteAllText(Path.Combine(directoryToDelete, "test_file.txt"), _testContent);
            Assert.IsTrue(Directory.Exists(directoryToDelete), "测试准备：目录应存在");
            
            // 执行
            IOUtils.DeleteDirectory(directoryToDelete);
            
            // 断言
            Assert.IsFalse(Directory.Exists(directoryToDelete), "目录及其内容应该被删除");
        }
        
        /// <summary>
        /// 测试DeleteDirectory在目录不存在时不抛出异常
        /// </summary>
        [Test]
        public void DeleteDirectory_WhenDirectoryDoesNotExist_DoesNotThrow()
        {
            // 安排
            string nonExistentDirectory = Path.Combine(_testFolderPath, "non_existent_directory");
            
            // 确保目录不存在
            if (Directory.Exists(nonExistentDirectory))
            {
                Directory.Delete(nonExistentDirectory, true);
            }
            
            // 执行和断言
            Assert.DoesNotThrow(() => IOUtils.DeleteDirectory(nonExistentDirectory), "删除不存在的目录不应抛出异常");
        }
        
        #endregion
        
        #region NormalizePath Tests
        
        /// <summary>
        /// 测试NormalizePath正确格式化路径
        /// </summary>
        [Test]
        public void NormalizePath_CombinesPathsCorrectly()
        {
            // 安排
            string path1 = "path1";
            string path2 = "path2";
            string path3 = "path3";
            string combined = Path.Combine(path1, path2, path3);
            
            // 执行
            string result = IOUtils.NormalizePath(combined);
            
            // 断言
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Does.Contain(path1));
            Assert.That(result, Does.Contain(path2));
            Assert.That(result, Does.Contain(path3));
        }
        
        /// <summary>
        /// 测试CombinePath处理空路径部分
        /// </summary>
        [Test]
        public void CombinePath_WithEmptyPathParts_HandlesCorrectly()
        {
            // 注意：由于IOUtils.CombinePath已被废弃，此测试现在使用System.IO.Path.Combine
            // 安排
            string part1 = "path1";
            string part2 = "";
            string part3 = "path3";
            
            // 执行
            string result = Path.Combine(part1, Path.Combine(part2, part3));
            
            // 断言
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Does.Contain(part1));
            Assert.That(result, Does.Contain(part3));
        }
        
        #endregion
        
        #region GetFileName Tests
        
        /// <summary>
        /// 测试GetFileName正确提取文件名
        /// </summary>
        [Test]
        public void GetFileName_ReturnsCorrectFileName()
        {
            // 注意：由于IOUtils.GetFileName已被废弃，此测试现在使用System.IO.Path.GetFileName
            // 安排
            string path = Path.Combine("directory", "subdirectory", "file.txt");
            
            // 执行
            string result = Path.GetFileName(path);
            
            // 断言
            Assert.AreEqual("file.txt", result, "应返回正确的文件名");
        }
        
        /// <summary>
        /// 测试GetFileName处理没有目录部分的路径
        /// </summary>
        [Test]
        public void GetFileName_WithFileNameOnly_ReturnsFileName()
        {
            // 注意：由于IOUtils.GetFileName已被废弃，此测试现在使用System.IO.Path.GetFileName
            // 安排
            string path = "file.txt";
            
            // 执行
            string result = Path.GetFileName(path);
            
            // 断言
            Assert.AreEqual("file.txt", result, "应返回正确的文件名");
        }
        
        #endregion
        
        #region GetFileNameWithoutExtension Tests
        
        /// <summary>
        /// 测试GetFileNameWithoutExtension正确提取不带扩展名的文件名
        /// </summary>
        [Test]
        public void GetFileNameWithoutExtension_ReturnsCorrectFileNameWithoutExtension()
        {
            // 注意：由于IOUtils.GetFileNameWithoutExtension已被废弃，此测试现在使用System.IO.Path.GetFileNameWithoutExtension
            // 安排
            string path = Path.Combine("directory", "subdirectory", "file.txt");
            
            // 执行
            string result = Path.GetFileNameWithoutExtension(path);
            
            // 断言
            Assert.AreEqual("file", result, "应返回不带扩展名的文件名");
        }
        
        /// <summary>
        /// 测试GetFileNameWithoutExtension处理没有扩展名的路径
        /// </summary>
        [Test]
        public void GetFileNameWithoutExtension_WithoutExtension_ReturnsFileName()
        {
            // 注意：由于IOUtils.GetFileNameWithoutExtension已被废弃，此测试现在使用System.IO.Path.GetFileNameWithoutExtension
            // 安排
            string path = Path.Combine("directory", "subdirectory", "file");
            
            // 执行
            string result = Path.GetFileNameWithoutExtension(path);
            
            // 断言
            Assert.AreEqual("file", result, "应返回文件名");
        }
        
        #endregion
        
        #region GetFileExtension Tests
        
        /// <summary>
        /// 测试GetFileExtension正确提取文件扩展名
        /// </summary>
        [Test]
        public void GetFileExtension_ReturnsCorrectExtension()
        {
            // 安排
            string path = Path.Combine("directory", "subdirectory", "file.txt");
            
            // 执行
            string result = IOUtils.GetFileExtension(path);
            
            // 断言
            Assert.AreEqual(".txt", result, "应返回正确的扩展名");
        }
        
        /// <summary>
        /// 测试GetFileExtension处理没有扩展名的路径
        /// </summary>
        [Test]
        public void GetFileExtension_WithoutExtension_ReturnsEmptyString()
        {
            // 安排
            string path = Path.Combine("directory", "subdirectory", "file");
            
            // 执行
            string result = IOUtils.GetFileExtension(path);
            
            // 断言
            Assert.AreEqual("", result, "应返回空字符串");
        }
        
        #endregion
    }
} 