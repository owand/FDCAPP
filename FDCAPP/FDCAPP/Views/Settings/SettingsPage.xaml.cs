using FDCAPP.Resources;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FDCAPP.Views.Settings
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        public Models.Settings.Settings settingsViewModel;

        public SettingsPage()
        {
            InitializeComponent();

            BindingContext = settingsViewModel = new Models.Settings.Settings();

            PickerLanguages.SelectedIndexChanged += OnLanguagesChanged;
            LayoutChanged += OnSizeChanged; // Определяем обработчик события, которое происходит, когда изменяется ширина или высота.
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                IsBusy = true;

                labAppName.Text = AppInfo.Name; // Application Name
                switch (Xamarin.Forms.Device.RuntimePlatform)
                {
                    case Xamarin.Forms.Device.iOS:
                    case Xamarin.Forms.Device.Android:
                        labAppVersion.Text = $"{AppInfo.VersionString}." + $"{AppInfo.BuildString}"; // Application Version (1.0.0)
                        break;

                    case Xamarin.Forms.Device.UWP:
                        labAppVersion.Text = $"{AppInfo.VersionString}"; // Application Version (1.0.0)
                        break;

                    default:
                        break;
                }

                IsBusy = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Происходит, когда ширина или высота свойств измените значение на этот элемент.
        private void OnSizeChanged(object sender, EventArgs e)
        {
            try
            {
                if (Shell.Current.Width > 1000)
                {
                    About.SetValue(Grid.ColumnProperty, 2);
                    About.SetValue(Grid.RowProperty, 0);
                    SettingsContent.ColumnDefinitions[2].Width = 500;
                }
                else
                {
                    About.SetValue(Grid.ColumnProperty, 0);
                    About.SetValue(Grid.RowProperty, 1);
                    SettingsContent.ColumnDefinitions[2].Width = 0;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        private void OnLanguagesChanged(object sender, EventArgs e)
        {
            try
            {
                Xamarin.Essentials.Preferences.Set("currentLanguage", settingsViewModel.LangCollection[PickerLanguages.SelectedIndex].LANGNAME);
                AppResource.Culture = new System.Globalization.CultureInfo(App.AppLanguage);

                //((App)Application.Current).MainPage = new MainPage(); // Refresh App
                //((App)Application.Current).MainPage = new AppShell(); // Refresh App
                App.Current.MainPage = new AppShell();
            }
            catch (Exception)
            {
                return;
            }
        }

        private void OnThemesChanged(object sender, EventArgs e)
        {
            try
            {
                Xamarin.Essentials.Preferences.Set("currentTheme", settingsViewModel.ThemesCollection[PickerThemes.SelectedIndex].THEMENAME);

                switch (settingsViewModel.ThemesCollection[PickerThemes.SelectedIndex].THEMENAME)
                {
                    case "myDarkTheme":
                        App.Current.UserAppTheme = OSAppTheme.Dark;
                        break;

                    case "myLightTheme":
                        App.Current.UserAppTheme = OSAppTheme.Light;
                        break;

                    case "myOSTheme":
                        App.Current.UserAppTheme = OSAppTheme.Unspecified;
                        break;

                    default:
                        App.Current.UserAppTheme = OSAppTheme.Unspecified;
                        break;
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        //------------------Tapped----------------------
        #region Tapped events

        private void Tapped_privacyPolicy(object sender, EventArgs e)
        {
            Launcher.OpenAsync(new Uri("https://sites.google.com/view/owand/privacy"));
        }

        private void Tapped_siteProject(object sender, EventArgs e)
        {
            Launcher.OpenAsync(new Uri("https://sites.google.com/view/owand/FDCApp"));
        }

        private void Tapped_mailAuthor(object sender, EventArgs e)
        {
            Launcher.OpenAsync(new Uri("mailto:plowand@outlook.com"));
        }

        private void OpenReviewStore(object sender, EventArgs e)
        {
            switch (Xamarin.Forms.Device.RuntimePlatform)
            {
                case Xamarin.Forms.Device.Android:
                    OpenGooglePlay();
                    break;
                case Xamarin.Forms.Device.UWP:
                    OpenMicrosoftStore();
                    break;
                default:
                    break;
            }
        }

        private void OpenStore(object sender, EventArgs e)
        {
            switch (Xamarin.Forms.Device.RuntimePlatform)
            {
                case Xamarin.Forms.Device.Android:
                    OpenMicrosoftStore();
                    break;
                case Xamarin.Forms.Device.UWP:
                    OpenGooglePlay();
                    break;
                default:
                    break;
            }
        }

        private void OpenMicrosoftStore()
        {
            switch (Xamarin.Forms.Device.RuntimePlatform)
            {
                case Xamarin.Forms.Device.Android:
                    Launcher.OpenAsync(new Uri("https://www.microsoft.com/store/apps/9NC7ZG5DWG34"));
                    break;
                case Xamarin.Forms.Device.UWP:
                    Launcher.OpenAsync(new Uri("ms-windows-store://review/?productid=9NC7ZG5DWG34"));
                    break;
                default:
                    break;
            }
        }

        private void OpenGooglePlay()
        {
            Launcher.OpenAsync(new Uri("https://play.google.com/store/apps/details?id=com.plowand.fdcapp"));
        }

        #endregion

        //------------------Purchases----------------------
        #region Purchases events

        private async void ProVersionPurchase(object sender, EventArgs e)
        {
            try
            {
                await settingsViewModel.ProVersionPurchase();
                if (App.ProState == true)
                {
                    slProState.IsVisible = false;
                }
            }
            catch (Exception ex) // Что-то пошло не так
            {
                await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
                return;
            }
        }

        #endregion

    }
}