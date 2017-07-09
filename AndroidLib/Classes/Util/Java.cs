/*
 * Java.cs - Developed by Dan Wager for AndroidLib.dll
 */

using System;
using System.IO;
using Microsoft.Win32;

namespace Headygains.Android.Classes.Util
{
    /// <summary>
    /// Contains information about the current machine's Java installation
    /// </summary>
    public static class Java
    {
        private static bool _isInstalled;
        private static string _installationPath;
        private static string _binPath;
        private static string _javaExecutable;
        private static string _javacExecutable;

        /// <summary>
        /// Gets a value indicating if Java is currently installed on the local machine
        /// </summary>
        public static bool IsInstalled => _isInstalled;

        /// <summary>
        /// Gets a value indicating the installation path of Java on the local machine
        /// </summary>
        public static string InstallationPath => _installationPath;

        /// <summary>
        /// Gets a value indicating the path to Java's bin directory on the local machine
        /// </summary>
        public static string BinPath => _binPath;

        /// <summary>
        /// Gets a value indicating the path to Java.exe on the local machine
        /// </summary>
        public static string JavaExe => _javaExecutable;

        /// <summary>
        /// Gets a value indicating the path to Javac.exe on the local machine
        /// </summary>
        public static string JavacExe => _javacExecutable;

        static Java()
        {
            Update();
        }

        /// <summary>
        /// Updates the information stored in the <see cref="Java"/> class
        /// </summary>
        /// <remarks>Generally called if Java installation might have changed</remarks>
        public static void Update()
        {
            _installationPath = GetJavaInstallationPath();
            _isInstalled = !string.IsNullOrEmpty(_installationPath);

            if (!_isInstalled) return;
            _binPath = Path.Combine(_installationPath, "bin");
            _javaExecutable = Path.Combine(_installationPath, "bin\\java.exe");
            _javacExecutable = Path.Combine(_installationPath, "bin\\javac.exe");
        }

        private static string GetJavaInstallationPath()
        {
            var environmentPath = Environment.GetEnvironmentVariable("JAVA_HOME");
            
            if (!string.IsNullOrEmpty(environmentPath))
                return environmentPath;

            var javaKey = "SOFTWARE\\JavaSoft\\Java Runtime Environment\\";

            try
            {
                using (var r = Registry.LocalMachine.OpenSubKey(javaKey))
                {
                    if (r != null)
                        using (var k = r.OpenSubKey(r.GetValue("CurrentVersion").ToString()))
                        {
                            if (k != null) environmentPath = k.GetValue("JavaHome").ToString();
                        }
                }
            }
            catch
            {
                environmentPath = null;
            }

            return environmentPath;
        }

        /// <summary>
        /// Runs the specified Jar file with the specified arguments
        /// </summary>
        /// <param name="pathToJar">Full path the Jar file on local machine</param>
        /// <param name="arguments">Arguments to pass to the Jar at runtime</param>
        /// <returns>True if successful run, false if Java is not installed or the Jar does not exist</returns>
        public static bool RunJar(string pathToJar, params string[] arguments)
        {
            if (!_isInstalled)
                return false;

            if (!File.Exists(pathToJar))
                return false;

            var args = "-jar " + pathToJar;

            for (var i = 0; i < arguments.Length; i++)
                args += " " + arguments[i];

            Command.RunProcessNoReturn(_javaExecutable, args, Command.DefaultTimeout);

            return true;
        }
    }
}