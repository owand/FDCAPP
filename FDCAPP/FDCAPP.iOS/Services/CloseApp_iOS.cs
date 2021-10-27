using FDCAPP.Services;
using System.Threading;

[assembly: Xamarin.Forms.Dependency(typeof(FDCAPP.iOS.Services.CloseApp_iOS))]
namespace FDCAPP.iOS.Services
{
    public class CloseApp_iOS : ICloseApplication
    {
        public void CloseApp()
        {
            Thread.CurrentThread.Abort();
        }
    }
}