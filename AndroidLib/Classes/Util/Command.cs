/*
 * Command.cs - Developed by Dan Wager for AndroidLib.dll - 04/12/12
 */

using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Headygains.Android.Classes.Util
{
    /// <summary>
    /// Class Used To Run Process.
    /// </summary>
    public static class Command
    {
        /// <summary>
        /// The default timeout for commands. -1 implies infinite time
        /// </summary>
        public const int DefaultTimeout = -1;

        /// <summary>
        /// Event Handler For Process Output Events.
        /// </summary>
        public static event EventHandler<OutputEventArgs> OutputReceived;
        
        /// <summary>
        /// Runs a process, does not return any data or exit code from process.
        /// </summary>
        /// <param name="executable"></param>
        /// <param name="arguments"></param>
        /// <param name="waitForExit"></param>
        [Obsolete("Method is deprecated, please use RunProcessNoReturn(string, string, int) instead.")]
        public static void RunProcessNoReturn(string executable, string arguments, bool waitForExit = true)
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

        /// <summary>
        /// Runs a process, does not return any data or exit code from process.
        /// </summary>
        /// <param name="executable"></param>
        /// <param name="arguments"></param>
        /// <param name="timeout"></param>
        public static void RunProcessNoReturn(string executable, string arguments, int timeout)
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

        public static string RunProcessReturnOutput(string executable, string arguments, int timeout)
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

        public static string RunProcessReturnOutput(string executable, string arguments, bool forceRegular, int timeout)
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

        /// <summary>
        /// Runs a process and Ties The Error/Output streams to <see cref="ReceiveOutput"/> .
        /// </summary>
        /// <param name="executable">Executable to run.</param>
        /// <param name="arguments">Arguments for executable.</param>
        /// <param name="timeout">Timeout ms</param>
        /// <returns><see cref="Task"/></returns>
        public static Task RunProcessOutputStream(string executable, string arguments, int timeout)
        {
            return Task.Factory.StartNew(() =>
            {
                using (var p = new Process())
                {
                    p.StartInfo.FileName = executable;
                    p.StartInfo.Arguments = arguments;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.ErrorDataReceived += ReceiveOutput;
                    p.OutputDataReceived += ReceiveOutput;
                    p.Start();
                    p.BeginErrorReadLine();
                    p.BeginOutputReadLine();
                    p.WaitForExit(timeout);
                }
                
            });
        }

        /// <summary>
        /// Runs a process and Ties The Error/Output streams to <see cref="ReceiveOutput"/>.
        /// You Must Use <see cref="OutputReceived"/> Event Handler.
        /// </summary>
        /// <param name="executable">Executable to run.</param>
        /// <param name="arguments">Arguments for executable.</param>
        /// <param name="timeout">Timeout ms</param>
        /// <returns><see cref="Task"/> <c>int</c></returns>
        public static Task<int> RunProcessOutputStreamReturnResult(string executable, string arguments, int timeout)
        {
            return Task<int>.Factory.StartNew(() =>
            {
                using (var p = new Process())
                {
                    p.StartInfo.FileName = executable;
                    p.StartInfo.Arguments = arguments;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.ErrorDataReceived += ReceiveOutput;
                    p.OutputDataReceived += ReceiveOutput;
                    p.Start();
                    p.BeginErrorReadLine();
                    p.BeginOutputReadLine();
                    p.WaitForExit(timeout);
                    return p.ExitCode;
                }

            });
        }

        /// <summary>
        /// Handles Process Output Streams.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        internal static void ReceiveOutput(object sender, DataReceivedEventArgs eventArgs)
        {
            if (eventArgs == null) return;
            var args = new OutputEventArgs {OutputData = eventArgs.Data};
            OnProcessOutput(sender,args);
        }

        /// <summary>
        /// Calls OutputReceived Event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        internal static void OnProcessOutput(object sender, OutputEventArgs eventArgs)
        {
            OutputReceived?.Invoke(sender, eventArgs);
        }
  

        public static string HandleOutput(Process p, AutoResetEvent outputWaitHandle, AutoResetEvent errorWaitHandle, int timeout, bool forceRegular)
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

        public static int RunProcessReturnExitCode(string executable, string arguments, int timeout)
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
        public static void RunProcessWriteInput(string executable, string arguments, params string[] input)
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

        public static void RunProcessWriteInput(string executable, string arguments, int timeout, params string[] input)
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

        public static bool IsProcessRunning(string processName)
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
