using System;
using System.IO;

namespace AudioController
{
    internal static class ErrorReporter
    {
        public static event ErrorEventHandler? ErrorOccured;

        public static void ReportException(object sender, Exception exception)
        {
            ErrorOccured?.Invoke(sender, new ErrorEventArgs(exception));
        }
    }
}