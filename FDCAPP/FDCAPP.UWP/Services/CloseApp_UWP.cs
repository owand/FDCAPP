using FDCAPP.Services;
using Windows.UI.Xaml;

[assembly: Xamarin.Forms.Dependency(typeof(FDCAPP.UWP.Services.CloseApp_UWP))]
namespace FDCAPP.UWP.Services
{
    public class CloseApp_UWP : ICloseApplication
    {
        public void CloseApp()
        {
            Application.Current.Exit();
        }
    }
}