/*
 * BusyBox.cs - Developed by Dan Wager for AndroidLib.dll
 */

using System.Collections.Generic;
using System.IO;

namespace RegawMOD.Android
{
    /// <summary>
    /// Conatins information about device's busybox
    /// </summary>
    public class BusyBox
    {
        internal const string Executable = "busybox";

        private Device _device;

        private bool _isInstalled;
        private string _version;
        private List<string> _commands;

        /// <summary>
        /// Gets a value indicating if busybox is installed on the current device
        /// </summary>
        public bool IsInstalled => this._isInstalled;

        /// <summary>
        /// Gets a value indicating the version of busybox installed
        /// </summary>
        public string Version => this._version;

        /// <summary>
        /// Gets a <c>List&lt;string&gt;</c> containing busybox's commands
        /// </summary>
        public List<string> Commands => this._commands;

        internal BusyBox(Device device)
        {
            this._device = device;

            this._commands = new List<string>();

            Update();
        }

        /// <summary>
        /// Updates the instance of busybox
        /// </summary>
        /// <remarks>Generally called only if busybox may have changed on the device</remarks>
        public void Update()
        {
            this._commands.Clear();

            if (!this._device.HasRoot || this._device.State != DeviceState.Online)
            {
                SetNoBusybox();
                return;
            }

            var adbCmd = Adb.FormAdbShellCommand(this._device, false, Executable);
            using (var s = new StringReader(Adb.ExecuteAdbCommand(adbCmd)))
            {
                var check = s.ReadLine();

                if (check.Contains(string.Format("{0}: not found", Executable)))
                {
                    SetNoBusybox();
                    return;
                }

                this._isInstalled = true;

                this._version = check.Split(' ')[1].Substring(1);

                while (s.Peek() != -1 && s.ReadLine() != "Currently defined functions:") { }

                var cmds = s.ReadToEnd().Replace(" ", "").Replace("\r\r\n\t", "").Trim('\t', '\r', '\n').Split(',');

                if (cmds.Length.Equals(0))
                {
                    SetNoBusybox();
                }
                else
                {
                    foreach (var cmd in cmds)
                        this._commands.Add(cmd);
                }
            }
        }

        private void SetNoBusybox()
        {
            this._isInstalled = false;
            this._version = null;
        }
    }
}