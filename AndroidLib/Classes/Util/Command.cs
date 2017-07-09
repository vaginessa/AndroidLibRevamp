/*
 * Command.cs - Developed by Dan Wager for AndroidLib.dll - 04/12/12
 */

using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Headygains.Android.Classes.Util
{
    internal static class Command
    {
        /// <summary>
        /// The default timeout for commands. -1 implies infinite time
        /// </summary>
        public const int DefaultTimeout = -1;
        
        [Obsolete("Method is deprecated, please use RunProcessNoReturn(string, string, int) instead.")]
        internal static void RunProcessNoReturn(string executable, string arguments, bool waitForExit = true)
        {
            using (var p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = true;

                p.Start();

                if (waitForExit)
                    p.WaitForExit();
            }
        }

        internal static void RunProcessNoReturn(string executable, string arguments, int timeout)
        {
            using (var p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = true;

                p.Start();

                p.WaitForExit(timeout);
            }
        }

        internal static Task RunProcessNoReturnAsync(string executable, string arguments, int timeout)
        {
            return Task.Factory.StartNew(() =>
            {
                using (var p = new Process())
                {
                    p.StartInfo.FileName = executable;
                    p.StartInfo.Arguments = arguments;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.UseShellExecute = true;

                    p.Start();

                    p.WaitForExit(timeout);
                }
            });
        }

        internal static string RunProcessReturnOutput(string executable, string arguments, int timeout)
        {
            using (var p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;

                using (var outputWaitHandle = new AutoResetEvent(false))
                    using (var errorWaitHandle = new AutoResetEvent(false))
                        return HandleOutput(p, outputWaitHandle, errorWaitHandle, timeout, false);
            }
        }

        /// <summary>
        /// Adds <see cref="Message"/> Object to ListBox Control.
        /// <remarks>Uses <see cref="Dispatcher"/>To Allow "Access" From A Non-Controlling Thread.</remarks>
        /// </summary>
        /// <param name="outputControl"></param>
        /// <param name="outputContent"></param>
        internal static void OutputToConsole(ListBox outputControl, Message outputContent)
        {
            // Check To See If We Have Thread Access.
            // If not then use Dispatcher to add item.
            if (outputControl.CheckAccess())
            {
                outputControl.Items.Add(outputContent);
                outputControl.UpdateLayout();
                outputControl.ScrollIntoView(outputControl.Items[outputControl.Items.Count - 1]);
            }
            else
            {
                outputControl.Dispatcher.BeginInvoke(new Action(() =>
                {
                    outputControl.Items.Add(outputContent);
                    outputControl.UpdateLayout();
                    outputControl.ScrollIntoView(outputControl.Items[outputControl.Items.Count - 1]);
                }));
            }
        }

        /// <summary>
        /// Creates And Starts A Process And Outputs ErrorStream and OutputStream Data to 
        /// <see cref="ListBox"/><paramref name="outputControl"/>.
        /// </summary>
        /// <param name="executable"></param>
        /// <param name="arguments"></param>
        /// <param name="timeout"></param>
        /// <param name="outputControl"><see cref="ListBox"/>ListBox Control Being used as an Output Console.</param>
        /// <returns><see cref="Process.ExitCode"/></returns>
        internal static int RunProcessOutputToConsole(string executable, string arguments, int timeout, ListBox outputControl)
        {
            const string sourceTag = "PROC";
            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = executable,
                Arguments = arguments,
                RedirectStandardError = true,
                RedirectStandardOutput = true    
            };
            using (var proc = new Process())
            {
                proc.StartInfo = startInfo;
                proc.ErrorDataReceived += (sendingProcess, output) =>
                {
                    if (string.IsNullOrEmpty(output.Data)) return;
                    var outputMessage = new Message(1,sourceTag,output.Data);
                    OutputToConsole(outputControl,outputMessage);
                };
                proc.OutputDataReceived += (sendingProcess, output) =>
                {
                    if (string.IsNullOrEmpty(output.Data)) return;
                    var outputMessage = new Message(0, sourceTag, output.Data);
                    OutputToConsole(outputControl, outputMessage);
                };
                proc.Start();
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();
                proc.WaitForExit(timeout);
                return proc.ExitCode;
            }
        }

        /// <summary>
        /// Creates And Starts A Process And Outputs ErrorStream and OutputStream Data to 
        /// <see cref="ListBox"/><paramref name="outputControl"/>.
        /// </summary>
        /// <param name="executable"></param>
        /// <param name="arguments"></param>
        /// <param name="timeout"></param>
        /// <param name="outputControl"><see cref="ListBox"/>ListBox Control Being used as an Output Console.</param>
        /// <returns><see cref="Process.ExitCode"/></returns>
        internal static Task<int> RunProcessOutputToConsoleAsync(string executable, string arguments, int timeout, ListBox outputControl)
        {
            const string sourceTag = "PROC";
            return Task<int>.Factory.StartNew(() =>
            {
                var startInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = executable,
                    Arguments = arguments,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };
                using (var proc = new Process())
                {
                    proc.StartInfo = startInfo;
                    proc.ErrorDataReceived += (sendingProcess, output) =>
                    {
                        if (string.IsNullOrEmpty(output.Data)) return;
                        var outputMessage = new Message(1, sourceTag, output.Data);
                        OutputToConsole(outputControl, outputMessage);
                    };
                    proc.OutputDataReceived += (sendingProcess, output) =>
                    {
                        if (string.IsNullOrEmpty(output.Data)) return;
                        var outputMessage = new Message(0, sourceTag, output.Data);
                        OutputToConsole(outputControl, outputMessage);
                    };
                    proc.Start();
                    proc.BeginErrorReadLine();
                    proc.BeginOutputReadLine();
                    proc.WaitForExit(timeout);
                    return proc.ExitCode;
                }
            });
            
        }

        internal static string RunProcessReturnOutput(string executable, string arguments, bool forceRegular, int timeout)
        {
            using (var p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;

                using (var outputWaitHandle = new AutoResetEvent(false))
                    using (var errorWaitHandle = new AutoResetEvent(false))
                        return HandleOutput(p, outputWaitHandle, errorWaitHandle, timeout, forceRegular);
            }
        }

        private static string HandleOutput(Process p, AutoResetEvent outputWaitHandle, AutoResetEvent errorWaitHandle, int timeout, bool forceRegular)
        {
            var output = new StringBuilder();
            var error = new StringBuilder();

            p.OutputDataReceived += (sender, e) =>
            {
                if (e.Data == null)
                    outputWaitHandle.Set();
                else
                    output.AppendLine(e.Data);
            };
            p.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data == null)
                    errorWaitHandle.Set();
                else
                    error.AppendLine(e.Data);
            };

            p.Start();

            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            if (p.WaitForExit(timeout) && outputWaitHandle.WaitOne(timeout) && errorWaitHandle.WaitOne(timeout))
            {
                var strReturn = "";

                if (error.ToString().Trim().Length.Equals(0) || forceRegular)
                    strReturn = output.ToString().Trim();
                else
                    strReturn = error.ToString().Trim();

                return strReturn;
            }
            else
            {
                // Timed out.
                return "PROCESS TIMEOUT";
            }
        }

        internal static int RunProcessReturnExitCode(string executable, string arguments, int timeout)
        {
            int exitCode;

            using (var p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = true;

                p.Start();
                p.WaitForExit(timeout);
                exitCode = p.ExitCode;
            }

            return exitCode;
        }

        [Obsolete("Method is deprecated, please use RunProcessWriteInput(string, string, int, string...) instead.")]
        internal static void RunProcessWriteInput(string executable, string arguments, params string[] input)
        {
            using (var p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;

                p.StartInfo.RedirectStandardInput = true;

                p.Start();

                using (var w = p.StandardInput)
                    for (var i = 0; i < input.Length; i++)
                        w.WriteLine(input[i]);

                p.WaitForExit();
            }
        }

        internal static void RunProcessWriteInput(string executable, string arguments, int timeout, params string[] input)
        {
            using (var p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;

                p.StartInfo.RedirectStandardInput = true;

                p.Start();

                using (var w = p.StandardInput)
                    for (var i = 0; i < input.Length; i++)
                        w.WriteLine(input[i]);

                p.WaitForExit(timeout);
            }
        }

        internal static bool IsProcessRunning(string processName)
        {
            var processes = Process.GetProcesses();

            foreach (var p in processes)
                if (p.ProcessName.ToLower().Contains(processName.ToLower()))
                    return true;

            return false;
        }

        internal static void KillProcess(string processName)
        {
            var processes = Process.GetProcesses();

            foreach (var p in processes)
            {
                if (!p.ProcessName.ToLower().Contains(processName.ToLower())) continue;
                p.Kill();
                return;
            }
        }


    }
}
