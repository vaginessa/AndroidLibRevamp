/*
 * Events.cs - Developed by headygains for AndroidLibWPF.dll - 07/17
 */
using System;

namespace Headygains.Android.Classes.Util
{
    /// <summary>
    /// Stores Output Data From Output Events.
    /// </summary>
    public class OutputEventArgs : EventArgs
    {
        /// <summary>
        /// Contains The Output Data For The Output Event as a <see cref="String"/>.
        /// </summary>
        public string OutputData { get; set; }
    }
}
