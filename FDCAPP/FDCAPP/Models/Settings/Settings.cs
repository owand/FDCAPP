using FDCAPP.Resources;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FDCAPP.Models.Settings
{
    public class Settings
    {
        public List<LangModel> LangCollection { get; }
        public List<ThemesModel> ThemesCollection { get; }

        public int AppLanguage => LangCollection.IndexOf(LangCollection.Where(X => X.LANGNAME == App.AppLanguage).FirstOrDefault());
        public int AppTheme => ThemesCollection.IndexOf(ThemesCollection.Where(X => X.THEMENAME == App.AppTheme).FirstOrDefault());

        public Settings()
        {
            LangCollection = new List<LangModel>()
            {
                new LangModel { LANGDISPLAY = AppResource.LanguageRus, LANGNAME = "ru" },
                new LangModel { LANGDISPLAY = AppResource.LanguageEng, LANGNAME = "en" }
            };

            ThemesCollection = new List<ThemesModel>()
            {
                new ThemesModel { THEMEDISPLAY = AppResource.ThemesDarkName, THEMENAME = "myDarkTheme" },
                new ThemesModel { THEMEDISPLAY = AppResource.ThemesLightName, THEMENAME = "myLightTheme" },
                new ThemesModel { THEMEDISPLAY =  AppResource.ThemesOSName, THEMENAME = "myOSTheme" }
            };
        }


        public class LangModel
        {
            public string LANGNAME { get; set; }

            public string LANGDISPLAY { get; set; }

            public LangModel()
            {
            }
        }

        public class ThemesModel
        {
            public string THEMENAME { get; set; }

            public string THEMEDISPLAY { get; set; }

            public ThemesModel()
            {
                //THEMENAME = App.AppTheme;
            }
        }

        public async Task ProVersionPurchase()
        {
            if (!Plugin.InAppBilling.CrossInAppBilling.IsSupported)
            {
                return;
            }

            Plugin.InAppBilling.IInAppBilling billing = Plugin.InAppBilling.CrossInAppBilling.Current;

            try
            {
                bool connected = await billing.ConnectAsync();
                if (!connected)
                {
                    await billing.DisconnectAsync();
                    return;
                }

                var verify = Xamarin.Forms.DependencyService.Get<Plugin.InAppBilling.IInAppBillingVerifyPurchase>();
                Plugin.InAppBilling.InAppBillingPurchase purchase = await billing?.PurchaseAsync(App.ProductID, Plugin.InAppBilling.ItemType.InAppPurchase, verify);
                //Plugin.InAppBilling.InAppBillingPurchase purchase = await billing?.PurchaseAsync(App.ProductID, Plugin.InAppBilling.ItemType.Subscription);
                if (purchase == null) // Покупка неудачна
                {
                    await billing.DisconnectAsync();
                    return;
                }
                else if (purchase?.State == Plugin.InAppBilling.PurchaseState.Purchased) // Покупка успешна
                {
                    App.ProState = true;
                    await billing.DisconnectAsync();
                }
                else
                {

                }
                await billing.DisconnectAsync();
                return;
            }
            catch (Plugin.InAppBilling.InAppBillingPurchaseException ex) // Что-то пошло не так
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
                await billing.DisconnectAsync();
                return;
            }
            catch (System.Exception ex) // Что-то пошло не так
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
                await billing.DisconnectAsync();
                return;
            }
            finally
            {
                await billing.DisconnectAsync();
            }
        }

        public static async Task ProVersionCheck()
        {
            //#if DEBUG
            //            await Xamarin.Forms.Application.Current.MainPage.DisplayAlert(AppResource.messageError, "Debug version", AppResource.messageOk); // Что-то пошло не так
            //            return;
            //#else

            switch (Xamarin.Essentials.Connectivity.NetworkAccess)
            {
                case Xamarin.Essentials.NetworkAccess.Internet:
                case Xamarin.Essentials.NetworkAccess.ConstrainedInternet:
                    break;

                default:
                    return;
            }

            if (!Plugin.InAppBilling.CrossInAppBilling.IsSupported)
            {
                return;
            }

            Plugin.InAppBilling.IInAppBilling billing = Plugin.InAppBilling.CrossInAppBilling.Current;
            try
            {
                bool connected = await billing.ConnectAsync();

                if (!connected)
                {
                    await billing.DisconnectAsync();
                    return; //Couldn't connect
                }

                //check purchases
                System.Collections.Generic.IEnumerable<Plugin.InAppBilling.InAppBillingPurchase> purchases = await billing.GetPurchasesAsync(Plugin.InAppBilling.ItemType.InAppPurchase);

                //check for null just incase
                if (purchases?.Any(p => p.ProductId == App.ProductID) ?? false)
                {
                    // покупка найдена
                    App.ProState = true;
                }
                await billing.DisconnectAsync();
                return;
            }
            catch (Plugin.InAppBilling.InAppBillingPurchaseException ex) // Что-то пошло не так
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
                await billing.DisconnectAsync();
                return;
            }
            catch (System.Exception ex)
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                await billing.DisconnectAsync();
                return;
            }
            finally
            {
                await billing.DisconnectAsync();
            }

            //#endif
        }


    }
}