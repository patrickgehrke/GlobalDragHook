using EasyHook;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;


namespace GlobalDragHook.DragMonitorHook
{
    public class InjectionEntryPoint : EasyHook.IEntryPoint
    {
        ServerInterface _server = null;

        Queue<string> _messageQueue = new Queue<string>();

        public InjectionEntryPoint(EasyHook.RemoteHooking.IContext context, string channelName)
        {
            // Connect to server object using provided channel name
            _server = RemoteHooking.IpcConnectClient<ServerInterface>(channelName);

            // If Ping fails then the Run method will be not be called
            _server.Ping();
        }

        /// <summary>
        /// The main entry point for our logic once injected within the target process. 
        /// This is where the hooks will be created, and a loop will be entered until host process exits.
        /// EasyHook requires a matching Run method for the constructor
        /// </summary>
        /// <param name="context">The RemoteHooking context</param>
        /// <param name="channelName">The name of the IPC channel</param>
        public void Run(
            EasyHook.RemoteHooking.IContext context,
            string channelName)
        {
            _server.IsInstalled(RemoteHooking.GetCurrentProcessId());

            var createFileHook = LocalHook.Create(LocalHook.GetProcAddress("Ole32.dll", "DoDragDrop"),
                                                  new DoDrag_Hook(DoDragDrop_Hook),
                                                  this);

            // Activate hooks on all threads except the current thread
            createFileHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
            _server.ReportMessage(RemoteHooking.GetCurrentProcessId(), "DoDragDrop hook installed");

            try
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(500);

                    string[] queued = null;

                    lock (_messageQueue)
                    {
                        queued = _messageQueue.ToArray();
                        _messageQueue.Clear();
                    }

                    if (queued != null && queued.Length > 0)
                    {
                        _server.ReportMessages(RemoteHooking.GetCurrentProcessId(), queued);
                    }
                    else
                    {
                        _server.Ping();
                    }
                }
            }
            catch
            {
                // Ping() or ReportMessages() will raise an exception if host is unreachable
            }

            // Remove hooks
            createFileHook.Dispose();

            // Finalise cleanup of hooks
            LocalHook.Release();
        }

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint GetFinalPathNameByHandle(IntPtr hFile, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszFilePath, uint cchFilePath, uint dwFlags);

        [DllImport("Ole32.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        static extern IntPtr DoDragDrop(IDataObject pDataObj, IDropSource pDropSource, uint dwOKEffects, uint[] pdwEffect);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate IntPtr DoDrag_Hook(IDataObject pDataObj, IDropSource pDropSource, uint dwOKEffects, uint[] pdwEffect);

        private IntPtr DoDragDrop_Hook(IDataObject pDataObj, IDropSource pDropSource, uint dwOKEffects, uint[] pdwEffect)
        {
            _server.ReportMessage(RemoteHooking.GetCurrentProcessId(), "[INFO] Drag intercepted");
            return DoDragDrop(pDataObj, pDropSource, dwOKEffects, pdwEffect);
        }
    }
}
