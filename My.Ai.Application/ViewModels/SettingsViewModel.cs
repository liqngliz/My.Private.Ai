using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace My.Ai.App.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private bool _isDarkMode;
        private string _username;

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SaveCommand { get; }

        public SettingsViewModel()
        {
            // Default values
            Username = "User";
            IsDarkMode = false;

            SaveCommand = new Command(SaveSettings);
        }

        private void SaveSettings()
        {
            // Here you would typically save settings to persistent storage
            // For example using Preferences or a local database
            
            // For demo purposes, just display an alert
            Application.Current.MainPage.DisplayAlert("Settings", "Settings saved successfully!", "OK");
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}