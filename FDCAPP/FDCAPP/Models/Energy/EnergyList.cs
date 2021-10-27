using FDCAPP.Resources;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace FDCAPP.Models.Energy
{
    public class EnergyList : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков

        #region --------- Объединенная коллекция --------

        public ObservableCollection<EnergyJoin> collection;
        public ObservableCollection<EnergyJoin> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<EnergyJoin> GetCollection(string FilterCriterion, string SearchCriterion)
        {
            string searchCriterion = SearchCriterion?.ToLower() ?? "";

            List<EnergyJoin> _collection = (from collection in App.Database.Table<EnergyModel>().ToList()
                                            join subCollection in App.Database.Table<EnergySubModel>().Where(a => a.LANGUAGE == App.AppLanguage).ToList() on collection.ENERGYID equals subCollection.ENERGYID into joinCollection
                                            from subCollection in joinCollection.DefaultIfEmpty(new EnergySubModel() { })
                                            select new EnergyJoin()
                                            {
                                                ID = collection.ENERGYID,
                                                TYPEID = collection.TYPEID,
                                                ENERGYNAME = collection.ENERGYNAME,
                                                DENSITY = collection.DENSITY,
                                                DESCRIPTION = subCollection.DESCRIPTION,
                                                NOTE = subCollection.NOTE
                                            }).OrderBy(a => a.ENERGYNAME).Where(a =>
                                            (string.IsNullOrEmpty(FilterCriterion) || a.TYPEID.ToString().Equals(FilterCriterion)) &&
                                            (a.ENERGYNAME.ToLowerInvariant().Contains(searchCriterion) ||
                                            (!string.IsNullOrEmpty(a.DENSITY) && a.DENSITY.ToLowerInvariant().Contains(searchCriterion)) ||
                                            (!string.IsNullOrEmpty(a.DESCRIPTION) && a.DESCRIPTION.ToLowerInvariant().Contains(searchCriterion)) ||
                                            (!string.IsNullOrEmpty(a.NOTE) && a.NOTE.ToLowerInvariant().Contains(searchCriterion)))).ToList();

            foreach (EnergyJoin element in _collection)
            {
                App.Database.GetChildren(element);
            }

            return new ObservableCollection<EnergyJoin>(_collection);
        }

        private EnergyJoin selectJoinItem = null;
        public EnergyJoin SelectJoinItem
        {
            get => selectJoinItem;
            set
            {
                selectJoinItem = value;
                OnPropertyChanged(nameof(SelectJoinItem));
                GetIndexTypeList();
            }
        }

        private EnergyJoin newJoinItem = null;
        public EnergyJoin NewJoinItem
        {
            get => newJoinItem;
            set
            {
                newJoinItem = value;
                OnPropertyChanged(nameof(NewJoinItem));
            }
        }

        public EnergyJoin PreSelectJoinItem { get; set; }

        #endregion ------------------------------------


        #region --------- Основная коллекция --------

        private EnergyModel selectItem = null;
        public EnergyModel SelectItem
        {
            get => selectItem;
            set
            {
                selectItem = value;
                OnPropertyChanged(nameof(SelectItem));
            }
        }

        private EnergyModel newItem = null;
        public EnergyModel NewItem
        {
            get => newItem;
            set
            {
                newItem = value;
                OnPropertyChanged(nameof(NewItem));
            }
        }
        public EnergyModel GetSelectItem()
        {
            return App.Database.Table<EnergyModel>().FirstOrDefault(a => a.ENERGYID == SelectJoinItem.ID);
        }

        #endregion ------------------------------------


        #region --------- Подчиненная коллекция --------

        private EnergySubModel selectSubItem = null;
        public EnergySubModel SelectSubItem
        {
            get => selectSubItem;
            set
            {
                selectSubItem = value;
                OnPropertyChanged(nameof(SelectSubItem));
            }
        }

        private EnergySubModel newSubItem = null;
        public EnergySubModel NewSubItem
        {
            get => newSubItem;
            set
            {
                newSubItem = value;
                OnPropertyChanged(nameof(NewSubItem));
            }
        }
        public EnergySubModel GetSelectSubItem()
        {
            return App.Database.Table<EnergySubModel>().FirstOrDefault(a => a.LANGUAGE == App.AppLanguage && a.ENERGYID == SelectJoinItem.ID);
        }

        #endregion ------------------------------------


        private List<EnergyTypeModel> typeList = App.Database.Table<EnergyTypeModel>().OrderBy(a => a.TYPENAME).ToList();
        public List<EnergyTypeModel> TypeList
        {
            get => typeList;
            set
            {
                typeList = value;
                OnPropertyChanged(nameof(TypeList));
            }
        }

        private int indexTypeList;
        public int IndexTypeList
        {
            get => indexTypeList;
            set
            {
                indexTypeList = value;
                OnPropertyChanged(nameof(IndexTypeList));
            }
        }
        public int GetIndexTypeList()
        {
            IndexTypeList = TypeList.IndexOf(TypeList.Where(X => X.TYPEID == SelectJoinItem?.TYPEID).FirstOrDefault());
            return IndexTypeList;
        }

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


        public EnergyList()
        {
            // If the table is empty, initialize the collection
            if (!App.Database.Table<EnergyModel>().ToList().Any())
            {
                Collection?.Add(new EnergyJoin { });
            }
        }

        // Создаем новую запись в объединенной коллекции
        public void AddItem()
        {
            try
            {
                NewJoinItem = new EnergyJoin();
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
                    App.Database.Delete<EnergyModel>(SelectJoinItem.ID);
                    App.Database.Delete<EnergySubModel>(SelectJoinItem.ID);
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
    public class EnergyJoin : ViewModelBase
    {
        // Catalog
        public int ID { get; set; }   // Уникальный код

        [ForeignKey(typeof(EnergyTypeModel))]     // Specify the foreign key
        public int TYPEID
        {
            get => typeId;
            set
            {
                typeId = value;
                OnPropertyChanged(nameof(TYPEID));
            }
        }

        public string ENERGYNAME
        {
            get => energyname?.ToUpper();
            set
            {
                energyname = value?.ToUpper();
                OnPropertyChanged(nameof(ENERGYNAME));
            }
        }

        public string DENSITY
        {
            get => density;
            set
            {
                density = value;
                OnPropertyChanged(nameof(DENSITY));
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

        [ManyToOne]
        public EnergyTypeModel EnergyType { get; set; }


        // Catalog
        public int typeId;
        public string energyname;
        public string density;

        // Sub Catalog
        public string description;
        public string note;

        public EnergyJoin()
        {
        }
    }


    [Table("tbEnergy")]
    public class EnergyModel : ViewModelBase
    {
        [Column("EnergyID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int ENERGYID { get; set; }   // Уникальный код

        [Column("EnergyTypeID"), NotNull, Indexed, ForeignKey(typeof(EnergyTypeModel))]     // Specify the foreign key
        public int TYPEID
        {
            get => typeId;
            set
            {
                typeId = value;
                OnPropertyChanged(nameof(TYPEID));
            }
        }

        [Column("Energy"), Unique, NotNull, Indexed]
        public string ENERGYNAME
        {
            get => energyname.ToUpper();
            set
            {
                energyname = value.ToUpper();
                OnPropertyChanged(nameof(ENERGYNAME));
            }
        }

        [Column("Density")]
        public string DENSITY
        {
            get => density;
            set
            {
                density = value;
                OnPropertyChanged(nameof(DENSITY));
            }
        }


        public string energyname;
        public int typeId;
        public string density;

        public EnergyModel()
        {
            //ENERGYNAME = null;
            DENSITY = null;
        }
    }



    [Table("tbEnergyML")]
    public class EnergySubModel : ViewModelBase
    {
        [Column("EnergyMLID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int ENERGYSUBID { get; set; }   // Уникальный код

        [Column("EnergyID"), NotNull, Indexed, ForeignKey(typeof(EnergyModel))]
        public int ENERGYID { get; set; }   // внешний ключ

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

        public EnergySubModel()
        {
        }
    }
}
