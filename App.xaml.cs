using Microsoft.Maui.Controls;
using MusicPlayerVS.Resources.Themes;
using System.Linq;

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
            _lightTheme = new LightTheme();
            _darkTheme = new DarkTheme();

            // Загружаем сохраненную тему или устанавливаем светлую по умолчанию
            var isDark = Preferences.Get("IsDarkTheme", false); // false - светлая тема по умолчанию
            if (isDark)
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
            // Очищаем словари и добавляем светлую тему
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(_lightTheme);
            Current.UserAppTheme = AppTheme.Light;

            // Сохраняем выбор пользователя
            Preferences.Set("IsDarkTheme", false); // Устанавливаем false для светлой темы
        }

        public void ApplyDarkTheme()
        {
            // Очищаем словари и добавляем темную тему
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(_darkTheme);
            Current.UserAppTheme = AppTheme.Dark;

            // Сохраняем выбор пользователя
            Preferences.Set("IsDarkTheme", true); // Устанавливаем true для темной темы
        }
    }
}
