using FDCAPP.Models.Settings;
using FDCAPP.Resources;
using FDCAPP.Services;
using FDCAPP.Views.Settings;
using SQLite;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
[assembly: ExportFont("MaterialDesignIcons.ttf#", Alias = "MaterialIcons")]
namespace FDCAPP
{
    public partial class App : Application
    {
        // Переменные для базы данных
        public const string dbName = "DBCatalog.db";
        public const int dbVersion = 64;

        public const SQLite.SQLiteOpenFlags Flags =
            SQLite.SQLiteOpenFlags.ReadWrite | // open the database in read/write mode
                                               //SQLite.SQLiteOpenFlags.Create | // create the database if it doesn't exist
            SQLite.SQLiteOpenFlags.SharedCache | // enable multi-threaded database access
            SQLite.SQLiteOpenFlags.FullMutex;

        public static SQLiteConnection database;
        public static SQLiteConnection Database
        {
            get
            {
                try
                {
                    // путь, по которому будет находиться база данных
                    string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), dbName);
                    //получаем текущую сборку
                    Assembly assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
                    Stream stream = assembly.GetManifestResourceStream($"FDCAPP.{dbName}");

                    if (database == null || database.ExecuteScalar<int>("pragma user_version") < dbVersion)
                    {
                        // если база данных не существует (еще не скопирована)
                        if (!File.Exists(dbPath))
                        {
                            //берем из нее ресурс базы данных и создаем из него поток
                            using (stream)
                            {
                                using (FileStream fs = new FileStream(dbPath, FileMode.OpenOrCreate))
                                {
                                    stream.CopyTo(fs);  // копируем файл базы данных в нужное нам место
                                    fs.Flush();
                                }
                            }
                        }

                    }
                    database = new SQLiteConnection(dbPath, Flags, false);
                    return database;
                }
                catch (Exception ex)
                {
                    Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                    if (Device.RuntimePlatform == Xamarin.Forms.Device.Android)
                    {
                        DependencyService.Get<ICloseApplication>().CloseApp();
                    }
                    return database = null;
                }
            }
        }

        // Переменные для подключения приложения к личному account Microsoft, используя Microsoft Graph API
        public static string ClientID = "7c1429f7-2dfb-467e-8e10-e79886bef26c";
        public static string[] Scopes = { "Files.ReadWrite.All", "Files.ReadWrite.AppFolder" };
        public static object ParentWindow { get; set; }

        //переменные для изменения локализации приложения
        public static string AppLanguage
        {
            get => Xamarin.Essentials.Preferences.Get("currentLanguage", "ru");
            set => Xamarin.Essentials.Preferences.Set("currentLanguage", value);
        }

        public static string ENERGY // Переменая фильтра по виду топлива
        {
            get => Xamarin.Essentials.Preferences.Get("ENERGYNEW", string.Empty);
            set => Xamarin.Essentials.Preferences.Set("ENERGYNEW", value);
        }
        public static string BaseTEMP // Переменая фильтра по базовой температуре
        {
            get => Xamarin.Essentials.Preferences.Get("BaseTEMPNEW", string.Empty);
            set => Xamarin.Essentials.Preferences.Set("BaseTEMPNEW", value);
        }


        //переменные для применения темы
        public static string AppTheme
        {
            get => Xamarin.Essentials.Preferences.Get("currentTheme", "myOSTheme");
            set => Xamarin.Essentials.Preferences.Set("currentTheme", value);
        }

        //переменные для Purchases State
        public static readonly string ProductID = "adblock";

        public static bool ProState
        {
            get => Xamarin.Essentials.Preferences.Get("ProState", false);
            set => Xamarin.Essentials.Preferences.Set("ProState", value);
        }



        public App()
        {
            Device.SetFlags(new string[] { "MediaElement_Experimental", "Shell_UWP_Experimental", "Visual_Experimental",
                                           "CollectionView_Experimental", "FastRenderers_Experimental", "CarouselView_Experimental",
                                           "IndicatorView_Experimental", "RadioButton_Experimental", "AppTheme_Experimental",
                                           "Markup_Experimental", "Expander_Experimental" });
            InitializeComponent();

            // Языковая культура приложения должна определяться как можно раньше.
            AppResource.Culture = new CultureInfo(AppLanguage);

            // Theme of the application
            switch (AppTheme)
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

            if (ProState == false)
            {
                Device.BeginInvokeOnMainThread(async () => { await Settings.ProVersionCheck(); });
            }

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
