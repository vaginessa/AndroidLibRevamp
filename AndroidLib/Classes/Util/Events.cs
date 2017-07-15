using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Headygains.Android.Classes.Util
{
    /// <summary>
    /// Stores Output Data From Output Events.
    /// </summary>
    public class OutputEventArgs : EventArgs
    {
        /// <summary>
        /// Contains The Output Data For The Output Event.
        /// </summary>
        public string OutputData { get; set; }
    }
}
