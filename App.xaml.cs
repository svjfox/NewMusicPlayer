using Microsoft.Maui.Controls;
using MusicPlayerVS.Resources.Themes;
using System.Linq;
using System.Diagnostics; // Добавлено пространство имен

namespace MusicPlayerVS
{
    public partial class App : Application
    {
        private readonly ResourceDictionary _lightTheme;
        private readonly ResourceDictionary _darkTheme;

        public App()
        {
            InitializeComponent();

            // Сохраняем ссылки на темы
            _lightTheme = new MusicPlayerVS.Resources.Themes.LightTheme();
            _darkTheme = new MusicPlayerVS.Resources.Themes.DarkTheme();

            // Загружаем сохраненную тему или устанавливаем светлую по умолчанию
            bool isDarkTheme = Preferences.Get("IsDarkTheme", false);
            if (isDarkTheme)
            {
                ApplyDarkTheme();
            }
            else
            {
                ApplyLightTheme();
            }

            MainPage = new NavigationPage(new MainPage());
        }

        public void ApplyLightTheme()
        {
            Debug.WriteLine("Applying Light Theme...");
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(_lightTheme);
            Current.UserAppTheme = AppTheme.Light;

            Preferences.Set("IsDarkTheme", false);
        }

        public void ApplyDarkTheme()
        {
            Debug.WriteLine("Applying Dark Theme...");
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(_darkTheme);
            Current.UserAppTheme = AppTheme.Dark;

            Preferences.Set("IsDarkTheme", true);
        }
    }
}
