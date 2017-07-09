/*
 * FileSystem.cs - Developed by Dan Wager for AndroidLib.dll
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Headygains.Android.Classes.AndroidController
{
    /// <summary>
    /// Contains mount directory information
    /// </summary>
    public class MountInfo
    {
        private string _directory;
        private string _block;
        private MountType _type;

        internal MountInfo(string directory, string block, MountType type)
        {
            this._directory = directory;
            this._block = block;
            this._type = type;
        }

        /// <summary>
        /// Gets a value indicating the mount directory
        /// </summary>
        public string Directory => this._directory;

        /// <summary>
        /// Gets a value indicating the mount block
        /// </summary>
        public string Block => this._block;

        /// <summary>
        /// Gets a value indicating how the mount directory is mounted
        /// </summary>
        /// <remarks>See <see cref="MountType"/> for more details</remarks>
        public MountType MountType => this._type;
    }

    /// <summary>
    /// Contains information about the Android device's file system
    /// </summary>
    public class FileSystem
    {
        private readonly Device _device;

        private MountInfo _systemMount;

        internal FileSystem(Device device)
        {
            this._device = device;
            UpdateMountPoints();
        }

        private void UpdateMountPoints()
        {
            if (this._device.State != DeviceState.Online)
            {
                this._systemMount = new MountInfo(null, null, MountType.None);
                return;
            }

            var adbCmd = Adb.FormAdbShellCommand(this._device, false, "mount");
            using (var r = new StringReader(Adb.ExecuteAdbCommand(adbCmd)))
            {
                string line;
                string[] splitLine;
                string dir, mount;
                MountType type;

                while (r.Peek() != -1)
                {
                    line = r.ReadLine();
                    splitLine = line.Split(' ');

                    try
                    {
                        if (line.Contains(" on /system "))
                        {
                            dir = splitLine[2];
                            mount = splitLine[0];
                            type = (MountType)Enum.Parse(typeof(MountType), splitLine[5].Substring(1, 2).ToUpper());
                            this._systemMount = new MountInfo(dir, mount, type);
                            return;
                        }

                        if (line.Contains(" /system "))
                        {
                            dir = splitLine[1];
                            mount = splitLine[0];
                            type = (MountType)Enum.Parse(typeof(MountType), splitLine[3].Substring(0, 2).ToUpper());
                            this._systemMount = new MountInfo(dir, mount, type);
                            return;
                        }
                    }
                    catch
                    {
                        dir = "/system";
                        mount = "ERROR";
                        type = MountType.None;
                        this._systemMount = new MountInfo(dir, mount, type);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="MountInfo"/> containing information about the /system mount directory
        /// </summary>
        /// <remarks>See <see cref="MountInfo"/> for more details</remarks>
        public MountInfo SystemMountInfo { get { UpdateMountPoints(); return this._systemMount; } }

        //void PushFile();
        //void PullFile();

        /// <summary>
        /// Mounts connected Android device's file system as specified
        /// </summary>
        /// <param name="type">The desired <see cref="MountType"/> (RW or RO)</param>
        /// <returns>True if remount is successful, False if remount is unsuccessful</returns>
        /// <example>The following example shows how you can mount the file system as Read-Writable or Read-Only
        /// <code>
        /// // This example demonstrates mounting the Android device's file system as Read-Writable
        /// using System;
        /// using RegawMOD.Android;
        /// 
        /// class Program
        /// {
        ///     static void Main(string[] args)
        ///     {
        ///         AndroidController android = AndroidController.Instance;
        ///         Device device;
        ///         
        ///         Console.WriteLine("Waiting For Device...");
        ///         android.WaitForDevice(); //This will wait until a device is connected to the computer
        ///         device = android.ConnectedDevices[0]; //Sets device to the first Device in the collection
        ///
        ///         Console.WriteLine("Connected Device - {0}", device.SerialNumber);
        ///
        ///         Console.WriteLine("Mounting System as RW...");
        ///     	Console.WriteLine("Mount success? - {0}", device.RemountSystem(MountType.RW));
        ///     }
        /// }
        /// 
        ///	// The example displays the following output (if mounting is successful):
        ///	//		Waiting For Device...
        ///	//		Connected Device - {serial # here}
        ///	//		Mounting System as RW...
        ///	//		Mount success? - true
        /// </code>
        /// </example>
        public bool RemountSystem(MountType type)
        {
            if (!this._device.HasRoot)
                return false;

            var adbCmd = Adb.FormAdbShellCommand(this._device, true, "mount", string.Format("-o remount,{0} -t yaffs2 {1} /system", type.ToString().ToLower(), this._systemMount.Block));
            Adb.ExecuteAdbCommandNoReturn(adbCmd);

            UpdateMountPoints();

            if (this._systemMount.MountType == type)
                return true;

            return false;
        }

        private const string IsFile = "if [ -f {0} ]; then echo \"1\"; else echo \"0\"; fi";
        private const string IsDirectory = "if [ -d {0} ]; then echo \"1\"; else echo \"0\"; fi";

        /// <summary>
        /// Gets a <see cref="ListingType"/> indicating is the requested location is a File or Directory
        /// </summary>
        /// <param name="location">Path of requested location on device</param>
        /// <returns>See <see cref="ListingType"/></returns>
        /// <remarks><para>Requires a device containing BusyBox for now, returns ListingType.ERROR if not installed.</para>
        /// <para>Returns ListingType.NONE if file/Directory does not exist</para></remarks>
        public ListingType FileOrDirectory(string location)
        {
            if (!this._device.BusyBox.IsInstalled)
                return ListingType.Error;

            var isFile = Adb.FormAdbShellCommand(this._device, false, string.Format(IsFile, location));
            var isDir = Adb.FormAdbShellCommand(this._device, false, string.Format(IsDirectory, location));

            if (Adb.ExecuteAdbCommand(isFile).Contains("1"))
                return ListingType.File;
            else if (Adb.ExecuteAdbCommand(isDir).Contains("1"))
                return ListingType.Directory;

            return ListingType.None;
        }

        /// <summary>
        /// Gets a <see cref="Dictionary<string, ListingType>"/> containing all the files and folders in the directory added as a parameter.
        /// </summary>
        /// <param name="rootDir">
        /// The directory you'd like to list the files and folders from.
        /// E.G.: /system/bin/
        /// </param>
        /// <returns>See <see cref="Dictionary"/></returns>
        public Dictionary<string, ListingType> GetFilesAndDirectories(string location)
        {
            if (location == null || string.IsNullOrEmpty(location) || Regex.IsMatch(location, @"\s"))
                throw new ArgumentException("rootDir must not be null or empty!");

            var filesAndDirs = new Dictionary<string, ListingType>();
            AdbCommand cmd = null;

            if (_device.BusyBox.IsInstalled)
                cmd = Adb.FormAdbShellCommand(_device, true, "busybox", "ls", "-a", "-p", "-l", location);
            else
                cmd = Adb.FormAdbShellCommand(_device, true, "ls", "-a", "-p", "-l", location);

            using (var reader = new StringReader(Adb.ExecuteAdbCommand(cmd)))
            {
                string line = null;
                while (reader.Peek() != -1)
                {
                    line = reader.ReadLine();
                    if (!string.IsNullOrEmpty(line) && !Regex.IsMatch(line, @"\s"))
                    {
                        filesAndDirs.Add(line, line.EndsWith("/") ? ListingType.Directory : ListingType.File);
                    }
                }
            }


            return filesAndDirs;
        }
    }
}