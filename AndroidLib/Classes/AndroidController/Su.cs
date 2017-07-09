/*
 * Su.cs - Developed by Dan Wager for AndroidLib.dll
 */

using System.IO;

namespace Headygains.Android.Classes.AndroidController
{
    /// <summary>
    /// Contains information about the Su binary on the Android device
    /// </summary>
    public class Su
    {
        private Device _device;

        private string _version;
        private bool _exists;

        internal Su(Device device)
        {
            this._device = device;
            GetSuData();
        }

        internal bool Exists => this._exists;

        /// <summary>
        /// Gets a value indicating the version of Su on the Android device
        /// </summary>
        public string Version => this._version;

        private void GetSuData()
        {
            if (this._device.State != DeviceState.Online)
            {
                this._version = null;
                this._exists = false;
                return;     
            }
            
            var adbCmd = Adb.FormAdbShellCommand(this._device, false, "su", "-v");
            using (var r = new StringReader(Adb.ExecuteAdbCommand(adbCmd)))
            {
                var line = r.ReadLine();

                if (line.Contains("not found") || line.Contains("permission denied"))
                {
                    this._version = "-1";
                    this._exists = false;
                }
                else
                {
                    this._version = line;
                    this._exists = true;
                }
            }
        }
    }
}