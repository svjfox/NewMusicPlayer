using Microsoft.Maui.Controls;
using MusicPlayerVS.Resources.Themes;
using System.Linq;

namespace MusicPlayerVS
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Загружаем сохраненную тему, по умолчанию светлая
            var isDark = Preferences.Get("IsDarkTheme", false); // Установлено false для светлой темы по умолчанию
            ApplyTheme(isDark);

            MainPage = new NavigationPage(new MainPage());
        }

        public static void ApplyTheme(bool isDark)
        {
            // Удаляем только ранее добавленные словари тем
            var themeDictionaries = Current.Resources.MergedDictionaries
                .Where(d => d.GetType() == typeof(DarkTheme) || d.GetType() == typeof(LightTheme))
                .ToList();

            foreach (var themeDictionary in themeDictionaries)
            {
                Current.Resources.MergedDictionaries.Remove(themeDictionary);
            }

            // Добавляем новый словарь тем
            if (isDark)
            {
                Current.Resources.MergedDictionaries.Add(new DarkTheme());
            }
            else
            {
                Current.Resources.MergedDictionaries.Add(new LightTheme());
            }

            // Сохраняем выбранную тему
            Preferences.Set("IsDarkTheme", isDark);
        }
    }
}
