using System.IO;
using System.Reflection;
using Microsoft.VisualBasic.FileIO;

namespace Core.Utils
{
    public static class FSUtils
    {
        public static string JoinPaths(params string[] paths)
        {
            return Path.Combine(paths);
        }

        public static string GetAbsolutePath(params string[] paths)
        {
            string execDir = new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName;

            return Path.GetFullPath(JoinPaths(execDir, JoinPaths(paths)));
        }

        public static void CopyDirectory(string fromDirectory, string toDirectory)
        {
            FileSystem.CopyDirectory(fromDirectory, toDirectory);
        }

        public static void EnsureDirectory(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                return;
            }

            Directory.CreateDirectory(dirPath);
        }

        public static void EnsureFileDirectory(string filePath)
        {
            string dirPath = Path.GetDirectoryName(filePath);

            EnsureDirectory(dirPath);
        }
    }
}
