using FDCAPP.Droid.Services;
using FDCAPP.Services;

[assembly: Xamarin.Forms.Dependency(typeof(CloseApp_Droid))]
namespace FDCAPP.Droid.Services
{
    public class CloseApp_Droid : ICloseApplication
    {
        public void CloseApp()
        {
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
        }
    }
}