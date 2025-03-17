using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace TByd.Core.Utils
{
    /// <summary>
    /// 提供高性能、跨平台的IO操作工具类
    /// </summary>
    /// <remarks>
    /// IOUtils提供了一系列处理文件和目录的方法，包括文件读写、路径处理、
    /// 文件监控、异步IO操作、文件类型检测和文件哈希计算等功能。
    /// 所有方法都经过优化，尽量减少GC分配和性能开销，并确保跨平台兼容性。
    /// </remarks>
    public static class IOUtils
    {
        #region 文件路径处理

        /// <summary>
        /// 获取规范化的文件路径（标准化分隔符，移除多余的分隔符和相对路径符号）
        /// </summary>
        /// <param name="path">需要规范化的路径</param>
        /// <returns>规范化后的路径</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        public static string NormalizePath(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            // 替换反斜杠为正斜杠（标准化分隔符）
            path = path.Replace('\\', '/');
            
            // 移除连续的斜杠
            while (path.Contains("//"))
            {
                path = path.Replace("//", "/");
            }
            
            // 解析相对路径，例如 "../"
            var parts = new List<string>();
            foreach (var part in path.Split('/'))
            {
                if (part == "..")
                {
                    if (parts.Count > 0)
                        parts.RemoveAt(parts.Count - 1);
                }
                else if (!string.IsNullOrEmpty(part) && part != ".")
                {
                    parts.Add(part);
                }
            }
            
            return string.Join("/", parts);
        }

        /// <summary>
        /// 组合多个路径部分，确保分隔符正确
        /// </summary>
        /// <param name="parts">路径部分</param>
        /// <returns>组合后的路径</returns>
        /// <exception cref="ArgumentNullException">parts为null时抛出</exception>
        public static string CombinePath(params string[] parts)
        {
            if (parts == null) throw new ArgumentNullException(nameof(parts));
            if (parts.Length == 0) return string.Empty;

            var result = parts[0] ?? string.Empty;
            for (int i = 1; i < parts.Length; i++)
            {
                var part = parts[i];
                if (string.IsNullOrEmpty(part)) continue;

                if (!result.EndsWith("/") && !result.EndsWith("\\") && 
                    !part.StartsWith("/") && !part.StartsWith("\\"))
                {
                    result += "/";
                }
                result += part;
            }

            return NormalizePath(result);
        }

        /// <summary>
        /// 获取相对路径
        /// </summary>
        /// <param name="basePath">基础路径</param>
        /// <param name="targetPath">目标路径</param>
        /// <returns>从基础路径到目标路径的相对路径</returns>
        /// <exception cref="ArgumentNullException">basePath或targetPath为null时抛出</exception>
        public static string GetRelativePath(string basePath, string targetPath)
        {
            if (basePath == null) throw new ArgumentNullException(nameof(basePath));
            if (targetPath == null) throw new ArgumentNullException(nameof(targetPath));

            basePath = NormalizePath(basePath);
            targetPath = NormalizePath(targetPath);

            string[] baseParts = basePath.Split('/');
            string[] targetParts = targetPath.Split('/');

            // 找到共同前缀的长度
            int commonLength = 0;
            int minLength = Math.Min(baseParts.Length, targetParts.Length);
            while (commonLength < minLength && 
                   string.Equals(baseParts[commonLength], targetParts[commonLength], StringComparison.OrdinalIgnoreCase))
            {
                commonLength++;
            }

            // 构建相对路径
            var relativePathBuilder = new StringBuilder();
            
            // 添加上级目录
            for (int i = commonLength; i < baseParts.Length; i++)
            {
                if (!string.IsNullOrEmpty(baseParts[i]))
                {
                    relativePathBuilder.Append("../");
                }
            }
            
            // 添加目标目录
            for (int i = commonLength; i < targetParts.Length; i++)
            {
                if (!string.IsNullOrEmpty(targetParts[i]))
                {
                    relativePathBuilder.Append(targetParts[i]);
                    if (i < targetParts.Length - 1)
                    {
                        relativePathBuilder.Append('/');
                    }
                }
            }

            return relativePathBuilder.ToString();
        }

        /// <summary>
        /// 获取文件扩展名（不包含点）
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>扩展名（小写，不包含点）。如果没有扩展名，则返回空字符串</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        public static string GetExtension(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            int lastDotIndex = path.LastIndexOf('.');
            int lastSeparatorIndex = Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));

            if (lastDotIndex > lastSeparatorIndex && lastDotIndex < path.Length - 1)
            {
                return path.Substring(lastDotIndex + 1).ToLowerInvariant();
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取不带扩展名的文件名
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>不带扩展名的文件名</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        public static string GetFileNameWithoutExtension(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            int lastSeparatorIndex = Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));
            int lastDotIndex = path.LastIndexOf('.');

            if (lastDotIndex > lastSeparatorIndex)
            {
                return path.Substring(lastSeparatorIndex + 1, lastDotIndex - lastSeparatorIndex - 1);
            }
            else
            {
                return path.Substring(lastSeparatorIndex + 1);
            }
        }

        /// <summary>
        /// 获取文件所在的目录路径
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>目录路径</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        public static string GetDirectoryPath(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            // 处理末尾带斜杠的情况，如"path/"，需要在标准化前检测
            bool endsWithSeparator = (path.EndsWith("/") || path.EndsWith("\\")) && path.Length > 1;
            
            path = NormalizePath(path);
            
            int lastSeparatorIndex = path.LastIndexOf('/');

            if (lastSeparatorIndex >= 0)
            {
                // 有斜杠，截取到最后一个斜杠之前
                return path.Substring(0, lastSeparatorIndex);
            }
            else if (endsWithSeparator)
            {
                // 原路径是"path/"格式的，返回去掉斜杠后的部分
                return path;
            }

            // 没有斜杠，如"file.txt"，返回空字符串
            return string.Empty;
        }

        #endregion

        #region 文件操作

        /// <summary>
        /// 安全地读取文件的所有文本内容
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="encoding">字符编码，默认为UTF8</param>
        /// <returns>文件的文本内容</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        /// <exception cref="FileNotFoundException">文件不存在时抛出</exception>
        /// <exception cref="IOException">读取文件时发生IO错误时抛出</exception>
        public static string ReadAllText(string path, Encoding encoding = null)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException("指定的文件不存在", path);

            encoding = encoding ?? Encoding.UTF8;
            
            try
            {
                return File.ReadAllText(path, encoding);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogError($"读取文件失败: {path}, 错误: {ex.Message}");
                throw new IOException($"读取文件失败: {path}", ex);
            }
        }

        /// <summary>
        /// 安全地读取文件的所有行
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="encoding">字符编码，默认为UTF8</param>
        /// <returns>文件的所有行</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        /// <exception cref="FileNotFoundException">文件不存在时抛出</exception>
        /// <exception cref="IOException">读取文件时发生IO错误时抛出</exception>
        public static string[] ReadAllLines(string path, Encoding encoding = null)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException("指定的文件不存在", path);

            encoding = encoding ?? Encoding.UTF8;
            
            try
            {
                return File.ReadAllLines(path, encoding);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogError($"读取文件失败: {path}, 错误: {ex.Message}");
                throw new IOException($"读取文件失败: {path}", ex);
            }
        }

        /// <summary>
        /// 安全地写入文本到文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="content">要写入的内容</param>
        /// <param name="encoding">字符编码，默认为UTF8</param>
        /// <param name="append">是否追加到文件末尾，而不是覆盖</param>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        /// <exception cref="IOException">写入文件时发生IO错误时抛出</exception>
        public static void WriteAllText(string path, string content, Encoding encoding = null, bool append = false)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            
            encoding = encoding ?? Encoding.UTF8;
            
            try
            {
                // 确保目录存在
                string directory = GetDirectoryPath(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (append && File.Exists(path))
                {
                    File.AppendAllText(path, content, encoding);
                }
                else
                {
                    File.WriteAllText(path, content, encoding);
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogError($"写入文件失败: {path}, 错误: {ex.Message}");
                throw new IOException($"写入文件失败: {path}", ex);
            }
        }

        /// <summary>
        /// 安全地写入多行文本到文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="lines">要写入的行</param>
        /// <param name="encoding">字符编码，默认为UTF8</param>
        /// <param name="append">是否追加到文件末尾，而不是覆盖</param>
        /// <exception cref="ArgumentNullException">path或lines为null时抛出</exception>
        /// <exception cref="IOException">写入文件时发生IO错误时抛出</exception>
        public static void WriteAllLines(string path, IEnumerable<string> lines, Encoding encoding = null, bool append = false)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (lines == null) throw new ArgumentNullException(nameof(lines));
            
            encoding = encoding ?? Encoding.UTF8;
            
            try
            {
                // 确保目录存在
                string directory = GetDirectoryPath(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (append && File.Exists(path))
                {
                    File.AppendAllLines(path, lines, encoding);
                }
                else
                {
                    File.WriteAllLines(path, lines, encoding);
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogError($"写入文件失败: {path}, 错误: {ex.Message}");
                throw new IOException($"写入文件失败: {path}", ex);
            }
        }

        /// <summary>
        /// 安全地读取文件的所有字节
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>文件的字节数组</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        /// <exception cref="FileNotFoundException">文件不存在时抛出</exception>
        /// <exception cref="IOException">读取文件时发生IO错误时抛出</exception>
        public static byte[] ReadAllBytes(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException("指定的文件不存在", path);
            
            try
            {
                return File.ReadAllBytes(path);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogError($"读取文件失败: {path}, 错误: {ex.Message}");
                throw new IOException($"读取文件失败: {path}", ex);
            }
        }

        /// <summary>
        /// 安全地写入字节数组到文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="bytes">要写入的字节数组</param>
        /// <param name="append">是否追加到文件末尾，而不是覆盖</param>
        /// <exception cref="ArgumentNullException">path或bytes为null时抛出</exception>
        /// <exception cref="IOException">写入文件时发生IO错误时抛出</exception>
        public static void WriteAllBytes(string path, byte[] bytes, bool append = false)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            
            try
            {
                // 确保目录存在
                string directory = GetDirectoryPath(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (append && File.Exists(path))
                {
                    using (var fileStream = new FileStream(path, FileMode.Append, FileAccess.Write))
                    {
                        fileStream.Write(bytes, 0, bytes.Length);
                    }
                }
                else
                {
                    File.WriteAllBytes(path, bytes);
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogError($"写入文件失败: {path}, 错误: {ex.Message}");
                throw new IOException($"写入文件失败: {path}", ex);
            }
        }

        /// <summary>
        /// 安全地复制文件
        /// </summary>
        /// <param name="sourcePath">源文件路径</param>
        /// <param name="destPath">目标文件路径</param>
        /// <param name="overwrite">是否覆盖已存在的文件</param>
        /// <returns>是否成功复制</returns>
        /// <exception cref="ArgumentNullException">sourcePath或destPath为null时抛出</exception>
        /// <exception cref="FileNotFoundException">源文件不存在时抛出</exception>
        /// <exception cref="IOException">复制文件时发生IO错误时抛出</exception>
        public static bool CopyFile(string sourcePath, string destPath, bool overwrite = true)
        {
            if (sourcePath == null) throw new ArgumentNullException(nameof(sourcePath));
            if (destPath == null) throw new ArgumentNullException(nameof(destPath));
            if (!File.Exists(sourcePath)) throw new FileNotFoundException("源文件不存在", sourcePath);
            
            try
            {
                // 确保目标目录存在
                string directory = GetDirectoryPath(destPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.Copy(sourcePath, destPath, overwrite);
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogError($"复制文件失败: {sourcePath} -> {destPath}, 错误: {ex.Message}");
                if (!overwrite && File.Exists(destPath))
                {
                    return false; // 目标文件已存在且不允许覆盖
                }
                throw new IOException($"复制文件失败: {sourcePath} -> {destPath}", ex);
            }
        }

        /// <summary>
        /// 安全地移动文件
        /// </summary>
        /// <param name="sourcePath">源文件路径</param>
        /// <param name="destPath">目标文件路径</param>
        /// <param name="overwrite">是否覆盖已存在的文件</param>
        /// <returns>是否成功移动</returns>
        /// <exception cref="ArgumentNullException">sourcePath或destPath为null时抛出</exception>
        /// <exception cref="FileNotFoundException">源文件不存在时抛出</exception>
        /// <exception cref="IOException">移动文件时发生IO错误时抛出</exception>
        public static bool MoveFile(string sourcePath, string destPath, bool overwrite = true)
        {
            if (sourcePath == null) throw new ArgumentNullException(nameof(sourcePath));
            if (destPath == null) throw new ArgumentNullException(nameof(destPath));
            if (!File.Exists(sourcePath)) throw new FileNotFoundException("源文件不存在", sourcePath);
            
            try
            {
                // 如果目标文件已存在且需要覆盖
                if (File.Exists(destPath))
                {
                    if (overwrite)
                    {
                        File.Delete(destPath);
                    }
                    else
                    {
                        return false;
                    }
                }

                // 确保目标目录存在
                string directory = GetDirectoryPath(destPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.Move(sourcePath, destPath);
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogError($"移动文件失败: {sourcePath} -> {destPath}, 错误: {ex.Message}");
                throw new IOException($"移动文件失败: {sourcePath} -> {destPath}", ex);
            }
        }

        /// <summary>
        /// 安全地删除文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>是否成功删除</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        public static bool DeleteFile(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    return true;
                }
                return false;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogError($"删除文件失败: {path}, 错误: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region 目录操作

        /// <summary>
        /// 安全地创建目录
        /// </summary>
        /// <param name="path">目录路径</param>
        /// <returns>是否成功创建，如果目录已存在则返回true</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        /// <exception cref="IOException">创建目录时发生IO错误时抛出</exception>
        public static bool CreateDirectory(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogError($"创建目录失败: {path}, 错误: {ex.Message}");
                throw new IOException($"创建目录失败: {path}", ex);
            }
        }

        /// <summary>
        /// 安全地复制目录及其所有内容
        /// </summary>
        /// <param name="sourcePath">源目录路径</param>
        /// <param name="destPath">目标目录路径</param>
        /// <param name="overwrite">是否覆盖已存在的文件</param>
        /// <returns>是否成功复制</returns>
        /// <exception cref="ArgumentNullException">sourcePath或destPath为null时抛出</exception>
        /// <exception cref="DirectoryNotFoundException">源目录不存在时抛出</exception>
        /// <exception cref="IOException">复制目录时发生IO错误时抛出</exception>
        public static bool CopyDirectory(string sourcePath, string destPath, bool overwrite = true)
        {
            if (sourcePath == null) throw new ArgumentNullException(nameof(sourcePath));
            if (destPath == null) throw new ArgumentNullException(nameof(destPath));
            if (!Directory.Exists(sourcePath)) throw new DirectoryNotFoundException("源目录不存在: " + sourcePath);
            
            try
            {
                // 创建目标目录
                if (!Directory.Exists(destPath))
                {
                    Directory.CreateDirectory(destPath);
                }

                // 复制所有文件
                foreach (string filePath in Directory.GetFiles(sourcePath))
                {
                    string fileName = Path.GetFileName(filePath);
                    string destFilePath = Path.Combine(destPath, fileName);
                    CopyFile(filePath, destFilePath, overwrite);
                }

                // 递归复制子目录
                foreach (string dirPath in Directory.GetDirectories(sourcePath))
                {
                    string dirName = Path.GetFileName(dirPath);
                    string destDirPath = Path.Combine(destPath, dirName);
                    CopyDirectory(dirPath, destDirPath, overwrite);
                }

                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogError($"复制目录失败: {sourcePath} -> {destPath}, 错误: {ex.Message}");
                throw new IOException($"复制目录失败: {sourcePath} -> {destPath}", ex);
            }
        }

        /// <summary>
        /// 安全地删除目录及其所有内容
        /// </summary>
        /// <param name="path">目录路径</param>
        /// <param name="recursive">是否递归删除子目录和文件</param>
        /// <returns>是否成功删除</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        public static bool DeleteDirectory(string path, bool recursive = true)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, recursive);
                    return true;
                }
                return false;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogError($"删除目录失败: {path}, 错误: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取目录中的所有文件（可选递归子目录）
        /// </summary>
        /// <param name="path">目录路径</param>
        /// <param name="searchPattern">搜索模式，例如 "*.txt"</param>
        /// <param name="recursive">是否递归搜索子目录</param>
        /// <returns>文件路径列表</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        /// <exception cref="DirectoryNotFoundException">目录不存在时抛出</exception>
        public static string[] GetFiles(string path, string searchPattern = "*", bool recursive = false)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException("目录不存在: " + path);
            
            try
            {
                return Directory.GetFiles(path, searchPattern, 
                    recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogError($"获取文件列表失败: {path}, 错误: {ex.Message}");
                throw new IOException($"获取文件列表失败: {path}", ex);
            }
        }

        /// <summary>
        /// 获取目录中的所有子目录（可选递归）
        /// </summary>
        /// <param name="path">目录路径</param>
        /// <param name="recursive">是否递归搜索子目录</param>
        /// <returns>子目录路径列表</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        /// <exception cref="DirectoryNotFoundException">目录不存在时抛出</exception>
        public static string[] GetDirectories(string path, bool recursive = false)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException("目录不存在: " + path);
            
            try
            {
                return Directory.GetDirectories(path, "*", 
                    recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogError($"获取子目录列表失败: {path}, 错误: {ex.Message}");
                throw new IOException($"获取子目录列表失败: {path}", ex);
            }
        }

        #endregion

        #region 文件信息

        /// <summary>
        /// 获取文件大小（字节）
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>文件大小（字节）</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        /// <exception cref="FileNotFoundException">文件不存在时抛出</exception>
        public static long GetFileSize(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException("文件不存在", path);
            
            try
            {
                var fileInfo = new FileInfo(path);
                return fileInfo.Length;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogError($"获取文件大小失败: {path}, 错误: {ex.Message}");
                throw new IOException($"获取文件大小失败: {path}", ex);
            }
        }

        /// <summary>
        /// 获取文件的最后修改时间
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>最后修改时间</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        /// <exception cref="FileNotFoundException">文件不存在时抛出</exception>
        public static DateTime GetLastModifiedTime(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException("文件不存在", path);
            
            try
            {
                return File.GetLastWriteTime(path);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogError($"获取文件修改时间失败: {path}, 错误: {ex.Message}");
                throw new IOException($"获取文件修改时间失败: {path}", ex);
            }
        }

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>文件是否存在</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        public static bool FileExists(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            return File.Exists(path);
        }

        /// <summary>
        /// 检查目录是否存在
        /// </summary>
        /// <param name="path">目录路径</param>
        /// <returns>目录是否存在</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        public static bool DirectoryExists(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            return Directory.Exists(path);
        }

        #endregion

        #region 文件类型检测

        /// <summary>
        /// 检查文件是否为图片文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>是否为图片文件</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        public static bool IsImageFile(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            
            string extension = GetExtension(path).ToLowerInvariant();
            string[] imageExtensions = { "jpg", "jpeg", "png", "gif", "bmp", "tiff", "tif", "webp" };
            
            return Array.IndexOf(imageExtensions, extension) >= 0;
        }

        /// <summary>
        /// 检查文件是否为文本文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>是否为文本文件</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        /// <exception cref="FileNotFoundException">文件不存在时抛出</exception>
        public static bool IsTextFile(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException("文件不存在", path);
            
            string extension = GetExtension(path).ToLowerInvariant();
            string[] textExtensions = { "txt", "csv", "json", "xml", "html", "htm", "css", "js", "cs", "c", "cpp", "h", "py", "md", "log" };
            
            if (Array.IndexOf(textExtensions, extension) >= 0)
            {
                return true;
            }
            
            // 如果扩展名不在列表中，尝试检查文件内容
            try
            {
                // 读取文件的前几千字节检查是否有二进制字符
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[4096]; // 4KB足够判断大部分情况
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    
                    // 检查是否有二进制字符
                    for (int i = 0; i < bytesRead; i++)
                    {
                        // 0是NULL，小于32且不是制表符、换行符等的都是控制字符
                        if (buffer[i] == 0 || (buffer[i] < 32 && buffer[i] != 9 && buffer[i] != 10 && buffer[i] != 13))
                        {
                            return false; // 发现二进制字符，不是文本文件
                        }
                    }
                    
                    return true; // 没有发现二进制字符，可能是文本文件
                }
            }
            catch
            {
                return false; // 读取失败，保守判断为非文本文件
            }
        }

        /// <summary>
        /// 检查文件是否为音频文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>是否为音频文件</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        public static bool IsAudioFile(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            
            string extension = GetExtension(path).ToLowerInvariant();
            string[] audioExtensions = { "mp3", "wav", "ogg", "flac", "aac", "m4a", "wma" };
            
            return Array.IndexOf(audioExtensions, extension) >= 0;
        }

        /// <summary>
        /// 检查文件是否为视频文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>是否为视频文件</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        public static bool IsVideoFile(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            
            string extension = GetExtension(path).ToLowerInvariant();
            string[] videoExtensions = { "mp4", "avi", "mov", "wmv", "flv", "mkv", "webm" };
            
            return Array.IndexOf(videoExtensions, extension) >= 0;
        }

        #endregion

        #region 文件哈希计算

        /// <summary>
        /// 计算文件的MD5哈希值
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>MD5哈希值的十六进制字符串</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        public static string CalculateMD5(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException("文件不存在", path);
            
            using (var md5 = MD5.Create())
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        /// <summary>
        /// 计算文件的SHA1哈希值
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>SHA1哈希值的十六进制字符串</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        /// <exception cref="FileNotFoundException">文件不存在时抛出</exception>
        public static string CalculateSHA1(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException("文件不存在", path);
            
            using (var sha1 = SHA1.Create())
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var hash = sha1.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        /// <summary>
        /// 计算文件的SHA256哈希值
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>SHA256哈希值的十六进制字符串</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        /// <exception cref="FileNotFoundException">文件不存在时抛出</exception>
        public static string CalculateSHA256(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException("文件不存在", path);
            
            using (var sha256 = SHA256.Create())
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var hash = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        #endregion

        #region 异步IO操作

        /// <summary>
        /// 异步读取文件的所有文本内容
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="encoding">字符编码，默认为UTF8</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>表示异步操作的任务，包含文件的文本内容</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        /// <exception cref="FileNotFoundException">文件不存在时抛出</exception>
        /// <exception cref="IOException">读取文件时发生IO错误时抛出</exception>
        public static async Task<string> ReadAllTextAsync(string path, Encoding encoding = null, CancellationToken cancellationToken = default)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException("指定的文件不存在", path);

            encoding = encoding ?? Encoding.UTF8;
            
            try
            {
                using (var reader = new StreamReader(path, encoding))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"读取文件操作被取消: {path}");
                throw;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogError($"异步读取文件失败: {path}, 错误: {ex.Message}");
                throw new IOException($"异步读取文件失败: {path}", ex);
            }
        }

        /// <summary>
        /// 异步读取文件的所有行
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="encoding">字符编码，默认为UTF8</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>表示异步操作的任务，包含文件的所有行</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        /// <exception cref="FileNotFoundException">文件不存在时抛出</exception>
        /// <exception cref="IOException">读取文件时发生IO错误时抛出</exception>
        public static async Task<string[]> ReadAllLinesAsync(string path, Encoding encoding = null, CancellationToken cancellationToken = default)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException("指定的文件不存在", path);

            encoding = encoding ?? Encoding.UTF8;
            
            try
            {
                var lines = new List<string>();
                using (var reader = new StreamReader(path, encoding))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        lines.Add(line);
                    }
                }
                return lines.ToArray();
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"读取文件操作被取消: {path}");
                throw;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogError($"异步读取文件失败: {path}, 错误: {ex.Message}");
                throw new IOException($"异步读取文件失败: {path}", ex);
            }
        }

        /// <summary>
        /// 异步写入文本到文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="content">要写入的内容</param>
        /// <param name="encoding">字符编码，默认为UTF8</param>
        /// <param name="append">是否追加到文件末尾，而不是覆盖</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>表示异步操作的任务</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        /// <exception cref="IOException">写入文件时发生IO错误时抛出</exception>
        public static async Task WriteAllTextAsync(string path, string content, Encoding encoding = null, bool append = false, CancellationToken cancellationToken = default)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            
            encoding = encoding ?? Encoding.UTF8;
            
            try
            {
                // 确保目录存在
                string directory = GetDirectoryPath(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var streamWriter = new StreamWriter(path, append, encoding))
                {
                    await streamWriter.WriteAsync(content);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"写入文件操作被取消: {path}");
                throw;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogError($"异步写入文件失败: {path}, 错误: {ex.Message}");
                throw new IOException($"异步写入文件失败: {path}", ex);
            }
        }

        /// <summary>
        /// 异步写入多行文本到文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="lines">要写入的行</param>
        /// <param name="encoding">字符编码，默认为UTF8</param>
        /// <param name="append">是否追加到文件末尾，而不是覆盖</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>表示异步操作的任务</returns>
        /// <exception cref="ArgumentNullException">path或lines为null时抛出</exception>
        /// <exception cref="IOException">写入文件时发生IO错误时抛出</exception>
        public static async Task WriteAllLinesAsync(string path, IEnumerable<string> lines, Encoding encoding = null, bool append = false, CancellationToken cancellationToken = default)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (lines == null) throw new ArgumentNullException(nameof(lines));
            
            encoding = encoding ?? Encoding.UTF8;
            
            try
            {
                // 确保目录存在
                string directory = GetDirectoryPath(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var streamWriter = new StreamWriter(path, append, encoding))
                {
                    foreach (var line in lines)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await streamWriter.WriteLineAsync(line);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"写入文件操作被取消: {path}");
                throw;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogError($"异步写入文件失败: {path}, 错误: {ex.Message}");
                throw new IOException($"异步写入文件失败: {path}", ex);
            }
        }

        /// <summary>
        /// 异步读取文件的所有字节
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>表示异步操作的任务，包含文件的字节数组</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        /// <exception cref="FileNotFoundException">文件不存在时抛出</exception>
        /// <exception cref="IOException">读取文件时发生IO错误时抛出</exception>
        public static async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException("指定的文件不存在", path);
            
            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var length = (int)stream.Length;
                    var buffer = new byte[length];
                    await stream.ReadAsync(buffer, 0, length, cancellationToken);
                    return buffer;
                }
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"读取文件操作被取消: {path}");
                throw;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogError($"异步读取文件失败: {path}, 错误: {ex.Message}");
                throw new IOException($"异步读取文件失败: {path}", ex);
            }
        }

        /// <summary>
        /// 异步写入字节数组到文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="bytes">要写入的字节数组</param>
        /// <param name="append">是否追加到文件末尾，而不是覆盖</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>表示异步操作的任务</returns>
        /// <exception cref="ArgumentNullException">path或bytes为null时抛出</exception>
        /// <exception cref="IOException">写入文件时发生IO错误时抛出</exception>
        public static async Task WriteAllBytesAsync(string path, byte[] bytes, bool append = false, CancellationToken cancellationToken = default)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            
            try
            {
                // 确保目录存在
                string directory = GetDirectoryPath(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var fileStream = new FileStream(
                    path, 
                    append ? FileMode.Append : FileMode.Create, 
                    FileAccess.Write))
                {
                    await fileStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"写入文件操作被取消: {path}");
                throw;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogError($"异步写入文件失败: {path}, 错误: {ex.Message}");
                throw new IOException($"异步写入文件失败: {path}", ex);
            }
        }

        /// <summary>
        /// 异步计算文件的MD5哈希值
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>表示异步操作的任务，包含MD5哈希值的十六进制字符串</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        /// <exception cref="FileNotFoundException">文件不存在时抛出</exception>
        public static async Task<string> CalculateMD5Async(string path, CancellationToken cancellationToken = default)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException("文件不存在", path);
            
            using (var md5 = MD5.Create())
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var hash = await Task.Run(() => md5.ComputeHash(stream), cancellationToken);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        #endregion

        #region 文件监控

        // 存储所有活动的文件监控器
        private static Dictionary<string, FileSystemWatcher> _activeWatchers = new Dictionary<string, FileSystemWatcher>();

        /// <summary>
        /// 开始监控文件或目录的变化
        /// </summary>
        /// <param name="path">要监控的文件或目录路径</param>
        /// <param name="includeSubdirectories">是否包含子目录（仅当path是目录时有效）</param>
        /// <param name="onChange">文件更改时的回调</param>
        /// <param name="onCreate">文件创建时的回调</param>
        /// <param name="onDelete">文件删除时的回调</param>
        /// <param name="onRename">文件重命名时的回调</param>
        /// <param name="filter">文件过滤器，如"*.txt"</param>
        /// <returns>监控器ID，可用于停止监控</returns>
        /// <exception cref="ArgumentNullException">path为null时抛出</exception>
        /// <exception cref="FileNotFoundException">如果path是文件且不存在时抛出</exception>
        /// <exception cref="DirectoryNotFoundException">如果path是目录且不存在时抛出</exception>
        public static string StartWatching(
            string path, 
            bool includeSubdirectories = false,
            Action<string, WatcherChangeTypes> onChange = null,
            Action<string> onCreate = null,
            Action<string> onDelete = null,
            Action<string, string> onRename = null,
            string filter = "*.*")
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            
            // 确保路径存在
            bool isDirectory = Directory.Exists(path);
            bool isFile = File.Exists(path);
            
            if (!isDirectory && !isFile)
            {
                throw new FileNotFoundException("指定的文件或目录不存在", path);
            }
            
            try
            {
                string directoryToWatch;
                string fileToWatch = null;
                
                if (isDirectory)
                {
                    directoryToWatch = path;
                }
                else // isFile
                {
                    directoryToWatch = GetDirectoryPath(path);
                    fileToWatch = Path.GetFileName(path);
                    filter = fileToWatch; // 只监控特定文件
                }
                
                var watcher = new FileSystemWatcher
                {
                    Path = directoryToWatch,
                    Filter = filter,
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size | NotifyFilters.CreationTime,
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = isDirectory && includeSubdirectories
                };
                
                // 生成唯一ID
                string watcherId = Guid.NewGuid().ToString();
                
                // 设置事件处理器
                if (onChange != null)
                {
                    watcher.Changed += (sender, e) => 
                    {
                        if (fileToWatch == null || e.Name == fileToWatch)
                        {
                            onChange(e.FullPath, e.ChangeType);
                        }
                    };
                }
                
                if (onCreate != null)
                {
                    watcher.Created += (sender, e) => 
                    {
                        if (fileToWatch == null || e.Name == fileToWatch)
                        {
                            onCreate(e.FullPath);
                        }
                    };
                }
                
                if (onDelete != null)
                {
                    watcher.Deleted += (sender, e) => 
                    {
                        if (fileToWatch == null || e.Name == fileToWatch)
                        {
                            onDelete(e.FullPath);
                        }
                    };
                }
                
                if (onRename != null)
                {
                    watcher.Renamed += (sender, e) => 
                    {
                        if (fileToWatch == null || e.OldName == fileToWatch || e.Name == fileToWatch)
                        {
                            onRename(e.OldFullPath, e.FullPath);
                        }
                    };
                }
                
                // 存储监控器
                _activeWatchers[watcherId] = watcher;
                
                return watcherId;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogError($"启动文件监控失败: {path}, 错误: {ex.Message}");
                throw new IOException($"启动文件监控失败: {path}", ex);
            }
        }

        /// <summary>
        /// 停止文件或目录监控
        /// </summary>
        /// <param name="watcherId">监控器ID，由StartWatching方法返回</param>
        /// <returns>是否成功停止监控</returns>
        public static bool StopWatching(string watcherId)
        {
            if (string.IsNullOrEmpty(watcherId)) return false;
            
            if (_activeWatchers.TryGetValue(watcherId, out var watcher))
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
                _activeWatchers.Remove(watcherId);
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// 停止所有文件和目录监控
        /// </summary>
        public static void StopAllWatching()
        {
            foreach (var watcher in _activeWatchers.Values)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
            
            _activeWatchers.Clear();
        }

        #endregion
    }
}
