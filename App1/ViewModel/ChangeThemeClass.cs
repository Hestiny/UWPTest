using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace App1.ViewModel
{
   
    class ChangeThemeClass : INotifyPropertyChanged
    {
        private ElementTheme _theme = ElementTheme.Light;
        public ChangeThemeClass()
        {

        }
        public ElementTheme Theme
        {
            get
            {
                return _theme;
            }
            set
            {
                _theme = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged()
        {
            try
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs("Theme"));
                }
            }
            catch (System.Exception ex)
            {

            }
        }
    }
}
