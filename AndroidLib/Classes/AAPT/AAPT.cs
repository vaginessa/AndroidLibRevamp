using System;
using System.Collections.Generic;
using System.IO;
using Headygains.Android.Classes.Util;

namespace Headygains.Android.Classes.AAPT
{
    /// <summary>
    /// Wrapper for the AAPT Android binary
    /// </summary>
    public partial class Aapt : IDisposable
    {
        private static Dictionary<string, string> _resources = new Dictionary<string, string>
        {
            {"aapt.exe", "26a35ee028ed08d7ad0d18ffb6bb587a"}
        };

        private string _resDir;

        /// <summary>
        /// Initializes a new instance of the <c>AAPT</c> class
        /// </summary>
        public Aapt()
        {
            ResourceFolderManager.Register("AAPT");
            this._resDir = ResourceFolderManager.GetRegisteredFolderPath("AAPT");

            ExtractResources(this._resDir);
        }

        /// <summary>
        /// Dumps the specified Apk's badging information
        /// </summary>
        /// <param name="source">Source Apk on local machine</param>
        /// <returns><see cref="Aapt.Badging"/> object containing badging information</returns>
        public Badging DumpBadging(FileInfo source)
        {
            if (!source.Exists)
                throw new FileNotFoundException();

            return new Badging(source, Command.RunProcessReturnOutput(Path.Combine(this._resDir, "aapt.exe"), "dump badging \"" + source.FullName + "\"", true, Command.DefaultTimeout));
        }

        private void ExtractResources(string path)
        {
            var res = new string[_resources.Count];
            _resources.Keys.CopyTo(res, 0);

            Extract.Resources("RegawMOD.Android", path, "Resources.AAPT", res);
        }

        /// <summary>
        /// Call to free up resources after use of <c>AAPT</c>
        /// </summary>
        public void Dispose()
        {
            ResourceFolderManager.Unregister("AAPT");
        }
    }
}
