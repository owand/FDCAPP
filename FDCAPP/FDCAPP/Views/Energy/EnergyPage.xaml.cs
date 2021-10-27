﻿using FDCAPP.Models.Energy;
using FDCAPP.Resources;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FDCAPP.Views.Energy
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EnergyPage : ContentPage
    {
        private EnergyList viewModel;

        public EnergyPage()
        {
            InitializeComponent();
            LayoutChanged += OnSizeChanged; // Определяем обработчик события, которое происходит, когда изменяется ширина или высота.
            Shell.Current.Navigating += Current_Navigating; // Определяем обработчик события Shell.OnNavigating
        }

        // События непосредственно перед тем как страница становится видимой.
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                if (viewModel == null)
                {
                    viewModel = new EnergyList();
                }

                BindingContext = viewModel;

                await RefreshListView();
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        private void Current_Navigating(object sender, ShellNavigatingEventArgs e)
        {
            if (e.CanCancel)
            {
                e.Cancel(); // Позволяет отменить навигацию
                OnBackButtonPressed();
            }
        }

        // Происходит, когда ширина или высота свойств измените значение на этот элемент.
        private void OnSizeChanged(object sender, EventArgs e)
        {
            OnSizeChangeInterface(); // Изменение интерфейса при изменении размера окна
        }

        // Событие, которое вызывается при выборе отличного от текущего или нового элемента.
        private void OnSelection(object sender, SelectedItemChangedEventArgs e)
        {
            try
            {
                if (e.SelectedItem != null) // Если в Collection есть записи.
                {
                    EditButton.IsEnabled = true; // Кнопка Редактирования активна.
                    DeleteButton.IsEnabled = true; // Кнопка Удаления записи активна.
                    MasterContent.ScrollTo(viewModel.SelectJoinItem, ScrollToPosition.Center, true); // Прокручиваем Scroll до активной записи.
                }
                else
                {
                    EditButton.IsEnabled = false; // Кнопка Редактирования неактивна.
                    DeleteButton.IsEnabled = false; // Кнопка Удаления записи неактивна.
                    return;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        private void OnTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                switch (Device.Idiom)
                {
                    case TargetIdiom.Desktop:
                    case TargetIdiom.Tablet:
                        if (Shell.Current.Width <= 800)
                        {
                            viewModel.DetailMode = true;
                            OnSizeChangeInterface(); // Изменение интерфейса при изменении размера окна
                        }
                        break;

                    case TargetIdiom.Phone:
                        viewModel.DetailMode = true;
                        OnSizeChangeInterface(); // Изменение интерфейса при изменении размера окна
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Фильтр записей отображаемых в ListView.
        private async void OnFilter(object sender, TextChangedEventArgs e)
        {
            try
            {
                await RefreshListView(); // Обновление записей в ListView Collection
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Событие при изменении текста в соответствующих полях.
        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if ((picTYPENAME.SelectedIndex != -1) && !string.IsNullOrEmpty(editNAME.Text))
                {
                    SaveButton.IsEnabled = true; // Кнопка Удаления записи неактивна.
                }
                else
                {
                    SaveButton.IsEnabled = false; // Кнопка Удаления записи неактивна.
                }
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Фильт по типу.
        private async void OnFilterType(object sender, EventArgs e)
        {
            try
            {
                await RefreshListView(); // Обновление записей в ListView Collection
                btCancelFilterType.IsEnabled = true;
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Очищаем фильтр.
        private async void OnCancelFilterType(object sender, EventArgs e)
        {
            try
            {
                picFILTERTYPE.SelectedIndex = -1;
                picFILTERTYPE.SelectedItem = null;

                await RefreshListView(); // Обновление записей в ListView Collection
                btCancelFilterType.IsEnabled = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        #region --------- Header - Command --------

        // Переходим в режим редактирования.
        private void OnEdit(object sender, EventArgs e)
        {
            try
            {
                viewModel.NewJoinItem = null;

                if (viewModel.SelectJoinItem != null)
                {
                    GoToEditState(); // Переходим в режим редактирования.
                }
                else
                {
                    EditButton.IsEnabled = false; // Если нет активной записи в MasterListView, кнопка Редактирования неактивна.
                    DisplayAlert(AppResource.messageAttention, AppResource.messageNoActiveRecord, AppResource.messageСancel); // Что-то пошло не так
                    return;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Создаем новую запись.
        private void OnAdd(object sender, EventArgs e)
        {
            try
            {
                GoToEditState(); // Переходим в режим редактирования.
                viewModel.AddItem(); // Создаем новую запись в объединенной коллекции
                MasterContent.ScrollTo(viewModel.SelectJoinItem, ScrollToPosition.Center, true); // Прокручиваем Scroll до активной записи.
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Удаляем текущую запись.async
        private void OnDelete(object sender, EventArgs e)
        {
            try
            {
                int indexItem = viewModel.Collection.IndexOf(viewModel.SelectJoinItem);

                Device.BeginInvokeOnMainThread(async () =>
                {
                    bool dialog = await DisplayAlert(AppResource.messageTitleAction, AppResource.messageDelete, AppResource.messageOk, AppResource.messageСancel);
                    if (dialog == true)
                    {
                        viewModel.DeleteItem(); // Удаляем текущую запись.

                        if (viewModel.Collection.Count != 0) // Если в Collection есть записи.
                        {
                            if (indexItem == 0) // Если текущая запись первая.
                            {
                                viewModel.SelectJoinItem = viewModel.Collection[indexItem]; // Переходим на следующую запись после удаленной, у которой такой же индекс как и у удаленной.
                            }
                            else
                            {
                                viewModel.SelectJoinItem = viewModel.Collection[indexItem - 1]; // Переходим на предыдующую запись перед удаленной.
                            }
                        }
                        MasterContent.ScrollTo(viewModel.SelectJoinItem, ScrollToPosition.Center, true); // Прокручиваем Scroll до активной записи.
                    }
                    else
                    {
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Сохраняем изменения.
        private void OnSave(object sender, EventArgs e)
        {
            try
            {
                if (viewModel.GetSelectItem() != null)
                {
                    viewModel.SelectItem = viewModel.GetSelectItem();
                    // Данные для текущей основной коллекции
                    viewModel.SelectItem.TYPEID = viewModel.TypeList[picTYPENAME.SelectedIndex].TYPEID;
                    viewModel.SelectItem.ENERGYNAME = viewModel.SelectJoinItem.ENERGYNAME;
                    viewModel.SelectItem.DENSITY = viewModel.SelectJoinItem.DENSITY;
                }
                else
                {
                    viewModel.NewItem = new EnergyModel() // Данные для новой основной коллекции
                    {
                        TYPEID = viewModel.TypeList[picTYPENAME.SelectedIndex].TYPEID,
                        ENERGYNAME = viewModel.SelectJoinItem.ENERGYNAME,
                        DENSITY = viewModel.SelectJoinItem.DENSITY
                    };
                    viewModel.NewSubItem = new EnergySubModel() // Данные для новой подчиненной коллекции
                    {
                        ENERGYID = viewModel.NewItem.ENERGYID,
                        DESCRIPTION = viewModel.SelectJoinItem.DESCRIPTION,
                        NOTE = viewModel.SelectJoinItem.NOTE,
                        LANGUAGE = App.AppLanguage
                    };
                }

                if (viewModel.GetSelectSubItem() != null)
                {
                    viewModel.SelectSubItem = viewModel.GetSelectSubItem();
                    // Данные для текущей подчиненной коллекции
                    viewModel.SelectSubItem.DESCRIPTION = viewModel.SelectJoinItem.DESCRIPTION;
                    viewModel.SelectSubItem.NOTE = viewModel.SelectJoinItem.NOTE;
                    viewModel.SelectSubItem.LANGUAGE = App.AppLanguage;
                }
                else
                {
                    viewModel.NewSubItem = new EnergySubModel() // Данные для новой подчиненной коллекции
                    {
                        ENERGYID = viewModel.SelectJoinItem.ID,
                        DESCRIPTION = viewModel.SelectJoinItem.DESCRIPTION,
                        NOTE = viewModel.SelectJoinItem.NOTE,
                        LANGUAGE = App.AppLanguage
                    };
                }
                viewModel.UpdateItem(); // Сохраняем запись в основной и подчиненной коллекции


                if (viewModel.NewItem != null)
                {
                    viewModel.NewSubItem = new EnergySubModel() // Данные для новой подчиненной коллекции
                    {
                        ENERGYID = viewModel.NewItem.ENERGYID,
                        DESCRIPTION = viewModel.SelectJoinItem.DESCRIPTION,
                        NOTE = viewModel.SelectJoinItem.NOTE,
                        LANGUAGE = App.AppLanguage
                    };
                }
                viewModel.InsertSubItem(); // Сохраняем запись в подчиненной коллекции

                SaveCommandBar.IsVisible = false;
                SearchBar.Focus();
                OnSizeChangeInterface(); // Изменение интерфейса при изменении размера окна
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Отмена изменений.
        private void OnCancel(object sender, EventArgs e)
        {
            try
            {
                Cancel(); // Отмена изменений в записи.
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        #endregion --------- Header - Command --------

        // После обновления записей отображаемых в ListView.
        private void AfterRefreshList()
        {
            try
            {
                if (viewModel.Collection.Count != 0)
                {
                    if (viewModel.NewJoinItem != null)
                    {
                        viewModel.SelectJoinItem = viewModel.PreSelectJoinItem;
                        viewModel.Collection.Remove(viewModel.NewJoinItem);
                    }
                    else if (viewModel.SelectJoinItem == null)
                    {
                        viewModel.SelectJoinItem = viewModel.Collection.FirstOrDefault();
                    }
                    else
                    {
                        viewModel.SelectJoinItem = viewModel.PreSelectJoinItem;
                    }
                    MasterContent.ScrollTo(viewModel.SelectJoinItem, ScrollToPosition.Center, true); // Прокручиваем Scroll до активной записи.
                }
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // hardware back button
        protected override bool OnBackButtonPressed()
        {
            base.OnBackButtonPressed();

            try
            {
                if (viewModel.DetailMode == false)
                {
                    Shell.Current.Navigating -= Current_Navigating; // Отписываемся от события Shell.OnNavigating
                    viewModel = null;
                    Device.BeginInvokeOnMainThread(async () => { await Shell.Current.GoToAsync("..", true); });
                }
                else if ((viewModel.DetailMode == true) && (SaveCommandBar.IsVisible == true))
                {
                    Cancel(); // Отмена изменений в записи.
                }
                else if (viewModel.DetailMode == true)
                {
                    viewModel.DetailMode = false;
                    OnSizeChangeInterface(); // Изменение интерфейса при изменении размера окна
                }
            }
            catch { return false; }
            // Always return true because this method is not asynchronous. We must handle the action ourselves: see above.
            return true;
        }

        // Отмена изменений в записи.
        private void Cancel()
        {
            try
            {
                SaveCommandBar.IsVisible = false;
                AfterRefreshList();  // После обновления записей отображаемых в ListView.

                viewModel.NewJoinItem = null;
                viewModel.SelectItem = null;
                viewModel.SelectSubItem = null;

                SearchBar.Focus();
                viewModel.DetailMode = Device.Idiom != TargetIdiom.Desktop && Device.Idiom != TargetIdiom.Tablet || Shell.Current.Width <= 800;

                OnSizeChangeInterface(); // Изменение интерфейса при изменении размера окна
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Переключение между режимами редактирования и чтения.
        private void GoToEditState()
        {
            try
            {
                viewModel.PreSelectJoinItem = viewModel.SelectJoinItem;
                viewModel.DetailMode = true;
                SaveCommandBar.IsVisible = true;
                OnSizeChangeInterface(); // Изменение интерфейса при изменении размера окна
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Изменение интерфейса при изменении размера окна.
        private void OnSizeChangeInterface()
        {
            try
            {
                switch (Device.Idiom)
                {
                    case TargetIdiom.Desktop:
                    case TargetIdiom.Tablet:
                        if (Shell.Current.Width <= 800)
                        {
                            if (viewModel?.DetailMode == false)
                            {
                                // Компактный вид, режим чтения, детали скрыты.
                                Body.ColumnDefinitions[0].Width = GridLength.Star;
                                Body.ColumnDefinitions[1].Width = new GridLength(0);
                                Master.IsVisible = true;
                            }
                            else
                            {
                                // Компактный вид, режим редактирования, только детали (список мастера скрыт).
                                Body.ColumnDefinitions[0].Width = new GridLength(0);
                                Body.ColumnDefinitions[1].Width = GridLength.Star;
                                Master.IsVisible = false;
                            }
                        }
                        else
                        {
                            // Расширенный (полный) вид, режим чтения.
                            Body.ColumnDefinitions[0].Width = new GridLength(320);
                            Body.ColumnDefinitions[1].Width = GridLength.Star;
                            Master.IsVisible = true;
                        }
                        break;

                    case TargetIdiom.Phone:
                        if (viewModel?.DetailMode == false)
                        {
                            // Компактный вид, режим чтения, детали скрыты.
                            Body.ColumnDefinitions[0].Width = GridLength.Star;
                            Body.ColumnDefinitions[1].Width = new GridLength(0);
                            Master.IsVisible = true;
                        }
                        else
                        {
                            // Компактный вид, режим редактирования, только детали (список мастера скрыт).
                            Body.ColumnDefinitions[0].Width = new GridLength(0);
                            Body.ColumnDefinitions[1].Width = GridLength.Star;
                            Master.IsVisible = false;
                        }
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        private async void OpenFilter(object sender, EventArgs e)
        {
            try
            {
                if (!FilterBar.IsVisible)
                {
                    btCancelFilterType.IsEnabled = false;
                    FilterBar.IsVisible = true;
                }
                else
                {
                    picFILTERTYPE.SelectedIndex = -1;
                    picFILTERTYPE.SelectedItem = null;
                    FilterBar.IsVisible = false;

                    await RefreshListView(); // Обновление записей в ListView Collection
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Обновление записей отображаемых в ListView.
        private async Task RefreshListView()
        {
            try
            {
                IsBusy = true; ;  // Затеняем задний фон и запускаем ProgressRing

                await Task.Delay(100);

                // Обновление записей в ListView Collection
                MasterContent.BeginRefresh();
                MasterContent.IsRefreshing = true;

                viewModel.Collection = viewModel.GetCollection(picFILTERTYPE.SelectedIndex < 0 ? null : viewModel.TypeList?[picFILTERTYPE.SelectedIndex].TYPEID.ToString(), SearchBar.Text);

                MasterContent.ItemsSource = viewModel.Collection;

                MasterContent.IsRefreshing = false;
                MasterContent.EndRefresh();

                IsBusy = false;

                AfterRefreshList();  // После обновления записей отображаемых в ListView.
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }
    }
}