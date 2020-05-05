/* Brother Control Center Manager
 *  (directly launches Brother's Control Center 4 program, to avoid having to launch through Brother Utilities program,
 *   waits in background until UI is closed, closes background processes and removes icon from system tray, then exits)
 * ---------------------------------------------------------------------------------------------------------------------------------
 * Copyright 2020 Alice Buchtufer (https://github.com/buchtufer)
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files 
 * (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, 
 * merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is 
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR 
 * IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * ---------------------------------------------------------------------------------------------------------------------------------
 */

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BrotherControlCenterManager
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            //Brother variables (see contents of config files ex. C:\Program Files (x86)\Brother\BrLauncher\<MODEL>.ini after Brother install/setup)
            const string EXE_INSTALL_PATH = @"C:\Program Files (x86)\ControlCenter4\BrCcBoot.exe";
            const string EXE_ARG_TEMPLATE = "/C /model=\"{0}\" /OpenTab=0x{1:X2}"; //ex. @"/C /model=""MFC-J880DW LAN"" /OpenTab=0x01"
            const string MODEL = "MFC-J880DW LAN"; //hard-coded so only for one (from files at C:\Program Files (x86)\Brother\BrLauncher\<MODEL>.ini)
            const int TAB = 1; //tab in Control Center to open to ex. SCAN=1, MORE = 2, PCFAX = 3, TOOL = 4, PHOTO = 5, SUPPLY = 6, SUPPORT = 7
            const string MAIN_PROC_NAME = "BrCtrlCntr"; //the constantly-running background / system tray process (ControlCenter4\BrCtrlCntr.exe)
            const string UI_PROC_NAME = "BrCcUxSys"; //the actual UI process - keeps running in background after close (ControlCenter4\BrCcUxSys.exe)

            //run ControlCenter4 directly
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = EXE_INSTALL_PATH;
            startInfo.Arguments = string.Format(EXE_ARG_TEMPLATE, MODEL, TAB);
            process.StartInfo = startInfo;
            process.Start();

            //don't run cleaner service if already running
            if (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location)).Count() > 1)
                Environment.Exit(0);

            //monitor ControlCenter4 actual UI
            bool runningUI;
            do
            {
                //wait, allow other processes to run (also allow app to start in first place)
                Thread.Sleep(2500);

                runningUI = false;
                Process[] cc4UIprocesses = Process.GetProcessesByName(UI_PROC_NAME);
                foreach (Process proc in cc4UIprocesses)
                {
                    //close any that are not actually running UI (probably unnecessary)
                    if (proc.MainWindowHandle == IntPtr.Zero)
                    {
                        proc.Kill();
                        RefreshTrayArea();
                    }
                    //if any ARE running, keep looping
                    else
                        runningUI = true;
                }
            }
            while (runningUI);

            //kill the background process (the one that stays in the system tray)
            Process[] cc4BGprocesses = Process.GetProcessesByName(MAIN_PROC_NAME);
            foreach (Process proc in cc4BGprocesses)
            {
                proc.Kill(); //doesn't play nice, have to kill
                Thread.Sleep(1000); //give process time to die from being killed (so icon is actually removed)
                RefreshTrayArea(); //killing doesn't remove icon from tray, refresh it
            }

            //exit
            Environment.Exit(0);
        }

        #region RefreshTrayArea
        //code to refresh icons in system tray

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        //HAVE to use for correct result, CAN'T use "equivalent" Rect in System.Windows (WindowBase) or Rectangle in System.Drawing
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        //find handle of tray area(s) and call refresh
        public static void RefreshTrayArea()
        {
            IntPtr systemTrayContainerHandle = FindWindow("Shell_TrayWnd", null);
            IntPtr systemTrayHandle = FindWindowEx(systemTrayContainerHandle, IntPtr.Zero, "TrayNotifyWnd", null);
            IntPtr sysPagerHandle = FindWindowEx(systemTrayHandle, IntPtr.Zero, "SysPager", null);
            IntPtr notificationAreaHandle = FindWindowEx(sysPagerHandle, IntPtr.Zero, "ToolbarWindow32", "Notification Area");
            if (notificationAreaHandle == IntPtr.Zero)
            {
                notificationAreaHandle = FindWindowEx(sysPagerHandle, IntPtr.Zero, "ToolbarWindow32", "User Promoted Notification Area");
                IntPtr notifyIconOverflowWindowHandle = FindWindow("NotifyIconOverflowWindow", null);
                IntPtr overflowNotificationAreaHandle = FindWindowEx(notifyIconOverflowWindowHandle, IntPtr.Zero, "ToolbarWindow32", "Overflow Notification Area");
                RefreshTrayArea(overflowNotificationAreaHandle);
            }
            RefreshTrayArea(notificationAreaHandle); //"Notification Area" OR "User Promoted Notification Area"
        }

        //refresh tray area - "mouse-over" all icons
        private static void RefreshTrayArea(IntPtr windowHandle)
        {
            const uint wmMousemove = 0x0200;
            RECT rect;
            GetClientRect(windowHandle, out rect);
            for (var x = 0; x < rect.Right; x += 5)
            {
                for (var y = 0; y < rect.Bottom; y += 5)
                {
                    SendMessage(windowHandle, wmMousemove, 0, (y << 16) + x);
                }
            }
        }
        #endregion
    }
}
