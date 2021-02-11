using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace VersiondogTimeout {
    class MainClass {
        // Imports for win32api GetLastInputInfo
        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO {
            public static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));

            [MarshalAs(UnmanagedType.U4)]
            public UInt32 cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dwTime;
        }

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        public static void Main() {
            String[] arguments = Environment.GetCommandLineArgs();

            // Versiondog waits for the program to exit before continuing
            // To get around this, execute this program ourselves before exiting
            // Versiondog will start the program with a whole bunch of arguments so
            // make use of this to identify which is the entry point
            // Dodgy way of doing fork basically
            if (arguments.Length != 1) {
                Process.Start(arguments[0]);
                System.Environment.Exit(0);
            }

            Process[] process = Process.GetProcessesByName("VDogClient");

            // Exit if versiondog isnt running
            while (process.Length > 0) {
                // 15 min timer
                if (GetLastInputTime() > 900) {
                    process[0].CloseMainWindow();
                    System.Environment.Exit(0);
                }
                System.Threading.Thread.Sleep(1000);
                process = Process.GetProcessesByName("VDogClient");
            }

            System.Environment.Exit(0);
        }


        // Get the current idle time in s
        static uint GetLastInputTime() {
            uint idleTime = 0;
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint) Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;

            uint envTicks = (uint) Environment.TickCount;

            if (GetLastInputInfo(ref lastInputInfo)) {
                uint lastInputTick = lastInputInfo.dwTime;

                idleTime = envTicks - lastInputTick;
            }

            return ((idleTime > 0) ? (idleTime / 1000) : 0);
        }
    }
}
