/*
 * Command.cs - Developed by Dan Wager for AndroidLib.dll - 04/12/12
 * Updates: Headygains 07/2017
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
    /// <remarks>
    /// To use <see cref="RunProcessOutputStream"/> methods you must set an <see cref="OutputReceived"/>
    /// event handler method.
    /// 
    /// Assignment
    /// Ex: <c>Command.OutputReceived += OnExampleOutputReceived;</c>
    /// 
    /// OnExampleOutputReceived Method
    /// Ex: string OutputStreamLine; // The variable that will store the output line of a process.
    /// Ex: void OnExampleOutputReceived(object sender, OutputEventArgs eventArgs){ OutputStreamLine = eventArgs.OutputData; }
    /// Ex: string OnExampleOutputReceived(object sender, OutputEventArgs eventArgs){ return eventArgs.OutputData; }
    /// 
    /// </remarks>
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

        public static event EventHandler<AssignEventArgs> AssignmentReceived;
        
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

        /// <summary>
        /// Returns Process output after process execution finishes.
        /// </summary>
        /// <param name="executable"></param>
        /// <param name="arguments"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns Process output after process execution finishes.
        /// </summary>
        /// <param name="executable"></param>
        /// <param name="arguments"></param>
        /// <param name="forceRegular"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
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

        public static Task RunProcessOutputStream(string executable, string arguments, string callingDeviceSerialNumber,
            int timeout)
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
                    AssignProcess(p,callingDeviceSerialNumber);
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
            var senderPid = ((Process) sender).Id;
            var args = new OutputEventArgs {OutputData = eventArgs.Data, ProcessId = senderPid};
            OnProcessOutput(sender,args);
        }

        internal static void AssignProcess(object sender, string deviceSerialNumber)
        {
            var args = new AssignEventArgs
            {
                AssignedProcessId = ((Process) sender).Id,
                DeviceAssignedTo = deviceSerialNumber
            };
            AssignmentReceived?.Invoke(sender,args);
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

        /// <summary>
        /// Returns the full process output as a string, after the process completes.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="outputWaitHandle"></param>
        /// <param name="errorWaitHandle"></param>
        /// <param name="timeout"></param>
        /// <param name="forceRegular"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Runs a process and returns the <see cref="Process.ExitCode"/>
        /// </summary>
        /// <param name="executable"></param>
        /// <param name="arguments"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Run a process write input stream to process. I haven't tested this yet - headygains.
        /// Could be used in the future to add a user interactable console in an application.
        /// </summary>
        /// <param name="executable"></param>
        /// <param name="arguments"></param>
        /// <param name="input"></param>
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

        /// <summary>
        /// Run a process write input stream to process. I haven't tested this yet - headygains.
        /// Could be used in the future to add a user interactable console in an application.
        /// </summary>
        /// <param name="executable"></param>
        /// <param name="arguments"></param>
        /// <param name="timeout"></param>
        /// <param name="input"></param>
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

        /// <summary>
        /// Returns if a process with <paramref name="processName"/> is running
        /// </summary>
        /// <param name="processName"> Name of process</param>
        /// <returns></returns>
        public static bool IsProcessRunning(string processName)
        {
            var processes = Process.GetProcesses();

            foreach (var p in processes)
                if (p.ProcessName.ToLower().Contains(processName.ToLower()))
                    return true;

            return false;
        }

        /// <summary>
        /// Kills a process by name.
        /// </summary>
        /// <param name="processName"> Name of process to kill</param>
        public static void KillProcess(string processName)
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
