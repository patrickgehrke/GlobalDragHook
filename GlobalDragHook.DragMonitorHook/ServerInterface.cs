using System;

namespace GlobalDragHook.DragMonitorHook
{
    public class ServerInterface : MarshalByRefObject
    {
        public delegate void DragTriggeredDel();
        public static DragTriggeredDel DragTriggered;

        public void IsInstalled(int clientPID)
        {
            Console.WriteLine("DragMonitorHook injected into process {0}.\r\n", clientPID);
        }

        /// <summary>
        /// Output messages to the console.
        /// </summary>
        /// <param name="clientPID"></param>
        /// <param name="fileNames"></param>
        public void ReportMessages(int clientPID, string[] messages)
        {
            for (int i = 0; i < messages.Length; i++)
            {
                Console.WriteLine(messages[i]);
            }
        }

        public void ReportMessage(int clientPID, string message)
        {
            if (message == "[INFO] Drag intercepted")
                DragTriggered();
            Console.WriteLine(message);
        }

        /// <summary>
        /// Report exception
        /// </summary>
        /// <param name="e"></param>
        public void ReportException(Exception e)
        {
            Console.WriteLine("The target process has reported an error:\r\n" + e.ToString());
        }

        /// <summary>
        /// Called to confirm that the IPC channel is still open / host application has not closed
        /// </summary>
        public void Ping()
        {
        }
    }
}
