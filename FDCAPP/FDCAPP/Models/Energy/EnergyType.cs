using FDCAPP.Resources;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace FDCAPP.Models.Energy
{
    public class EnergyType : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков

        #region --------- Объединенная коллекция --------

        private ObservableCollection<EnergyTypeJoin> collection;
        public ObservableCollection<EnergyTypeJoin> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged(nameof(EnergyTypeJoin));
            }
        }
        public ObservableCollection<EnergyTypeJoin> GetCollection(string SearchCriterion)
        {
            string searchCriterion = SearchCriterion?.ToLower() ?? "";

            List<EnergyTypeJoin> _collection = (from collection in App.Database.Table<EnergyTypeModel>().ToList()
                                                join subCollection in App.Database.Table<EnergyTypeSubModel>().Where(a => a.LANGUAGE == App.AppLanguage).ToList() on collection.TYPEID equals subCollection.TYPEID into joinCollection
                                                from subCollection in joinCollection.DefaultIfEmpty(new EnergyTypeSubModel() { })
                                                select new EnergyTypeJoin()
                                                {
                                                    ID = collection.TYPEID,
                                                    TYPENAME = collection.TYPENAME,
                                                    DESCRIPTION = subCollection.DESCRIPTION,
                                                    NOTE = subCollection.NOTE
                                                }).OrderBy(a => a.TYPENAME).Where(a => a.TYPENAME.ToLowerInvariant().Contains(searchCriterion) ||
                                                (!string.IsNullOrEmpty(a.DESCRIPTION) && a.DESCRIPTION.ToLowerInvariant().Contains(searchCriterion)) ||
                                                (!string.IsNullOrEmpty(a.NOTE) && a.NOTE.ToLowerInvariant().Contains(searchCriterion))).ToList();

            return new ObservableCollection<EnergyTypeJoin>(_collection);
        }

        private EnergyTypeJoin selectJoinItem = null;
        public EnergyTypeJoin SelectJoinItem
        {
            get => selectJoinItem;
            set
            {
                selectJoinItem = value;
                OnPropertyChanged(nameof(SelectJoinItem));
            }
        }

        private EnergyTypeJoin newJoinItem = null;
        public EnergyTypeJoin NewJoinItem
        {
            get => newJoinItem;
            set
            {
                newJoinItem = value;
                OnPropertyChanged(nameof(NewJoinItem));
            }
        }
        public EnergyTypeJoin PreSelectJoinItem { get; set; }

        #endregion ------------------------------------


        #region --------- Основная коллекция --------

        private EnergyTypeModel selectItem = null;
        public EnergyTypeModel SelectItem
        {
            get => selectItem;
            set
            {
                selectItem = value;
                OnPropertyChanged(nameof(SelectItem));
            }
        }

        private EnergyTypeModel newItem = null;
        public EnergyTypeModel NewItem
        {
            get => newItem;
            set
            {
                newItem = value;
                OnPropertyChanged(nameof(NewItem));
            }
        }
        public EnergyTypeModel GetSelectItem()
        {
            return App.Database.Table<EnergyTypeModel>().FirstOrDefault(a => a.TYPEID == SelectJoinItem.ID);
        }

        #endregion ------------------------------------


        #region --------- Подчиненная коллекция --------

        private EnergyTypeSubModel selectSubItem = null;
        public EnergyTypeSubModel SelectSubItem
        {
            get => selectSubItem;
            set
            {
                selectSubItem = value;
                OnPropertyChanged(nameof(SelectSubItem));
            }
        }

        private EnergyTypeSubModel newSubItem = null;
        public EnergyTypeSubModel NewSubItem
        {
            get => newSubItem;
            set
            {
                newSubItem = value;
                OnPropertyChanged(nameof(NewSubItem));
            }
        }
        public EnergyTypeSubModel GetSelectSubItem()
        {
            return App.Database.Table<EnergyTypeSubModel>().FirstOrDefault(a => a.LANGUAGE == App.AppLanguage && a.TYPEID == SelectJoinItem.ID);
        }

        #endregion ------------------------------------


        public bool detailMode = false;
        public bool DetailMode
        {
            get => detailMode;
            set
            {
                detailMode = value;
                OnPropertyChanged(nameof(DetailMode));
            }
        }


        public EnergyType()
        {
            // If the table is empty, initialize the collection
            if (!App.Database.Table<EnergyTypeModel>().ToList().Any())
            {
                Collection?.Add(new EnergyTypeJoin { });
            }
        }

        // Создаем новую запись в объединенной коллекции
        public void AddItem()
        {
            try
            {
                NewJoinItem = new EnergyTypeJoin();
                Collection.Add(NewJoinItem);
                SelectJoinItem = NewJoinItem;
            }
            catch (SQLiteException ex)
            {
                Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                if (NewJoinItem != null)
                {
                    Collection.Remove(NewJoinItem);
                    NewJoinItem = null;
                }
                return;
            }
        }

        // Сохраняем новую или изменяем запись в основной и подчиненной коллекциях
        public void UpdateItem()
        {
            try
            {
                lock (collisionLock)
                {
                    if (SelectItem != null)
                    {
                        App.Database.Update(SelectItem);

                        if (SelectSubItem != null)
                        {
                            App.Database.Update(SelectSubItem);
                            SelectSubItem = null;
                        }
                        else
                        {
                            App.Database.Insert(NewSubItem);
                            NewSubItem = null;
                        }

                        SelectItem = null;
                    }
                    else
                    {
                        App.Database.Insert(NewItem);
                    }
                }

                NewJoinItem = null;

                DetailMode = true;
            }
            catch (SQLiteException ex)
            {
                Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Сохраняем новую в подчиненной коллекции
        public void InsertSubItem()
        {
            try
            {
                lock (collisionLock)
                {
                    App.Database.Insert(NewSubItem);
                }

                NewItem = null;
                NewSubItem = null;
            }
            catch (SQLiteException ex)
            {
                Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Удаляем текущую запись
        public void DeleteItem()
        {
            try
            {
                lock (collisionLock)
                {
                    App.Database.Delete<EnergyTypeModel>(SelectJoinItem.ID);
                    App.Database.Delete<EnergyTypeSubModel>(SelectJoinItem.ID);
                    Collection.Remove(SelectJoinItem);
                }
            }
            catch (SQLiteException ex)
            {
                Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

    }

    // Объединенная коллекция
    public class EnergyTypeJoin : ViewModelBase
    {
        public int ID { get; set; }   // Уникальный код группы
        public string TYPENAME   // Название номенклатурной группы
        {
            get => typename?.ToUpper();
            set
            {
                typename = value?.ToUpper();
                OnPropertyChanged(nameof(TYPENAME));
            }
        }
        public string DESCRIPTION   // Описание
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged(nameof(DESCRIPTION));
            }
        }
        public string NOTE   // Примечания
        {
            get => note;
            set
            {
                note = value;
                OnPropertyChanged(nameof(NOTE));
            }
        }

        public string typename;
        public string description;
        public string note;
    }

    [Table("tbEnergyType")]
    public class EnergyTypeModel : ViewModelBase
    {
        [Column("EnergyTypeID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int TYPEID { get; set; }   // Уникальный код группы

        [Column("EnergyType"), NotNull, Unique, Indexed]
        public string TYPENAME   // Название номенклатурной группы
        {
            get => typename.ToUpper();
            set
            {
                typename = value.ToUpper();
                OnPropertyChanged(nameof(TYPENAME));
            }
        }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<EnergyModel> EnergyModel { get; set; }

        //[OneToMany(CascadeOperations = CascadeOperation.All)]
        //public List<EnergyTypeSubModel> SubModel { get; set; }

        public string typename;
        //public List<EnergyModel> energy;

        public EnergyTypeModel()
        {
        }
    }


    [Table("tbEnergyTypeML")]
    public class EnergyTypeSubModel : ViewModelBase
    {
        [Column("EnergyTypeMLID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int ENERGYTYPESUBID { get; set; }   // Уникальный код

        [Column("EnergyTypeID"), NotNull, Indexed, ForeignKey(typeof(EnergyTypeModel))]
        public int TYPEID { get; set; }  // внешний ключ

        [Column("Description")]
        public string DESCRIPTION   // Описание
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged(nameof(DESCRIPTION));
            }
        }

        [Column("Note")]
        public string NOTE   // Примечания
        {
            get => note;
            set
            {
                note = value;
                OnPropertyChanged(nameof(NOTE));
            }
        }

        [Column("Language"), NotNull, Indexed]
        public string LANGUAGE   // Язык
        {
            get => language;
            set
            {
                language = value;
                OnPropertyChanged(nameof(LANGUAGE));
            }
        }

        public string description;
        public string note;
        public string language;

        public EnergyTypeSubModel()
        {
        }
    }

}
