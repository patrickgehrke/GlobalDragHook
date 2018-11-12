using System;
using System.Drawing;
using System.IO;
using System.Runtime.Remoting.Channels.Ipc;
using System.Windows.Forms;

using GlobalDragHook.DragMonitorHook;
using GlobalDragHook.Ui.MouseHooking;

namespace GlobalDragHook.Ui
{
    public partial class Form1 : Form
    {
        private bool _dragged;
        private MouseHook _mouseHook;
        private IpcServerChannel _server;
        private Timer _timer;

        public Form1()
        {
            InitializeComponent();
            this.notifyIcon.ContextMenu = new ContextMenu(new MenuItem[] { new MenuItem("Exit", Exit) });

            this.InjectIntoExplorer();

            this._timer = new Timer() { Interval = 1000 };
            this._timer.Tick += Timer_Tick;
            this._timer.Start();

            this._mouseHook = new MouseHook();
            this._mouseHook.InstallMouseHook();

            ServerInterface.DragTriggered += () =>
            {
                this._dragged = true;
            };
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized &&
                _mouseHook.ElapsedMilliseconds / 1000 >= 2 &&
                IsFileOrFolderSelected() && this._dragged)
            {
                Show();
                WindowState = FormWindowState.Normal;
                notifyIcon.Visible = false;
            }

            this._dragged = false;
        }

        private bool IsFileOrFolderSelected()
        {
            IntPtr handle = User32Interop.GetForegroundWindow();

            var shell = new Shell32.Shell();
            foreach (SHDocVw.InternetExplorer window in shell.Windows())
            {
                if (window.HWND == (int)handle)
                {
                    Shell32.FolderItems items = ((Shell32.IShellFolderViewDual2)window.Document).SelectedItems();
                    if (items.Count > 0)
                        return true;
                }
            }

            return false;
        }

        private void InjectIntoExplorer()
        {
            Int32 targetPID = 2604;
            string channelName = null;

            this._server = EasyHook.RemoteHooking.IpcCreateServer<ServerInterface>(ref channelName, System.Runtime.Remoting.WellKnownObjectMode.Singleton);

            string injectionLibrary = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "GlobalDragHook.DragMonitorHook.dll");
            EasyHook.RemoteHooking.Inject(targetPID, "", injectionLibrary, channelName);
        }

        #region WinForms Events

        private void Exit(object sender, EventArgs e)
        {
            Form1_FormClosed(sender, null);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.notifyIcon.Visible = false;
            this._mouseHook.Dispose();
            this._timer.Dispose();
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                this.notifyIcon.Icon = SystemIcons.Exclamation;
                notifyIcon.Visible = true;
            }
        }

        #endregion
    }
}
