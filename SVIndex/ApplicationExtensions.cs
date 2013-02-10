using System.Windows;
using System.Windows.Threading;

namespace SV_PLI
{
    public static class ApplicationExtensions
    {
        public static void DoEvents(this Application application)
        {
            var frame = new DispatcherFrame();
            application.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        private static object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;

            return null;
        } 
    }
}