﻿using System.IO;
using System.Reflection;

namespace TVGamingService.Source.Utils
{
    internal static class FSUtils
    {
        public static string GetAbsolutePath(string relativePath)
        {
            string execDir = new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName;
            return Path.GetFullPath($"{execDir}/{relativePath}");
        }
    }
}