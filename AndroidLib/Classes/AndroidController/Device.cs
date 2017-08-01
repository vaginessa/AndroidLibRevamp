/*
 * Device.cs - Developed by Dan Wager for AndroidLib.dll
 * Updated: Headygains 07/17
 */

using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
using Headygains.Android.Classes.Util;

namespace Headygains.Android.Classes.AndroidController
{
    /// <summary>
    /// Manages connected Android device's info and commands
    /// </summary>
    public partial class Device
    {
        private BatteryInfo _battery;
        private BuildProp _buildProp;
        private BusyBox _busyBox;
        private FileSystem _fileSystem;
        //private PackageManager packageManager;
        private Phone _phone;
        //private Processes processes;
        private Su _su;
        private string _serialNumber;
        private DeviceState _state;
        private bool _hasProcess;
        private int _attachedProcessId = 0;

        /// <summary>
        /// Initializes a new instance of the Device class
        /// </summary>
        /// <param name="deviceSerial">Serial number of Android device</param>
        internal Device(string deviceSerial)
        {
            _serialNumber = deviceSerial;
            Update();
        }

        /// <summary>
        /// Initializes a new instance of the Device class
        /// </summary>
        /// <param name="deviceSerial">Serial number of Android device</param>
        /// <param name="mode">Currently Does Nothing (Default = false)</param>
        public Device(string deviceSerial, bool mode = false)
        {
            _serialNumber = deviceSerial;
            Update();
        }

        private DeviceState SetState()
        {
            string state = null;

            using (var r = new StringReader(Adb.Devices()))
            {
                string line;

                while (r.Peek() != -1)
                {
                    line = r.ReadLine();

                    if (line != null && line.Contains(_serialNumber))
                        state = line.Substring(line.IndexOf('\t') + 1);
                }
            }

            if (state == null)
            {
                using (var r = new StringReader(Fastboot.Devices()))
                {
                    string line;

                    while (r.Peek() != -1)
                    {
                        line = r.ReadLine();

                        if (line != null && line.Contains(_serialNumber))
                            state = line.Substring(line.IndexOf('\t') + 1);
                    }
                }
            }

            switch (state)
            {
                case "device":
                    return DeviceState.Online;
                case "recovery":
                    return DeviceState.Recovery;
                case "fastboot":
                    return DeviceState.Fastboot;
                case "sideload":
                    return DeviceState.Sideload;
                case "unauthorized":
                    return DeviceState.Unauthorized;
                default:
                    return DeviceState.Unknown;
            }
        }

        /// <summary>
        /// Gets the device's <see cref="BatteryInfo"/> instance
        /// </summary>
        /// <remarks>See <see cref="BatteryInfo"/> for more details</remarks>
        public BatteryInfo Battery => _battery;

        /// <summary>
        /// Gets the device's <see cref="BuildProp"/> instance
        /// </summary>
        /// <remarks>See <see cref="BuildProp"/> for more details</remarks>
        public BuildProp BuildProp => _buildProp;

        /// <summary>
        /// Gets the device's <see cref="BusyBox"/> instance
        /// </summary>
        /// <remarks>See <see cref="BusyBox"/> for more details</remarks>
        public BusyBox BusyBox => _busyBox;

        /// <summary>
        /// Gets the device's <see cref="FileSystem"/> instance
        /// </summary>
        /// <remarks>See <see cref="FileSystem"/> for more details</remarks>
        public FileSystem FileSystem => _fileSystem;

        ///// <summary>
        ///// Gets the device's <see cref="PackageManager"/> instance
        ///// </summary>
        ///// <remarks>See <see cref="PackageManager"/> for more details</remarks>
        //public PackageManager PackageManager { get { return this.packageManager; } }

        /// <summary>
        /// Gets the device's <see cref="Phone"/> instance
        /// </summary>
        /// <remarks>See <see cref="Phone"/> for more details</remarks>
        public Phone Phone => _phone;

        ///// <summary>
        ///// Gets the device's <see cref="Processes"/> instance
        ///// </summary>
        ///// <remarks>See <see cref="Processes"/> for more details</remarks>
        //public Processes Processes { get { return this.processes; } }

        /// <summary>
        /// Gets the device's <see cref="Su"/> instance
        /// </summary>
        /// <remarks>See <see cref="Su"/> for more details</remarks>
        public Su Su => _su;

        /// <summary>
        /// Gets the device's serial number
        /// </summary>
        public string SerialNumber => _serialNumber;

        /// <summary>
        /// Gets a value indicating the device's current state
        /// </summary>
        /// <remarks>See <see cref="DeviceState"/> for more details</remarks>
        public DeviceState State { get => _state;
            internal set => _state = value;
        }

        /// <summary>
        /// Gets a value indicating if the device has root
        /// </summary>
        public bool HasRoot => _su.Exists;

        /// <summary>
        /// Gets / Sets A Process ID That's associated with this device.
        /// </summary>
        public int AttachedProcessId
        {
            get => _attachedProcessId;
            set => _attachedProcessId = value;
        }

        /// <summary>
        /// Performs a fastboot flash <paramref name="flashTarget"/> <paramref name="filePath"/>
        /// </summary>
        /// <param name="flashTarget"></param>
        /// <param name="filePath"></param>
        public async Task<string> FastbootFlashAsync(string flashTarget, string filePath)
        {
            if (State != DeviceState.Fastboot) return "Device Not In Fastboot Mode!";
            var result = await FastbootFlashTask(flashTarget, filePath);
            return result;
        }

        private Task<string> FastbootFlashTask(string flashTarget, string filePath)
        {
            return Task<string>.Factory.StartNew(
                () => Fastboot.ExecuteFastbootCommand(Fastboot.FormFastbootCommand(this, "flash", flashTarget, filePath)));
        }

        public async Task<string> FastbootOemUnlockAsync(string oemPassword)
        {
            if (State != DeviceState.Fastboot) return "Device Not In Fastboot Mode!";
            var result = await FastbootOemUnlockTask(oemPassword);
            return result;
        }

        private Task<string> FastbootOemUnlockTask(string oemPassword)
        {
            return Task<string>.Factory.StartNew(
                () => Fastboot.ExecuteFastbootCommand(Fastboot.FormFastbootCommand(this, "oem unlock", oemPassword)));
        }

        public async Task<string> FastbootOemFormatAsync()
        {
            if (State != DeviceState.Fastboot) return "Device Not In Fastboot Mode!";
            var result = await FastbootOemFormatTask();
            return result;
        }

        private Task<string> FastbootOemFormatTask()
        {
            return Task<string>.Factory.StartNew(
                () => Fastboot.ExecuteFastbootCommand(Fastboot.FormFastbootCommand(this, "oem format")));
        }

        /// <summary>
        /// Reboots the device regularly from fastboot
        /// </summary>
        public async void FastbootReboot()
        {
            if (State == DeviceState.Fastboot)
                await FastbootRebootTask();
        }

        private Task FastbootRebootTask()
        {
            return Task.Factory.StartNew(() =>
            {
                Fastboot.ExecuteFastbootCommandNoReturn(Fastboot.FormFastbootCommand(this, "reboot"));
            });
  
        }

        /// <summary>
        /// Reboots the device regularly
        /// </summary>
        public async void Reboot()
        {
            if (State.Equals(DeviceState.Online))
                await RebootTask();
        }

        private Task RebootTask()
        {
            return Task.Factory.StartNew(() =>
            {
                Adb.ExecuteAdbCommandNoReturn(Adb.FormAdbCommand(this, "reboot"));
            });    
        }

        /// <summary>
        /// Reboots the device into recovery
        /// </summary>
        public async void RebootRecovery()
        {
            if (State.Equals(DeviceState.Online))
                await RebootRecoveryTask();
        }

        private Task RebootRecoveryTask()
        {
            return Task.Factory.StartNew(() =>
            {
                Adb.ExecuteAdbCommandNoReturn(Adb.FormAdbCommand(this, "reboot", "recovery"));
            });
        }

        /// <summary>
        /// Reboots the device into the bootloader
        /// </summary>
        public async Task RebootBootloader()
        {
            if (State.Equals(DeviceState.Online))
                await RebootBootloaderTask();
        }

        private Task RebootBootloaderTask()
        {
            return Task.Factory.StartNew(() =>
            {
                Adb.ExecuteAdbCommandNoReturn(Adb.FormAdbCommand(this, "reboot-bootloader"));
            }); 
        }

        /// <summary>
        /// Pulls a file from the device
        /// </summary>
        /// <param name="fileOnDevice">Path to file to pull from device</param>
        /// <param name="destinationDirectory">Directory on local computer to pull file to</param>
        /// /// <param name="timeout">The timeout for this operation in milliseconds (Default = -1)</param>
        /// <returns>True if file is pulled, false if pull failed</returns>
        public bool PullFile(string fileOnDevice, string destinationDirectory, int timeout = Command.DefaultTimeout)
        {
            var adbCmd = Adb.FormAdbCommand(this, "pull", "\"" + fileOnDevice + "\"", "\"" + destinationDirectory + "\"");
            return (Adb.ExecuteAdbCommandReturnExitCode(adbCmd.WithTimeout(timeout)) == 0);
        }

        /// <summary>
        /// Pulls a file from the device, and outputs the OutputStream and Error Stream of the process 
        /// using an <see cref="Command.OutputReceived"/> Event.
        /// </summary>
        /// <param name="fileOnDevice">Path to the file on the device</param>
        /// <param name="destinationDirectory">Path to the destination on local machine</param>
        /// <param name="timeout"></param>
        public void PullFileWithStream(string fileOnDevice, string destinationDirectory, int timeout = Command.DefaultTimeout)
        {
            var adbCmd = Adb.FormAdbCommand(this, "pull", "\"" + fileOnDevice + "\"", "\"" + destinationDirectory + "\"");
            Adb.ExecuteAdbCommandWithOutstream(adbCmd);
        }

        /// <summary>
        /// Pulls a file from the device asynchronously
        /// </summary>
        /// <param name="fileOnDevice">Path to file to pull from device</param>
        /// <param name="destinationDirectory">Directory on local computer to pull file to</param>
        /// /// <param name="timeout">The timeout for this operation in milliseconds (Default = -1)</param>
        /// <returns>True if file is pulled, false if pull failed</returns>
        public async Task<bool> PullFileAsync(string fileOnDevice, string destinationDirectory,
            int timeout = Command.DefaultTimeout)
        {
            if (!State.Equals(DeviceState.Online)) return false;
            return await PullFileTask(fileOnDevice, destinationDirectory, timeout);
        }

        /// <summary>
        /// Task That Handles PullFile Logic For <see cref="PullFileAsync"/>
        /// </summary>
        /// <param name="fileOnDevice">Path to file to pull from device</param>
        /// <param name="destinationDirectory">Directory on local computer to pull file to</param>
        /// <param name="timeout">The timeout for this operation in milliseconds (Default = -1)</param>
        /// <returns>True if file is pulled, false if pull failed</returns>
        private Task<bool> PullFileTask(string fileOnDevice, string destinationDirectory, int timeout)
        {
            return Task<bool>.Factory.StartNew(() =>
            {
                var adbCmd = Adb.FormAdbCommand(this, "pull", "\"" + fileOnDevice + "\"", "\"" + destinationDirectory + "\"");
                return (Adb.ExecuteAdbCommandReturnExitCode(adbCmd.WithTimeout(timeout)) == 0);
            });
        }

        /// <summary>
        /// Pushes a file to the device
        /// </summary>
        /// <param name="filePath">The path to the file on the computer you want to push</param>
        /// <param name="destinationFilePath">The desired full path of the file after pushing to the device (including file name and extension)</param>
        /// <param name="timeout">The timeout for this operation in milliseconds (Default = -1)</param>
        /// <returns>If the push was successful</returns>
        public bool PushFile(string filePath, string destinationFilePath, int timeout = Command.DefaultTimeout)
        {
            var adbCmd = Adb.FormAdbCommand(this, "push", "\"" + filePath + "\"", "\"" + destinationFilePath + "\"");
            return (Adb.ExecuteAdbCommandReturnExitCode(adbCmd.WithTimeout(timeout)) == 0);
        }

        /// <summary>
        /// Pushes a file to the device asynchronously
        /// </summary>
        /// <param name="filePath">The path to the file on the computer you want to push</param>
        /// <param name="destinationFilePath">The desired full path of the file after pushing to the device (including file name and extension)</param>
        /// <param name="timeout">The timeout for this operation in milliseconds (Default = -1)</param>
        /// <returns>If the push was successful</returns>
        public async Task<bool> PushFileAsync(string filePath, string destinationFilePath,
            int timeout = Command.DefaultTimeout)
        {
            if (!State.Equals(DeviceState.Online)) return false;
            return await PushFileTask(filePath, destinationFilePath, timeout);
        }

        private Task<bool> PushFileTask(string filePath, string destinationFilePath, int timeout)
        {
            return Task<bool>.Factory.StartNew(() =>
            {
                var adbCmd = Adb.FormAdbCommand(this, "push", "\"" + filePath + "\"", "\"" + destinationFilePath + "\"");
                return (Adb.ExecuteAdbCommandReturnExitCode(adbCmd.WithTimeout(timeout)) == 0);
            });
        }

        /// <summary>
        /// Pulls a full directory recursively from the device
        /// </summary>
        /// <param name="location">Path to folder to pull from device</param>
        /// <param name="destination">Directory on local computer to pull file to</param>
        /// <param name="timeout">The timeout for this operation in milliseconds (Default = -1)</param>
        /// <returns>True if directory is pulled, false if pull failed</returns>
        public bool PullDirectory(string location, string destination, int timeout = Command.DefaultTimeout)
        {
            var adbCmd = Adb.FormAdbCommand(this, "pull", "\"" + (location.EndsWith("/") ? location : location + "/") + "\"", "\"" + destination + "\"");
            return (Adb.ExecuteAdbCommandReturnExitCode(adbCmd.WithTimeout(timeout)) == 0);
        }

        /// <summary>
        /// Installs an application from the local computer to the Android device
        /// </summary>
        /// <param name="location">Full path of apk on computer</param>
        /// <param name="timeout">The timeout for this operation in milliseconds (Default = -1)</param>
        /// <returns>True if install is successful, False if install fails for any reason</returns>
        public bool InstallApk(string location, int timeout = Command.DefaultTimeout)
        {
            return !Adb.ExecuteAdbCommand(Adb.FormAdbCommand(this, "install", "\"" + location + "\"").WithTimeout(timeout), true).Contains("Failure");
        }

        /// <summary>
        /// Installs an application from the local computer to the Android device
        /// </summary>
        /// <param name="location">Full path of apk on computer</param>
        /// <param name="timeout">The timeout for this operation in milliseconds (Default = -1)</param>
        /// <returns>True if install is successful, False if install fails for any reason</returns>
        public async Task<bool> InstallApkAsync(string location, int timeout = Command.DefaultTimeout)
        {
            if (State.Equals(DeviceState.Online))
                return await InstallApkTask(location, timeout);
            return false;
        }

        /// <summary>
        /// Task That Handles InstallApk Logic For <see cref="InstallApkAsync"/>
        /// </summary>
        /// <param name="location">Full path of apk on computer</param>
        /// <param name="timeout">The timeout for the operation in milliseconds (Default = -1)</param>
        /// <returns>True if install is successful, else False</returns>
        private Task<bool> InstallApkTask(string location, int timeout)
        {
            return Task<bool>.Factory.StartNew(() => !Adb.ExecuteAdbCommand(
                    Adb.FormAdbCommand(this, "install", "\"" + location + "\"").WithTimeout(timeout), true).Contains("Failure"));
        }

        /// <summary>
        /// Pushes <paramref name="sideloadPackage"/> to Device Via <c>adb sideload</c>.
        /// Only Usable If Using a WPF ListBox Control As Output Console.
        /// </summary>
        /// <param name="sideloadPackage">Sideload Package To Send</param>
        public void Sideload(string sideloadPackage)
        {
            // *** If Device State Isn't DeviceState.Sideload ***
            if (!State.Equals(DeviceState.Sideload)) return;

            // *** If Device State Is DeviceState.Sideload ***
            Adb.Sideload(sideloadPackage);
        }

        /// <summary>
        /// Updates all values in current instance of <see cref="Device"/>
        /// </summary>
        public void Update()
        {
            _state = SetState();

            _su = new Su(this);
            _battery = new BatteryInfo(this);
            _buildProp = new BuildProp(this);
            _busyBox = new BusyBox(this);
            _phone = new Phone(this);
            _fileSystem = new FileSystem(this);
        }
    }
}