using System;
using System.Collections.Generic;
using System.IO;

namespace Headygains.Android.Classes.AAPT
{
    public partial class Aapt
    {
        /// <summary>
        /// Contains Apk Badging Dump Information 
        /// </summary>
        public class Badging
        {
            #region Constants
            private const string Apostrophe = "'";

            private const string PACKAGE = "package:";
            private const string PackageName = "name='";
            private const string PackageVersionCode = "versionCode='";
            private const string PackageVersionName = "versionName='";

            private const string APPLICATION = "application:";
            private const string ApplicationLabel = "label='";
            private const string ApplicationIcon = "icon='";

            private const string ACTIVITY = "launchable-activity:";
            private const string ActivityName = "name='";
            private const string ActivityLabel = "label='";
            private const string ActivityIcon = "icon='";

            private const string SDK_VERSION = "sdkVersion:'";
            private const string SdkTarget = "targetSdkVersion:'";

            private const string UsesPermission = "uses-permission:'";

            private const string Densities = "densities:";
            #endregion

            #region Fields
            private FileInfo _source;

            private PackageInfo _package;
            private ApplicationInfo _application;
            private LaunchableActivity _activity;

            private string _sdkVersion;
            private string _targetSdkVersion;

            private List<string> _usesPermission;
            private List<int> _densities;
            #endregion

            #region Properties
            /// <summary>
            /// Gets a <c>FileInfo</c> containing information about the source Apk
            /// </summary>
            public FileInfo Source => this._source;

            /// <summary>
            /// Gets a <see cref="PackageInfo"/> containing infomation about the Apk
            /// </summary>
            public PackageInfo Package => this._package;

            /// <summary>
            /// Gets a <see cref="ApplicationInfo"/> containing infomation about the Apk
            /// </summary>
            public ApplicationInfo Application => this._application;

            /// <summary>
            /// Gets a <see cref="LaunchableActivity"/> containing infomation about the Apk
            /// </summary>
            public LaunchableActivity Activity => this._activity;

            /// <summary>
            /// Gets a value indicating the Android Sdk Version of the Apk
            /// </summary>
            public string SdkVersion => this._sdkVersion;

            /// <summary>
            /// Gets a value indicating the Target Android Sdk Version of the Apk
            /// </summary>
            public string TargetSdkVersion => this._targetSdkVersion;

            /// <summary>
            /// Gets a <c>List&lt;string&gt;</c> containing the Android Permissions used by the Apk
            /// </summary>
            public List<string> Permissions => this._usesPermission;

            /// <summary>
            /// Gets a <c>List&lt;int&gt;</c> containing the supported screen densities of the Apk
            /// </summary>
            public List<int> ScreenDensities => this._densities;

            #endregion

            internal Badging(FileInfo source, string dump)
            {
                this._source = source;
                
                this._package = new PackageInfo();
                this._application = new ApplicationInfo();
                this._activity = new LaunchableActivity();

                this._sdkVersion = "";
                this._targetSdkVersion = "";

                this._usesPermission = new List<string>();
                this._densities = new List<int>();

                ProcessDump(dump);
            }

            private void ProcessDump(string dump)
            {
                using (var r = new StringReader(dump))
                {
                    string line;

                    while (r.Peek() != -1)
                    {
                        line = r.ReadLine();

                        if (line.StartsWith(PACKAGE))
                        {
                            //find name
                            var nameStart = line.IndexOf(PackageName) + PackageName.Length;
                            var nameLength = line.IndexOf(Apostrophe, nameStart) - nameStart;
                            var name = line.Substring(nameStart, nameLength);

                            //find versionCode
                            var versionCodeStart = line.IndexOf(PackageVersionCode) + PackageVersionCode.Length;
                            var versionCodeLength = line.IndexOf(Apostrophe, versionCodeStart) - versionCodeStart;
                            var versionCode = line.Substring(versionCodeStart, versionCodeLength);

                            //find versionName
                            var versionNameStart = line.IndexOf(PackageVersionName) + PackageVersionName.Length;
                            var versionNameLength = line.IndexOf(Apostrophe, versionNameStart) - versionNameStart;
                            var versionName = line.Substring(versionNameStart, versionNameLength);

                            this._package = new PackageInfo(name, versionCode, versionName);
                        }
                        else if (line.StartsWith(APPLICATION))
                        {
                            //find label
                            var labelStart = line.IndexOf(ApplicationLabel) + ApplicationLabel.Length;
                            var labelLength = line.IndexOf(Apostrophe, labelStart) - labelStart;
                            var label = line.Substring(labelStart, labelLength);

                            //find icon
                            var iconStart = line.IndexOf(ApplicationIcon) + ApplicationIcon.Length;
                            var iconLength = line.IndexOf(Apostrophe, iconStart) - iconStart;
                            var icon = line.Substring(iconStart, iconLength);

                            this._application = new ApplicationInfo(label, icon);
                        }
                        else if (line.StartsWith(ACTIVITY))
                        {
                            //find name
                            var nameStart = line.IndexOf(ActivityName) + ActivityName.Length;
                            var nameLength = line.IndexOf(Apostrophe, nameStart) - nameStart;
                            var name = line.Substring(nameStart, nameLength);

                            //find label
                            var labelStart = line.IndexOf(ActivityLabel) + ActivityLabel.Length;
                            var labelLength = line.IndexOf(Apostrophe, labelStart) - labelStart;
                            var label = line.Substring(labelStart, labelLength);

                            //find icon
                            var iconStart = line.IndexOf(ActivityIcon) + ActivityIcon.Length;
                            var iconLength = line.IndexOf(Apostrophe, iconStart) - iconStart;
                            var icon = line.Substring(iconStart, iconLength);

                            this._activity = new LaunchableActivity(name, label, icon);
                        }
                        else if (line.StartsWith(SDK_VERSION))
                        {
                            this._sdkVersion = line.Substring(SDK_VERSION.Length).Replace(Apostrophe, "");
                        }
                        else if (line.StartsWith(SdkTarget))
                        {
                            this._targetSdkVersion = line.Substring(SdkTarget.Length).Replace(Apostrophe, "");
                        }
                        else if (line.StartsWith(UsesPermission))
                        {
                            this._usesPermission.Add(line.Substring(UsesPermission.Length).Replace(Apostrophe, ""));
                        }
                        else if (line.StartsWith(Densities))
                        {
                            var densities = line.Substring(Densities.Length + 2).Split(new char[] { '\'', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            for (var i = 0; i < densities.Length; i++)
                                this._densities.Add(int.Parse(densities[i]));
                        }
                    }
                }
            }

            /// <summary>
            /// Contains information about an Apk's package
            /// </summary>
            public class PackageInfo
            {
                private string _name;
                private string _versionCode;
                private string _versionName;

                internal PackageInfo() : this(null, null, null) { }
                internal PackageInfo(string name, string versionCode, string versionName)
                {
                    this._name = name;
                    this._versionCode = versionCode;
                    this._versionName = versionName;
                }

                /// <summary>
                /// Gets a value indicating the Apk's package name
                /// </summary>
                public string Name => this._name;

                /// <summary>
                /// Gets a value indicating the Version Code of the Apk's package
                /// </summary>
                public string VersionCode => this._versionCode;

                /// <summary>
                /// Gets a value indicating the Version Name of the Apk's package
                /// </summary>
                public string VersionName => this._versionName;
            }

            /// <summary>
            /// Contains general information about an Apk
            /// </summary>
            public class ApplicationInfo
            {
                private string _label;
                private string _icon;

                internal ApplicationInfo() : this(null, null) { }
                internal ApplicationInfo(string label, string icon)
                {
                    this._label = label;
                    this._icon = icon;
                }

                /// <summary>
                /// Gets a value indicating the Application's Label
                /// </summary>
                public string Label => this._label;

                /// <summary>
                /// Gets a value indicating the path inside the apk to the Application's default icon
                /// </summary>
                public string Icon => this._icon;
            }

            /// <summary>
            /// Contains information about an Apk's main Activity
            /// </summary>
            public class LaunchableActivity
            {
                private string _name;
                private string _label;
                private string _icon;

                internal LaunchableActivity() : this(null, null, null) { }
                internal LaunchableActivity(string name, string label, string icon)
                {
                    this._name = name;
                    this._label = label;
                    this._icon = icon;
                }

                /// <summary>
                /// Gets a value indicating the name of the Apk's main Activity
                /// </summary>
                public string Name => this._name;

                /// <summary>
                /// Gets a value indicating the label of the Apk's main Activity
                /// </summary>
                public string Label => this._label;

                /// <summary>
                /// Gets a value indicating the path to the default icon of the Apk's main Activity
                /// </summary>
                public string Icon => this._icon;
            }
        }
    }
}