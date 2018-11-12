using System;
using System.Runtime.InteropServices;

namespace GlobalDragHook.DragMonitorHook
{
    [ComImport, Guid("00000121-0000-0000-C000-000000000046"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDropSource
    {
        [PreserveSig]
        uint QueryContinueDrag([MarshalAs(UnmanagedType.Bool)] bool fEscapePressed, uint grfKeyState);

        [PreserveSig]
        uint GiveFeedback(uint dwEffect);
    }
}
