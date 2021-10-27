using FDCAPP.Views.Energy;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FDCAPP.Views.Settings
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppShell : Shell
    {
        public Command GoFuelDenTableCommand { get; private set; }
        public Command GoEnergyCommand { get; private set; }
        public Command GoEnergyTypeCommand { get; private set; }
        public Command GoToSettingsCommand { get; private set; }

        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(FuelDenCalcPage), typeof(FuelDenCalcPage));
            Routing.RegisterRoute(nameof(FuelDenTablePage), typeof(FuelDenTablePage));
            Routing.RegisterRoute(nameof(EnergyPage), typeof(EnergyPage));
            Routing.RegisterRoute(nameof(EnergyTypePage), typeof(EnergyTypePage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));

            GoFuelDenTableCommand = new Command(async () => { await Shell.Current.GoToAsync(nameof(FuelDenTablePage)); Shell.Current.FlyoutIsPresented = false; });
            GoEnergyCommand = new Command(async () => { await Shell.Current.GoToAsync(nameof(EnergyPage)); Shell.Current.FlyoutIsPresented = false; });
            GoEnergyTypeCommand = new Command(async () => { await Shell.Current.GoToAsync(nameof(EnergyTypePage)); Shell.Current.FlyoutIsPresented = false; });

            GoToSettingsCommand = new Command(async () => { await Shell.Current.GoToAsync(nameof(SettingsPage)); Shell.Current.FlyoutIsPresented = false; });

            BindingContext = this;
        }

        //private void OpenSettings(object sender, System.EventArgs e)
        //{
        //    try
        //    {
        //        Device.BeginInvokeOnMainThread(async () =>
        //        {
        //            //await Navigation.PushAsync(new SettingsPage());
        //            await Shell.Current.GoToAsync("settingspage");
        //            Shell.Current.FlyoutIsPresented = false;
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Что-то пошло не так
        //        Device.BeginInvokeOnMainThread(async () =>
        //        {
        //            await Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
        //        });
        //        return;
        //    }
        //}



    }
}