using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Headygains;
using Headygains.Android.Classes.AndroidController;

namespace ALRTestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            ConnectedDevices();
            Thread.Sleep(100000);
        }

        static void ConnectedDevices()
        {
            var connectedDevices = AndroidController.Instance.ConnectedDevices;
            connectedDevices.ForEach(Console.WriteLine);
        }
    }
}
