﻿using System;
using System.IO;

namespace Headygains.Android.Classes.Util
{
    internal static class Logger
    {
        private static string _errorLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Path.Combine("AndroidLib", "ErrorLog.txt"));

        internal static bool WriteLog(string message, string title, string stackTrace)
        {
            try
            {
                using (var fs = new FileStream(_errorLogPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    using (var sw = new StreamWriter(fs))
                        sw.WriteLine(String.Join(" ", new string[] { title, message, stackTrace }));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
