using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace TByd.Core.Utils.Tests
{
    public class IOUtilsTests
    {
        private string _testDirectory;
        private string _testFile;
        private string _testContent;

        [SetUp]
        public void SetUp()
        {
            // 创建测试目录和文件
            _testDirectory = Path.Combine(Application.temporaryCachePath, "IOUtilsTests");
            _testFile = Path.Combine(_testDirectory, "test.txt");
            _testContent = "这是测试内容\n这是第二行";

            // 确保测试开始前目录清空
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }

            Directory.CreateDirectory(_testDirectory);
            File.WriteAllText(_testFile, _testContent);
        }

        [TearDown]
        public void TearDown()
        {
            // 清理测试目录
            try
            {
                IOUtils.StopAllWatching(); // 确保停止所有文件监控
                
                if (Directory.Exists(_testDirectory))
                {
                    Directory.Delete(_testDirectory, true);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"清理测试目录失败: {ex.Message}");
            }
        }

        #region 文件路径处理测试

        [Test]
        public void NormalizePath_StandardizesSeparators()
        {
            // 测试混合使用反斜杠和正斜杠的路径
            string path = "foo\\bar/baz\\qux";
            string normalized = IOUtils.NormalizePath(path);

            Assert.AreEqual("foo/bar/baz/qux", normalized);
        }

        [Test]
        public void NormalizePath_RemovesRedundantSeparators()
        {
            // 测试包含重复分隔符的路径
            string path = "foo//bar///baz";
            string normalized = IOUtils.NormalizePath(path);

            Assert.AreEqual("foo/bar/baz", normalized);
        }

        [Test]
        public void NormalizePath_ResolvesDotDotNotation()
        {
            // 测试包含父目录引用的路径
            string path = "foo/bar/../baz/./qux/..";
            string normalized = IOUtils.NormalizePath(path);

            Assert.AreEqual("foo/baz", normalized);
        }

        [Test]
        public void CombinePath_CombinesPathsCorrectly()
        {
            // 组合路径测试
            string result = IOUtils.CombinePath("foo", "bar", "baz");
            Assert.AreEqual("foo/bar/baz", result);

            // 处理前导/尾随分隔符
            result = IOUtils.CombinePath("foo/", "/bar/", "/baz");
            Assert.AreEqual("foo/bar/baz", result);

            // 处理空路径部分
            result = IOUtils.CombinePath("foo", "", "baz");
            Assert.AreEqual("foo/baz", result);
        }

        [Test]
        public void GetRelativePath_ReturnsCorrectRelativePath()
        {
            // 测试获取相对路径
            string basePath = "foo/bar/baz";
            string targetPath = "foo/qux/quux";
            string result = IOUtils.GetRelativePath(basePath, targetPath);

            Assert.AreEqual("../../qux/quux", result);
        }

        [Test]
        public void GetExtension_ReturnsCorrectExtension()
        {
            // 测试获取扩展名
            Assert.AreEqual("txt", IOUtils.GetExtension("foo.txt"));
            Assert.AreEqual("jpg", IOUtils.GetExtension("path/to/image.jpg"));
            Assert.AreEqual("", IOUtils.GetExtension("no-extension"));
            Assert.AreEqual("gz", IOUtils.GetExtension("archive.tar.gz"));
        }

        [Test]
        public void GetFileNameWithoutExtension_ReturnsCorrectName()
        {
            // 测试获取不带扩展名的文件名
            Assert.AreEqual("foo", IOUtils.GetFileNameWithoutExtension("foo.txt"));
            Assert.AreEqual("image", IOUtils.GetFileNameWithoutExtension("path/to/image.jpg"));
            Assert.AreEqual("no-extension", IOUtils.GetFileNameWithoutExtension("no-extension"));
            Assert.AreEqual("archive.tar", IOUtils.GetFileNameWithoutExtension("archive.tar.gz"));
        }

        [Test]
        public void GetDirectoryPath_ReturnsCorrectPath()
        {
            // 测试获取目录路径
            Assert.AreEqual("path/to", IOUtils.GetDirectoryPath("path/to/file.txt"));
            Assert.AreEqual("", IOUtils.GetDirectoryPath("file.txt"));
            Assert.AreEqual("path", IOUtils.GetDirectoryPath("path/"));
        }

        #endregion

        #region 文件操作测试

        [Test]
        public void ReadAllText_ReadsTextCorrectly()
        {
            // 测试读取文本
            string content = IOUtils.ReadAllText(_testFile);
            Assert.AreEqual(_testContent, content);
        }

        [Test]
        public void ReadAllLines_ReadsLinesCorrectly()
        {
            // 测试读取行
            string[] lines = IOUtils.ReadAllLines(_testFile);
            Assert.AreEqual(2, lines.Length);
            Assert.AreEqual("这是测试内容", lines[0]);
            Assert.AreEqual("这是第二行", lines[1]);
        }

        [Test]
        public void WriteAllText_WritesTextCorrectly()
        {
            // 测试写入文本
            string newFile = Path.Combine(_testDirectory, "write-test.txt");
            string content = "新内容";
            
            IOUtils.WriteAllText(newFile, content);
            
            Assert.IsTrue(File.Exists(newFile));
            Assert.AreEqual(content, File.ReadAllText(newFile));
        }

        [Test]
        public void WriteAllLines_WritesLinesCorrectly()
        {
            // 测试写入行
            string newFile = Path.Combine(_testDirectory, "write-lines-test.txt");
            string[] lines = { "第一行", "第二行", "第三行" };
            
            IOUtils.WriteAllLines(newFile, lines);
            
            Assert.IsTrue(File.Exists(newFile));
            CollectionAssert.AreEqual(lines, File.ReadAllLines(newFile));
        }

        [Test]
        public void AppendText_AppendsTextCorrectly()
        {
            // 测试追加文本
            string appendedContent = "\n追加的内容";
            IOUtils.WriteAllText(_testFile, appendedContent, append: true);
            
            string newContent = IOUtils.ReadAllText(_testFile);
            Assert.AreEqual(_testContent + appendedContent, newContent);
        }

        [Test]
        public void CopyFile_CopiesFileCorrectly()
        {
            // 测试复制文件
            string destFile = Path.Combine(_testDirectory, "copy-test.txt");
            bool result = IOUtils.CopyFile(_testFile, destFile);
            
            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(destFile));
            Assert.AreEqual(_testContent, File.ReadAllText(destFile));
        }

        [Test]
        public void MoveFile_MovesFileCorrectly()
        {
            // 测试移动文件
            string sourceFile = Path.Combine(_testDirectory, "move-source.txt");
            string destFile = Path.Combine(_testDirectory, "move-dest.txt");
            
            File.WriteAllText(sourceFile, _testContent);
            bool result = IOUtils.MoveFile(sourceFile, destFile);
            
            Assert.IsTrue(result);
            Assert.IsFalse(File.Exists(sourceFile));
            Assert.IsTrue(File.Exists(destFile));
            Assert.AreEqual(_testContent, File.ReadAllText(destFile));
        }

        [Test]
        public void DeleteFile_DeletesFileCorrectly()
        {
            // 测试删除文件
            string fileToDelete = Path.Combine(_testDirectory, "to-delete.txt");
            File.WriteAllText(fileToDelete, "删除我");
            
            Assert.IsTrue(File.Exists(fileToDelete));
            bool result = IOUtils.DeleteFile(fileToDelete);
            
            Assert.IsTrue(result);
            Assert.IsFalse(File.Exists(fileToDelete));
        }

        #endregion

        #region 目录操作测试

        [Test]
        public void CreateDirectory_CreatesDirectoryCorrectly()
        {
            // 测试创建目录
            string newDir = Path.Combine(_testDirectory, "new-dir");
            bool result = IOUtils.CreateDirectory(newDir);
            
            Assert.IsTrue(result);
            Assert.IsTrue(Directory.Exists(newDir));
        }

        [Test]
        public void CopyDirectory_CopiesDirectoryCorrectly()
        {
            // 准备测试目录结构
            string sourceDir = Path.Combine(_testDirectory, "source-dir");
            string destDir = Path.Combine(_testDirectory, "dest-dir");
            
            Directory.CreateDirectory(sourceDir);
            string subDir = Path.Combine(sourceDir, "sub-dir");
            Directory.CreateDirectory(subDir);
            
            File.WriteAllText(Path.Combine(sourceDir, "file1.txt"), "文件1");
            File.WriteAllText(Path.Combine(subDir, "file2.txt"), "文件2");

            // 执行复制
            bool result = IOUtils.CopyDirectory(sourceDir, destDir);
            
            // 验证结果
            Assert.IsTrue(result);
            Assert.IsTrue(Directory.Exists(destDir));
            Assert.IsTrue(Directory.Exists(Path.Combine(destDir, "sub-dir")));
            Assert.IsTrue(File.Exists(Path.Combine(destDir, "file1.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(destDir, "sub-dir", "file2.txt")));
            Assert.AreEqual("文件1", File.ReadAllText(Path.Combine(destDir, "file1.txt")));
            Assert.AreEqual("文件2", File.ReadAllText(Path.Combine(destDir, "sub-dir", "file2.txt")));
        }

        [Test]
        public void DeleteDirectory_DeletesDirectoryCorrectly()
        {
            // 准备测试目录
            string dirToDelete = Path.Combine(_testDirectory, "dir-to-delete");
            Directory.CreateDirectory(dirToDelete);
            File.WriteAllText(Path.Combine(dirToDelete, "file.txt"), "内容");
            
            // 执行删除
            bool result = IOUtils.DeleteDirectory(dirToDelete);
            
            // 验证结果
            Assert.IsTrue(result);
            Assert.IsFalse(Directory.Exists(dirToDelete));
        }

        [Test]
        public void GetFiles_ReturnsCorrectFiles()
        {
            // 准备测试目录和文件
            string dir = Path.Combine(_testDirectory, "files-test");
            Directory.CreateDirectory(dir);
            string subDir = Path.Combine(dir, "sub-dir");
            Directory.CreateDirectory(subDir);
            
            File.WriteAllText(Path.Combine(dir, "file1.txt"), "");
            File.WriteAllText(Path.Combine(dir, "file2.txt"), "");
            File.WriteAllText(Path.Combine(dir, "image.jpg"), "");
            File.WriteAllText(Path.Combine(subDir, "file3.txt"), "");
            
            // 测试非递归获取所有文件
            string[] files = IOUtils.GetFiles(dir);
            Assert.AreEqual(3, files.Length);
            
            // 测试递归获取所有文件
            files = IOUtils.GetFiles(dir, recursive: true);
            Assert.AreEqual(4, files.Length);
            
            // 测试使用模式过滤
            files = IOUtils.GetFiles(dir, "*.txt", recursive: true);
            Assert.AreEqual(3, files.Length);
        }

        [Test]
        public void GetDirectories_ReturnsCorrectDirectories()
        {
            // 准备测试目录结构
            string dir = Path.Combine(_testDirectory, "dirs-test");
            Directory.CreateDirectory(dir);
            
            string subDir1 = Path.Combine(dir, "sub-dir1");
            string subDir2 = Path.Combine(dir, "sub-dir2");
            string subSubDir = Path.Combine(subDir1, "sub-sub-dir");
            
            Directory.CreateDirectory(subDir1);
            Directory.CreateDirectory(subDir2);
            Directory.CreateDirectory(subSubDir);
            
            // 测试非递归获取子目录
            string[] dirs = IOUtils.GetDirectories(dir);
            Assert.AreEqual(2, dirs.Length);
            
            // 测试递归获取所有子目录
            dirs = IOUtils.GetDirectories(dir, true);
            Assert.AreEqual(3, dirs.Length);
        }

        #endregion

        #region 文件信息测试

        [Test]
        public void GetFileSize_ReturnsCorrectSize()
        {
            // 创建测试文件
            string filePath = Path.Combine(_testDirectory, "size-test.txt");
            string content = "测试文件大小";
            File.WriteAllText(filePath, content, Encoding.UTF8);
            
            // 获取实际文件大小
            long expectedSize = new FileInfo(filePath).Length;
            
            // 获取通过IOUtils计算的文件大小
            long actualSize = IOUtils.GetFileSize(filePath);
            
            // 验证文件大小
            Assert.AreEqual(expectedSize, actualSize);
        }

        [Test]
        public void GetLastModifiedTime_ReturnsCorrectTime()
        {
            // 测试获取文件修改时间
            DateTime modTime = IOUtils.GetLastModifiedTime(_testFile);
            DateTime actual = File.GetLastWriteTime(_testFile);
            
            // 验证修改时间
            Assert.AreEqual(actual, modTime);
        }

        [Test]
        public void FileExists_ReturnsCorrectStatus()
        {
            // 测试文件存在检查
            Assert.IsTrue(IOUtils.FileExists(_testFile));
            Assert.IsFalse(IOUtils.FileExists(Path.Combine(_testDirectory, "not-exist.txt")));
        }

        [Test]
        public void DirectoryExists_ReturnsCorrectStatus()
        {
            // 测试目录存在检查
            Assert.IsTrue(IOUtils.DirectoryExists(_testDirectory));
            Assert.IsFalse(IOUtils.DirectoryExists(Path.Combine(_testDirectory, "not-exist-dir")));
        }

        #endregion

        #region 文件类型检测测试

        [Test]
        public void IsImageFile_IdentifiesImagesCorrectly()
        {
            // 测试图片文件识别
            Assert.IsTrue(IOUtils.IsImageFile("test.jpg"));
            Assert.IsTrue(IOUtils.IsImageFile("test.png"));
            Assert.IsTrue(IOUtils.IsImageFile("test.gif"));
            Assert.IsFalse(IOUtils.IsImageFile("test.txt"));
            Assert.IsFalse(IOUtils.IsImageFile("test.mp3"));
        }

        [Test]
        public void IsTextFile_IdentifiesTextFilesCorrectly()
        {
            // 创建测试文本文件
            string textFile = Path.Combine(_testDirectory, "text.txt");
            File.WriteAllText(textFile, "这是文本内容");
            
            // 创建测试二进制文件
            string binaryFile = Path.Combine(_testDirectory, "binary.bin");
            using (var stream = new FileStream(binaryFile, FileMode.Create))
            {
                stream.WriteByte(0);  // 写入NULL字节，这在文本文件中通常不会出现
                stream.WriteByte(1);
                stream.WriteByte(2);
            }
            
            // 测试文本文件识别
            Assert.IsTrue(IOUtils.IsTextFile(textFile));
            Assert.IsFalse(IOUtils.IsTextFile(binaryFile));
        }

        [Test]
        public void IsAudioFile_IdentifiesAudioFilesCorrectly()
        {
            // 测试音频文件识别
            Assert.IsTrue(IOUtils.IsAudioFile("test.mp3"));
            Assert.IsTrue(IOUtils.IsAudioFile("test.wav"));
            Assert.IsTrue(IOUtils.IsAudioFile("test.ogg"));
            Assert.IsFalse(IOUtils.IsAudioFile("test.txt"));
            Assert.IsFalse(IOUtils.IsAudioFile("test.jpg"));
        }

        [Test]
        public void IsVideoFile_IdentifiesVideoFilesCorrectly()
        {
            // 测试视频文件识别
            Assert.IsTrue(IOUtils.IsVideoFile("test.mp4"));
            Assert.IsTrue(IOUtils.IsVideoFile("test.avi"));
            Assert.IsTrue(IOUtils.IsVideoFile("test.mov"));
            Assert.IsFalse(IOUtils.IsVideoFile("test.txt"));
            Assert.IsFalse(IOUtils.IsVideoFile("test.jpg"));
        }

        #endregion

        #region 文件哈希计算测试

        [Test]
        public void CalculateMD5_CalculatesHashCorrectly()
        {
            // 创建一个内容固定的测试文件
            string hashTestFile = Path.Combine(_testDirectory, "hash-test.txt");
            string content = "测试哈希计算";
            File.WriteAllText(hashTestFile, content, Encoding.UTF8);
            
            // 从文件读取实际写入的内容，确保与方法计算时使用相同的数据
            byte[] fileBytes = File.ReadAllBytes(hashTestFile);
            
            // 使用文件中的实际字节计算预期的MD5
            string expectedMD5;
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var hash = md5.ComputeHash(fileBytes);
                expectedMD5 = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
            
            // 使用IOUtils计算MD5
            string actualMD5 = IOUtils.CalculateMD5(hashTestFile);
            
            // 验证哈希值
            Assert.AreEqual(expectedMD5, actualMD5);
        }

        [Test]
        public void CalculateSHA1_CalculatesHashCorrectly()
        {
            // 创建一个内容固定的测试文件
            string hashTestFile = Path.Combine(_testDirectory, "sha1-test.txt");
            string content = "测试SHA1哈希计算";
            File.WriteAllText(hashTestFile, content, Encoding.UTF8);
            
            // 从文件读取实际写入的内容，确保与方法计算时使用相同的数据
            byte[] fileBytes = File.ReadAllBytes(hashTestFile);
            
            // 使用文件中的实际字节计算预期的SHA1
            string expectedSHA1;
            using (var sha1 = System.Security.Cryptography.SHA1.Create())
            {
                var hash = sha1.ComputeHash(fileBytes);
                expectedSHA1 = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
            
            // 使用IOUtils计算SHA1
            string actualSHA1 = IOUtils.CalculateSHA1(hashTestFile);
            
            // 验证哈希值
            Assert.AreEqual(expectedSHA1, actualSHA1);
        }

        [Test]
        public void CalculateSHA256_CalculatesHashCorrectly()
        {
            // 创建一个内容固定的测试文件
            string hashTestFile = Path.Combine(_testDirectory, "sha256-test.txt");
            string content = "测试SHA256哈希计算";
            File.WriteAllText(hashTestFile, content, Encoding.UTF8);
            
            // 从文件读取实际写入的内容，确保与方法计算时使用相同的数据
            byte[] fileBytes = File.ReadAllBytes(hashTestFile);
            
            // 使用文件中的实际字节计算预期的SHA256
            string expectedSHA256;
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hash = sha256.ComputeHash(fileBytes);
                expectedSHA256 = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
            
            // 使用IOUtils计算SHA256
            string actualSHA256 = IOUtils.CalculateSHA256(hashTestFile);
            
            // 验证哈希值
            Assert.AreEqual(expectedSHA256, actualSHA256);
        }

        #endregion

        #region 异步IO操作测试

        [UnityTest]
        public IEnumerator ReadAllTextAsync_ReadsTextCorrectly()
        {
            // 测试异步读取文本
            Task<string> task = IOUtils.ReadAllTextAsync(_testFile);
            
            while (!task.IsCompleted)
            {
                yield return null;
            }
            
            // 验证结果
            Assert.AreEqual(_testContent, task.Result);
        }

        [UnityTest]
        public IEnumerator WriteAllTextAsync_WritesTextCorrectly()
        {
            // 测试异步写入文本
            string asyncWriteFile = Path.Combine(_testDirectory, "async-write.txt");
            string content = "异步写入测试";
            
            Task task = IOUtils.WriteAllTextAsync(asyncWriteFile, content);
            
            while (!task.IsCompleted)
            {
                yield return null;
            }
            
            // 验证结果
            Assert.IsTrue(File.Exists(asyncWriteFile));
            Assert.AreEqual(content, File.ReadAllText(asyncWriteFile));
        }

        [UnityTest]
        public IEnumerator ReadAllBytesAsync_ReadsBytesCorrectly()
        {
            // 创建一个二进制测试文件
            string bytesFile = Path.Combine(_testDirectory, "bytes-test.bin");
            byte[] testBytes = { 0, 1, 2, 3, 4, 5 };
            File.WriteAllBytes(bytesFile, testBytes);
            
            // 测试异步读取字节
            Task<byte[]> task = IOUtils.ReadAllBytesAsync(bytesFile);
            
            while (!task.IsCompleted)
            {
                yield return null;
            }
            
            // 验证结果
            CollectionAssert.AreEqual(testBytes, task.Result);
        }

        [UnityTest]
        public IEnumerator CalculateMD5Async_CalculatesHashCorrectly()
        {
            // 创建一个内容固定的测试文件
            string hashTestFile = Path.Combine(_testDirectory, "async-hash-test.txt");
            string content = "异步哈希计算测试";
            File.WriteAllText(hashTestFile, content, Encoding.UTF8);
            
            // 从文件读取实际写入的内容，确保与方法计算时使用相同的数据
            byte[] fileBytes = File.ReadAllBytes(hashTestFile);
            
            // 使用文件中的实际字节计算预期的MD5
            string expectedMD5;
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var hash = md5.ComputeHash(fileBytes);
                expectedMD5 = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
            
            // 使用IOUtils异步计算MD5
            Task<string> task = IOUtils.CalculateMD5Async(hashTestFile);
            
            while (!task.IsCompleted)
            {
                yield return null;
            }
            
            // 验证哈希值
            Assert.AreEqual(expectedMD5, task.Result);
        }

        #endregion

        #region 文件监控测试

        [UnityTest]
        public IEnumerator StartWatching_DetectsFileChanges()
        {
            // 准备测试
            string watchFile = Path.Combine(_testDirectory, "watch-test.txt");
            File.WriteAllText(watchFile, "初始内容");
            
            bool changeDetected = false;
            
            // 启动监控
            string watcherId = IOUtils.StartWatching(
                watchFile,
                onChange: (path, changeType) => { changeDetected = true; }
            );
            
            // 等待文件系统监控器初始化
            yield return new WaitForSeconds(0.5f);
            
            // 修改文件
            File.AppendAllText(watchFile, " - 修改的内容");
            
            // 等待事件触发
            yield return new WaitForSeconds(1.0f);
            
            // 停止监控
            IOUtils.StopWatching(watcherId);
            
            // 验证是否检测到变化
            Assert.IsTrue(changeDetected, "未检测到文件变化");
        }

        [Test]
        public void StopWatching_StopsMonitoring()
        {
            // 准备测试
            string watchFile = Path.Combine(_testDirectory, "stop-watch-test.txt");
            File.WriteAllText(watchFile, "监控测试");
            
            // 启动监控
            string watcherId = IOUtils.StartWatching(watchFile);
            
            // 停止监控
            bool result = IOUtils.StopWatching(watcherId);
            
            // 验证停止成功
            Assert.IsTrue(result);
            
            // 尝试停止一个不存在的监控器
            result = IOUtils.StopWatching("non-existent-id");
            Assert.IsFalse(result);
        }

        #endregion
    }
} 