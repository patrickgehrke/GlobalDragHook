using System;
using System.Diagnostics;

namespace GlobalDragHook.Ui.MouseHooking
{
    public class MouseHook : IDisposable
    {
        private const int WH_MOUSE_LL = 14;
        private Stopwatch _stopwatch;
        private IntPtr _hookId;

        public MouseHook()
        {
            this._stopwatch = new Stopwatch();
        }

        public void InstallMouseHook()
        {
            using (var currentProcess = Process.GetCurrentProcess())
            using (var currentModule = currentProcess.MainModule)

            {
                var moduleHandle = Kernel32Interop.GetModuleHandle(currentModule.ModuleName);
                this._hookId = User32Interop.SetWindowsHookEx(WH_MOUSE_LL, MouseHookCallback, moduleHandle, 0);
            }
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && Messages.WM_LBUTTONDOWN == (Messages)wParam)
            {
                _stopwatch.Reset();
                _stopwatch.Start();
            }

            if (nCode >= 0 && Messages.WM_LBUTTONUP == (Messages)wParam)
            {
                _stopwatch.Stop();
            }

            return User32Interop.CallNextHookEx(this._hookId, nCode, wParam, lParam);
        }

        public long ElapsedMilliseconds { get { return this._stopwatch.ElapsedMilliseconds; } }

        public void Dispose()
        {
            User32Interop.UnhookWindowsHookEx(this._hookId);
        }

        private enum Messages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }
    }
}
