using System;
using System.ComponentModel;
using Serilog;

namespace AudioController
{
    public partial class MainWindow
    {
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private bool _isExit;

        public MainWindow()
        {
            this.InitializeComponent();
            Log.Logger = new LoggerConfiguration().MinimumLevel?.Debug()?.WriteTo.RollingFile("AudioControl.log")?.CreateLogger();
            this.DataContext = new AudioControl();

            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += LogExceptionHandler;


            this.notifyIcon = new System.Windows.Forms.NotifyIcon();
            this.notifyIcon.DoubleClick += (s, args) => this.ShowMainWindow();
            this.notifyIcon.Icon = Properties.Resources.Icon;
            this.notifyIcon.Visible = true;
            this.Closing += MainWindow_Closing;

            this.CreateContextMenu();

        }

        private void LogExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            var e = (Exception)args.ExceptionObject;
            Log.Error(e, string.Empty);
        }

        private void CreateContextMenu()
        {
            this.notifyIcon.ContextMenuStrip =
                new System.Windows.Forms.ContextMenuStrip();
            //this.notifyIcon.ContextMenuStrip.Items.Add("MainWindow...").Click += (s, e) => this.ShowMainWindow();
            this.notifyIcon.ContextMenuStrip.Items.Add("Exit").Click += (s, e) => this.ExitApplication();
        }

        private void ExitApplication()
        {
            _isExit = true;
            this.Close();
            this.notifyIcon.Dispose();
            this.notifyIcon = null;
        }

        private void ShowMainWindow()
        {
            if (this.IsVisible)
            {
                //if (this.WindowState == WindowState.Minimized)
                //{
                //    this.WindowState = WindowState.Normal;
                //}
                this.Activate();
            }
            else
            {
                this.Show();
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!_isExit)
            {
                e.Cancel = true;
                this.Hide(); // A hidden window can be shown again, a closed one not
            }
        }
    }
}
