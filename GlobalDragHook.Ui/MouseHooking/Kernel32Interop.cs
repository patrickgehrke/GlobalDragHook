using System;
using System.Runtime.InteropServices;

namespace GlobalDragHook.Ui.MouseHooking
{
    public static class Kernel32Interop
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
