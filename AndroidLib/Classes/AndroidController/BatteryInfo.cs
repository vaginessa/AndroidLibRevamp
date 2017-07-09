/*
 * Battery.cs - Developed by Dan Wager for AndroidLib.dll
 */

using System.IO;

namespace Headygains.Android.Classes.AndroidController
{
    /// <summary>
    /// Contains information about connected Android device's battery
    /// </summary>
    public class BatteryInfo
    {
        private Device _device;
        private string _dump;

        private bool _acPower;
        private bool _usbPower;
        private bool _wirelessPower;
        private int _status;
        private int _health;
        private bool _present;
        private int _level;
        private int _scale;
        private int _voltage;
        private int _temperature;
        private string _technology;

        private string _outString;

        /// <summary>
        /// Gets a value indicating if the connected Android device is on AC Power
        /// </summary>
        public bool AcPower
        {
            get { Update(); return this._acPower; }
        }

        /// <summary>
        /// Gets a value indicating if the connected Android device is on USB Power
        /// </summary>
        public bool UsbPower
        {
            get { Update(); return _usbPower; }
        }
        
        /// <summary>
        /// Gets a value indicating if the connected Android device is on Wireless Power
        /// </summary>
        public bool WirelessPower
        {
            get { Update(); return _wirelessPower; }
        }
        
        /// <summary>
        /// Gets a value indicating the status of the battery
        /// </summary>
        public string Status
        {
            /* As defined in: http://developer.android.com/reference/android/os/BatteryManager.html
             * Property "Status" is changed from type "int" to type "string" to give a string representation
             * of the value obtained from dumpsys regarding battery status.
             * Submitted By: Omar Bizreh [DeepUnknown from Xda-Developers.com]
             */
            get
            {
                Update();
                switch (_status)
                {
                    case 1:
                        return "Unknown Battery Status: " + _status;
                    case 2:
                        return "Charging";
                    case 3:
                        return "Discharging";
                    case 4:
                        return "Not charging";
                    case 5:
                        return "Full";
                    default:
                        return "Unknown Value: " + _status;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating the health of the battery
        /// </summary>
        public string Health
        {
        
            /* As defined in: http://developer.android.com/reference/android/os/BatteryManager.html
             * Property "Health" is changed from type "int" to type "string" to give a string representation
             * of the value obtained from dumpsys regarding battery health.
             * Submitted By: Omar Bizreh [DeepUnknown from Xda-Developers.com]
             */
            get
            {
                Update();
                switch (_health)
                {
                    case 1:
                        return "Unknown Health State: " + _health;
                    case 2:
                        return "Good";
                    case 3:
                        return "Over Heat";
                    case 4:
                        return "Dead";
                    case 5:
                        return "Over Voltage";
                    case 6:
                        return "Unknown Failure";
                    case 7:
                        return "Cold Battery";
                    default:
                        return "Unknown Value: " + _health;
                }

            }
        }

        /// <summary>
        /// Gets a value indicating if there is a battery present
        /// </summary>
        public bool Present
        {
            get { Update(); return _present; }
        }

        /// <summary>
        /// Gets a value indicating the current charge level of the battery
        /// </summary>
        public int Level
        {
            get { Update(); return _level; }
        }

        /// <summary>
        /// Gets a value indicating the scale of the battery
        /// </summary>
        public int Scale
        {
            get { Update(); return _scale; }
        }
        
        /// <summary>
        /// Gets a value indicating the current voltage of the battery
        /// </summary>
        public int Voltage
        {
            get { Update(); return _voltage; }
        }
        
        /// <summary>
        /// Gets a value indicating the current temperature of the battery
        /// </summary>
        public int Temperature
        {
            get { Update(); return _temperature; }
        }
        
        /// <summary>
        /// Gets a value indicating the battery's technology
        /// </summary>
        public string Technology
        {
            get { Update(); return _technology; }
        }

        /// <summary>
        /// Initializes a new instance of the BatteryInfo class
        /// </summary>
        /// <param name="device">Serial number of Android device</param>
        internal BatteryInfo(Device device)
        {
            this._device = device;
            Update();
        }

        private void Update()
        {
            if (this._device.State != DeviceState.Online)
            {
                this._acPower = false;
                this._dump = null;
                this._health = -1;
                this._level = -1;
                this._present = false;
                this._scale = -1;
                this._status = -1;
                this._technology = null;
                this._temperature = -1;
                this._usbPower = false;
                this._voltage = -1;
                this._wirelessPower = false;
                this._outString = "Device Not Online";
                return;
            }

            var adbCmd = Adb.FormAdbShellCommand(this._device, false, "dumpsys", "battery");
            this._dump = Adb.ExecuteAdbCommand(adbCmd);

            using (var r = new StringReader(this._dump))
            {
                string line;

                while (true)
                {
                    line = r.ReadLine();

                    if (!line.Contains("Current Battery Service state"))
                    {
                        continue;
                    }
                    else
                    {
                        this._dump = line + r.ReadToEnd();
                        break;
                    }
                }
            }

            using (var r = new StringReader(this._dump))
            {
                var line = "";

                while (r.Peek() != -1)
                {
                    line = r.ReadLine();

                    if (line == "")
                        continue;
                    else if (line.Contains("AC "))
                        bool.TryParse(line.Substring(14), out this._acPower);
                    else if (line.Contains("USB"))
                        bool.TryParse(line.Substring(15), out this._usbPower);
                    else if (line.Contains("Wireless"))
                        bool.TryParse(line.Substring(20), out this._wirelessPower);
                    else if (line.Contains("status"))
                        int.TryParse(line.Substring(10), out this._status);
                    else if (line.Contains("health"))
                        int.TryParse(line.Substring(10), out this._health);
                    else if (line.Contains("present"))
                        bool.TryParse(line.Substring(11), out this._present);
                    else if (line.Contains("level"))
                        int.TryParse(line.Substring(9), out this._level);
                    else if (line.Contains("scale"))
                        int.TryParse(line.Substring(9), out this._scale);
                    else if (line.Contains("voltage"))
                        int.TryParse(line.Substring(10), out this._voltage);
                    else if (line.Contains("temp"))
                        int.TryParse(line.Substring(15), out this._temperature);
                    else if (line.Contains("tech"))
                        this._technology = line.Substring(14);
                }
            }

            this._outString = this._dump.Replace("Service state", "State For Device " + this._device.SerialNumber);
        }

        /// <summary>
        /// Returns a formatted string object containing all battery stats
        /// </summary>
        /// <returns>A formatted string containing all battery stats</returns>
        public override string ToString()
        {
            Update();
            return this._outString;
        }
    }
}
