using System;
using System.ComponentModel;
using System.Reflection;
using Serilog;

namespace AudioController
{
    public partial class MainWindow
    {
        private bool _isExit;
        private System.Windows.Forms.NotifyIcon notifyIcon;

        public MainWindow()
        {
            this.InitializeComponent();
            Log.Logger = new LoggerConfiguration().MinimumLevel?.Debug()?.WriteTo.File("AudioControl.log")?.CreateLogger();
            this.DataContext = new AudioControl();

            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += LogExceptionHandler;

            this.notifyIcon = new System.Windows.Forms.NotifyIcon();
            this.notifyIcon.DoubleClick += (s, args) => this.ShowMainWindow();
            this.notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            this.notifyIcon.Visible = true;
            this.Closing += this.MainWindow_Closing;

            this.CreateContextMenu();
        }

        private void CreateContextMenu()
        {
            this.notifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            this.notifyIcon.ContextMenuStrip.Items.Add("Exit").Click += (s, e) => this.ExitApplication();
        }

        private void ExitApplication()
        {
            _isExit = true;
            this.Close();
            this.notifyIcon.Dispose();
        }

        private void LogExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            var e = (Exception)args.ExceptionObject;
            Log.Error(e, string.Empty);
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (!_isExit)
            {
                e.Cancel = true;
                this.Hide(); // A hidden window can be shown again, a closed one not
            }
        }

        private void ShowMainWindow()
        {
            if (this.IsVisible)
            {
                this.Activate();
            }
            else
            {
                this.Show();
            }
        }
    }
}