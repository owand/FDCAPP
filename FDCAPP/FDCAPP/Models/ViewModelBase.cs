using System.Collections.Generic;
using System.Windows.Input;

namespace FDCAPP.Models
{
    public abstract class ViewModelBase : ObservableProperty
    {
        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set
            {
                isBusy = value;
                OnPropertyChanged();
            }
        }

        public Dictionary<string, ICommand> Commands { get; protected set; }

        public ViewModelBase()
        {
            Commands = new Dictionary<string, ICommand>();
        }





    }
}
